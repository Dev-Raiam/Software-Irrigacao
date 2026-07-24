using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Toolbox.Industrial.Core.Messages.Commands;
using Toolbox.Core.Mediator;

namespace SoftwareIrrigacao.Edpoints;

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
            .RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }
}
