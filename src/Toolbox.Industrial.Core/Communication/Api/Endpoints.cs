using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages.Commands;
using Toolbox.Industrial.Core.Security;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using SerilogConfig = Toolbox.Industrial.Core.Setup.Configuration;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Toolbox.Industrial.Core.Communication.Api;

public static class Endpoints
{
    public static string RateLimitingPolicy = "limite-tentativas";

    public static void RegisterEndpoints(this WebApplication app)
    {
        app.UseHsts();
        app.UseHttpsRedirection();
        app.UseRateLimiter();
        //app.UseJwksDiscovery();
        app.UseAuthentication();
        app.UseAuthorization();

        #region endpoints de configuracao

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
                    CancellationToken cancellationToken
                ) =>
                {
                    var json = System.Text.Json.JsonSerializer.Serialize(cfg);
                    var config = store.FirstOrDefault<Configuracao>(x =>
                        x.Id == Entity.Keys.Serilog.Config
                    );

                    if (config == null)
                    {
                        config = new Configuracao(
                            id: Entity.Keys.Serilog.Config,
                            configuracao: json,
                            grupo: Grupo.Log,
                            tipo: Tipo.Config
                        );
                    }
                    else
                    {
                        config.Atualizar(json);
                    }
                    await store.UpsertAsync(config);
                    Environment.Exit(1);
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

        #endregion endpoints de configuracao

        #region endpoints de sistema


        //app.MapGet(
        //        "/system/security/{Guid:id}",
        //        async ([FromServices] IEntityStore store, CancellationToken cancellationToken, Guid id) =>
        //        {
        //            var certificate = store
        //                .FirstOrDefault<Configuracao>(x => x.Id == id)
        //                ?.Valor as Certificate;

        //            if (certificate == null)
        //            {
        //                return Results.NotFound();
        //            }

        //            return Results.Ok(certificate);
        //        }
        //    )
        //    .RequireAuthorization()
        //    .RequireRateLimiting(RateLimitingPolicy);

        app.MapGet(
                "/system/logs",
                async ([FromServices] IEntityStore store, CancellationToken cancellationToken) =>
                {
                    var logs = store
                        .Query<Serilog.Events.LogEvent>("logs").ToList();
                    
                    return Results.Ok(logs.OrderByDescending(x => x.Timestamp));
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapPost(
                "/system/restart",
                async (
                    CancellationToken cancellationToken
                ) =>
                {
                    Environment.Exit(1);
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
                            await process.WaitForExitAsync();
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
                            await process.WaitForExitAsync();
                            result = Results.Accepted("O dispositivo será desligado.");
                        }
                    });
                    return result;
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        #endregion endpoints de sistema
    }
}
