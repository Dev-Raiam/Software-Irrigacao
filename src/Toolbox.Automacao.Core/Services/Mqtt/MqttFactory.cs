namespace Toolbox.Automacao.Core.Services.Mqtt;

/// <summary>
/// Factory para criar instâncias do MqttFacade
/// </summary>
public interface IMqttFactory
{
    /// <summary>
    /// Cria uma nova instância do MqttFacade com a configuração fornecida
    /// </summary>
    IMqtt Criar(MqttConfig config);
}

/// <summary>
/// Implementação da factory para criar instâncias do MqttFacade
/// </summary>
public class MqttFactory : IMqttFactory
{
    public IMqtt Criar(MqttConfig config)
    {
        return new Mqtt(config);
    }
}
