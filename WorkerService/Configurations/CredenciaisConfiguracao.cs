namespace WorkerService.Configurations
{
    public class CredenciaisConfiguracao
    {
        public Guid ContaId { get; set; }
        public string TopicoConfiguracao { get; set; } = string.Empty;
        public IntegracaoConfiguracao Integracao { get; set; } = null!;
    }
}
