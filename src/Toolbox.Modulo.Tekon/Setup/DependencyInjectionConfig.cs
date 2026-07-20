using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon.Interfaces;

namespace Toolbox.Modulo.Tekon.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<ITekonDispositivoFactory, TekonDispositivoFactory>();
            services.AddSingleton<IModbusFacadeFactory, ModbusFacadeFactory>();
            services.AddSingleton<ITekonDriverFactory, TekonDriverFactory>();

        }
    }
}
