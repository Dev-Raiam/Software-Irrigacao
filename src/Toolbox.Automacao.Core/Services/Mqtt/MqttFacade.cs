using System.Text;
using MQTTnet;
using MQTTnet.Protocol;
using Toolbox.Automacao.Core.Services.Mqtt.Exceptions;

namespace Toolbox.Automacao.Core.Services.Mqtt;

/// <summary>
/// Facade para operações MQTT usando MQTTnet
/// Simplifica a interação com brokers MQTT
/// </summary>
internal sealed class MqttFacade : IMqttFacade
{
    private readonly MqttConfig _config;
    private readonly IMqttClient _mqttClient;
    private readonly Dictionary<string, Action<string, string>> _messageHandlers;
    private Action<string, string>? _globalHandler;
    private bool _disposed;

    public bool IsConnected => _mqttClient.IsConnected;

    public MqttFacade(MqttConfig config)
    {
        _config = config;
        _mqttClient = new MqttClientFactory().CreateMqttClient();
        _messageHandlers = new Dictionary<string, Action<string, string>>();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            // Chama handler global primeiro (se definido)
            _globalHandler?.Invoke(topic, payload);

            // Chama handler específico do tópico (se existir)
            if (_messageHandlers.TryGetValue(topic, out var handler))
            {
                handler?.Invoke(topic, payload);
            }

            await Task.CompletedTask;
        };
    }

    /// <summary>
    /// Conecta ao broker MQTT
    /// </summary>
    public async Task ConectarAsync()
    {
        if (_mqttClient.IsConnected)
            return;

        try
        {
            var options = new MqttClientOptionsBuilder()
                .WithClientId(_config.ClientId)
                .WithTcpServer(_config.Host, _config.Port)
                .WithCleanSession(_config.CleanSession)
                .WithTimeout(TimeSpan.FromSeconds(_config.ConnectionTimeoutSeconds));

            if (!string.IsNullOrEmpty(_config.Username))
            {
                options.WithCredentials(_config.Username, _config.Password);
            }

            var result = await _mqttClient.ConnectAsync(options.Build());

            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                throw new MqttConexaoException(
                    $"Falha ao conectar ao broker MQTT: {result.ResultCode} - {result.ReasonString}"
                );
            }
        }
        catch (MqttConexaoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MqttConexaoException(
                $"Erro ao conectar ao broker MQTT {_config.Host}:{_config.Port}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Desconecta do broker MQTT
    /// </summary>
    public async Task DesconectarAsync()
    {
        if (!_mqttClient.IsConnected)
            return;

        try
        {
            await _mqttClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            throw new MqttConexaoException($"Erro ao desconectar do broker MQTT: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Publica uma mensagem em um tópico MQTT
    /// </summary>
    public async Task PublicarAsync(string topic, string payload, bool retain = false, int qos = 0)
    {
        if (!_mqttClient.IsConnected)
            throw new MqttConexaoException(
                "Cliente MQTT não está conectado. Chame ConectarAsync() primeiro."
            );

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .Build();

            await _mqttClient.PublishAsync(message);
        }
        catch (MqttConexaoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MqttPublicacaoException(
                $"Erro ao publicar no tópico {topic}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Publica uma mensagem em um tópico MQTT
    /// </summary>
    public async Task PublicarAsync(string topic, byte[] payload, bool retain = false, int qos = 0)
    {
        if (!_mqttClient.IsConnected)
            throw new MqttConexaoException(
                "Cliente MQTT não está conectado. Chame ConectarAsync() primeiro."
            );

        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .Build();

            await _mqttClient.PublishAsync(message);
        }
        catch (MqttConexaoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MqttPublicacaoException(
                $"Erro ao publicar no tópico {topic}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Assina um tópico MQTT para receber mensagens
    /// </summary>
    public async Task AssinarAsync(
        string topic,
        int qos = 0,
        Action<string, string>? messageHandler = null
    )
    {
        if (!_mqttClient.IsConnected)
            throw new MqttConexaoException(
                "Cliente MQTT não está conectado. Chame ConectarAsync() primeiro."
            );

        try
        {
            var options = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .Build();

            await _mqttClient.SubscribeAsync(options);

            if (messageHandler != null)
            {
                _messageHandlers[topic] = messageHandler;
            }
        }
        catch (MqttConexaoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MqttAssinaturaException($"Erro ao assinar tópico {topic}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cancela assinatura de um tópico MQTT
    /// </summary>
    public async Task DesassinarAsync(string topic)
    {
        if (!_mqttClient.IsConnected)
            throw new MqttConexaoException(
                "Cliente MQTT não está conectado. Chame ConectarAsync() primeiro."
            );

        try
        {
            await _mqttClient.UnsubscribeAsync(topic);

            if (_messageHandlers.ContainsKey(topic))
            {
                _messageHandlers.Remove(topic);
            }
        }
        catch (MqttConexaoException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new MqttAssinaturaException(
                $"Erro ao desassinar tópico {topic}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Define um handler global que será chamado para todas as mensagens recebidas
    /// </summary>
    public void DefinirManipuladorGlobal(Action<string, string>? handler)
    {
        _globalHandler = handler;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        if (_mqttClient.IsConnected)
        {
            _mqttClient.DisconnectAsync().GetAwaiter().GetResult();
        }

        _mqttClient.Dispose();
        _messageHandlers.Clear();
        _globalHandler = null;
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
