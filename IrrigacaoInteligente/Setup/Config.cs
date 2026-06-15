using System.Threading.RateLimiting;
using IrrigacaoInteligente.Core.DataBase;
using IrrigacaoInteligente.Features.Configuracao.Edpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Toolbox.Automacao.Sincronizacao.Data;

namespace IrrigacaoInteligente.Setup;

public static class Config
{
    public static void AddConfiguration(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        builder
            .Configuration.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();

        services.AddDbContext<SincronizacaoDbContext, IrrigacaoInteligenteContext>(options =>
            options.UseSqlite(
                builder.Configuration.GetSection("PathDatabase:Path").Value,
                b =>
                {
                    b.MigrationsAssembly("IrrigacaoInteligente");
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
