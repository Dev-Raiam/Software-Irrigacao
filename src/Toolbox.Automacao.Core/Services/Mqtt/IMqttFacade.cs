namespace Toolbox.Automacao.Core.Services.Mqtt;

/// <summary>
/// Interface Facade para simplificar operações MQTT
/// </summary>
public interface IMqttFacade : IDisposable
{
    /// <summary>
    /// Conecta ao broker MQTT
    /// </summary>
    Task ConectarAsync();

    /// <summary>
    /// Desconecta do broker MQTT
    /// </summary>
    Task DesconectarAsync();

    /// <summary>
    /// Publica uma mensagem em um tópico MQTT
    /// </summary>
    /// <param name="topic">Tópico onde publicar</param>
    /// <param name="payload">Conteúdo da mensagem</param>
    /// <param name="retain">Se a mensagem deve ser retida no broker</param>
    /// <param name="qos">Quality of Service (0, 1 ou 2)</param>
    Task PublicarAsync(string topic, string payload, bool retain = false, int qos = 0);

    /// <summary>
    /// Publica uma mensagem em um tópico MQTT
    /// </summary>
    /// <param name="topic">Tópico onde publicar</param>
    /// <param name="payload">Conteúdo da mensagem como bytes</param>
    /// <param name="retain">Se a mensagem deve ser retida no broker</param>
    /// <param name="qos">Quality of Service (0, 1 ou 2)</param>
    Task PublicarAsync(string topic, byte[] payload, bool retain = false, int qos = 0);

    /// <summary>
    /// Assina um tópico MQTT para receber mensagens
    /// </summary>
    /// <param name="topic">Tópico para assinar</param>
    /// <param name="qos">Quality of Service (0, 1 ou 2)</param>
    /// <param name="messageHandler">Handler para processar mensagens recebidas</param>
    Task AssinarAsync(string topic, int qos = 0, Action<string, string>? messageHandler = null);

    /// <summary>
    /// Cancela assinatura de um tópico MQTT
    /// </summary>
    /// <param name="topic">Tópico para cancelar assinatura</param>
    Task DesassinarAsync(string topic);

    /// <summary>
    /// Verifica se está conectado ao broker
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Define um handler global que será chamado para todas as mensagens recebidas
    /// </summary>
    /// <param name="handler">Handler que recebe tópico e payload de todas as mensagens</param>
    void DefinirManipuladorGlobal(Action<string, string>? handler);
}
