using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Sincronizacao.Core.Abstractions;
using Toolbox.Automacao.Sincronizacao.Provider;
using Toolbox.Automacao.Sincronizacao.Sync;

namespace Toolbox.Automacao.Sincronizacao.Extensions;

internal static class DependencyInjectionConfig
{
    public static void AddRegisterServices(this IServiceCollection services)
    {
        services.AddScoped<ISincronizarControladores,SincronizarControladores>();
        services.AddScoped<IProviderSincronizacao,ProviderSincronizacao>();
    }
}
