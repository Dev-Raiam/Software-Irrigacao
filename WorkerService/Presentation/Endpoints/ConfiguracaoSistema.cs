using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WorkerService.Features.Configuracao.ConfiguracaoSistema;
using WorkerService.Presentation.DTOs;

namespace WorkerService.Presentation.Endpoints;

public static class ConfiguracaoSistema
{
    public static void MapCredencial(this IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/configuracao-sistema",
                async (
                    [FromBody] Configuracao configuracao,
                    ConfigurarSistema configurarSistema,
                    CancellationToken cancellationToken
                ) =>
                {
                    var sucesso = await configurarSistema.Executar(
                        configuracao.ContaId,
                        configuracao.TopicoConfiguracao,
                        configuracao.Integracao.Chave,
                        configuracao.Integracao.Segredo,
                        configuracao.Integracao.ContextoId,
                        cancellationToken
                    );

                    if (!sucesso)
                        return Results.Problem(
                            "Erro ao salvar configuração",
                            statusCode: StatusCodes.Status500InternalServerError
                        );

                    return Results.Ok();
                }
            )
            .RequireAuthorization();
    }
}
