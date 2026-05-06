// using IrrigacaoInteligente.Features.Shared.Extensions;
// using IrrigacaoInteligente.Features.Sincronizacao.Automacao.Interfaces;
// using IrrigacaoInteligente.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;
// using Toolbox.Core.Api.Data;
// using Toolbox.Core.Mediator;
// using Toolbox.Core.Messages;

// namespace IrrigacaoInteligente.Features.Sincronizacao.Automacao;

// public class SincronizarDispositivosCommand : Command { }

// public class SincronizarDispositivosHandler
//     : CommandHandler,
//         ICommandHandler<SincronizarDispositivosCommand>
// {
//     private readonly IAutomacaoApi _automacaoApi;
//     private readonly IrrigacaoInteligenteContext _context;
//     private readonly ILogger<SincronizarDispositivosHandler> _logger;

//     public SincronizarDispositivosHandler(
//         IAutomacaoApi automacaoApi,
//         IUnitOfWork<IrrigacaoInteligenteContext> uow,
//         ILogger<SincronizarDispositivosHandler> logger
//     )
//         : base(uow)
//     {
//         _automacaoApi = automacaoApi;
//         _context = uow.Context;
//         _logger = logger;
//     }

//     public async Task<ResponseResult> Handle(
//         SincronizarDispositivosCommand request,
//         CancellationToken cancellationToken
//     )
//     {
//         var paineis = await _context.Paineis.AsNoTracking().ToListAsync(cancellationToken);

//         foreach (var painel in paineis)
//         {
//             var dispositivosResponse = await _automacaoApi.ObterDispositivosPorPainelAsync(
//                 painel.Id,
//                 cancellationToken
//             );

//             if (dispositivosResponse is not null)
//             {
//                 foreach (var dispositivoResponse in dispositivosResponse)
//                 {
//                     var dispositivo = dispositivoResponse.ToEntity(painel.Id);

//                     var dispositivoExistente = await _context.Dispositivos.FirstOrDefaultAsync(
//                         d => d.Id == dispositivo.Id,
//                         cancellationToken
//                     );

//                     if (dispositivoExistente is null)
//                     {
//                         await _context.Dispositivos.AddAsync(dispositivo, cancellationToken);
//                     }
//                     else
//                     {
//                         dispositivoExistente.Atualizar(
//                             dispositivo.Arquivado,
//                             dispositivo.Tipo,
//                             dispositivo.Parametros,
//                             dispositivo.Descricao
//                         );
//                     }
//                 }
//             }
//         }
//         try
//         {
//             var salvar = await _context.SaveChangesAsync(cancellationToken);
//             if (salvar > 0)
//                 _logger.LogInformation("Dispositivos sincronizados");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Erro ao salvar dispositivos");
//         }

//         return Ok<ResponseResult>();
//     }
// }
