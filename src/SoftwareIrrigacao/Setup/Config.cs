using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SoftwareIrrigacao.Data;
using SoftwareIrrigacao.Features.Configuracao.Edpoints;
using Toolbox.Automacao.Sincronizacao.Infrastructure.Data;

namespace SoftwareIrrigacao.Setup;

public static class Config
{
    public static void AddConfiguration(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        var keysPath =
            builder.Configuration["DataProtection:KeysPath"]
            ?? Path.Combine(AppContext.BaseDirectory, "Keys");

        services
            .AddDataProtection()
            .SetApplicationName("SoftwareIrrigacao")
            .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

        builder
            .Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();

        services.AddDbContext<SincronizacaoDbContext, SoftwareIrrigacaoContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetSection("PathDatabase:Path").Value,
                b =>
                {
                    b.MigrationsAssembly("SoftwareIrrigacao");
                    b.MigrationsHistoryTable("__EFMigrationsHistory_Irrigacao");
                }
            )
        );

        builder.Services.AddRateLimiter(options =>
        {
            options.AddConcurrencyLimiter(
                "limite-tentativas",
                options =>
                {
                    options.PermitLimit = 5;
                    options.QueueLimit = 5;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                }
            );
        });
    }

    public static void UseConfig(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        AdicionarCredenciais.Endpoint(app);
        AtualizarCredenciais.Endpoint(app);
    }
}
