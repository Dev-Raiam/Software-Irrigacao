using Microsoft.AspNetCore.Builder;
using Namotion.Reflection;
using Serilog;

namespace SoftwareIrrigacao.Setup;

public static class SerilogConfig
{
    public static void AddSerilogConfiguration(
        this IServiceCollection services,
        WebApplicationBuilder builder
    )
    {
        builder.Host.UseSerilog(
            (context, config) =>
            {
                // Ler as configurações do serilog pelo appSettings.json
                config.ReadFrom.Configuration(context.Configuration);

                //config.WriteTo.LiteDB("Irrigacao.db", logCollectionName: "logs");
            }
        );
    }
}
