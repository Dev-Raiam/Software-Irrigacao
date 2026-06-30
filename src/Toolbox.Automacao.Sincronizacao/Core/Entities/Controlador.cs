using System.Text.Json.Serialization;

namespace Toolbox.Automacao.Sincronizacao.Core.Entities;

public sealed class Controlador
{
    public Guid Id { get; init; }
    public bool Master { get; init; }
    public string Estagio { get; init; } = null!;
    public string Descricao { get; init; } = null!;
    public string Marca { get; init; } = null!;
    public string Modelo { get; init; } = null!;
    public Conexao Conexoes { get; init; } = null!;
    public IEnumerable<Modulo> Modulos { get; init; } = null!;
    public IEnumerable<Interface> Interfaces { get; init; } = null!;
    public IEnumerable<Dispositivo> Dispositivos { get; init; } = null!;

    public class Conexao
    {
        public string Host { get; init; } = null!;
        public IEnumerable<Porta> Saidas { get; init; } = null!;
        public IEnumerable<Porta> Entradas { get; init; } = null!;
        public IEnumerable<Interface> Interfaces { get; init; } = null!;
    }
}

public sealed class Modulo
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = null!;
    public bool Master { get; init; }
    public string Marca { get; init; } = null!;
    public string Modelo { get; init; } = null!;
    public string Estagio { get; init; } = null!;
    public string Protocolo { get; init; } = null!;
    public Parametros Parametros { get; init; } = null!;
    public Conexao Conexoes { get; init; } = null!;

    public class Conexao
    {
        public Dispositivo.Conexao? Conectado { get; init; }
        public IEnumerable<Porta> Saidas { get; init; } = null!;
        public IEnumerable<Porta> Entradas { get; init; } = null!;
        public IEnumerable<Interface> Interfaces { get; init; } = null!;
    }
}

public sealed class Dispositivo
{
    public Guid Id { get; init; }
    public bool Habilitado { get; init; }
    public string Descricao { get; init; } = null!;
    public string Tipo { get; init; } = null!;
    public string Sinal { get; init; } = null!;
    public string Categoria { get; init; } = null!;
    public Parametros? Parametros { get; init; }
    public Conexao Conectado { get; init; } = null!;

    public class Conexao
    {
        public Guid Id { get; init; }
        public string Tipo { get; init; } = null!;
        public Canal Canal { get; init; } = null!;
    }
}

public sealed class Interface
{
    public Guid Id { get; init; }
    public string Tipo { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string Nome { get; init; } = null!;
    public string? Borne { get; init; }
    public string? Endereco { get; init; }
    public Parametros? Parametros { get; init; }
    public IEnumerable<Conexao> Conectados { get; init; } = null!;

    public class Conexao
    {
        public Guid Id { get; init; }
        public string Tipo { get; init; } = null!;
    }
}

public sealed class Porta
{
    public Guid Id { get; init; }
    public string Sinal { get; init; } = null!;
    public string[] Faixa { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string? Borne { get; init; }
    public string? Endereco { get; init; }
    public Parametros? Parametros { get; init; }

    // public Modbus? Modbus { get; init; }
    public Conexao? Conectado { get; init; }

    public class Conexao
    {
        public Guid Id { get; init; }
        public string Tipo { get; init; } = null!;
    }
}

public sealed class Canal
{
    public Guid Id { get; init; }
    public string Tipo { get; init; } = null!;
}

// public class Modbus
// {
//     public int? Indice { get; init; }
//     public int? Endereco { get; init; }
// }'

public sealed class Parametros
{
    [JsonIgnore]
    public bool PossuiParametros => Parametro?.Count > 0;

    [JsonExtensionData]
    public Dictionary<string, object> Parametro { get; set; } =
        new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
}
