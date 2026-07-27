using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages.Commands;
using SerilogConfig = Toolbox.Industrial.Core.Setup.Configuration;

namespace Toolbox.Industrial.Core.Communication.Api;

public static class Endpoints
{
    public static void AuthRegister(this IEndpointRouteBuilder app)
    {
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
            //.RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");

        app.MapGet(
                "/configuracao/logs",
                async (
                    [FromServices] IEntityStore store,
                    CancellationToken cancellationToken
                ) =>
                {
                    var config = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Serilog.Config)?.Value?.ToString();

                    if (config == null)
                    {
                        return Results.Ok(new SerilogConfig());
                    }

                    return Results.Ok(System.Text.Json.JsonSerializer.Deserialize<SerilogConfig>(config));
                }
            )
            //.RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");

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
                    var config = store
                        .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Serilog.Config);

                    if (config == null)
                    {
                        config = new Configuracao(id: Entity.Keys.Serilog.Config, value: json);
                    }
                    else
                    {
                        config.Update(json);
                    }
                    await store.UpsertAsync(config);
                    lifetime.StopApplication();
                    return Results.Ok(new string[] { "Configuração realizada com sucesso.", "Aplicação será encerrada." });
                }
            )
            //.RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");

        app.MapPost(
                "/system/restart",
                async (
                    [FromServices] IHostApplicationLifetime lifetime,
                    CancellationToken cancellationToken
                ) =>
                {
                    lifetime.StopApplication();
                    return Results.Ok("Aplicação será encerrada.");
                }
            )
            //.RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");

        app.MapPost(
                "/system/reboot",
                async () =>
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);

                        var process = Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = "systemctl",
                                ArgumentList = { "reboot" },
                                RedirectStandardOutput = true,
                                RedirectStandardError = true,
                                UseShellExecute = false,
                            }
                        );

                        await process!.WaitForExitAsync();
                    });

                    return Results.Accepted("O dispositivo será reiniciado.");
                }
            )
            //.RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");

        app.MapPost(
                "/system/shutdown",
                async () =>
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(1000);

                        var process = Process.Start(
                            new ProcessStartInfo
                            {
                                FileName = "systemctl",
                                ArgumentList = { "poweroff" },
                                UseShellExecute = false,
                            }
                        );

                        await process!.WaitForExitAsync();
                    });

                    return Results.Accepted("O dispositivo será desligado.");
                }
            )
            //.RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }
}
