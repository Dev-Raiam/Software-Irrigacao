using System.Text;
using System.Threading.Tasks;
using IrrigacaoInteligente.Features.Sincronizacao.Automacao;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Org.BouncyCastle.Asn1.Ess;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Infrastructure.Mqtt
{
    public class MqttClienteLocal : MqttCliente
    {
        private readonly IServiceProvider _serviceProvider;

        public MqttClienteLocal(
            IMqttClient mqttCliente,
            IServiceProvider serviceProvider,
            ILogger<MqttCliente> logger
        )
            : base("Local", mqttCliente, logger)
        {
            _serviceProvider = serviceProvider;
        }

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
