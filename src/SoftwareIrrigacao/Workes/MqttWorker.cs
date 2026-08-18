using Toolbox.Industrial.Core.Communication.Mqtt;

namespace SoftwareIrrigacao.Workes;

public class MqttWorker : BackgroundService
{
    private bool _disposed = false;
    private readonly MqttManager _mqttLocal;
    private readonly MqttManager _mqttRemoto;
    private readonly MqttManager _mqttInterno;
    private readonly ILogger<MqttWorker> _logger;

    public MqttWorker(
        ILogger<MqttWorker> logger,
        [FromKeyedServices(Mqtt.Local)] MqttManager mqttLocal,
        [FromKeyedServices(Mqtt.Remoto)] MqttManager mqttRemoto,
        [FromKeyedServices(Mqtt.Interno)] MqttManager mqttInterno
    )
    {
        _logger = logger;
        _mqttLocal = mqttLocal;
        _mqttRemoto = mqttRemoto;
        _mqttInterno = mqttInterno;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var localStarted = false;
        var remoteStarted = false;
        var internalStarted = false;
        while (!_disposed && !stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!internalStarted && _mqttInterno.Current != null)
                {
                    internalStarted = await _mqttInterno.Current.ConnectAsync();
                }

                if (!localStarted && _mqttLocal.Current != null)
                {
                    localStarted = await _mqttLocal.Current.ConnectAsync();
                }

                if (!remoteStarted && _mqttRemoto.Current != null)
                {
                    remoteStarted = await _mqttRemoto.Current.ConnectAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na preparação do MQTT");
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_disposed)
            return;

        _disposed = true;
        await Task.Delay(10);
        try
        {
            _mqttLocal.Current?.Dispose();
            _mqttRemoto.Current?.Dispose();
            _mqttInterno.Current?.Dispose();
        }
        finally
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
