using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Timers;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Security;
using Timer = System.Timers.Timer;

namespace Toolbox.Industrial.Core.Communication.Mqtt;

public sealed class Mqtt : IMqtt
{
    public const string Local = "local";
    public const string Remoto = "remoto";
    private X509Certificate2? _certificate;
    private readonly List<MqttTopicFilter> _topics;
    private readonly MqttClientOptions _options;
    private readonly IServiceProvider _provider;
    private readonly IMqttClient _mqttClient;
    private Action<string, string>? _handler;
    private readonly ILogger<Mqtt> _logger;
    private readonly Timer _connectGuard;
    private bool _reconnecting = false;
    private readonly string _purpose;
    private readonly string _host;
    private readonly int _port;

    private bool _disposed;

    internal X509Certificate2? Certificate
    {
        get { return _certificate; }
        set { _certificate = value; }
    }
    internal ILogger<Mqtt> Logger => _logger;
    internal string Host => _host;
    internal int Port => _port;

    internal IReadOnlyList<MqttTopicFilter> Topics => _topics;

    public bool IsConnected => _mqttClient.IsConnected;

    public Action<string, string>? Handler => _handler;
    public string Purpose => _purpose;
    public IServiceProvider Provider => _provider;

    public Mqtt(
        IServiceProvider provider,
        string purpose,
        Configuration config,
        X509Certificate2? certificate = null,
        IEnumerable<MqttTopicFilter>? topics = null
    )
    {
        _provider = provider;
        _purpose = purpose;
        _host = config.Host;
        _port = config.Port;
        _logger = provider.GetRequiredService<ILogger<Mqtt>>();

        _topics = new List<MqttTopicFilter>(topics ?? []);
        _certificate = certificate;

        var options = new MqttClientOptionsBuilder()
            .WithClientId(config.ClientId)
            .WithTcpServer(config.Host, config.Port)
            .WithCleanSession(config.CleanSession)
            .WithTimeout(TimeSpan.FromSeconds(config.ConnectionTimeoutSeconds));

        if (certificate != null)
        {
            options = options.WithTlsOptions(tls =>
            {
                var ca = X509CertificateLoader.LoadCertificateFromFile("ca.crt");

                tls.UseTls();
                tls.WithCertificateValidationHandler(context =>
                {
                    if (context.Certificate == null)
                        return false;

                    using var certificate = new X509Certificate2(context.Certificate);

                    using var chain = new X509Chain();

                    chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                    chain.ChainPolicy.CustomTrustStore.Clear();
                    chain.ChainPolicy.CustomTrustStore.Add(ca);
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                    chain.ChainPolicy.DisableCertificateDownloads = true;
                    //chain.ChainPolicy.VerificationFlags =
                    //    X509VerificationFlags.IgnoreEndRevocationUnknown
                    //    | X509VerificationFlags.IgnoreCertificateAuthorityRevocationUnknown
                    //    | X509VerificationFlags.IgnoreRootRevocationUnknown;

                    var valid = chain.Build(certificate);
                    if (!valid)
                    {
                        foreach (var status in chain.ChainStatus)
                        {
                            Console.WriteLine(
                                $"MQTT TLS: {status.Status} - {status.StatusInformation}");
                        }
                    }
                    return valid;
                });

                tls.WithClientCertificates([certificate]);

                tls.WithSslProtocols(SslProtocols.Tls12 | SslProtocols.Tls13);
            });
        }
        else if (!string.IsNullOrEmpty(config.Username))
        {
            options = options.WithCredentials(config.Username, config.Password);
        }

        _options = options.Build();
        _connectGuard = new Timer();
        _connectGuard.Elapsed += new ElapsedEventHandler(Reconnect!);

        _mqttClient = new MqttClientFactory().CreateMqttClient();

        _mqttClient.ConnectedAsync += async e =>
        {
            if (_connectGuard.Enabled)
            {
                _logger.LogInformation($"Sucesso na reconexão com broker MQTT ({_host}:{_port})");
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
            if (!_disposed)
            {
                if (!_connectGuard.Enabled)
                {
                    _logger.LogInformation($"Reconectando broker MQTT ({_host}:{_port})");
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
                var result = await _mqttClient.ConnectAsync(_options);
                if (result != null && result.ResultCode == MqttClientConnectResultCode.Success)
                {
                    if (_connectGuard.Enabled)
                    {
                        _connectGuard.Stop();
                        _connectGuard.Interval = 1000;
                        _logger.LogInformation(
                            $"Sucesso na reconexão com broker MQTT ({_host}:{_port})"
                        );
                    }
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
            catch (Exception)
            {
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
            //_logger.LogInformation($"Conectando ao broker MQTT ({_host}:{_port})");
            var result = await _mqttClient.ConnectAsync(_options);
            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                _logger.LogError(
                    $"Falha ao conectar ao broker MQTT ({_host}:{_port}): {result.ResultCode} - {result.ReasonString}"
                );
                if (!_connectGuard.Enabled)
                {
                    _connectGuard.Interval = 1000;
                    _connectGuard.Start();
                }
            }
        }
        catch (MqttCommunicationException ex)
        {
            if (ex.Message.Contains("remote certificate was rejected", StringComparison.OrdinalIgnoreCase))
            {
                if (_purpose == Local)
                {
                    var store = _provider.GetRequiredService<IEntityStore>();
                    var token = _provider.GetRequiredService<Token>();
                    var authority = _provider.GetRequiredService<ICertificateAuthorityService>();
                    await store.DeleteManyAsync<Configuracao>(x =>
                        x.Id == Entity.Keys.Security.CertificateMqttLocal
                    );
                    await SeedData.LoadCertificateAuthorityMaster(token, authority, _host);

                    await Task.Delay(1000);
                    Environment.Exit(1);
                }
            }
            _logger.LogError(
                ex,
                $"Falha ao conectar ao broker MQTT ({_host}:{_port}): {ex.HResult} - {ex.Message}"
            );
            if (!_connectGuard.Enabled)
            {
                _connectGuard.Interval = 1000;
                _connectGuard.Start();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao conectar ao broker MQTT ({_host}:{_port})");
            if (!_connectGuard.Enabled)
            {
                _connectGuard.Interval = 1000;
                _connectGuard.Start();
            }
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
            _certificate?.Dispose();
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

    public string Host => _current?.Host ?? "";
    public int Port => _current?.Port ?? 0;

    public async Task Reload(Configuration config)
    {
        if (_current == null)
            return;

        var topics = _current.Topics.ToList();
        var handler = _current.Handler;
        var purpose = _current.Purpose;
        var provider = _current.Provider;
        var connected = _current.IsConnected;
        var certificate = _current.Certificate;
        _current.Certificate = null;
        _current.Dispose();

        _current = new Mqtt(
            provider: provider,
            config: config,
            topics: topics,
            purpose: purpose,
            certificate: certificate
        );
        _current.SetHandler(handler);
        if (connected)
        {
            await _current.ConnectAsync();
        }
    }
}
