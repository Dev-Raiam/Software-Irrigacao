using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Autenticacao;
using Toolbox.Automacao.Autenticacao.Setup;
using Toolbox.Automacao.Sincronizacao.Extensions;

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
            setup =>
            {
                setup.PainelId = Guid.Parse("c0f34ad2-6725-48fd-b68e-29f98dd9092d");
                setup.Automatica = true;
                setup.Agendamento.Timer = TimeSpan.FromSeconds(3);
            }
        );
    }
}
