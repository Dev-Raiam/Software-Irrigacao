using System.Text;
using IrrigacaoInteligente.Features.Shared.Abstractions;
using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toolbox.Automacao.Irrigacao.Comandos;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Infrastructure.Mqtt
{
    public class MqttClienteRemoto : MqttCliente
    {
        private readonly IServiceProvider _serviceProvider;

        public MqttClienteRemoto(
            IMqttClient mqttCliente,
            IServiceProvider serviceProvider,
            ILogger<MqttCliente> logger
        )
            : base("Remoto", mqttCliente, logger)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly JsonSerializerSettings _settings = new()
        {
            Formatting = Formatting.None,
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
