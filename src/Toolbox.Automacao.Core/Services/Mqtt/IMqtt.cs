namespace Toolbox.Automacao.Core.Services.Mqtt;

/// <summary>
/// Interface Facade para simplificar operações MQTT
/// </summary>
public interface IMqtt : IDisposable
{
    /// <summary>
    /// Connects to the MQTT broker
    /// </summary>
    Task ConnectAsync();

    /// <summary>
    /// Disconnects from the MQTT broker
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Publishes a message to an MQTT topic
    /// </summary>
    /// <param name="topic">Topic to publish to</param>
    /// <param name="payload">Message content</param>
    /// <param name="retain">Whether the message should be retained on the broker</param>
    /// <param name="qos">Quality of Service (0, 1, or 2)</param>
    Task PublishAsync(string topic, string payload, bool retain = false, int qos = 0);

    /// <summary>
    /// Publishes a message to an MQTT topic
    /// </summary>
    /// <param name="topic">Topic to publish to</param>
    /// <param name="payload">Message content as bytes</param>
    /// <param name="retain">Whether the message should be retained on the broker</param>
    /// <param name="qos">Quality of Service (0, 1, or 2)</param>
    Task PublishAsync(string topic, byte[] payload, bool retain = false, int qos = 0);

    /// <summary>
    /// Subscribes to an MQTT topic to receive messages
    /// </summary>
    /// <param name="topic">Topic to subscribe to</param>
    /// <param name="qos">Quality of Service (0, 1, or 2)</param>
    /// <param name="messageHandler">Handler to process received messages</param>
    Task SubscribeAsync(string topic, int qos = 0, Action<string, string>? messageHandler = null);

    /// <summary>
    /// Unsubscribes from an MQTT topic
    /// </summary>
    /// <param name="topic">Topic to unsubscribe from</param>
    Task UnsubscribeAsync(string topic);

    /// <summary>
    /// Checks if connected to the broker
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Sets a global handler that will be called for all received messages
    /// </summary>
    /// <param name="handler">Handler that receives topic and payload of all messages</param>
    void SetGlobalHandler(Action<string, string>? handler);
}
