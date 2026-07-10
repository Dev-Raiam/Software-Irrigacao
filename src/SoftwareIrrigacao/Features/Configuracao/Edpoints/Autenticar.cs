using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Toolbox.Automacao.Core.Services;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace SoftwareIrrigacao.Features.Configuracao.Edpoints;

public static class Autenticar
{
    public class Credencial : Toolbox.Core.Messages.Command
    {
        [Required(ErrorMessage = "Chave é obrigatório")]
        public string Chave { get; init; } = null!;

        [Required(ErrorMessage = "Segredo é obrigatório")]
        public string Segredo { get; init; } = null!;

        [Required(ErrorMessage = "ContextoId é obrigatório")]
        public Guid ContextoId { get; init; }

        [Required(ErrorMessage = "PainelId é obrigatório")]
        public Guid PainelId { get; init; }
    }

    public class Handler : ICommandHandler<Credencial>
    {
        private readonly IMediator _mediator;
        private readonly IGerenciadorConfiguracao _gerenciadorConfiguracao;

        public Handler(IMediator mediator, IGerenciadorConfiguracao gerenciadorConfiguracao)
        {
            _mediator = mediator;
            _gerenciadorConfiguracao = gerenciadorConfiguracao;
        }

        public async Task<ResponseResult> Handle(
            Credencial request,
            CancellationToken cancellationToken
        )
        {
            _gerenciadorConfiguracao.AdicionarCredenciais(
                new Toolbox.Automacao.Core.Services.Credencial(
                    request.Chave, 
                    request.Segredo,
                    request.ContextoId,
                    request.PainelId)
            );

            return ResponseResult.Result(HttpStatusCode.OK);
        }
    }

    public static void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/software-irrigacao/autenticar",
                async (
                    [FromBody] Credencial command,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken
                ) =>
                {
                    var resposta = await mediator.Execute(
                        command,
                        cancellationToken: cancellationToken
                    );

                    return Results.Json(resposta, statusCode: (int)resposta.HttpStatusCode);
                }
            )
            .RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }
}
