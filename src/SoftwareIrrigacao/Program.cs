using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Serilog;
using SoftwareIrrigacao.Setup;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Setup;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
if (args.Length > 0 && (args[0] == "--version" || args[0] == "-v"))
{
    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    Console.WriteLine(version);
    return;
}

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.LiteDB(ApiConfig.ConnectionString, logCollectionName: "logs")
    .CreateBootstrapLogger();

try
{
    Log.Information($"Inicializando aplicação");
    Log.Information($"Atualizada Agora Nova Versão");
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApiConfiguration(builder);
    builder.Services.RegisterServices();
    var app = builder.Build();
    app.UseConfig();

    await Application.RunAsync(
        app,
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
