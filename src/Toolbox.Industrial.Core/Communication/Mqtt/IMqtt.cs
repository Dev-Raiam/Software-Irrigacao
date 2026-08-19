using Newtonsoft.Json;
using Toolbox.Core.Extensions;
using Toolbox.Industrial.Core.Messages.Integration.Events;

namespace Toolbox.Industrial.Core.Communication.Mqtt;

public interface IMqtt : IDisposable
{
    bool IsConnected { get; }

    Task<bool> ConnectAsync();

    Task DisconnectAsync();

    Task<PendingProcess<TContent>?> PublishAsync<TContent>(
        string topic,
        TContent content,
        bool retain = false,
        QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce
    )
        where TContent : class;

    Task SubscribeAsync(string topic, QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce);

    Task UnsubscribeAsync(string topic);

    //void SetHandler(Action<string, string>? handler);
}

public sealed record Configuration
{
    public Configuration(string host = "localhost")
    {
        Host = host;
    }

    internal static Configuration PythonSettings(Configuration config)
    {
        return new Configuration(host: config.Host)
        {
            Port = config.Port,
            ClientId = config.ClientId.GetId().ToString(),
            Username = config.Username,
            Password = config.Password,
            DefaultQoS = config.DefaultQoS,
            CleanSession = config.CleanSession,
            DefaultRetain = config.DefaultRetain,
            ConnectionTimeoutSeconds = config.ConnectionTimeoutSeconds,
        };
    }

    public string Host { get; private set; } = "localhost";
    public int Port { get; init; } = 8883;
    public string ClientId { get; init; } = Guid.NewGuid().ToString();
    public string? Username { get; init; }
    public string? Password { get; init; }
    public int ConnectionTimeoutSeconds { get; init; } = 10;
    public QualityOfServiceLevel DefaultQoS { get; init; } = QualityOfServiceLevel.AtMostOnce;
    public bool DefaultRetain { get; init; } = false;
    public bool CleanSession { get; init; } = true;

    public void SetHost(string host)
    {
        Host = host;
    }
}

public enum QualityOfServiceLevel
{
    AtMostOnce = 0,
    AtLeastOnce = 1,
    ExactlyOnce = 2,
}
