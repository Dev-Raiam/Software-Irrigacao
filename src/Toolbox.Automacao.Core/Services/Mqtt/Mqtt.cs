using MQTTnet;
using MQTTnet.Protocol;
using System.Text;
using static Toolbox.Automacao.Core.Services.Mqtt.IMqtt;

namespace Toolbox.Automacao.Core.Services.Mqtt;

/// <summary>
/// Facade para operações MQTT usando MQTTnet
/// Simplifica a interação com brokers MQTT
/// </summary>
public sealed class Mqtt : IMqtt
{
    public const string Local = "local";
    public const string Remoto = "remoto";
    private readonly MqttClientOptions _options;
    private readonly string _host;
    private readonly int _port;
    private readonly IMqttClient _mqttClient;
    private Action<string, string>? _handler;
    private bool _disposed;

    public bool IsConnected => _mqttClient.IsConnected;
    public Action<string, string>? Handler => _handler;

    public Mqtt(Configuration config)
    {
        _host = config.Host;
        _port = config.Port;
        var options = new MqttClientOptionsBuilder()
            .WithClientId(config.ClientId)
            .WithTcpServer(config.Host, config.Port)
            .WithCleanSession(config.CleanSession)
            .WithTimeout(TimeSpan.FromSeconds(config.ConnectionTimeoutSeconds));

        if (!string.IsNullOrEmpty(config.Username))
        {
            options.WithCredentials(config.Username, config.Password);
        }
        
        _options = options.Build();

        _mqttClient = new MqttClientFactory().CreateMqttClient();

        //_messageHandlers = new Dictionary<string, Action<string, string>>();

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            if (_handler != null)
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                // Chama handler global primeiro (se definido)
                _handler?.Invoke(topic, payload);

                //// Chama handler específico do tópico (se existir)
                //if (_messageHandlers.TryGetValue(topic, out var handler))
                //{
                //    handler?.Invoke(topic, payload);
                //}
            }

            await Task.CompletedTask;
        };
    }

    /// <summary>
    /// Connects to the MQTT broker
    /// </summary>
    public async Task ConnectAsync()
    {
        if (_mqttClient.IsConnected)
            return;

        MqttClientConnectResult result;
        try
        {
            result = await _mqttClient.ConnectAsync(_options);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao conectar ao broker MQTT {_host}:{_port}: {ex.Message}",
                ex
            );
        }

        if (result.ResultCode != MqttClientConnectResultCode.Success)
        {
            throw new Exception(
                $"Falha ao conectar ao broker MQTT {_host}:{_port}: {result.ResultCode} - {result.ReasonString}"
            );
        }
    }

    /// <summary>
    /// Disconnects from the MQTT broker
    /// </summary>
    public async Task DisconnectAsync()
    {
        if (!_mqttClient.IsConnected)
            return;

        try
        {
            await _mqttClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao desconectar do broker MQTT {_host}:{_port}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Publishes a message to an MQTT topic
    /// </summary>
    public async Task PublishAsync(string topic, string payload, bool retain = false, int qos = 0)
    {
        await ConnectAsync();
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
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao publicar no tópico {topic} em {_host}:{_port}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Publishes a message to an MQTT topic
    /// </summary>
    public async Task PublishAsync(string topic, byte[] payload, bool retain = false, int qos = 0)
    {
        await ConnectAsync();
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
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao publicar no tópico {topic} em {_host}:{_port}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Subscribes to an MQTT topic to receive messages
    /// </summary>
    public async Task SubscribeAsync(
        string topic,
        int qos = 0
        //Action<string, string>? messageHandler = null
    )
    {
        await ConnectAsync();

        try
        {
            var options = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .Build();

            await _mqttClient.SubscribeAsync(options);

            //if (messageHandler != null)
            //{
            //    _messageHandlers[topic] = messageHandler;
            //}
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao assinar tópico {topic} em {_host}:{_port}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Unsubscribes from an MQTT topic
    /// </summary>
    public async Task UnsubscribeAsync(string topic)
    {
        await ConnectAsync();

        try
        {
            await _mqttClient.UnsubscribeAsync(topic);

            //if (_messageHandlers.ContainsKey(topic))
            //{
            //    _messageHandlers.Remove(topic);
            //}
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao desassinar tópico {topic} em {_host}:{_port}: {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Sets a global handler that will be called for all received messages
    /// </summary>
    public void SetHandler(Action<string, string>? handler)
    {
        _handler = handler;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (_mqttClient.IsConnected)
            {
                _mqttClient.DisconnectAsync().GetAwaiter().GetResult();
            }
        }
        finally
        {
            _mqttClient.Dispose();
            //_messageHandlers.Clear();
            _handler = null;
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

public sealed class MqttManager
{
    private Mqtt _current;
    public MqttManager(Configuration config)
    {
        _current = new Mqtt(config);
    }

    public IMqtt Current => _current;

    public MqttManager Reload(Configuration config)
    {
        var handler = _current.Handler;
        _current.Dispose();
        _current = new Mqtt(config);
        _current.SetHandler(handler);
        return this;
    }
}
