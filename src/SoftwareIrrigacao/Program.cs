using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Serilog;
using SoftwareIrrigacao.Setup;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Setup;
using ApplicationBuilder = Toolbox.Industrial.Core.Setup.ApplicationBuilder;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.LiteDB(ApiConfig.ConnectionString, logCollectionName: "logs")
    .CreateBootstrapLogger();

try
{
    ApplicationBuilder.Stopwatch = Stopwatch.StartNew();
    var hostName = Dns.GetHostEntry(Environment.MachineName).HostName;

    Log.Information($"Inicializando aplicação {hostName}");
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApiConfiguration(builder);
    builder.Services.RegisterServices();
    var app = builder.Build();
    app.UseConfig();

    await app.RunAsync(
        (provider, store) =>
        {
            //exemplos de uso
            var entityConfig = provider.GetRequiredService<EntityConfiguration>();
            entityConfig.ApplyConfiguration = (IEntityStore store) => {
                //store.Configure<Configuracao>().Field(x => x.Value, "Dados");
            };
            return Task.CompletedTask;
        }
    );
}
catch (Exception ex)
{
    Log.Fatal("A Aplicação falhou ao iniciar. {ex}", ex);
}
finally
{
    Log.CloseAndFlush();
}
