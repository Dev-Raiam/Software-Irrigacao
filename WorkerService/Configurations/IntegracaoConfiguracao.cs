namespace WorkerService.Configurations
{
    public class IntegracaoConfiguracao
    {
        public string Chave { get; set; } = null!;
        public string Segredo { get; set; } = null!;
        public Guid ContextoId { get; set; }

        public bool Invalida => Chave is null || Segredo is null || ContextoId == Guid.Empty;
    };
}
