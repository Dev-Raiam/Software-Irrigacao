using System.Text;
using MQTTnet;
using WorkerService.Features.Mensageria;
using WorkerService.Features.Mensageria.Remota;

namespace WorkerService.Infrastructure.Mqtt
{
    public class MqttClienteRemoto : MqttCliente
    {
        private readonly MqttClienteLocal _mqttClienteLocal;
        private readonly ProcessarMensagemRemota _processarMensagemRemota;

        public MqttClienteRemoto(
            string nomeInstancia,
            IMqttClient mqttCliente,
            MqttClienteLocal mqttClienteLocal,
            ProcessarMensagemRemota processarMensagemRemota,
            ILogger<MqttCliente> logger
        )
            : base(nomeInstancia, mqttCliente, logger)
        {
            _mqttClienteLocal = mqttClienteLocal;
            _processarMensagemRemota = processarMensagemRemota;
        }

        public override void ExecutarCallbackMensageria(CancellationToken cancellationToken)
        {
            _mqttCliente.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    /// Esse cara é quem vai escutar os comandos enviados Remotos da api
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    _processarMensagemRemota.Processar(payload, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao processar mensagem MQTT: {Message}", ex.Message);
                }
            };
        }
    }
}
