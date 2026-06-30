namespace Toolbox.Automacao.Sincronizacao.Provider
{
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

        public sealed class Conexao
        {
            public Guid Id { get; init; }
            public string Tipo { get; init; } = null!;
            public Canal Canal { get; init; } = null!;
        }
    }
}
