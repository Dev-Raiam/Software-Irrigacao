using Microsoft.Extensions.DependencyInjection;
using NModbus;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon;
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

            //services.AddScoped<IModbusFacade>(sp =>
            //{
            //    var modbus = new ModbusFacadeFactory().CriarRtuMaster(new ModbusConfig());
            //    return modbus;
            //});

            //services.AddTransient<TesteWGW420>();
            // DriverTekon é uma classe base abstrata

            // Implementações concretas devem ser registradas nos módulos específicos
        }
    }
}
