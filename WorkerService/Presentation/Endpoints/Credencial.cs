using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using WorkerService.Configurations;
using WorkerService.Infrastructure.Services;

namespace WorkerService.Presentation.Endpoints;

public static class Credencial
{
    public static void MapCredencial(this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/credenciais",
            async (
                ArmazenamentoCredenciais armazenamentoCredenciais,
                [FromBody] CredenciaisConfiguracao credenciais,
                CancellationToken cancellationToken
            ) =>
            {
                var topicoConfiguracaoSalvo =
                    await armazenamentoCredenciais.DefinirTopicoConfiguracao(
                        credenciais.TopicoConfiguracao,
                        cancellationToken
                    );
                var contaIdSalva = await armazenamentoCredenciais.DefinirContaId(
                    credenciais.ContaId,
                    cancellationToken
                );
                var integracaoSalva = await armazenamentoCredenciais.DefinirCredencialIntegracao(
                    credenciais.Integracao.Chave,
                    credenciais.Integracao.Segredo,
                    credenciais.Integracao.ContextoId,
                    cancellationToken
                );
                if (!contaIdSalva || !integracaoSalva || !topicoConfiguracaoSalvo)
                {
                    return Results.NotFound();
                }
                return Results.Ok();
            }
        );
    }
}
