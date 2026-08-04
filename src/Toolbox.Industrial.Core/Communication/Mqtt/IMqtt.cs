using Newtonsoft.Json;

namespace Toolbox.Industrial.Core.Communication.Mqtt;

public interface IMqtt : IDisposable
{
    bool IsConnected { get; }

    Task ConnectAsync();

    Task DisconnectAsync();

    Task PublishAsync(
        string topic,
        string payload,
        bool retain = false,
        QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce
    );

    Task PublishAsync(
        string topic,
        byte[] payload,
        bool retain = false,
        QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce
    );

    Task SubscribeAsync(string topic, QualityOfServiceLevel qos = QualityOfServiceLevel.AtMostOnce);

    Task UnsubscribeAsync(string topic);

    void SetHandler(Action<string, string>? handler);
}

public sealed record Configuration
{
    public Configuration(string host = "localhost")
    {
        Host = host;
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
