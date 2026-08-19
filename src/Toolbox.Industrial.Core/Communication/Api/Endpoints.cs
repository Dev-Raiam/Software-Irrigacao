using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Serilog;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Messages.Commands;
using Toolbox.Industrial.Core.Security;
using Toolbox.Industrial.Core.Setup;
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
        app.Use(
            async (context, next) =>
            {
                if (!context.Request.IsHttps)
                {
                    // Permitir em HTTP
                    if (
                        context.Request.Path.StartsWithSegments(
                            "/system/security/certificate-authority"
                        )
                    )
                    {
                        await next();
                        return;
                    }

                    var host = context.Request.Host.Host;

                    var location =
                        $"https://{host}{context.Request.Path}{context.Request.QueryString}";

                    context.Response.Redirect(location, permanent: false);
                    return;
                }
                await next();
            }
        );
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();

        #region endpoints de configuracao

        app.Use(
            async (context, next) =>
            {
                if (!context.Request.IsHttps)
                {
                    if (
                        !context.Request.Path.StartsWithSegments(
                            "/system/security/certificate-authority"
                        )
                    )
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }
                }
                await next();
            }
        );

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
                    var config = await store.ObterConfiguracao<string>(Entity.Keys.Serilog.Config);

                    if (string.IsNullOrWhiteSpace(config))
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
                    var config = await store.ObterConfiguracao<Configuracao>(
                        Entity.Keys.Serilog.Config
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

                    Log.Warning(
                        $"Aplicação será reiniciada para completar a configuração dos logs."
                    );
                    await Application.Restart();
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


        app.MapGet(
                "/system/security/certificate-authority/{id:guid}",
                async (
                    [FromServices] IEntityStore store,
                    Guid id,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = store.GetCertificate<Certificate>(
                        Entity.Keys.Security.CertificateAuthority,
                        subject: "localhost"
                    );
                    if (result == null)
                    {
                        return Results.NotFound();
                    }
                    return Results.Ok(result);
                }
            )
            .RequireHost("*:80")
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapGet(
                "/system/logs",
                async ([FromServices] IEntityStore store, CancellationToken cancellationToken) =>
                {
                    var logs = store
                        .Query<Dictionary<string, object>>("logs")
                        .OrderByDescending("_t")
                        .ToList();

                    return Results.Ok(logs);
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitingPolicy);

        app.MapPost(
                "/system/restart",
                async (
                    CancellationToken cancellationToken,
                    [FromServices] IHostApplicationLifetime _lifetime
                ) =>
                {
                    Log.Warning($"Aplicação será reiniciada através de uma solicitação.");
                    await Application.Restart();
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
                        Log.Warning($"O dispositivo será reiniciado através de uma solicitação.");
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
                        Log.Warning($"O dispositivo será desligado através de uma solicitação.");
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
