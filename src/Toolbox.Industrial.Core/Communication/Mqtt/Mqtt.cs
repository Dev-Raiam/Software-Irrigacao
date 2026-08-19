using System.Collections.Concurrent;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Exceptions;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Serilog;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Communication.RaspIO;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages;
using Toolbox.Industrial.Core.Messages.Integration.Events;
using Toolbox.Industrial.Core.Security;
using Toolbox.Industrial.Core.Setup;
using Timer = System.Timers.Timer;
using Token = Toolbox.Industrial.Core.Communication.Api.Contracts.Token;

namespace Toolbox.Industrial.Core.Communication.Mqtt;

public sealed class Mqtt : IMqtt
{
    private static readonly JsonSerializerSettings _serializer =
        JsonConvert.DefaultSettings!.Invoke();
    public const string Interno = "mqttinterno";
    public const string Local = "mqttlocal";
    public const string Remoto = "mqttremoto";
    private readonly List<MqttTopicFilter> _topics;
    private readonly int _reconnectIntervalFactor;
    private readonly MqttClientOptions _options;
    private readonly IServiceProvider _provider;
    private readonly IMqttClient _mqttClient;
    private readonly int _reconnectInterval;
    private readonly ILogger<Mqtt> _logger;
    private X509Certificate2? _certificate;
    private readonly Timer _connectGuard;
    private readonly IMediator _mediator;
    private bool _reconnecting = false;
    private readonly string _brokerKey;
    private bool _initializing = true;
    private readonly string _host;
    private readonly int _port;
    private bool _disposed;

    internal IReadOnlyList<MqttTopicFilter> Topics => _topics;
    internal X509Certificate2? Certificate
    {
        get { return _certificate; }
        set { _certificate = value; }
    }
    internal string Host => _host;
    internal int Port => _port;

    public bool IsConnected => _mqttClient.IsConnected;
    public IServiceProvider Provider => _provider;
    public string BrokerKey => _brokerKey;

    public Mqtt(
        IServiceProvider provider,
        string brokerKey,
        Configuration config,
        X509Certificate2? certificate = null,
        IEnumerable<MqttTopicFilter>? topics = null
    )
    {
        _provider = provider;
        _brokerKey = brokerKey;
        _host = config.Host;
        _port = config.Port;
        _topics = [.. topics ?? []];
        _certificate = certificate;
        _serializer.TypeNameHandling = TypeNameHandling.Objects;
        _logger = provider.GetRequiredService<ILogger<Mqtt>>();
        _mediator = provider.GetRequiredService<IMediator>();
        _reconnectInterval = brokerKey == Remoto ? 1000 : 20;
        _reconnectIntervalFactor = brokerKey == Remoto ? 2 : 1;
        var options = new MqttClientOptionsBuilder()
            .WithClientId(config.ClientId)
            .WithTcpServer(config.Host, config.Port)
            .WithCleanSession(config.CleanSession)
            .WithTimeout(TimeSpan.FromSeconds(config.ConnectionTimeoutSeconds));

        if (certificate != null)
        {
            options = options.WithTlsOptions(tls =>
            {
                var ca = X509CertificateLoader.LoadCertificateFromFile($"{_brokerKey}.cer");
                tls.UseTls();
                tls.WithCertificateValidationHandler(context =>
                {
                    if (context.Certificate == null)
                        return false;

                    using var certificate = new X509Certificate2(context.Certificate);
                    using var chain = new X509Chain();

                    chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                    chain.ChainPolicy.CustomTrustStore.Clear();
                    if (ca != null)
                    {
                        chain.ChainPolicy.CustomTrustStore.Add(ca);
                    }
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                    chain.ChainPolicy.DisableCertificateDownloads = true;

                    var valid = chain.Build(certificate);
                    if (!valid)
                    {
                        foreach (var status in chain.ChainStatus)
                        {
                            if (
                                status.Status == X509ChainStatusFlags.NotSignatureValid
                                && BrokerKey == Local
                            )
                            {
                                ReloadCertificateMaster();
                            }
                            Log.Error($"MQTT TLS: {status.Status} - {status.StatusInformation}");
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
                _connectGuard.Stop();
                _logger.LogInformation($"Sucesso na reconexão com broker MQTT ({_host}:{_port})");
                Thread.Sleep(10);
                _connectGuard.Interval = _reconnectInterval;
            }
            if (_initializing)
            {
                _logger.LogInformation($"Conectado ao broker MQTT ({_host}:{_port})");
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
                if (!_initializing && !_connectGuard.Enabled)
                {
                    _logger.LogInformation($"Reconectando broker MQTT ({_host}:{_port})");
                    _connectGuard.Interval = _reconnectInterval;
                    _connectGuard.Start();
                }
            }
            return Task.CompletedTask;
        };

        _mqttClient.ApplicationMessageReceivedAsync += e =>
        {
            try
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                var message = JsonConvert.DeserializeObject(payload, _serializer)!;
                Console.WriteLine(
                    $"Mensagem recebida [{_brokerKey}]: {topic} => {message.GetType().Name} => {payload}"
                );
                if (message is Command command)
                {
                    command.Mqtt = this;
                    command.Topic = topic;
                    return _mediator.Execute((dynamic)command);
                }
                else if (message is ResponseRequest response)
                {
                    response.Mqtt = this;
                    response.Topic = topic;
                    return _mediator.Publish(response);
                }
                else if (message is Toolbox.Core.Messages.IEvent @event)
                {
                    return _mediator.Publish(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar mensagem MQTT [{purpose}]: {Message}",
                    _brokerKey,
                    ex.Message
                );
            }

            return Task.CompletedTask;
        };
    }

    private void ReloadCertificateMaster()
    {
        var store = _provider.GetRequiredService<IEntityStore>();
        var token = _provider.GetRequiredService<Token>();
        var authority = _provider.GetRequiredService<ICertificateAuthorityService>();
        store
            .DeleteManyAsync<Configuracao>(x => x.Id == Entity.Keys.Security.CertificateMqttLocal)
            .GetAwaiter()
            .GetResult();
        Application
            .LoadCertificateAuthorityMaster(token, authority, _host)
            .GetAwaiter()
            .GetResult();
        _logger.LogWarning(
            $"A aplicação será finalizada para completar a reimplantação do certificado do {_host}"
        );
        Application.Restart().GetAwaiter().GetResult();
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
                        _connectGuard.Interval = _reconnectInterval;
                        _logger.LogInformation(
                            $"Sucesso na reconexão com broker MQTT ({_host}:{_port})"
                        );
                    }
                    return;
                }
                if (result != null && result.ResultCode != MqttClientConnectResultCode.Success)
                {
                    _connectGuard.Interval *= _reconnectIntervalFactor;
                    if (_connectGuard.Interval > 15000) // Limite máximo de espera de 1 minuto
                    {
                        _connectGuard.Interval = 15000;
                    }
                }
            }
            catch (Exception)
            {
                _connectGuard.Interval *= _reconnectIntervalFactor;
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

    public async Task<bool> ConnectAsync()
    {
        if (_mqttClient.IsConnected || _connectGuard.Enabled)
            return _mqttClient.IsConnected;

        try
        {
            var result = await _mqttClient.ConnectAsync(_options);
            if (result.ResultCode != MqttClientConnectResultCode.Success)
            {
                _logger.LogError(
                    $"Falha ao conectar ao broker MQTT ({_host}:{_port}): {result.ResultCode} - {result.ReasonString}"
                );
                if (!_connectGuard.Enabled)
                {
                    _connectGuard.Interval = _reconnectInterval;
                    _connectGuard.Start();
                }
                return _mqttClient.IsConnected;
            }
            _initializing = false;
            return _mqttClient.IsConnected;
        }
        catch (MqttCommunicationException ex)
        {
            if (
                ex.Message.Contains(
                    "remote certificate was rejected",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                if (_brokerKey == Local)
                {
                    ReloadCertificateMaster();
                }
            }

            if (ex.HResult == -2146233088)
            {
                //Tratar erro An unknown chain building error occurred
                //Na primeira conexão com o broker.
                //System.Security.Cryptography.CryptographicException: An unknown chain building error occurred.
                //System.Security.Cryptography.X509Certificates.X509Chain.Build(X509Certificate2 certificate, Boolean throwOnException) at System.Net.Security.SslStream.SelectClientCertificate()
                return await ConnectAsync();
            }
            _logger.LogError(
                ex,
                $"Falha ao conectar ao broker MQTT ({_host}:{_port}): {ex.HResult} - {ex.Message}"
            );
            if (!_connectGuard.Enabled)
            {
                _connectGuard.Interval = _reconnectInterval;
                _connectGuard.Start();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao conectar ao broker MQTT ({_host}:{_port})");
            if (!_connectGuard.Enabled)
            {
                _connectGuard.Interval = _reconnectInterval;
                _connectGuard.Start();
            }
        }
        return _mqttClient.IsConnected;
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

    public async Task<PendingProcess<TContent>?> PublishAsync<TContent>(
        string topic,
        TContent content,
        bool retain = false,
        QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce
    )
        where TContent : class
    {
        await ConnectAsync();
        PendingProcess<TContent>? result = null;
        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JsonConvert.SerializeObject(content, _serializer))
                .WithRetainFlag(retain)
                .WithQualityOfServiceLevel((MqttQualityOfServiceLevel)qos)
                .Build();

            if (content is Command command)
            {
                result = new PendingProcess<TContent>
                {
                    Id = command.ProcessId,
                    Topic = topic,
                    Content = content,
                    BrokerKey = command.Mqtt.BrokerKey,
                    Completion = new TaskCompletionSource<ResponseRequest>(
                        TaskCreationOptions.RunContinuationsAsynchronously
                    ),
                };
                MqttManager.Process.Add(result);
            }
            await _mqttClient.PublishAsync(message);
        }
        catch (Exception ex)
        {
            throw new Exception(
                $"Erro ao publicar no tópico {topic} em {_host}:{_port}: {ex.Message}",
                ex
            );
        }
        return result;
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
            GC.SuppressFinalize(this);
        }
    }
}

public sealed class MqttManager
{
    public MqttManager(Mqtt? mqtt)
    {
        _current = mqtt;
    }

    private Mqtt? _current;
    public IMqtt? Current => _current;

    public static readonly MqttProcessManager Process = new MqttProcessManager();

    public async Task Reload(Configuration config)
    {
        if (_current == null)
            return;

        var topics = _current.Topics.ToList();
        var provider = _current.Provider;
        var brokerKey = _current.BrokerKey;
        var connected = _current.IsConnected;
        var certificate = _current.Certificate;
        _current.Certificate = null;
        _current.Dispose();

        _current = new Mqtt(
            brokerKey: brokerKey,
            provider: provider,
            config: config,
            topics: topics,
            certificate: certificate
        );
        if (connected)
        {
            await _current.ConnectAsync();
        }
    }
}

public sealed class MqttProcessManager
{
    private readonly ConcurrentDictionary<string, IPendingProcess> _pendings = new();
    public IReadOnlyDictionary<string, IPendingProcess> Pendings => _pendings;

    public bool Add(IPendingProcess process)
    {
        return _pendings.TryAdd(process.Id, process);
    }

    public bool Completed(string processId, ResponseRequest response)
    {
        var result = _pendings.TryRemove(processId, out var process);
        if (result)
        {
            process!.Completed(response);
        }
        return result;
    }
}
