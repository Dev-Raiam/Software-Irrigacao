namespace Toolbox.Automacao.Core.Services.Mqtt.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre erro de assinatura/subscription MQTT
/// </summary>
public sealed class MqttAssinaturaException : MqttException
{
    public MqttAssinaturaException(string message) : base(message)
    {
    }

    public MqttAssinaturaException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
