using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Services.Automacao.Autenticacao;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace SoftwareIrrigacao.Features.Configuracao.Edpoints;

public static class Autenticar
{
    public class Integracao : Toolbox.Core.Messages.Command
    {
        [Required(ErrorMessage = "Chave é obrigatório")]
        public string Chave { get; init; } = null!;

        [Required(ErrorMessage = "Segredo é obrigatório")]
        public string Segredo { get; init; } = null!;

        [Required(ErrorMessage = "ContextoId é obrigatório")]
        public Guid ContextoId { get; init; }
    }

    public class Handler : ICommandHandler<Integracao>
    {
        private readonly IMediator _mediator;
        private readonly IConfiguracaoAutenticacao _configuracaoAutenticacao;

        public Handler(IMediator mediator, IConfiguracaoAutenticacao configuracaoAutenticacao)
        {
            _mediator = mediator;
            _configuracaoAutenticacao = configuracaoAutenticacao;
        }

        public async Task<ResponseResult> Handle(
            Integracao request,
            CancellationToken cancellationToken
        )
        {
            var credencial = new Credencial(request.Chave, request.Segredo, request.ContextoId);

            _configuracaoAutenticacao.AdicionarCredenciais(credencial);

            return ResponseResult.Result(HttpStatusCode.OK);
        }
    }

    public static void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPost(
                "/software-irrigacao/autenticar",
                async (
                    [FromBody] Integracao command,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken
                ) => await mediator.Execute(command, cancellationToken: cancellationToken)
            )
            .RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }
}
