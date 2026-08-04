using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using SoftwareIrrigacao.Infrastructure.Handlers.Exceptions;
using SoftwareIrrigacao.Workes;
using System.Reflection;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Setup;

namespace SoftwareIrrigacao.Setup;

public static class ApiConfig
{
    /// <summary>
    /// Password = 9ee58e75-0741-47dd-4ea6-cf2559eac5a3
    /// </summary>
    internal static string ConnectionString = //Password={"Irrigacao.db".GetId()};
        $"Filename=Irrigacao.db;Collation=pt-BR/IgnoreCase,IgnoreNonSpace;Connection=Shared";

    public static void AddApiConfiguration(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        //builder
        //    .Configuration.SetBasePath(Directory.GetCurrentDirectory())
        //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        //    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
        //    .AddUserSecrets<Program>()
        //    .AddEnvironmentVariables();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.DefaultIgnoreCondition = System
                .Text
                .Json
                .Serialization
                .JsonIgnoreCondition
                .WhenWritingNull;
        });

        //builder.WebHost.ConfigureKestrel(options =>
        //{
        //    options.ListenAnyIP(5000);
        //});

        services.AddCors(options =>
        {
            options.AddPolicy("AllRequests",
                builder =>
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
        });

        //services.AddHostedService<WorkerRaspIO>();
        services.AddHostedService<MqttWorker>();
        services
            .AddIndustrialCore(Assembly.GetExecutingAssembly())
            .AddLiteDbEntityStore(builder, ConnectionString);

        //services.AddModuloTekon();
    }

    public static void UseConfig(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseCors("AllRequests");
        app.RegisterEndpoints();
    }
}
