using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Autenticacao.Dtos;
using Toolbox.Automacao.Autenticacao.Http;

namespace Toolbox.Automacao.Autenticacao.Setup;

public static class AutenticacaoConfig
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
