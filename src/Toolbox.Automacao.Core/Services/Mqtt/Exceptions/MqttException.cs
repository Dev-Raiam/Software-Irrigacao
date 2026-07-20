namespace Toolbox.Automacao.Core.Services.Mqtt.Exceptions;

/// <summary>
/// Exceção base para erros relacionados ao MQTT
/// </summary>
public abstract class MqttException : Exception
{
    protected MqttException(string message) : base(message)
    {
    }

    protected MqttException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
