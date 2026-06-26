using Microsoft.Extensions.Options;
using SoftwareIrrigacao.Data;
using SoftwareIrrigacao.Infrastructure.Mqtt;
using SoftwareIrrigacao.Setup;
using SoftwareIrrigacao.Shared.Configuration;

namespace SoftwareIrrigacao.Workers;

public class MqttWorker : BackgroundService
{
    private readonly MqttClienteRemoto _mqttClienteRemoto;
    private readonly MqttClienteLocal _mqttClienteLocal;
    private readonly ILogger<MqttWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MqttConfiguracao _mqttConfiguracao;

    public MqttWorker(
        MqttClienteRemoto mqttClienteRemoto,
        MqttClienteLocal mqttClienteLocal,
        ILogger<MqttWorker> logger,
        IServiceProvider serviceProvider,
        IOptions<MqttConfiguracao> mqttConfiguracao
    )
    {
        _mqttClienteRemoto = mqttClienteRemoto;
        _mqttClienteLocal = mqttClienteLocal;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _mqttConfiguracao = mqttConfiguracao.Value;
    }

    private bool ConexaoLocalAtiva = false;
    private bool ConexaoRemotaAtiva = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var _context = scope.ServiceProvider.GetRequiredService<SoftwareIrrigacaoContext>();

                if (!_mqttClienteLocal.Conectado)
                    ConexaoLocalAtiva = false;

                if (!_mqttClienteRemoto.Conectado)
                    ConexaoRemotaAtiva = false;

                if (!ConexaoLocalAtiva)
                {
                    await _mqttClienteLocal.ConectarAsync(
                        _mqttConfiguracao.Servidor,
                        _mqttConfiguracao.Porta,
                        Guid.NewGuid().ToString(),
                        _mqttConfiguracao.Usuario,
                        _mqttConfiguracao.Senha,
                        stoppingToken
                    );
                }

                if (!ConexaoRemotaAtiva)
                {
                    await _mqttClienteRemoto.ConectarAsync(
                        "broker.freemqtt.com",
                        1883,
                        Guid.NewGuid().ToString(),
                        "freemqtt",
                        "public",
                        stoppingToken
                    );
                }

                if (_mqttClienteRemoto.Conectado && !ConexaoRemotaAtiva)
                {
                    ConexaoRemotaAtiva = true;

                    _mqttClienteRemoto.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteRemoto.ExecutarCallbackDesconectado(stoppingToken);
                }

                if (_mqttClienteLocal.Conectado && !ConexaoLocalAtiva)
                {
                    ConexaoLocalAtiva = true;

                    _mqttClienteLocal.ExecutarCallbackMensageria(stoppingToken);
                    _mqttClienteLocal.ExecutarCallbackDesconectado(stoppingToken);
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
}
