using Microsoft.AspNetCore.Builder;
using Serilog;
using SoftwareIrrigacao.Infra.Data;
using SoftwareIrrigacao.Setup;
using Toolbox.Automacao.Core.Services;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilogConfiguration(builder);
builder.Services.AddConfiguration(builder);
builder.Services.AddRegisterServices();
builder.Services.AddJwtConfiguration();

var scoped = builder.Services.BuildServiceProvider();
var sincronizar = scoped.GetRequiredService<ISincronizarControladores>();

await sincronizar.ExecutarAsync(Guid.Parse("c0f34ad2-6725-48fd-b68e-29f98dd9092d"), CancellationToken.None);

var app = builder.Build();

app.UseConfig();

await SeedData.Seed(app.Services);

try
{
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
