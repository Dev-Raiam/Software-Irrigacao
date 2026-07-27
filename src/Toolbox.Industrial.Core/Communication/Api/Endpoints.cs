using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Messages.Commands;

namespace Toolbox.Industrial.Core.Communication.Api;

public static class Endpoints
{

    public static void AuthRegister(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/software-irrigacao/credenciais",
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

        app.MapPost("/system/reboot", async () =>
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "systemctl",
                    ArgumentList = { "reboot" },
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                });

                await process!.WaitForExitAsync();
            });

            return Results.Accepted("O dispositivo será reiniciado.");
        })            
        //.RequireAuthorization()
        .RequireRateLimiting("limite-tentativas");

        app.MapPost("/system/shutdown", async () =>
        {
            _ = Task.Run(async () =>
            {
                await Task.Delay(1000);

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "systemctl",
                    ArgumentList = { "poweroff" },
                    UseShellExecute = false
                });

                await process!.WaitForExitAsync();
            });

            return Results.Accepted("O dispositivo será reiniciado.");
        })
            //.RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }
}
