using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkerService.Infrastructure.Data;

namespace WorkerService.Infrastructure.SeedData;

public static class SeedData
{
    public static async Task Seed(IServiceProvider serviceProvider)
    {
        var scoped = serviceProvider.CreateScope();
        var context = scoped.ServiceProvider.GetRequiredService<WorkerServiceContext>();
        await context.Database.MigrateAsync();
    }
}
