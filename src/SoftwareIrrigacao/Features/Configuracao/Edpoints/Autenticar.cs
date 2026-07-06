using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
        private readonly ILogger<Handler> _logger;

        public Handler(
            IMediator mediator,
            ILogger<Handler> logger,
            IConfiguracaoAutenticacao configuracaoAutenticacao
        )
        {
            _mediator = mediator;
            _logger = logger;
            _configuracaoAutenticacao = configuracaoAutenticacao;
        }

        public async Task<ResponseResult> Handle(
            Integracao request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                _configuracaoAutenticacao.AdicionarCredenciais(
                    new Credencial(request.Chave, request.Segredo, request.ContextoId)
                );
            }
            catch (IOException ex)
            {
                _logger.LogError(
                    "O processo não pode acessar o arquivo do banco de dados. {ex}",
                    ex.Message
                );

                return ResponseResult
                    .Result(HttpStatusCode.NotFound)
                    .AddError("erro ao salvar dados");
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro desconhecido. {ex}", ex.Message);

                return ResponseResult
                    .Result(HttpStatusCode.InternalServerError)
                    .AddError("erro desconhecido");
            }

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
