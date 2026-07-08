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
        var readerOptions = new ConfigurationReaderOptions(
            typeof(Serilog.ConsoleLoggerConfigurationExtensions).Assembly,
            typeof(Serilog.FileLoggerConfigurationExtensions).Assembly
        );

        builder.Host.UseSerilog(
            (context, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration, readerOptions);
            }
        );

        //builder.Services.AddSerilog();

        //var logBasePath = builder.Configuration["Log:Path"];

        //if (builder.Environment.IsDevelopment())
        //{
        //    Log.Logger = new LoggerConfiguration()
        //        .MinimumLevel.Information()
        //        //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        //        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        //        //.MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
        //        .MinimumLevel.Override(
        //            "Microsoft.EntityFrameworkCore",
        //            Serilog.Events.LogEventLevel.Warning
        //        )
        //        .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
        //        .WriteTo.Console()
        //        .WriteTo.File(
        //            $"{logBasePath}/log-.txt",
        //            rollingInterval: RollingInterval.Day,
        //            retainedFileCountLimit: 7
        //        )
        //        .CreateLogger();
        //}
        //else
        //{
        //    Log.Logger = new LoggerConfiguration()
        //        .MinimumLevel.Information()
        //        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        //        .MinimumLevel.Override(
        //            "Microsoft.Hosting.Lifetime",
        //            Serilog.Events.LogEventLevel.Warning
        //        )
        //        .MinimumLevel.Override(
        //            "Microsoft.EntityFrameworkCore",
        //            Serilog.Events.LogEventLevel.Warning
        //        )
        //        .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
        //        .WriteTo.File(
        //            $"{logBasePath}/log-.txt",
        //            rollingInterval: RollingInterval.Day,
        //            retainedFileCountLimit: 7
        //        )
        //        .CreateLogger();
        //}

        //builder.Services.AddSerilog();
    }
}
