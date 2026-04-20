// using Microsoft.EntityFrameworkCore;
// using WorkerService.Application.Extensions;
// using WorkerService.Configurations;
// using WorkerService.Domain.Entities;
// using WorkerService.Features.Shared.Extensions;
// using WorkerService.Infrastructure.Data;
// using WorkerService.Infrastructure.Interfaces;

// namespace WorkerService.Features.Sincronizacao.Automacao;

// // public sealed class Sincronizador(
// //     ApplicationDbContext _context,
// //     IAutomacaoApi _automacaoApi,
// //     ContaConfiguracao _contaConfiguracao
// // )
// // {
// //     public async Task SincronizarAsync(CancellationToken cancellationToken)
// //     {
// //         var painelIds = await SincronizarPainel(cancellationToken);

// //         if (painelIds.Count == 0)
// //         {
// //             return;
// //         }

// //         await SincronizarDispositivosPorPaineis(painelIds, cancellationToken);

// //         var listaModulos = await ObterModulos(cancellationToken);
// //         var controladores = listaModulos.Where(m => m.Controlador).ToList();
// //         var modulos = listaModulos.Where(m => !m.Controlador).ToList();

// //         await SincronizarPortasPorControlador(controladores, cancellationToken);
// //         await SincronizarPortasPorModulo(modulos, cancellationToken);
// //         await SincronizarInterfacesPorControlador(controladores, cancellationToken);
// //         await SincronizarInterfacesPorModulo(modulos, cancellationToken);
// //     }

//     // public async Task<List<Guid>> SincronizarPainel(CancellationToken cancellationToken)
//     // {
//     //     var painelIds = new List<Guid>();
//     //     var paineisResponse = await _automacaoApi.ObterPaineisAsync(
//     //         _contaConfiguracao.ContaId,
//     //         cancellationToken
//     //     );

//     //     if (paineisResponse is not null)
//     //     {
//     //         var paineis = paineisResponse.Select(p => p.ToEntity()).ToList();

//     //         foreach (var painel in paineis)
//     //         {
//     //             painelIds.Add(painel.Id);
//     //             var painelExistente = await _context
//     //                 .Paineis.Include(p => p.Modulos)
//     //                 .FirstOrDefaultAsync(p => p.Id == painel.Id, cancellationToken);
//     //             if (painelExistente == null)
//     //             {
//     //                 await _context.Paineis.AddAsync(painel, cancellationToken);
//     //             }
//     //             else
//     //             {
//     //                 painelExistente.Atualizar(
//     //                     painel.Arquivado,
//     //                     painel.Primario,
//     //                     painel.Descricao,
//     //                     painel.Referencia,
//     //                     painel.Modulos
//     //                 );
//     //             }
//     //         }

//     //         await _context.SaveChangesAsync(cancellationToken);
//     //     }
//     //     return painelIds;
//     // }

//     // public async Task SincronizarDispositivosPorPaineis(
//     //     List<Guid> painelIds,
//     //     CancellationToken cancellationToken
//     // )
//     // {
//     //     foreach (var painelId in painelIds)
//     //     {
//     //         var dispositivosResponse = await _automacaoApi.ObterDispositivosPorPainelAsync(
//     //             painelId,
//     //             cancellationToken
//     //         );

//     //         if (dispositivosResponse is not null)
//     //         {
//     //             foreach (var dispositivoResponse in dispositivosResponse)
//     //             {
//     //                 var dispositivo = dispositivoResponse.ToEntity(painelId);

//     //                 var dispositivoExistente = await _context.Dispositivos.FirstOrDefaultAsync(
//     //                     d => d.Id == dispositivo.Id,
//     //                     cancellationToken
//     //                 );

//     //                 if (dispositivoExistente is null)
//     //                 {
//     //                     await _context.Dispositivos.AddAsync(dispositivo, cancellationToken);
//     //                 }
//     //                 else
//     //                 {
//     //                     dispositivoExistente.Atualizar(
//     //                         dispositivo.Arquivado,
//     //                         dispositivo.Tipo,
//     //                         dispositivo.Parametros,
//     //                         dispositivo.Descricao
//     //                     );
//     //                 }
//     //             }
//     //         }

//     //         await _context.SaveChangesAsync(cancellationToken);
//     //     }
//     // }

//     // public async Task SincronizarPortasPorControlador(
//     //     List<Domain.Entities.Modulo> controladores,
//     //     CancellationToken cancellationToken
//     // )
//     // {
//     //     if (controladores is not null)
//     //     {
//     //         foreach (var controlador in controladores)
//     //         {
//     //             var portasResponse = await _automacaoApi.ObterPortasPorControladorAsync(
//     //                 controlador.PainelId,
//     //                 controlador.Id,
//     //                 cancellationToken
//     //             );

//     //             if (portasResponse is not null)
//     //             {
//     //                 foreach (var portaResponse in portasResponse)
//     //                 {
//     //                     var porta = portaResponse.ToEntity(controlador.Id);

//     //                     var portaExistente = await _context.Portas.FirstOrDefaultAsync(
//     //                         p => p.Id == porta.Id,
//     //                         cancellationToken
//     //                     );

//     //                     if (portaExistente is null)
//     //                     {
//     //                         await _context.Portas.AddAsync(porta, cancellationToken);
//     //                     }
//     //                     else
//     //                     {
//     //                         portaExistente.Atualizar(
//     //                             porta.DispositivoConectadoId,
//     //                             porta.Tipo,
//     //                             porta.Sinal,
//     //                             porta.Status,
//     //                             porta.Nome,
//     //                             porta.EnderecoBorne,
//     //                             porta.EnderecoLogico
//     //                         );
//     //                     }
//     //                 }
//     //             }

//     //             await _context.SaveChangesAsync(cancellationToken);
//     //         }
//     //     }
//     // }

//     // public async Task SincronizarPortasPorModulo(
//     //     List<Domain.Entities.Modulo> modulos,
//     //     CancellationToken cancellationToken
//     // )
//     // {
//     //     if (modulos is not null)
//     //     {
//     //         foreach (var modulo in modulos)
//     //         {
//     //             var portasResponse = await _automacaoApi.ObterPortasPorModuloAsync(
//     //                 modulo.PainelId,
//     //                 modulo.Id,
//     //                 cancellationToken
//     //             );

//     //             if (portasResponse is not null)
//     //             {
//     //                 foreach (var portaResponse in portasResponse)
//     //                 {
//     //                     var porta = portaResponse.ToEntity(modulo.Id);

//     //                     var portaExistente = await _context.Portas.FirstOrDefaultAsync(
//     //                         p => p.Id == porta.Id,
//     //                         cancellationToken
//     //                     );

//     //                     if (portaExistente is null)
//     //                     {
//     //                         await _context.Portas.AddAsync(porta, cancellationToken);
//     //                     }
//     //                     else
//     //                     {
//     //                         portaExistente.Atualizar(
//     //                             porta.DispositivoConectadoId,
//     //                             porta.Tipo,
//     //                             porta.Sinal,
//     //                             porta.Status,
//     //                             porta.Nome,
//     //                             porta.EnderecoBorne,
//     //                             porta.EnderecoLogico
//     //                         );
//     //                     }
//     //                 }
//     //             }

//     //             await _context.SaveChangesAsync(cancellationToken);
//     //         }
//     //     }
//     // }

//     // public async Task SincronizarInterfacesPorControlador(
//     //     List<Domain.Entities.Modulo> controladores,
//     //     CancellationToken cancellationToken
//     // )
//     // {
//     //     if (controladores is not null)
//     //     {
//     //         foreach (var controlador in controladores)
//     //         {
//     //             var interfaceResponses = await _automacaoApi.ObterInterfacesPorControladorAsync(
//     //                 controlador.PainelId,
//     //                 controlador.Id,
//     //                 cancellationToken
//     //             );

//     //             if (interfaceResponses is not null)
//     //             {
//     //                 foreach (var interfaceResponse in interfaceResponses)
//     //                 {
//     //                     var interfaceEntity = interfaceResponse.ToEntity(controlador.Id);

//     //                     var interfaceExistente = await _context.Interfaces.FirstOrDefaultAsync(
//     //                         i => i.Id == interfaceEntity.Id,
//     //                         cancellationToken
//     //                     );

//     //                     if (interfaceExistente is null)
//     //                     {
//     //                         await _context.Interfaces.AddAsync(interfaceEntity, cancellationToken);
//     //                     }
//     //                     else
//     //                     {
//     //                         interfaceExistente.Atualizar(
//     //                             controlador.Id,
//     //                             interfaceEntity.ModuloConectadoId,
//     //                             interfaceEntity.Tipo,
//     //                             interfaceEntity.Status,
//     //                             interfaceEntity.Marca,
//     //                             interfaceEntity.Modelo,
//     //                             interfaceEntity.Categoria,
//     //                             interfaceEntity.IndiceModbus,
//     //                             interfaceEntity.EnderecoModbus,
//     //                             interfaceEntity.Porta,
//     //                             interfaceEntity.EnderecoBorne,
//     //                             interfaceEntity.EnderecoLogico
//     //                         );
//     //                     }
//     //                 }
//     //             }

//     //             await _context.SaveChangesAsync(cancellationToken);
//     //         }
//     //     }
//     // }

//     // public async Task SincronizarInterfacesPorModulo(
//     //     List<Domain.Entities.Modulo> modulos,
//     //     CancellationToken cancellationToken
//     // )
//     // {
//     //     if (modulos is not null)
//     //     {
//     //         foreach (var modulo in modulos)
//     //         {
//     //             var interfaceResponses = await _automacaoApi.ObterInterfacesPorModuloAsync(
//     //                 modulo.PainelId,
//     //                 modulo.Id,
//     //                 cancellationToken
//     //             );

//     //             if (interfaceResponses is not null)
//     //             {
//     //                 foreach (var interfaceResponse in interfaceResponses)
//     //                 {
//     //                     var interfaceEntity = interfaceResponse.ToEntity(modulo.Id);

//     //                     var interfaceExistente = await _context.Interfaces.FirstOrDefaultAsync(
//     //                         i => i.Id == interfaceEntity.Id,
//     //                         cancellationToken
//     //                     );

//     //                     if (interfaceExistente is null)
//     //                     {
//     //                         await _context.Interfaces.AddAsync(interfaceEntity, cancellationToken);
//     //                     }
//     //                     else
//     //                     {
//     //                         interfaceExistente.Atualizar(
//     //                             modulo.Id,
//     //                             interfaceEntity.ModuloConectadoId,
//     //                             interfaceEntity.Tipo,
//     //                             interfaceEntity.Status,
//     //                             interfaceEntity.Marca,
//     //                             interfaceEntity.Modelo,
//     //                             interfaceEntity.Categoria,
//     //                             interfaceEntity.IndiceModbus,
//     //                             interfaceEntity.EnderecoModbus,
//     //                             interfaceEntity.Porta,
//     //                             interfaceEntity.EnderecoBorne,
//     //                             interfaceEntity.EnderecoLogico
//     //                         );
//     //                     }
//     //                 }
//     //             }

//     //             await _context.SaveChangesAsync(cancellationToken);
//     //         }
//     //     }
//     // }

//     #region Repository
//     public async Task<List<Domain.Entities.Modulo>> ObterModulos(
//         CancellationToken cancellationToken
//     )
//     {
//         return await _context.Modulos.AsNoTracking().ToListAsync(cancellationToken);
//     }
//     #endregion
// }
