using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Core.Setup;

namespace Toolbox.Modulo.Tekon.Setup
{
    public static class ModuloConfig
    {
        public static void AddModuloTekon(
            this IServiceCollection services
        )
        {
            services.RegisterServices();
        }
    }
}
