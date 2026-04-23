using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using WorkerService.Configurations;
using WorkerService.Infrastructure.SeedData;
using WorkerService.Presentation.Endpoints;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
    //.WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegistrarContexto();

await SeedData.Seed(builder.Services.BuildServiceProvider());

builder.Services.RegistrarServicos(builder.Configuration);
builder.Services.RegistrarAuthenticacao();
builder.Services.AddSerilog();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.MapCredencial();

try
{
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
