using Microsoft.Extensions.DependencyInjection;
using Toolbox.Industrial.Driver.Tekon;
using Toolbox.Industrial.Driver.Tekon.Interfaces;

namespace Toolbox.Industrial.Driver.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddSingleton<ITekonDispositivoFactory, TekonDispositivoFactory>();
            //services.AddSingleton<IModbusFactory, Modbus>();
            services.AddSingleton<ITekonDriverFactory, TekonDriverFactory>();
        }
    }
}
