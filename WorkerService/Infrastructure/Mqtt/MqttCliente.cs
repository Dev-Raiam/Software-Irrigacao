using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Protocol;
using WorkerService.Features.Comandos;
using WorkerService.Infrastructure;
using WorkerService.Infrastructure.Interfaces;

namespace WorkerService.Infrastructure.Mqtt
{
    public class MqttCliente(
        IMqttClient _mqttCliente,
        IProcessadorMensageria _servicoMensageria,
        ILogger<MqttCliente> _logger
    ) : IMqttCliente
    {
        private bool Conectado { get; set; }

        public async Task Conectar(
            string servidor,
            int porta,
            string clienteId,
            string? usuario = null,
            string? senha = null,
            CancellationToken cancellationToken = default
        )
        {
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(servidor, porta)
                .WithCredentials(usuario, senha)
                .WithClientId(clienteId)
                .WithCleanSession()
                .Build();

            var resposta = await _mqttCliente.ConnectAsync(options, cancellationToken);
            Conectado = resposta.ResultCode == MqttClientConnectResultCode.Success;
            if (!Conectado)
                _logger.LogError("Falha ao conectar ao MQTT: {Codigo}", resposta.ResultCode);
        }

        public async Task Assinar(string topico, CancellationToken cancellationToken = default)
        {
            if (!Conectado)
                return;

            var resposta = await _mqttCliente.SubscribeAsync(
                topico,
                cancellationToken: cancellationToken
            );

            foreach (var item in resposta.Items)
            {
                if (item.ResultCode <= MqttClientSubscribeResultCode.GrantedQoS2)
                    _logger.LogInformation(
                        "Subscrito: {Topico} | QoS: {Resultado}",
                        item.TopicFilter.Topic,
                        item.ResultCode
                    );
                else
                    _logger.LogError(
                        "Falha ao subscrever: {Topico} | {Resultado}",
                        item.TopicFilter.Topic,
                        item.ResultCode
                    );
            }
        }

        public async Task Publicar(
            string topico,
            object mensagem,
            CancellationToken cancellationToken = default
        )
        {
            var jsonOptions = new JsonSerializerOptions { IndentSize = 2, WriteIndented = true };
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topico)
                .WithPayload(JsonSerializer.Serialize(mensagem, jsonOptions))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                // .WithRetainFlag()
                .Build();
            await _mqttCliente.PublishAsync(message, cancellationToken);
        }

        public void IniciarMensageria(CancellationToken cancellationToken)
        {
            _mqttCliente.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    await _servicoMensageria.Processar(payload, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Erro ao processar mensagem MQTT: {Message}", ex.Message);
                }
            };
        }
    }
}
