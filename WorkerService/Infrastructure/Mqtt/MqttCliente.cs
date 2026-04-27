using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using MQTTnet;
using MQTTnet.Adapter;
using MQTTnet.Exceptions;
using MQTTnet.Protocol;
using WorkerService.Features.Mensageria;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Features.Shared.Response;

namespace WorkerService.Infrastructure.Mqtt
{
    public abstract class MqttCliente() : IMqttCliente
    {
        protected readonly IMqttClient _mqttCliente = null!;
        protected readonly ILogger<MqttCliente> _logger = null!;

        protected readonly string _nomeInstancia = null!;
        protected bool _reconectando = false;
        protected bool _conectando = false;

        protected List<Dictionary<Guid, DateTime>> _comandosPendentes = [];
        protected readonly Guid _comandoId = Guid.NewGuid();
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            IndentSize = 2,
            WriteIndented = true,
        };

        public bool Conectado => _mqttCliente.IsConnected;

        protected MqttCliente(
            string nomeInstancia,
            IMqttClient mqttCliente,
            ILogger<MqttCliente> logger
        )
            : this()
        {
            _mqttCliente = mqttCliente;
            _logger = logger;
            _nomeInstancia = nomeInstancia;
        }

        public async Task<bool> Conectar(
            string servidor,
            int porta,
            string clienteId,
            string? usuario,
            string? senha,
            CancellationToken cancellationToken = default
        )
        {
            if (_mqttCliente.IsConnected || _conectando)
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
                    _logger.LogInformation($"Conectado ao broker MQTT {_nomeInstancia}");
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
                .WithResponseTopic("topico-resposta")
                .WithCorrelationData(Guid.NewGuid().ToByteArray())
                .WithPayload(JsonSerializer.Serialize(mensagem, _jsonOptions))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                // .WithRetainFlag()
                .Build();

            _comandosPendentes.Add(
                new Dictionary<Guid, DateTime> { { _comandoId, DateTime.UtcNow } }
            );
            Console.WriteLine($"Lista de Comandos: {_comandosPendentes.Count}");
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

        public abstract void ExecutarCallbackMensageria(CancellationToken cancellationToken);

        public void ExecutarCallbackDesconectado(CancellationToken cancellationToken)
        {
            _mqttCliente.DisconnectedAsync += async e =>
            {
                if (_reconectando)
                    return;
                _reconectando = true;

                while (!_mqttCliente.IsConnected && !cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation(
                        "Tentando reconectar {instanciasMqtt} em 5 segundos...",
                        _nomeInstancia
                    );
                    await Task.Delay(TimeSpan.FromSeconds(5));

                    try
                    {
                        await _mqttCliente.ReconnectAsync();
                        _logger.LogInformation(
                            "Reconectado com sucesso! {instanciasMqtt}",
                            _nomeInstancia
                        );
                        break;
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(
                            "Falha ao reconectar {instanciasMqtt}: {ex.Message}",
                            _nomeInstancia,
                            ex.Message
                        );
                    }
                }
                _reconectando = false;
            };
        }

        public async Task Desconectar(CancellationToken cancellationToken)
        {
            await _mqttCliente.DisconnectAsync(cancellationToken: cancellationToken);
        }
    }
}
