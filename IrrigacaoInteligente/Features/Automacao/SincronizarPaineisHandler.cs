// using IrrigacaoInteligente.Features.Shared.Extensions;
// using IrrigacaoInteligente.Features.Sincronizacao.Automacao.Interfaces;
// using IrrigacaoInteligente.Infrastructure.Data;
// using IrrigacaoInteligente.State;
// using Microsoft.EntityFrameworkCore;
// using Toolbox.Core.Api.Data;
// using Toolbox.Core.Mediator;
// using Toolbox.Core.Messages;

// namespace IrrigacaoInteligente.Features.Sincronizacao.Automacao;

// public class SincronizarPaineisCommand : Command { }

// public class SincronizarPainelHandler : CommandHandler, ICommandHandler<SincronizarPaineisCommand>
// {
//     private readonly IAutomacaoApi _automacaoApi;
//     private readonly CredenciaisAplicacao _credenciaisAplicacao;
//     private readonly IrrigacaoInteligenteContext _context;
//     private readonly ILogger<SincronizarPainelHandler> _logger;

//     public SincronizarPainelHandler(
//         IAutomacaoApi automacaoApi,
//         CredenciaisAplicacao credenciaisAplicacao,
//         IUnitOfWork<IrrigacaoInteligenteContext> uow,
//         ILogger<SincronizarPainelHandler> logger
//     )
//         : base(uow)
//     {
//         _automacaoApi = automacaoApi;
//         _credenciaisAplicacao = credenciaisAplicacao;
//         _context = uow.Context;
//         _logger = logger;
//     }

//     public async Task<ResponseResult> Handle(
//         SincronizarPaineisCommand request,
//         CancellationToken cancellationToken
//     )
//     {
//         var paineisResponse = await _automacaoApi.ObterPaineisAsync(
//             _credenciaisAplicacao.ContaId,
//             cancellationToken
//         );
//         if (paineisResponse is not null)
//         {
//             var paineis = paineisResponse.Select(p => p.ToEntity()).ToList();

//             foreach (var painel in paineis)
//             {
//                 var painelExistente = await _context
//                     .Paineis.Include(p => p.Modulos)
//                     .FirstOrDefaultAsync(p => p.Id == painel.Id, cancellationToken);

//                 if (painelExistente == null)
//                 {
//                     await _context.Paineis.AddAsync(painel, cancellationToken);
//                 }
//                 else
//                 {
//                     painelExistente.Atualizar(
//                         painel.Arquivado,
//                         painel.Primario,
//                         painel.Descricao,
//                         painel.Referencia,
//                         painel.Modulos
//                     );
//                 }
//             }

//             try
//             {
//                 var salvar = await _context.SaveChangesAsync(cancellationToken);
//                 if (salvar > 0)
//                     _logger.LogInformation("Paineis sincronizados");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError(ex, "Erro ao salvar paineis");
//             }
//         }
//         return Ok<ResponseResult>();
//     }
// }
