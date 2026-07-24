using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages;
using Controlador = Toolbox.Industrial.Core.Data.Controlador;

namespace SoftwareIrrigacao.Workes;

public class MqttWorker : BackgroundService
{
    private readonly MqttManager _mqttLocal;
    private readonly MqttManager _mqttRemoto;
    private readonly ILogger<MqttWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityStore _store;
    private readonly JsonSerializerSettings _jsonSettingsLocal;
    private readonly JsonSerializerSettings _jsonSettingsRemoto;

    public MqttWorker(
        [FromKeyedServices(Mqtt.Local)] MqttManager mqttLocal,
        [FromKeyedServices(Mqtt.Remoto)] MqttManager mqttRemoto,
        ILogger<MqttWorker> logger,
        IServiceProvider serviceProvider,
        IEntityStore store
    )
    {
        _mqttLocal = mqttLocal;
        _mqttRemoto = mqttRemoto;
        _logger = logger;
        _serviceProvider = serviceProvider;
        _store = store;

        _jsonSettingsLocal = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects,
        };

        _jsonSettingsRemoto = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects,
        };

        _mqttLocal.Current.SetHandler(
            async (topic, payload) =>
            {
                await ProcessarMensagemLocalAsync(topic, payload);
            }
        );

        _mqttRemoto.Current.SetHandler(
            async (topic, payload) =>
            {
                await ProcessarMensagemRemotoAsync(topic, payload);
            }
        );
    }

    private bool ConexaoLocalAtiva = false;
    private bool ConexaoRemotaAtiva = false;

    private async Task<Toolbox.Industrial.Core.Communication.Api.Contracts.Controlador?> ObterControladorMaster(
        CancellationToken cancellationToken = default
    )
    {
        var configuracao = await _store.FirstOrDefaultAsync<Controlador>(c => c.Value.Master);

        var controlador = configuracao == null ? null : configuracao.Value;

        return controlador;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_mqttLocal.Current.IsConnected)
                    ConexaoLocalAtiva = false;

                if (!_mqttRemoto.Current.IsConnected)
                    ConexaoRemotaAtiva = false;

                if (!ConexaoLocalAtiva)
                {
                    await _mqttLocal.Current.ConnectAsync();
                    await _mqttLocal.Current.SubscribeAsync("topic", qos: 0);
                }

                if (!ConexaoRemotaAtiva)
                {
                    await _mqttRemoto.Current.ConnectAsync();

                    var controlador = ObterControladorMaster();

                    if (controlador != null)
                    {
                        await _mqttRemoto.Current.SubscribeAsync(
                            $"comando/{controlador.Id}",
                            qos: 0
                        );
                    }

                    await _mqttRemoto.Current.SubscribeAsync(
                        $"comando/4fcb13a6-7e9d-4dd1-ab6f-2a87c9f36b76",
                        qos: 0
                    );
                }

                if (_mqttRemoto.Current.IsConnected && !ConexaoRemotaAtiva)
                {
                    ConexaoRemotaAtiva = true;
                    _logger.LogInformation("Conectado ao broker MQTT REMOTO");
                }

                if (_mqttLocal.Current.IsConnected && !ConexaoLocalAtiva)
                {
                    ConexaoLocalAtiva = true;
                    _logger.LogInformation("Conectado ao broker MQTT LOCAL");
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

            var mensagem = JsonConvert.DeserializeObject(payload, _jsonSettingsLocal)!;

            if (mensagem is Command command)
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
            var dispatcher = scope.ServiceProvider.GetRequiredService<CommandDispatcher>();
            await dispatcher.DispatchAsync(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem MQTT [REMOTO]: {Message}", ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Encerrando Serviço MQTT...");

        await _mqttLocal.Current.DisconnectAsync();
        _mqttLocal.Current.Dispose();

        await _mqttRemoto.Current.DisconnectAsync();
        _mqttRemoto.Current.Dispose();

        await base.StopAsync(cancellationToken);
    }
}
