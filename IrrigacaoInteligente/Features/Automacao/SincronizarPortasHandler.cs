// using IrrigacaoInteligente.Features.Shared.Extensions;
// using IrrigacaoInteligente.Features.Sincronizacao.Automacao.Interfaces;
// using IrrigacaoInteligente.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;
// using Toolbox.Core.Api.Data;
// using Toolbox.Core.Mediator;
// using Toolbox.Core.Messages;

// namespace IrrigacaoInteligente.Features.Sincronizacao.Automacao;

// public class SincronizarPortasCommand : Command { }

// public class SincronizarPortasHandler : CommandHandler, ICommandHandler<SincronizarPortasCommand>
// {
//     private readonly IAutomacaoApi _automacaoApi;
//     private readonly IrrigacaoInteligenteContext _context;
//     private readonly ILogger<SincronizarPortasHandler> _logger;

//     public SincronizarPortasHandler(
//         IAutomacaoApi automacaoApi,
//         IUnitOfWork<IrrigacaoInteligenteContext> uow,
//         ILogger<SincronizarPortasHandler> logger
//     )
//         : base(uow)
//     {
//         _automacaoApi = automacaoApi;
//         _context = uow.Context;
//         _logger = logger;
//     }

//     public async Task<ResponseResult> Handle(
//         SincronizarPortasCommand request,
//         CancellationToken cancellationToken
//     )
//     {
//         var modulos = await _context.Modulos.AsNoTracking().ToListAsync(cancellationToken);
//         var controladores = modulos.Where(m => m.Controlador).ToList();
//         var modulosSemControlador = modulos.Where(m => !m.Controlador).ToList();

//         foreach (var controlador in controladores)
//         {
//             var portasResponse = await _automacaoApi.ObterPortasPorControladorAsync(
//                 controlador.PainelId,
//                 controlador.Id,
//                 cancellationToken
//             );

//             if (portasResponse is not null)
//             {
//                 foreach (var portaResponse in portasResponse)
//                 {
//                     var porta = portaResponse.ToEntity(controlador.Id);

//                     var portaExistente = await _context.Portas.FirstOrDefaultAsync(
//                         p => p.Id == porta.Id,
//                         cancellationToken
//                     );

//                     if (portaExistente is null)
//                     {
//                         await _context.Portas.AddAsync(porta, cancellationToken);
//                     }
//                     else
//                     {
//                         portaExistente.Atualizar(
//                             porta.DispositivoConectadoId,
//                             porta.Tipo,
//                             porta.Sinal,
//                             porta.Status,
//                             porta.Nome,
//                             porta.EnderecoBorne,
//                             porta.EnderecoLogico
//                         );
//                     }
//                 }
//             }
//         }

//         foreach (var modulo in modulosSemControlador)
//         {
//             var portasResponse = await _automacaoApi.ObterPortasPorModuloAsync(
//                 modulo.PainelId,
//                 modulo.Id,
//                 cancellationToken
//             );

//             if (portasResponse is not null)
//             {
//                 foreach (var portaResponse in portasResponse)
//                 {
//                     var porta = portaResponse.ToEntity(modulo.Id);

//                     var portaExistente = await _context.Portas.FirstOrDefaultAsync(
//                         p => p.Id == porta.Id,
//                         cancellationToken
//                     );

//                     if (portaExistente is null)
//                     {
//                         await _context.Portas.AddAsync(porta, cancellationToken);
//                     }
//                     else
//                     {
//                         portaExistente.Atualizar(
//                             porta.DispositivoConectadoId,
//                             porta.Tipo,
//                             porta.Sinal,
//                             porta.Status,
//                             porta.Nome,
//                             porta.EnderecoBorne,
//                             porta.EnderecoLogico
//                         );
//                     }
//                 }
//             }
//         }
//         try
//         {
//             var salvar = await _context.SaveChangesAsync(cancellationToken);
//             if (salvar > 0)
//                 _logger.LogInformation("Portas sincronizadas");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Erro ao salvar portas");
//         }

//         return Ok<ResponseResult>();
//     }
// }
