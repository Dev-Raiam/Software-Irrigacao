using System.Threading.RateLimiting;
using IrrigacaoInteligente.Configurations;
using IrrigacaoInteligente.Infrastructure.SeedData;
using IrrigacaoInteligente.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;

//Directory.SetCurrentDirectory(AppContext.BaseDirectory);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegistrarContexto();
builder.Services.RegistrarAuthenticacao();
builder.Services.RegistrarServicos(builder.Configuration);
builder.Services.AddSerilog();
builder.Services.AddRateLimiter(options =>
{
    options.AddConcurrencyLimiter(
        "limite-tentativas",
        options =>
        {
            options.PermitLimit = 5;
            options.QueueLimit = 5;
            options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        }
    );
});

var app = builder.Build();

await SeedData.Seed(app.Services);

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapCredencial();
app.MapAtualizarConta();

try
{
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
