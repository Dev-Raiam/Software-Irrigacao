using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO.Ports;
using Toolbox.Automacao.Core.Services.Mqtt;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Modulo.Tekon.Interfaces;
using Toolbox.Modulo.Tekon.Models;

namespace SoftwareIrrigacao.Workes;

public class MqttWorker : BackgroundService
{
    private readonly IMqtt _mqttLocal;
    private readonly IMqtt _mqttRemoto;
    private readonly ILogger<MqttWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITekonDriver _driver;
    private readonly JsonSerializerSettings _jsonSettingsLocal;
    private readonly JsonSerializerSettings _jsonSettingsRemoto;

    public MqttWorker(
        [FromKeyedServices("local")] IMqtt mqttLocal,
        [FromKeyedServices("remoto")] IMqtt mqttRemoto,
        ILogger<MqttWorker> logger,
        IServiceProvider serviceProvider,
        ITekonDriverFactory factory
    )
    {
        _mqttLocal = mqttLocal;
        _mqttRemoto = mqttRemoto;
        _logger = logger;
        _serviceProvider = serviceProvider;

        var config = new TekonDriverConfig
        {
            Porta = "COM6",
            BaudRate = 19200,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.Two,
            ReadTimeout = 1000,
            WriteTimeout = 1000,
        };
        _driver = factory.CriarDriver(config);

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

        _mqttLocal.SetGlobalHandler(
            async (topic, payload) =>
            {
                await ProcessarMensagemLocalAsync(topic, payload);
            }
        );

        _mqttRemoto.SetGlobalHandler(
            async (topic, payload) =>
            {
                await ProcessarMensagemRemotoAsync(topic, payload);
            }
        );
    }

    private bool ConexaoLocalAtiva = false;
    private bool ConexaoRemotaAtiva = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!_mqttLocal.IsConnected)
                    ConexaoLocalAtiva = false;

                if (!_mqttRemoto.IsConnected)
                    ConexaoRemotaAtiva = false;

                if (!ConexaoLocalAtiva)
                {
                    await _mqttLocal.ConnectAsync();
                    await _mqttLocal.SubscribeAsync("teste/local", qos: 0);
                }

                if (!ConexaoRemotaAtiva)
                {
                    await _mqttRemoto.ConnectAsync();
                    await _mqttRemoto.SubscribeAsync("teste/remoto", qos: 0);
                }

                if (_mqttRemoto.IsConnected && !ConexaoRemotaAtiva)
                {
                    ConexaoRemotaAtiva = true;
                    _logger.LogInformation("Conectado ao broker MQTT REMOTO");
                }

                if (_mqttLocal.IsConnected && !ConexaoLocalAtiva)
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

            if (topic == "telemetria/resposta")
            {
                //var command = new SalvarTelemetria { Dados = payload };
                //await mediator.Execute(command, cancellationToken: default);
                return;
            }

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
            if (topic == "teste/remoto")
            {
                var body = System.Text.Json.JsonSerializer.Deserialize<Body>(payload);
                if (body == null)
                    return;
                await _driver.EscreverPortaDigital(
                    body.modelo,
                    body.slaveAddress,
                    body.index,
                    body.porta,
                    body.valor
                );
            }
            //using var scope = _serviceProvider.CreateScope();
            //var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            //var mensagem = JsonConvert.DeserializeObject(payload, _jsonSettingsRemoto)!;

            //if (mensagem is Command command)
            //{
            //    await mediator.Execute((dynamic)command, cancellationToken: cancellationToken);
            //}
            //else if (mensagem is Event @event)
            //{
            //    Console.WriteLine($"Event [REMOTO]: {@event.GetType().Name}");
            //    await mediator.Publish(@event, cancellationToken: default);
            //}
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar mensagem MQTT [REMOTO]: {Message}", ex.Message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Encerrando Serviço MQTT...");

        await _mqttLocal.DisconnectAsync();
        _mqttLocal.Dispose();

        await _mqttRemoto.DisconnectAsync();
        _mqttRemoto.Dispose();

        await base.StopAsync(cancellationToken);
    }

    public record Body(string modelo, byte slaveAddress, byte index, string porta, bool valor);
}
