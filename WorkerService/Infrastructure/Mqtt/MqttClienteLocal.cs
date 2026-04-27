using System.Text;
using MQTTnet;
using Org.BouncyCastle.Asn1.Ess;
using WorkerService.Features.Mensageria;
using WorkerService.Features.Mensageria.Remota;

namespace WorkerService.Infrastructure.Mqtt
{
    public class MqttClienteLocal : MqttCliente
    {
        private readonly ProcessarMensagemLocal _processarMensagemLocal;

        public MqttClienteLocal(
            string nomeInstancia,
            IMqttClient _mqttCliente,
            ProcessarMensagemLocal processarMensagemLocal,
            ILogger<MqttCliente> _logger
        )
            : base(nomeInstancia, _mqttCliente, _logger)
        {
            _processarMensagemLocal = processarMensagemLocal;
        }

        public override void ExecutarCallbackMensageria(CancellationToken cancellationToken)
        {
            _mqttCliente.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    /// Esse cara é quem vai escutar as respostas dos comandos enviados Locais para Python
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    _processarMensagemLocal.Processar(payload, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao processar mensagem MQTT: {Message}", ex.Message);
                }
            };
        }
    }
}
