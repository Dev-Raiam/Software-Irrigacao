using Autenticacao.Dtos;
using SoftwareIrrigacao.Autenticacao;
using SoftwareIrrigacao.Autenticacao.Http;

namespace Autenticacao.Setup;

internal static class AutenticacaoConfig
{
    public static void AddAutenticacao(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient<IAutenticacaoApi, AutenticacaoApi>();
        services.AddSingleton<Token>();
        services.Configure<ApiConfiguracao>(configuration.GetSection("ApiConfiguracao"));

        services.AddTransient<AutenticacaoHandler>();
    }
}
