using Microsoft.Extensions.DependencyInjection;

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
