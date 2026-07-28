using Microsoft.AspNetCore.Builder;
using SoftwareIrrigacao.Infrastructure.Handlers.Exceptions;
using SoftwareIrrigacao.Workes;
using System.Reflection;
using Toolbox.Core.Extensions;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Setup;

namespace SoftwareIrrigacao.Setup;

public static class ApiConfig
{
    /// <summary>
    /// Password = 9ee58e75-0741-47dd-4ea6-cf2559eac5a3
    /// </summary>
    internal static string ConnectionString = $"Filename=Irrigacao.db;Password={"Irrigacao.db".GetId()};Collation=pt-BR/IgnoreCase,IgnoreNonSpace;Connection=Shared";
    public static void AddApiConfiguration(
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
        services
            .AddIndustrialCore(builder.Configuration, Assembly.GetExecutingAssembly())
            .AddLiteDbEntityStore(builder, ConnectionString);

        //services.AddModuloTekon();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
    }

    public static void UseConfig(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.RegisterEndpoints();
    }
}
