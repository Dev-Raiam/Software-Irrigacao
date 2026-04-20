using Microsoft.EntityFrameworkCore;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Configurations;

public static class ContextoConfiguracao
{
    public static void RegistrarContexto(this IServiceCollection services)
    {
        services.AddDbContext<WorkerServiceContext>(options =>
            options.UseSqlite("Data Source=database.db")
        );
    }
}
