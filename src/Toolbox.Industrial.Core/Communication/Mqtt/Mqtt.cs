using System.Text;
using System.Timers;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using MQTTnet;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using Serilog;
using Toolbox.Industrial.Core.Communication.Api;
using static Toolbox.Industrial.Core.Data.Entity.Keys;
using Timer = System.Timers.Timer;

namespace Toolbox.Industrial.Core.Communication.Mqtt;

public sealed class Mqtt : IMqtt
{
    public const string Local = "local";
    public const string Remoto = "remoto";
    private readonly List<MqttTopicFilter> _topics;
    private readonly MqttClientOptions _options;
    private readonly IMqttClient _mqttClient;
    private Action<string, string>? _handler;
    private readonly ILogger<Mqtt> _logger;
    private readonly Timer _connectGuard;
    private bool _reconnecting = false;
    private readonly string _host;
    private readonly int _port;
    private bool _disposed;

    internal ILogger<Mqtt> Logger => _logger;
    internal IReadOnlyList<MqttTopicFilter> Topics => _topics;

    public bool IsConnected => _mqttClient.IsConnected;

    public Action<string, string>? Handler => _handler;

    public Mqtt(
        Configuration config,
        ILogger<Mqtt> logger,
        IEnumerable<MqttTopicFilter>? topics = null
    )
    {
        _host = config.Host;
        _port = config.Port;
        _logger = logger;
        _topics = new List<MqttTopicFilter>(topics ?? []);
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
        _connectGuard = new Timer();
        _connectGuard.Elapsed += new ElapsedEventHandler(Reconnect!);

        _mqttClient = new MqttClientFactory().CreateMqttClient();

        _mqttClient.ConnectedAsync += async e =>
        {
            if (_connectGuard.Enabled)
            {
                _connectGuard.Stop();
                Thread.Sleep(500);
                _connectGuard.Interval = 1000;
            }

            foreach (var topic in _topics)
            {
                var result = await _mqttClient.SubscribeAsync(topic);
                _logger.LogInformation($"Inscrito no tópico {topic.Topic}");
            }
        };

        _mqttClient.DisconnectedAsync += e =>
        {
            _logger.LogInformation($"Desconectado do broker MQTT ({_host}:{_port})");
            if (!_disposed)
            {
                if (!_connectGuard.Enabled)
                {
                    _connectGuard.Interval = 1000;
                    _connectGuard.Start();
                }
            }
            return Task.CompletedTask;
        };

        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            if (_handler != null)
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _handler?.Invoke(topic, payload);
            }

            await Task.CompletedTask;
        };
    }

    private async void Reconnect(object source, ElapsedEventArgs e)
    {
        if (_reconnecting || _disposed)
            return;

        try
        {
            _reconnecting = true;

            if (_mqttClient.IsConnected)
                return;

            try
            {
                _logger.LogInformation($"Reconectando broker MQTT ({_host}:{_port})");
                var result = await _mqttClient.ConnectAsync(_options);
                if (result != null && result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    _connectGuard.Stop();
                    _connectGuard.Interval = 1000;
                    _logger.LogInformation(
                        $"Reconectado com sucesso ao broker MQTT ({_host}:{_port})"
                    );
                    return;
                }
                if (result != null && result.ResultCode != MqttClientConnectResultCode.Success)
                {
                    _connectGuard.Interval *= 2;
                    if (_connectGuard.Interval > 15000) // Limite máximo de espera de 1 minuto
                    {
                        _connectGuard.Interval = 15000;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Falha na tentativa de reconexão MQTT ({_host}:{_port})");
                _connectGuard.Interval *= 2;
                if (_connectGuard.Interval > 15000) // Limite máximo de espera de 1 minuto
                {
                    _connectGuard.Interval = 15000;
                }
            }
        }
        finally
        {
            _reconnecting = false;
        }
    }

    public async Task ConnectAsync()
    {
        if (_mqttClient.IsConnected || _connectGuard.Enabled)
            return;

        try
        {
            _logger.LogInformation($"Conectando ao broker MQTT ({_host}:{_port})");
            var result = await _mqttClient.ConnectAsync(_options);
            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                if (!_connectGuard.Enabled)
                {
                    _connectGuard.Interval = 1000;
                    _connectGuard.Start();
                }
                _logger.LogError(
                    $"Falha ao conectar ao broker MQTT ({_host}:{_port}): {result.ResultCode} - {result.ReasonString}"
                );
            }
        }
        catch (Exception ex)
        {
            if (!_connectGuard.Enabled)
            {
                _connectGuard.Interval = 1000;
                _connectGuard.Start();
            }
            _logger.LogError(
                ex,
                $"Erro ao conectar ao broker MQTT ({_host}:{_port}): {ex.Message}"
            );
        }
    }

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
            _logger.LogError(
                ex,
                $"Erro ao desconectar do broker MQTT ({_host}:{_port}): {ex.Message}"
            );
        }
    }

    public async Task PublishAsync(
        string topic,
        string payload,
        bool retain = false,
        QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce
    )
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

    public async Task PublishAsync(
        string topic,
        byte[] payload,
        bool retain = false,
        QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce
    )
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

    public async Task SubscribeAsync(
        string topic,
        QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce
    )
    {
        await ConnectAsync();

        try
        {
            if (!_topics.Any(t => t.Topic == topic))
            {
                var options = new MqttTopicFilterBuilder()
                    .WithTopic(topic)
                    .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                    .Build();

                await _mqttClient.SubscribeAsync(options);
                _topics.Add(options);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao assinar tópico {topic} em {_host}:{_port}: {ex.Message}",
                ex
            );
        }
    }

    public async Task UnsubscribeAsync(string topic)
    {
        await ConnectAsync();

        try
        {
            await _mqttClient.UnsubscribeAsync(topic);
            _topics.RemoveAll(t => t.Topic == topic);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao cancelar assinatura do tópico {topic} em {_host}:{_port}: {ex.Message}",
                ex
            );
        }
    }

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
            _disposed = true;
            if (_mqttClient.IsConnected)
            {
                _mqttClient.DisconnectAsync().GetAwaiter().GetResult();
            }
        }
        finally
        {
            _mqttClient.Dispose();
            _handler = null;
            GC.SuppressFinalize(this);
        }
    }
}

public sealed class MqttManager
{
    //private readonly ILogger<Mqtt> _logger;
    private Mqtt? _current;

    public MqttManager(Mqtt? mqtt) //Configuration config, ILogger<Mqtt> logger
    {
        //_logger = logger;
        _current = mqtt; //new Mqtt(config, _logger);
    }

    public IMqtt? Current => _current;

    public async Task Reload(Configuration config)
    {
        if (_current == null)
            return;

        var logger = _current.Logger;
        var topics = _current.Topics;
        var handler = _current.Handler;
        var conected = _current.IsConnected;
        _current.Dispose();
        _current = new Mqtt(config, logger, topics);
        _current.SetHandler(handler);
        if (conected)
        {
            await _current.ConnectAsync();
        }
    }
}
