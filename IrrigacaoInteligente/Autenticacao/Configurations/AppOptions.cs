namespace Autenticacao.Configurations;

public class AppOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}
