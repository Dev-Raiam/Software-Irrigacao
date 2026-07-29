using LiteDB;
using Microsoft.AspNetCore.Builder;
using Serilog;
using SoftwareIrrigacao.Setup;
using Toolbox.Industrial.Core.Data;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.LiteDB(ApiConfig.ConnectionString, logCollectionName: "logs")
    .CreateBootstrapLogger();

//Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Aplicação inicializando...");
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddApiConfiguration(builder);
    builder.Services.RegisterServices();

    var app = builder.Build();
    app.UseConfig();

    //using (var scope = app.Services.CreateScope())
    //{
    //    var sincronizar = scope.ServiceProvider.GetRequiredService<ISincronizarControladores>();

    //    var configuracao = scope.ServiceProvider.GetRequiredService<IGerenciadorConfiguracao>();

    //    var painelId = configuracao.ObterCredencialPainel();

    //    if (painelId != Guid.Empty)
    //        await sincronizar.ExecutarAsync(painelId, CancellationToken.None);
    //}

    await app.EnsureSeedData(
        (provider, store) =>
        {
            //exemplos de uso
            var entityConfig = provider.GetRequiredService<EntityConfiguration>();
            entityConfig.ApplyConfiguration = (IEntityStore store) =>
            {
                //store.Configure<Configuracao>().Field(x => x.Value, "Dados");
            };
            return Task.CompletedTask;
        }
    );

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal("A Aplicação falhou ao iniciar. {ex}", ex);
}
finally
{
    Log.CloseAndFlush();
}
