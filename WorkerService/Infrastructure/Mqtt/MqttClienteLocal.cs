using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.Ess;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Features.Sincronizacao.Automacao;

namespace WorkerService.Infrastructure.Mqtt
{
    public class MqttClienteLocal(
        string nomeInstancia,
        IMqttClient _mqttCliente,
        IServiceProvider _serviceProvider,
        ILogger<MqttCliente> _logger
    ) : MqttCliente(nomeInstancia, _mqttCliente, _logger)
    {
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            TypeNameHandling = TypeNameHandling.Objects,
        };

        public override void ExecutarCallbackMensageria(CancellationToken cancellationToken)
        {
            _mqttCliente.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                    var mensagem = JsonConvert.DeserializeObject(payload, _settings)!;

                    if (mensagem is Command command)
                    {
                        await mediator.Execute(
                            (dynamic)command,
                            cancellationToken: cancellationToken
                        );
                    }
                    if (mensagem is Event @event)
                    {
                        Console.WriteLine($"Event: {@event.GetType().Name}");
                        await mediator.Publish(@event, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao processar mensagem MQTT: {Message}", ex.Message);
                }
            };
        }
    }
}
