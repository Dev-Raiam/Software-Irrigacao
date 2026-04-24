using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using WorkerService.Features.Mensageria;
using WorkerService.Features.Shared.Abstractions;

namespace WorkerService.Infrastructure.Mqtt
{
    public sealed class MqttCliente(
        string nomeInstancia,
        IMqttClient _mqttCliente,
        ProcessadorMensageria _servicoMensageria,
        ILogger<MqttCliente> _logger
    ) : IMqttCliente
    {
        private bool _reconectando = false;
        private bool _conectando = false;
        private bool _callbacksRegistrados = false;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            IndentSize = 2,
            WriteIndented = true,
        };

        bool IMqttCliente.Conectado => _mqttCliente.IsConnected;

        public async Task<bool> Conectar(
            string servidor,
            int porta,
            string clienteId,
            string? usuario,
            string? senha,
            CancellationToken cancellationToken = default
        )
        {
            if (_mqttCliente.IsConnected || _conectando || _reconectando)
                return _mqttCliente.IsConnected;

            _conectando = true;
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(servidor, porta)
                .WithCredentials(usuario, senha)
                .WithClientId(clienteId)
                .WithCleanSession()
                .Build();

            try
            {
                var resposta = await _mqttCliente.ConnectAsync(options, cancellationToken);

                if (resposta.ResultCode != MqttClientConnectResultCode.Success)
                {
                    _logger.LogError("Falha ao conectar ao MQTT: {Codigo}", resposta.ResultCode);
                }
                else
                {
                    _logger.LogInformation($"Conectado ao broker MQTT {nomeInstancia}");
                }
            }
            catch (MqttConnectingFailedException ex)
            {
                _logger.LogError("Broker foi alcançado, mas rejeitou a conexão {ex}", ex);
            }
            catch (MqttCommunicationException ex)
            {
                _logger.LogError("Erro de comunicação com o Broker {ex}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro inesperado {ex}", ex);
            }
            finally
            {
                _conectando = false;
            }
            return _mqttCliente.IsConnected;
        }

        public async Task AssinarTopico(
            string topico,
            CancellationToken cancellationToken = default
        )
        {
            if (!_mqttCliente.IsConnected)
                return;

            await _mqttCliente.SubscribeAsync(topico, cancellationToken: cancellationToken);
        }

        public async Task Publicar(
            string topico,
            object mensagem,
            CancellationToken cancellationToken
        )
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topico)
                .WithPayload(JsonSerializer.Serialize(mensagem, _jsonOptions))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                // .WithRetainFlag()
                .Build();
            await _mqttCliente.PublishAsync(message, cancellationToken);
        }

        public async Task Publicar(
            string topico,
            string mensagem,
            CancellationToken cancellationToken,
            MqttApplicationMessageBuilder? messageBuilder = null!
        )
        {
            var message =
                messageBuilder?.Build()
                ?? new MqttApplicationMessageBuilder()
                    .WithTopic(topico)
                    .WithPayload(mensagem)
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    // .WithRetainFlag()
                    .Build();
            await _mqttCliente.PublishAsync(message, cancellationToken);
        }

        public void ExecutarCallbackMensageria(CancellationToken cancellationToken)
        {
            if (_callbacksRegistrados)
                return;

            _callbacksRegistrados = true;
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

        public void ExecutarCallbackDesconectado(CancellationToken cancellationToken)
        {
            _mqttCliente.DisconnectedAsync += async e =>
            {
                // if (!_reconectando)
                // {
                //     _logger.LogWarning($"Cliente MQTT desconectado {nomeInstancia}");
                //     if (!cancellationToken.IsCancellationRequested)
                //         await Reconectar(cancellationToken);
                // }
                if (e.Reason == MqttClientDisconnectReason.NormalDisconnection)
                    return;

                Console.WriteLine("Tentando reconectar em 5 segundos...");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await _mqttCliente.ReconnectAsync(); // ← usa as opções da última conexão
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Falha ao reconectar: {ex.Message}");
                }
            };
        }

        public async Task Reconectar(CancellationToken cancellationToken)
        {
            if (_mqttCliente.IsConnected)
                return;

            _reconectando = true;
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    return;

                await _mqttCliente.ReconnectAsync(cancellationToken);
                if (_mqttCliente.IsConnected)
                    _logger.LogInformation($"Cliente MQTT reconectado {nomeInstancia}");
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Erro ao reconectar MQTT {nomeInstancia}: {Message}",
                    nomeInstancia,
                    ex.Message
                );
            }
            finally
            {
                _reconectando = false;
            }
        }

        public async Task Desconectar(CancellationToken cancellationToken)
        {
            await _mqttCliente.DisconnectAsync(cancellationToken: cancellationToken);
        }
    }
}
