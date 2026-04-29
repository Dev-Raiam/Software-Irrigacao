using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WorkerService.Configurations;
using WorkerService.Infrastructure.SeedData;
using WorkerService.Presentation.Endpoints;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

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

await SeedData.Seed(builder.Services.BuildServiceProvider());

builder.Services.RegistrarServicos(builder.Configuration);
builder.Services.RegistrarAuthenticacao();
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
