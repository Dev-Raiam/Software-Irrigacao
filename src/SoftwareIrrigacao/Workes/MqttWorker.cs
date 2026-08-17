using Newtonsoft.Json;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;

namespace SoftwareIrrigacao.Workes;

public class MqttWorker : BackgroundService
{
    private bool _disposed = false;
    private readonly IEntityStore _store;
    private readonly MqttManager _mqttLocal;
    private readonly MqttManager _mqttRemoto;
    private readonly MqttManager _mqttInterno;
    private readonly ILogger<MqttWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly JsonSerializerSettings _mqttRemotoSerializer;
    private readonly JsonSerializerSettings _mqttInternoSerializer;

    public MqttWorker(
        [FromKeyedServices(Mqtt.Interno)] MqttManager mqttInterno,
        [FromKeyedServices(Mqtt.Local)] MqttManager mqttLocal,
        [FromKeyedServices(Mqtt.Remoto)] MqttManager mqttRemoto,
        IServiceProvider serviceProvider,
        ILogger<MqttWorker> logger,
        IEntityStore store
    )
    {
        _store = store;
        _logger = logger;
        _mqttLocal = mqttLocal;
        _mqttRemoto = mqttRemoto;
        _mqttInterno = mqttInterno;
        _serviceProvider = serviceProvider;

        _mqttInternoSerializer = JsonConvert.DefaultSettings!.Invoke();
        _mqttInternoSerializer.Formatting = Formatting.Indented;
        _mqttInternoSerializer.TypeNameHandling = TypeNameHandling.Objects;
        _mqttInterno.Current?.SetHandler(
            async (topic, payload) =>
            {
                await ProcessarMensagemInternoAsync(topic, payload);
            }
        );

        _mqttLocal.Current?.SetHandler(
            async (topic, payload) =>
            {
                await ProcessarMensagemLocalAsync(topic, payload);
            }
        );

        _mqttRemotoSerializer = JsonConvert.DefaultSettings!.Invoke();
        _mqttRemotoSerializer.TypeNameHandling = TypeNameHandling.Objects;
        _mqttRemoto.Current?.SetHandler(
            async (topic, payload) =>
            {
                await ProcessarMensagemRemotoAsync(topic, payload);
            }
        );
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

    private async Task ProcessarMensagemInternoAsync(
        string topic,
        string payload,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var mensagem = JsonConvert.DeserializeObject(payload, _mqttInternoSerializer)!;

            Console.WriteLine(
                $"Mensagem recebida [INTERNO]: {topic} => {mensagem.GetType().Name} => {payload}"
            );
            if (mensagem is Toolbox.Core.Messages.Command command)
            {
                await mediator.Execute((dynamic)command, cancellationToken: cancellationToken);
            }
            else if (mensagem is Event @event)
            {
                Console.WriteLine($"Event [INTERNO]: {@event.GetType().Name}");
                await mediator.Publish(@event, cancellationToken: default);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem MQTT [LOCAL]: {Message}", ex.Message);
        }
    }

    private async Task ProcessarMensagemLocalAsync(
        string topic,
        string payload,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var mensagem = JsonConvert.DeserializeObject(payload, _mqttInternoSerializer)!;

            Console.WriteLine(
                $"Mensagem recebida [LOCAL]: {topic} => {mensagem.GetType().Name} => {payload}"
            );
            if (mensagem is Toolbox.Core.Messages.Command command)
            {
                await mediator.Execute((dynamic)command, cancellationToken: cancellationToken);
            }
            else if (mensagem is Event @event)
            {
                Console.WriteLine($"Event [LOCAL]: {@event.GetType().Name}");
                await mediator.Publish(@event, cancellationToken: default);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem MQTT [LOCAL]: {Message}", ex.Message);
        }
    }

    private async Task ProcessarMensagemRemotoAsync(
        string topic,
        string payload,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var mensagem = JsonConvert.DeserializeObject(payload, _mqttRemotoSerializer)!;

            Console.WriteLine(
                $"Mensagem recebida [REMOTO]: {topic} => {mensagem.GetType().Name} => {payload}"
            );
            if (mensagem is Toolbox.Core.Messages.Command command)
            {
                await mediator.Execute((dynamic)command, cancellationToken: cancellationToken);
                return;
            }

            //var dispatcher = scope.ServiceProvider.GetRequiredService<CommandDispatcher>();
            //await dispatcher.DispatchAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem MQTT [REMOTO]: {Message}", ex.Message);
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
