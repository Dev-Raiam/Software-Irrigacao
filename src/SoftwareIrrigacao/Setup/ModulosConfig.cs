using Autenticacao.Setup;
using SoftwareIrrigacao.Autenticacao;
using Toolbox.Automacao.Sincronizacao.Setup;

namespace SoftwareIrrigacao.Setup;

public static class ModulosConfig
{
    public static void AddRegisterModulos(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddAutenticacao(configuration);

        services.AddSincronizacao(
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
