namespace WorkerService.Configurations
{
    public class ContaConfiguracao
    {
        public Guid ContaId { get; set; }
        public bool Invalida => ContaId == Guid.Empty;
    }
}
