namespace Toolbox.Automacao.Core.Services.Mqtt;

/// <summary>
/// Factory para criar instâncias do MqttFacade
/// </summary>
public interface IMqttFacadeFactory
{
    /// <summary>
    /// Cria uma nova instância do MqttFacade com a configuração fornecida
    /// </summary>
    IMqttFacade Criar(MqttConfig config);
}

/// <summary>
/// Implementação da factory para criar instâncias do MqttFacade
/// </summary>
public class MqttFacadeFactory : IMqttFacadeFactory
{
    public IMqttFacade Criar(MqttConfig config)
    {
        return new MqttFacade(config);
    }
}
