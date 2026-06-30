using Microsoft.AspNetCore.Builder;
using Serilog;
using SoftwareIrrigacao.Data;
using SoftwareIrrigacao.Setup;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.AddSerilogConfiguration();
builder.Services.AddConfiguration(builder);
builder.Services.AddRegisterServices(configuration);
builder.Services.AddRegisterModulos(configuration);
builder.Services.AddRegisterWorkers();
builder.Services.AddJwtConfiguration();

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
