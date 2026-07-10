using Microsoft.AspNetCore.Builder;
using Serilog;
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

    using (var scope = app.Services.CreateScope())
    {
        var sincronizar = scope.ServiceProvider.GetRequiredService<ISincronizarControladores>();
        var configuracao = scope.ServiceProvider.GetRequiredService<IGerenciadorConfiguracao>();
        var painelId = configuracao.ObterCredencialPainel();

        if (painelId != Guid.Empty)
            await sincronizar.ExecutarAsync(painelId, CancellationToken.None);
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
