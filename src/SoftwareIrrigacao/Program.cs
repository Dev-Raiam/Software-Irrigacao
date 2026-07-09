using Microsoft.AspNetCore.Builder;
using Serilog;
using SoftwareIrrigacao.Infra.Data;
using SoftwareIrrigacao.Setup;
using Toolbox.Automacao.Core.Services;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    Log.Information("Aplicação inicializando...");
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilogConfiguration(builder);
    builder.Services.AddConfiguration(builder);
    builder.Services.AddRegisterServices();
    builder.Services.AddJwtConfiguration();

    var app = builder.Build();
    app.UseConfig();

    await SeedData.Seed(app.Services);

    using (var scope = app.Services.CreateScope())
    {
        var sincronizar = scope.ServiceProvider.GetRequiredService<ISincronizarControladores>();
        await sincronizar.ExecutarAsync(
            Guid.Parse("fcf8723b-86ff-4f7b-a81d-2a87c8fda090"),
            CancellationToken.None
        );
    }

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
