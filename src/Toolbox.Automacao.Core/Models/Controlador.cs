using System.Text.Json.Serialization;
using ConexaoControlador = Toolbox.Automacao.Core.Models.Controlador.Conexao;
using ConexaoDispositivo = Toolbox.Automacao.Core.Models.Dispositivo.Conexao;
using ConexaoInterface = Toolbox.Automacao.Core.Models.Interface.Conexao;
using ConexaoModulo = Toolbox.Automacao.Core.Models.Modulo.Conexao;
using ConexaoPorta = Toolbox.Automacao.Core.Models.Porta.Conexao;

namespace Toolbox.Automacao.Core.Models;

public sealed record Controlador(
    Guid Id,
    bool Master,
    string Estagio,
    string Descricao,
    string Marca,
    string Modelo,
    ConexaoControlador Conexoes,
    IEnumerable<Modulo> Modulos,
    IEnumerable<Interface> Interfaces,
    IEnumerable<Dispositivo> Dispositivos
)
{
    public sealed record Conexao(
        string Host,
        IEnumerable<Porta> Saidas,
        IEnumerable<Porta> Entradas,
        IEnumerable<Interface> Interfaces
    );
}

public sealed record Modulo(
    Guid Id,
    string Descricao,
    bool Master,
    string Marca,
    string Modelo,
    string Estagio,
    string Protocolo,
    Parametros Parametros,
    ConexaoModulo Conexoes
)
{
    public sealed record Conexao(
        Dispositivo.Conexao? Conectado,
        IEnumerable<Porta> Saidas,
        IEnumerable<Porta> Entradas,
        IEnumerable<Interface> Interfaces
    );
}

public sealed record Dispositivo(
    Guid Id,
    bool Habilitado,
    string Descricao,
    string Tipo,
    string Sinal,
    string Categoria,
    Parametros? Parametros,
    ConexaoDispositivo Conectado
)
{
    public sealed record Conexao(Guid Id, string Tipo, Canal Canal);
}

public sealed record Interface(
    Guid Id,
    string Tipo,
    string Status,
    string Nome,
    string? Borne,
    string? Endereco,
    Parametros? Parametros,
    IEnumerable<ConexaoInterface> Conectados
)
{
    public sealed record Conexao(Guid Id, string Tipo);
}

public sealed record Porta(
    Guid Id,
    string Sinal,
    string[] Faixa,
    string Status,
    string? Borne,
    string? Endereco,
    Parametros? Parametros,
    ConexaoPorta? Conectado
)
{
    public sealed record Conexao(Guid Id, string Tipo);
}

public sealed record Canal(Guid Id, string Tipo);

public sealed record Parametros
{
    [JsonIgnore]
    public bool PossuiParametros => Parametro?.Count > 0;

    [JsonExtensionData]
    public Dictionary<string, object> Parametro { get; set; } =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
