namespace IrrigacaoInteligente.Setup
{
    public class ApiOptions
    {
        public string BaseUrl { get; set; } = null!;
        public string MediaType { get; set; } = null!;
        public int TimeoutSeconds { get; set; }
    };
}
