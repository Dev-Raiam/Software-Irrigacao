using Microsoft.Extensions.DependencyInjection;

namespace Toolbox.Industrial.Driver.Setup
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
