using Microsoft.AspNetCore.Builder;
using Serilog;
using Serilog.Settings.Configuration;

namespace SoftwareIrrigacao.Setup;

public static class SerilogConfig
{
    public static void AddSerilogConfiguration(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        var logPath = Environment.GetEnvironmentVariable("LOG_PATH") ?? "logs";

        builder.Host.UseSerilog(
            (context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration);

                if (!builder.Environment.IsDevelopment())
                {
                    config.WriteTo.File($"{logPath}/log-.txt");
                }
            }
        );
    }
}
