namespace Autenticacao.Setup;

public class ApiConfiguracao
{
    public string BaseUrl { get; init; } = null!;
    public string MediaType { get; init; } = null!;
    public int TimeoutSeconds { get; init; }
}
