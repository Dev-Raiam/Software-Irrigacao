using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Sincronizacao.Core.Abstractions;
using Toolbox.Automacao.Sincronizacao.Extensions.Options;
using Toolbox.Automacao.Sincronizacao.Infrastructure.Http;
using Toolbox.Automacao.Sincronizacao.Sync;

namespace Toolbox.Automacao.Sincronizacao.Extensions;

public static class PackageConfig
{
    public static IServiceCollection AddSincronizacao(
        this IServiceCollection services,
        IConfiguration configuration,
        Func<IHttpClientBuilder, IHttpClientBuilder>? httpBuilderConfig = null,
        Action<SincronizacaoConfiguracao>? configuracao = null
    )
    {
        var sincronizacaoConfiguracao = new SincronizacaoConfiguracao();

        configuracao?.Invoke(sincronizacaoConfiguracao);

        services.AddSingleton(sincronizacaoConfiguracao);

        services.AddConfiguration(configuration);
        services.AddRegisterServices();

        services.Configure<ApiConfiguracao>(configuration.GetSection("ApiConfiguracao"));

        var builder = services.AddHttpClient<IApiAutomacao, ApiAutomacao>();

        if (sincronizacaoConfiguracao.Automatica)
        {
            services.AddHostedService<SincronizacaoBackground>();
        }

        httpBuilderConfig?.Invoke(builder);

        return services;
    }
}