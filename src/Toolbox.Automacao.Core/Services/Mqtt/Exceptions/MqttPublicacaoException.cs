namespace Toolbox.Automacao.Core.Services.Mqtt.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre erro de publicação MQTT
/// </summary>
public sealed class MqttPublicacaoException : MqttException
{
    public MqttPublicacaoException(string message) : base(message)
    {
    }

    public MqttPublicacaoException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
