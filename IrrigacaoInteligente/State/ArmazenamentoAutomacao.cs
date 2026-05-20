using System.Text.Json.Serialization;

namespace IrrigacaoInteligente.State
{
    public class ArmazenamentoAutomacao
    {
        public List<Controlador> Controladores { get; private set; } = [];

        public bool Invalido => Controladores.Count == 0;
    }

    public class Controlador
    {
        public Guid Id { get; init; }
        public bool Master { get; init; }
        public string Estagio { get; init; } = null!;
        public string Descricao { get; init; } = null!;
        public string Marca { get; init; } = null!;
        public string Modelo { get; init; } = null!;
        public Parametros Parametros { get; init; } = new Parametros();
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

    public class Modulo
    {
        public Guid Id { get; init; }
        public string Descricao { get; init; } = null!;
        public bool Master { get; init; }
        public string Marca { get; init; } = null!;
        public string Modelo { get; init; } = null!;
        public string Estagio { get; init; } = null!;
        public string Protocolo { get; init; } = null!;
        public Parametros Parametros { get; init; } = new Parametros();
        public Conexao Conexoes { get; init; } = null!;

        public class Conexao
        {
            public Interface.Conexao? Conectado { get; init; }
            public IEnumerable<Porta> Saidas { get; init; } = null!;
            public IEnumerable<Porta> Entradas { get; init; } = null!;
            public IEnumerable<Interface> Interfaces { get; init; } = null!;
        }
    }

    public class Dispositivo
    {
        public Guid Id { get; init; }
        public bool Habilitado { get; init; }
        public string Descricao { get; init; } = null!;
        public string Tipo { get; init; } = null!;
        public string Sinal { get; init; } = null!;
        public string Categoria { get; init; } = null!;
        public Parametros Parametros { get; init; } = new Parametros();
        public Conexao? Conectado { get; init; }

        public class Conexao
        {
            public Guid Id { get; init; }
            public string Tipo { get; init; } = null!;
            public Canal Canal { get; init; } = null!;
        }
    }

    public class Interface
    {
        public Guid Id { get; init; }
        public string Tipo { get; init; } = null!;
        public string Status { get; init; } = null!;
        public string Nome { get; init; } = null!;
        public string? Borne { get; init; }
        public string? Endereco { get; init; }
        public Parametros Parametros { get; init; } = new Parametros();
        public IEnumerable<Porta.Conexao> Conectados { get; init; } = null!;

        public class Conexao
        {
            public Guid Id { get; init; }
            public string Tipo { get; init; } = null!;
            public Canal Canal { get; init; } = null!;
        }
    }

    public class Porta
    {
        public Guid Id { get; init; }
        public string Sinal { get; init; } = null!;
        public string Faixa { get; init; } = null!;
        public string Status { get; init; } = null!;
        public string Descricao { get; init; } = null!;
        public string? Borne { get; init; }
        public string? Endereco { get; init; }

        // public Modbus? Modbus { get; init; }
        public Conexao? Conectado { get; init; }

        public class Conexao
        {
            public Guid Id { get; init; }
            public string Tipo { get; init; } = null!;
        }
    }

    public class Canal
    {
        public Guid Id { get; init; }
        public string Tipo { get; init; } = null!;
    }

    // public class Modbus
    // {
    //     public int? Indice { get; init; }
    //     public int? Endereco { get; init; }
    // }

    public class Parametros
    {
        [JsonIgnore]
        public bool PossuiParametros => Parametro?.Count > 0;

        [JsonExtensionData]
        public Dictionary<string, object> Parametro { get; init; } =
            new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
    }
}
