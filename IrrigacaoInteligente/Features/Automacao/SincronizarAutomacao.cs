// using Toolbox.Core.Mediator;

// namespace IrrigacaoInteligente.Features.Automacao;

// public class SincronizarAutomacao
// {
//     private readonly ILogger<SincronizarAutomacao> _logger;
//     private readonly IMediator _mediator;

//     public SincronizarAutomacao(ILogger<SincronizarAutomacao> logger, IMediator mediator)
//     {
//         _logger = logger;
//         _mediator = mediator;
//     }

//     public async Task Executar(CancellationToken cancellationToken)
//     {
//         // await _mediator.Execute(
//         //     new SincronizarPaineisCommand(),
//         //     cancellationToken: cancellationToken
//         // );
//         // await _mediator.Execute(
//         //     new SincronizarDispositivosCommand(),
//         //     cancellationToken: cancellationToken
//         // );
//         // await _mediator.Execute(
//         //     new SincronizarPortasCommand(),
//         //     cancellationToken: cancellationToken
//         // );
//         // await _mediator.Execute(
//         //     new SincronizarInterfacesCommand(),
//         //     cancellationToken: cancellationToken
//         // );
//         // await _mediator.Execute(
//         //     new SincronizarControladorCommand(),
//         //     cancellationToken: cancellationToken
//         // );

//         _logger.LogInformation("Sincronização dos dados de Automação executada com sucesso");
//     }
// }
