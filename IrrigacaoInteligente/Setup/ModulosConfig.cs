using Autenticacao.Configurations;
using Autenticacao.Services;
using Toolbox.Automacao.Sincronizacao.Configurations;

namespace IrrigacaoInteligente.Setup;

public static class ModulosConfig
{
    public static void AddRegisterModulos(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddModuloAutenticacao(configuration);

        // Modulo Sincronizacao
        services.AddModuloSincronizacao(
            configuration,
            builder => builder.AddHttpMessageHandler<AutenticacaoHandler>(),
            config =>
            {
                config.Auto = false;
                config.TempoSincronizacao = TimeSpan.FromSeconds(1000);
            }
        );
    }
}
