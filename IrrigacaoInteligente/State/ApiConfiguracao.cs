namespace IrrigacaoInteligente.State
{
    public class ApiConfiguracao
    {
        public string BaseUrl { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
        public int RetryAttempts { get; set; }
        public int RetryDelaySeconds { get; set; }
    };
}
