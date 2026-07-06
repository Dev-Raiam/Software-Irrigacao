using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SoftwareIrrigacao.Features.Configuracao.Edpoints;
using SoftwareIrrigacao.Infra.Data;
using SoftwareIrrigacao.Infra.Mqtt;
using SoftwareIrrigacao.Workers;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Setup;

namespace SoftwareIrrigacao.Setup;

public static class ModuloConfig
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

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition =
                System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        });

        //services.AddHostedService<ProntidaoWorker>();
        services.AddHostedService<MqttWorker>();

        var pathDbConfig = builder.Configuration.GetSection("PathDatabase:Path").Value;

        var connectionString = !string.IsNullOrWhiteSpace(pathDbConfig)
            ? $"Data Source={pathDbConfig}"
            : "Data Source=Irrigacao.db";

        services.AddDbContext<IrrigacaoDbContext>(options => options.UseSqlite(connectionString));

        services.AddScoped<AutomacaoDbContext, IrrigacaoDbContext>();

        services.AddModuloCore(builder.Configuration, connectionString);

        services.AddRateLimiter(options =>
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
        //AtualizarCredenciais.Endpoint(app);
        Autenticar.Endpoint(app);
    }
}
