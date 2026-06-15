using Microsoft.AspNetCore.Builder;
using Serilog;

namespace IrrigacaoInteligente.Setup;

public static class SerilogConfig
{
    public static void AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        var logBasePath = builder.Configuration["Log:Path"];

        if (builder.Environment.IsDevelopment())
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                //.MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override(
                    "Microsoft.EntityFrameworkCore",
                    Serilog.Events.LogEventLevel.Warning
                )
                .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File(
                    $"{logBasePath}/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7
                )
                .CreateLogger();
        }
        else
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override(
                    "Microsoft.Hosting.Lifetime",
                    Serilog.Events.LogEventLevel.Warning
                )
                .MinimumLevel.Override(
                    "Microsoft.EntityFrameworkCore",
                    Serilog.Events.LogEventLevel.Warning
                )
                .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.File(
                    $"{logBasePath}/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7
                )
                .CreateLogger();
        }

        builder.Services.AddSerilog();
    }
}
