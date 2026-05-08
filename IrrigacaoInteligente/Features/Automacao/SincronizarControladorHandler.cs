using System.Text.Json;
using IrrigacaoInteligente.Domain.Entities;
using IrrigacaoInteligente.Features.Automacao.Interfaces;
using IrrigacaoInteligente.Features.Shared.Extensions;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.State;
using Microsoft.EntityFrameworkCore;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Automacao;

// public class SincronizarControladorCommand : Command { }

// public class SincronizarControladorHandler
//     : CommandHandler,
//         ICommandHandler<SincronizarControladorCommand>
// {
//     private readonly IAutomacaoApi _automacaoApi;
//     private readonly IrrigacaoInteligenteContext _context;
//     private readonly ILogger<SincronizarControladorCommand> _logger;
//     private readonly CredenciaisAplicacao _credenciaisAplicacao;

//     public SincronizarControladorHandler(
//         IAutomacaoApi automacaoApi,
//         IUnitOfWork<IrrigacaoInteligenteContext> uow,
//         ILogger<SincronizarControladorCommand> logger,
//         CredenciaisAplicacao credenciaisAplicacao
//     )
//         : base(uow)
//     {
//         _automacaoApi = automacaoApi;
//         _context = uow.Context;
//         _logger = logger;
//         _credenciaisAplicacao = credenciaisAplicacao;
//     }

//     public async Task<ResponseResult> Handle(
//         SincronizarControladorCommand request,
//         CancellationToken cancellationToken
//     )
//     {
//         var controladores = await _automacaoApi.ObterControladoresPorPainelAsync(
//             _credenciaisAplicacao.PainelId,
//             cancellationToken
//         );

//         if (controladores is not null)
//         {
//             await _context.Controladores.ExecuteDeleteAsync(cancellationToken);

//             var controladoresDeserializados = JsonSerializer.Deserialize<
//                 List<Dictionary<string, object>>
//             >(controladores);

//             foreach (var controlador in controladoresDeserializados!)
//             {
//                 var controladorSerializado = JsonSerializer.Serialize(controlador);

//                 _context.Controladores.Add(
//                     new Controlador(
//                         Guid.Parse(controlador["id"].ToString()!),
//                         controladorSerializado
//                     )
//                 );
//             }

//             await _context.SaveChangesAsync(cancellationToken);
//         }

//         return Ok<ResponseResult>();
//     }
// }
