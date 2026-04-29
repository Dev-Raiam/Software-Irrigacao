using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Toolbox.Core.Mediator;
using WorkerService.Features.Configuracao.ConfiguracaoSistema;

namespace WorkerService.Presentation.Endpoints;

public static class ConfiguracaoSistema
{
    public static void MapCredencial(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/configuracao-sistema/credenciais",
                async (
                    [FromBody] AdicionarCredenciais command,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken
                ) =>
                {
                    return await mediator.Execute(command, cancellationToken: cancellationToken);
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }

    public static void MapAtualizarConta(this IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/configuracao-sistema/credenciais",
                async (
                    [FromBody] AtualizarCredenciais command,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken
                ) =>
                {
                    return await mediator.Execute(command, cancellationToken: cancellationToken);
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }
}
