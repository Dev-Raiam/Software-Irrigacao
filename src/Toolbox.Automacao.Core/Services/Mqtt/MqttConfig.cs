namespace Toolbox.Automacao.Core.Services.Mqtt;

/// <summary>
/// Configuração para conexão MQTT
/// </summary>
public sealed class MqttConfig
{
    /// <summary>
    /// Endereço do broker MQTT (ex: localhost, 192.168.1.100)
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Porta do broker MQTT (ex: 1883 para TCP, 8883 para TLS)
    /// </summary>
    public int Port { get; set; } = 1883;

    /// <summary>
    /// Client ID para identificação no broker
    /// </summary>
    public string ClientId { get; set; } = "ToolboxClient";

    /// <summary>
    /// Nome de usuário para autenticação (opcional)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Senha para autenticação (opcional)
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Timeout de conexão em segundos
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Quality of Service padrão para publicações
    /// </summary>
    public int DefaultQoS { get; set; } = 0;

    /// <summary>
    /// Retain padrão para publicações
    /// </summary>
    public bool DefaultRetain { get; set; } = false;

    /// <summary>
    /// Clean Session - limpa sessão anterior ao conectar
    /// </summary>
    public bool CleanSession { get; set; } = true;
}
