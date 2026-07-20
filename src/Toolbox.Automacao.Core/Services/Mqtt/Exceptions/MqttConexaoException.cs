namespace Toolbox.Automacao.Core.Services.Mqtt.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre erro de conexão MQTT
/// </summary>
public sealed class MqttConexaoException : MqttException
{
    public MqttConexaoException(string message) : base(message)
    {
    }

    public MqttConexaoException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
