namespace Toolbox.Automacao.Sincronizacao.Provider
{
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

        public sealed class Conexao
        {
            public Dispositivo.Conexao? Conectado { get; init; }
            public IEnumerable<Porta> Saidas { get; init; } = null!;
            public IEnumerable<Porta> Entradas { get; init; } = null!;
            public IEnumerable<Interface> Interfaces { get; init; } = null!;
        }
    }
}
