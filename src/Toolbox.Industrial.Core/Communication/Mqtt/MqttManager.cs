namespace Toolbox.Industrial.Core.Communication.Mqtt;

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
