using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SoftwareIrrigacao.Infra.Cache;
using SoftwareIrrigacao.Infra.Data;
using System.ComponentModel.DataAnnotations;
using System.Net;
using Toolbox.Automacao.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace SoftwareIrrigacao.Features.Configuracao.Edpoints;

public static class AtualizarCredenciais
{
    public class Command : Toolbox.Core.Messages.Command
    {
        [Required(ErrorMessage = "ContaId é obrigatório")]
        public Guid ContaId { get; init; }

        [Required(ErrorMessage = "PainelId é obrigatório")]
        public Guid PainelId { get; init; }
    }

    public class Handler : ICommandHandler<Command>
    {
        private readonly CredenciaisAplicacao _credenciaisAplicacao;
        private readonly IrrigacaoDbContext _context;
        private readonly IMediator _mediator;

        public Handler(
            CredenciaisAplicacao credenciaisAplicacao,
            IrrigacaoDbContext context,
            IMediator mediator
        )
        {
            _credenciaisAplicacao = credenciaisAplicacao;
            _context = context;
            _mediator = mediator;
        }

        private void AtualizarCredenciaisAplicacao(Guid contaId, Guid painelId)
        {
            _credenciaisAplicacao.AdicionarConta(contaId);
            _credenciaisAplicacao.AdicionarPainel(painelId);
        }

        public async Task<ResponseResult> Handle(
            Command request,
            CancellationToken cancellationToken = default
        )
        {
            var conta = await _context.Set<Toolbox.Automacao.Core.Models.Configuracao>().FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.ContaId,
                cancellationToken
            );

            var painel = await _context.Set<Toolbox.Automacao.Core.Models.Configuracao>().FirstOrDefaultAsync(
                x => x.Chave == ChavesBanco.Padrao.PainelId,
                cancellationToken
            );

            if (conta == null || painel == null)
            {
                return ResponseResult
                    .Result(HttpStatusCode.NotFound)
                    .AddError("Conta ou Painel não Cadastrados");
            }

            conta.Atualizar(request.ContaId.ToString());
            painel.Atualizar(request.PainelId.ToString());

            AtualizarCredenciaisAplicacao(request.ContaId, request.PainelId);

            await _context.SaveChangesAsync(cancellationToken);

            return ResponseResult.Result(HttpStatusCode.OK);
        }
    }

    public static void Endpoint(IEndpointRouteBuilder app)
    {
        app.MapPut(
                "/configuracao/credenciais",
                async (
                    [FromBody] Command command,
                    [FromServices] IMediator mediator,
                    CancellationToken cancellationToken
                ) => await mediator.Execute(command, cancellationToken: cancellationToken)
            )
            .RequireAuthorization()
            .RequireRateLimiting("limite-tentativas");
    }
}
