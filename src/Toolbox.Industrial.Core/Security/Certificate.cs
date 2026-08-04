namespace Toolbox.Industrial.Core.Security;

internal sealed class Certificate
{
    internal enum Purpose
    {
        //Grpc,
        MqttLocal,
        MqttRemoto,
        HttpsLocal,
        //OpcUa,
    }

    public byte[] Content { get; init; } = [];

    public string Password { get; init; } = string.Empty;

    public string Thumbprint { get; init; } = string.Empty;

    public DateTime NotBefore { get; init; }

    public DateTime NotAfter { get; init; }

    public DateTime CreatedAt { get; init; }
}
