using Toolbox.Industrial.Core.Messages;
using Toolbox.Industrial.Driver.TekonBkp;
using Toolbox.Industrial.Driver.TekonBkp.Interfaces;

namespace SoftwareIrrigacao.Setup;

public static class DependencyInjectionConfig
{
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<ITekonDispositivoFactory, TekonDispositivoFactory>();
        services.AddSingleton<ITekonDriverFactory, TekonDriverFactory>();
        //services.AddHostedService<WorkerTeste>();
    }
}
