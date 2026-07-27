using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
    }
}
