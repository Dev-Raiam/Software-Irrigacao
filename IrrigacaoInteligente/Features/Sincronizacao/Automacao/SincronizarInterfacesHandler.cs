// using IrrigacaoInteligente.Features.Shared.Extensions;
// using IrrigacaoInteligente.Features.Sincronizacao.Automacao.Interfaces;
// using IrrigacaoInteligente.Infrastructure.Data;
// using Microsoft.EntityFrameworkCore;
// using Toolbox.Core.Api.Data;
// using Toolbox.Core.Mediator;
// using Toolbox.Core.Messages;

// namespace IrrigacaoInteligente.Features.Sincronizacao.Automacao;

// public class SincronizarInterfacesCommand : Command { }

// public class SincronizarInterfacesHandler
//     : CommandHandler,
//         ICommandHandler<SincronizarInterfacesCommand>
// {
//     private readonly IAutomacaoApi _automacaoApi;
//     private readonly IrrigacaoInteligenteContext _context;
//     private readonly ILogger<SincronizarInterfacesHandler> _logger;

//     public SincronizarInterfacesHandler(
//         IAutomacaoApi automacaoApi,
//         IUnitOfWork<IrrigacaoInteligenteContext> uow,
//         ILogger<SincronizarInterfacesHandler> logger
//     )
//         : base(uow)
//     {
//         _automacaoApi = automacaoApi;
//         _context = uow.Context;
//         _logger = logger;
//     }

//     public async Task<ResponseResult> Handle(
//         SincronizarInterfacesCommand request,
//         CancellationToken cancellationToken
//     )
//     {
//         var modulosDinamicos = await _context.Modulos.AsNoTracking().ToListAsync(cancellationToken);
//         var controladores = modulosDinamicos.Where(m => m.Controlador).ToList();
//         var modulos = modulosDinamicos.Where(m => !m.Controlador).ToList();

//         foreach (var controlador in controladores)
//         {
//             var interfaceResponses = await _automacaoApi.ObterInterfacesPorControladorAsync(
//                 controlador.PainelId,
//                 controlador.Id,
//                 cancellationToken
//             );

//             if (interfaceResponses is not null)
//             {
//                 foreach (var interfaceResponse in interfaceResponses)
//                 {
//                     var interfaceEntity = interfaceResponse.ToEntity(controlador.Id);

//                     var interfaceExistente = await _context.Interfaces.FirstOrDefaultAsync(
//                         i => i.Id == interfaceEntity.Id,
//                         cancellationToken
//                     );

//                     if (interfaceExistente is null)
//                     {
//                         await _context.Interfaces.AddAsync(interfaceEntity, cancellationToken);
//                     }
//                     else
//                     {
//                         interfaceExistente.Atualizar(
//                             controlador.Id,
//                             interfaceEntity.ModuloConectadoId,
//                             interfaceEntity.Tipo,
//                             interfaceEntity.Status,
//                             interfaceEntity.Marca,
//                             interfaceEntity.Modelo,
//                             interfaceEntity.Categoria,
//                             interfaceEntity.IndiceModbus,
//                             interfaceEntity.EnderecoModbus,
//                             interfaceEntity.Porta,
//                             interfaceEntity.EnderecoBorne,
//                             interfaceEntity.EnderecoLogico
//                         );
//                     }
//                 }
//             }
//         }

//         foreach (var modulo in modulos)
//         {
//             var interfaceResponses = await _automacaoApi.ObterInterfacesPorModuloAsync(
//                 modulo.PainelId,
//                 modulo.Id,
//                 cancellationToken
//             );

//             if (interfaceResponses is not null)
//             {
//                 foreach (var interfaceResponse in interfaceResponses)
//                 {
//                     var interfaceEntity = interfaceResponse.ToEntity(modulo.Id);

//                     var interfaceExistente = await _context.Interfaces.FirstOrDefaultAsync(
//                         i => i.Id == interfaceEntity.Id,
//                         cancellationToken
//                     );

//                     if (interfaceExistente is null)
//                     {
//                         await _context.Interfaces.AddAsync(interfaceEntity, cancellationToken);
//                     }
//                     else
//                     {
//                         interfaceExistente.Atualizar(
//                             modulo.Id,
//                             interfaceEntity.ModuloConectadoId,
//                             interfaceEntity.Tipo,
//                             interfaceEntity.Status,
//                             interfaceEntity.Marca,
//                             interfaceEntity.Modelo,
//                             interfaceEntity.Categoria,
//                             interfaceEntity.IndiceModbus,
//                             interfaceEntity.EnderecoModbus,
//                             interfaceEntity.Porta,
//                             interfaceEntity.EnderecoBorne,
//                             interfaceEntity.EnderecoLogico
//                         );
//                     }
//                 }
//             }
//         }
//         try
//         {
//             var salvar = await _context.SaveChangesAsync(cancellationToken);
//             if (salvar > 0)
//                 _logger.LogInformation("Interfaces sincronizadas");
//         }
//         catch (Exception ex)
//         {
//             _logger.LogError(ex, "Erro ao salvar interfaces");
//         }

//         return Ok<ResponseResult>();
//     }
// }
