using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using SoftwareIrrigacao.Infrastructure.Handlers.Exceptions;
using SoftwareIrrigacao.Presentation.Edpoints;
using SoftwareIrrigacao.Workes;
using System.Threading.RateLimiting;
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
            options.SerializerOptions.DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull;
        });

        services.AddHostedService<MqttWorker>();

        var pathDbConfig = builder.Configuration.GetSection("PathDatabase:Path").Value;

        var connectionString = !string.IsNullOrWhiteSpace(pathDbConfig)
            ? $"Data Source={pathDbConfig}"
            : "Data Source=Irrigacao.db";

        services.AddModuloCore(builder.Configuration, connectionString);
        //services.AddModuloTekon();

        services.AddRateLimiter(options =>
        {
            options.AddConcurrencyLimiter(
                "limite-tentativas",
                options =>
                {
                    options.PermitLimit = 2;
                    options.QueueLimit = 2;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                }
            );
        });

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }

    public static void UseConfig(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseRateLimiter();

        Credenciais.Endpoint(app);
    }
}
