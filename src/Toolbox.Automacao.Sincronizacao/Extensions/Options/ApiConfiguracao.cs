namespace Toolbox.Automacao.Sincronizacao.Extensions.Options;

public class ApiConfiguracao
{
    public string BaseUrl { get; set; } = null!;
    public string MediaType { get; set; } = null!;
    public int TimeoutSeconds { get; set; }
}
