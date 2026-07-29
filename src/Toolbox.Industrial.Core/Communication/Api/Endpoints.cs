using System.Diagnostics;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages.Commands;
using SerilogConfig = Toolbox.Industrial.Core.Setup.Configuration;

namespace Toolbox.Industrial.Core.Communication.Api;

public static class Endpoints
{
    public static string RateLimitingPolicy = "limite-tentativas";

    public static void RegisterEndpoints(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
        app.MapPost(
                "/configuracao/credenciais",
                async (
                    [FromBody] RegistrarCredenciais command,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken
                ) =>
                {
                    var response = await mediator.Execute(
                        command,
                        cancellationToken: cancellationToken
                    );

                    return Results.Json(response, statusCode: (int)response.HttpStatusCode);
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapGet(
                "/configuracao/logs",
                async ([FromServices] IEntityStore store, CancellationToken cancellationToken) =>
                {
                    var config = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Serilog.Config)
                        ?.Valor?.ToString();

                    if (config == null)
                    {
                        return Results.Ok(new SerilogConfig());
                    }

                    return Results.Ok(
                        System.Text.Json.JsonSerializer.Deserialize<SerilogConfig>(config)
                    );
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapPost(
                "/configuracao/logs",
                async (
                    [FromBody] SerilogConfig cfg,
                    [FromServices] IEntityStore store,
                    [FromServices] IHostApplicationLifetime lifetime,
                    CancellationToken cancellationToken
                ) =>
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(cfg);
                    var config = store.FirstOrDefault<Configuracao>(x =>
                        x.Id == Entity.Keys.Serilog.Config
                    );

                    if (config == null)
                    {
                        config = new Configuracao(id: Entity.Keys.Serilog.Config, configuracao: json);
                    }
                    else
                    {
                        config.Atualizar(json);
                    }
                    await store.UpsertAsync(config);
                    lifetime.StopApplication();
                    return Results.Accepted(
                        value: new string[]
                        {
                            "Configuração realizada com sucesso.",
                            "Aplicação será reiniciada.",
                        }
                    );
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapPost(
                "/system/restart",
                async (
                    [FromServices] IHostApplicationLifetime lifetime,
                    CancellationToken cancellationToken
                ) =>
                {
                    lifetime.StopApplication();
                    return Results.Accepted(value: "Aplicação será reiniciada.");
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapPost(
                "/system/reboot",
                async () =>
                {
                    var result = Results.NoContent();
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        Process? process = null;
                        if (OperatingSystem.IsWindows())
                        {
                            process = Process.Start(
                                new ProcessStartInfo
                                {
                                    FileName = "shutdown",
                                    ArgumentList =
                                    {
                                        "/r", // Restart
                                        "/f", //Força o encerramento dos aplicativos.
                                        "/t",
                                        "0", // Sem atraso <segundos>
                                    },
                                    UseShellExecute = false,
                                }
                            );
                        }
                        else if (OperatingSystem.IsLinux())
                        {
                            process = Process.Start(
                                new ProcessStartInfo
                                {
                                    FileName = "systemctl",
                                    ArgumentList = { "reboot" },
                                    RedirectStandardOutput = true,
                                    RedirectStandardError = true,
                                    UseShellExecute = false,
                                }
                            );
                        }
                        if (process != null)
                        {
                            await process!.WaitForExitAsync();
                            result = Results.Accepted("O dispositivo será reiniciado.");
                        }
                    });
                    return result;
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapPost(
                "/system/shutdown",
                async () =>
                {
                    var result = Results.NoContent();
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);
                        Process? process = null;
                        if (OperatingSystem.IsWindows())
                        {
                            process = Process.Start(
                                new ProcessStartInfo
                                {
                                    FileName = "shutdown",
                                    ArgumentList =
                                    {
                                        "/s", // Shutdown
                                        "/f", //Força o encerramento dos aplicativos.
                                        "/t",
                                        "0", // Sem atraso <segundos>
                                    },
                                    UseShellExecute = false,
                                }
                            );
                        }
                        else if (OperatingSystem.IsLinux())
                        {
                            process = Process.Start(
                                new ProcessStartInfo
                                {
                                    FileName = "systemctl",
                                    ArgumentList = { "poweroff" },
                                    UseShellExecute = false,
                                }
                            );
                        }
                        if (process != null)
                        {
                            await process!.WaitForExitAsync();
                            result = Results.Accepted("O dispositivo será desligado.");
                        }
                    });
                    return result;
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);
    }
}
