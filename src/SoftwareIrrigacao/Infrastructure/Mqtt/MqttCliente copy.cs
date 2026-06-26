// using System.Text;
// using System.Text.Json;
// using SoftwareIrrigacao.Core.Mqtt;
// using SoftwareIrrigacao.Setup;
// using MQTTnet;
// using MQTTnet.Adapter;
// using MQTTnet.Exceptions;
// using MQTTnet.Packets;
// using MQTTnet.Protocol;

// namespace SoftwareIrrigacao.Core.Mqtt
// {
//     public interface IMqttClienteCopy
//     {
//         bool Conectado { get; }
//         Task ConnectAsync(CancellationToken cancellationToken);
//         Task PublishAsync(string topico, object mensagem, CancellationToken cancellationToken);
//         Task SubscribeAsync(string topico, CancellationToken cancellationToken);
//     }

//     public abstract class MqttClienteCopy : IMqttClienteCopy
//     {
//         private readonly IMqttClient _mqttClient;
//         private readonly MqttConfiguracao _mqttConfiguracao;
//         private readonly IServiceProvider _serviceProvider;
//         private readonly ILogger<MqttClienteCopy> _logger;

//         protected MqttClienteCopy(
//             ILogger<MqttClienteCopy> logger,
//             IMqttClient mqttClient,
//             MqttConfiguracao mqttConfiguracao,
//             IServiceProvider serviceProvider
//         )
//         {
//             _logger = logger;
//             _mqttClient = mqttClient;
//             _mqttConfiguracao = mqttConfiguracao;
//             _serviceProvider = serviceProvider;
//         }

//         public bool Conectado => _mqttClient.IsConnected;
//         private bool _handlerRegistrado;

//         public async Task ConnectAsync(CancellationToken cancellationToken)
//         {
//             var options = new MqttClientOptionsBuilder()
//                 .WithTcpServer(_mqttConfiguracao.Servidor, _mqttConfiguracao.Porta)
//                 .WithClientId("123")
//                 .WithCredentials(_mqttConfiguracao.Usuario, _mqttConfiguracao.Senha)
//                 .WithCleanSession()
//                 .Build();

//             try
//             {
//                 var connectResult = await _mqttClient.ConnectAsync(options, cancellationToken);

//                 if (connectResult.ResultCode == MqttClientConnectResultCode.Success)
//                 {
//                     _logger.LogInformation(
//                         "Conectado com ao {Servidor} sucesso",
//                         _mqttConfiguracao.Servidor
//                     );

//                     RegistrarHandler(cancellationToken);
//                 }
//                 else
//                 {
//                     _logger.LogError(
//                         "Erro ao conectar com ao {servidor} - {codigo}",
//                         _mqttConfiguracao.Servidor,
//                         connectResult.ResultCode
//                     );
//                 }
//             }
//             catch (MqttConnectingFailedException ex)
//             {
//                 _logger.LogError("Broker foi alcançado, mas rejeitou a conexão {ex}", ex);
//             }
//             catch (MqttCommunicationException ex)
//             {
//                 _logger.LogError(
//                     ex,
//                     "Erro de comunicação ao conectar no MQTT {Servidor}",
//                     _mqttConfiguracao.Servidor
//                 );
//             }
//             catch (OperationCanceledException)
//             {
//                 _logger.LogWarning("Conexão MQTT cancelada");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(
//                     ex,
//                     "Erro inesperado ao conectar no MQTT {Servidor}",
//                     _mqttConfiguracao.Servidor
//                 );
//             }
//         }

//         public async Task PublishAsync(
//             string topico,
//             object mensagem,
//             CancellationToken cancellationToken
//         )
//         {
//             var message = new MqttApplicationMessageBuilder()
//                 .WithTopic(topico)
//                 .WithPayload(JsonSerializer.Serialize(mensagem))
//                 .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
//                 .WithRetainFlag()
//                 .Build();

//             await _mqttClient.PublishAsync(message, cancellationToken);
//         }

//         public async Task SubscribeAsync(string topico, CancellationToken cancellationToken)
//         {
//             var options = new MqttClientSubscribeOptions();

//             options.TopicFilters.Add(new MqttTopicFilter { Topic = topico });

//             await _mqttClient.SubscribeAsync(options, cancellationToken);
//         }

//         private void RegistrarHandler(CancellationToken cancellationToken)
//         {
//             if (_handlerRegistrado)
//                 return;
//             _handlerRegistrado = true;

//             _mqttClient.ApplicationMessageReceivedAsync += async e =>
//             {
//                 try
//                 {
//                     using var scope = _serviceProvider.CreateScope();
//                     var topico = e.ApplicationMessage.Topic;
//                     var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
//                     await ProcessarMensagemAsync(
//                         topico,
//                         payload,
//                         scope.ServiceProvider,
//                         cancellationToken
//                     );
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogError("Erro ao processar mensagem MQTT: {Message}", ex.Message);
//                 }
//             };
//         }

//         protected abstract Task ProcessarMensagemAsync(
//             string topico,
//             string payload,
//             IServiceProvider scope,
//             CancellationToken cancellationToken
//         );
//     }
// }

// public sealed class MqttClienteLocalCopy : MqttClienteCopy
// {
//     private readonly Newtonsoft.Json.JsonSerializerSettings _settings = new()
//     {
//         Formatting = Newtonsoft.Json.Formatting.Indented,
//         DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
//         DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.RoundtripKind,
//         ContractResolver =
//             new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
//         NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
//         TypeNameHandling = Newtonsoft.Json.TypeNameHandling.Objects, // rede interna confiável
//     };

//     public MqttClienteLocalCopy(
//         ILogger<MqttClienteCopy> logger,
//         IMqttClient mqttClient,
//         IServiceProvider serviceProvider,
//         MqttConfiguracao mqttConfiguracao
//     )
//         : base(logger, mqttClient, mqttConfiguracao, serviceProvider) { }

//     protected override async Task ProcessarMensagemAsync(
//         string topico,
//         string payload,
//         IServiceProvider scope,
//         CancellationToken cancellationToken
//     )
//     {
//         //var mediator = scope.GetRequiredService<IMediator>();

//         if (topico == "telemetria/resposta")
//         {
//             // tratamento específico do local (ex.: salvar telemetria)
//             return;
//         }

//         ///var mensagem = JsonConvert.DeserializeObject(payload, _settings)!;
//         // if (mensagem is Command command)
//         //     await mediator.Execute((dynamic)command, cancellationToken: cancellationToken);
//         // else if (mensagem is Event @event)
//         //     await mediator.Publish(@event, cancellationToken);
//     }
// }

// public sealed class MqttClienteRemoto : MqttClienteCopy
// {
//     private readonly Newtonsoft.Json.JsonSerializerSettings _settings = new()
//     {
//         Formatting = Newtonsoft.Json.Formatting.None,
//         ContractResolver =
//             new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
//         NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
//         // SEM TypeNameHandling
//     };

//     public MqttClienteRemoto(
//         ILogger<MqttClienteCopy> logger,
//         IMqttClient mqttClient,
//         IServiceProvider serviceProvider,
//         MqttConfiguracao mqttConfiguracao
//     )
//         : base(logger, mqttClient, mqttConfiguracao, serviceProvider) { }

//     protected override async Task ProcessarMensagemAsync(
//         string topico,
//         string payload,
//         IServiceProvider scope,
//         CancellationToken ct
//     )
//     {
//         // var mediator = scope.GetRequiredService<IMediator>();

//         // ex.: topico = "comando/abrirValvula"
//         // var chave = topico.Split('/').Last();
//         // if (!_comandosPermitidos.TryGetValue(chave, out var tipo))
//         //     return; // comando externo não autorizado ? descarta

//         // var command = JsonConvert.DeserializeObject(payload, tipo, _settings);
//         // if (command is Command cmd)
//         //     await mediator.Execute((dynamic)cmd, cancellationToken: ct);
//     }
// }
