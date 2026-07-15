using Microsoft.Extensions.DependencyInjection;
using NModbus;
using Toolbox.Automacao.Core.Services.Modbus;
using Toolbox.Modulo.Tekon;
using Toolbox.Modulo.Tekon.Abstractions;

namespace Toolbox.Modulo.Tekon.Setup
{
    internal static class DependencyInjectionConfig
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            //services.AddTransient<ITekonDispositivoFactory, TekonDispositivoFactory>();
            services.AddTransient<IModbusFacadeFactory, ModbusFacadeFactory>();
            //services.AddScoped<IModbusFacade>(sp =>
            //{
            //    var modbus = new ModbusFacadeFactory().CriarRtuMaster(new ModbusConfig());
            //    return modbus;
            //});

            services.AddTransient<TesteWGW420>();
            // DriverTekon é uma classe base abstrata

            // Implementações concretas devem ser registradas nos módulos específicos
        }
    }
}
