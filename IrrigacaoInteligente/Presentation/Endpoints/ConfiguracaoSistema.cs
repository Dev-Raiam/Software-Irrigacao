using System.Net;
using IrrigacaoInteligente.Features.Configuracao.ConfiguracaoSistema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Toolbox.Core.Mediator;

namespace IrrigacaoInteligente.Presentation.Endpoints;

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
