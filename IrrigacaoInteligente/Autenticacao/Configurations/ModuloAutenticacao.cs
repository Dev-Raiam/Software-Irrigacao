using Autenticacao.Interfaces;
using Autenticacao.Models;
using Autenticacao.Services;

namespace Autenticacao.Configurations;

internal static class ModuloAutenticacao
{
    public static void AddModuloAutenticacao(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient<IAutenticacaoApi, AutenticacaoApi>();
        services.AddSingleton<Token>();
        services.Configure<AppOptions>(configuration.GetSection("ApiOptions"));

        services.AddTransient<AutenticacaoHandler>();
    }
}
