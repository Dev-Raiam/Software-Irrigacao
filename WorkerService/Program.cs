using Microsoft.AspNetCore.Builder;
using Serilog;
using WorkerService.Configurations;
using WorkerService.Presentation.Endpoints;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    //.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("System.Net.Http", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 7)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.RegistrarContexto();
builder.Services.RegistrarServicos(builder.Configuration);
builder.Services.AddSerilog();

var app = builder.Build();
app.MapCredencial();
try
{
    await app.RunAsync();
}
finally
{
    Log.CloseAndFlush();
}
