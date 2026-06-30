using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Sincronizacao.Infrastructure.Data;

namespace Toolbox.Automacao.Sincronizacao.Extensions
{
    internal static class Config
    {
        public static void AddConfiguration(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.AddDbContext<SincronizacaoDbContext>(options =>
                options.UseSqlite(
                    configuration.GetSection("PathDatabase:Path").Value,
                    b => b.MigrationsHistoryTable("__EFMigrationsHistory_Sincronizacao")
                )
            );
        }
    }
}
