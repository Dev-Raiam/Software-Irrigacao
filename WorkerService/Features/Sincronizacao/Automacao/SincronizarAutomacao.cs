using Toolbox.Core.Mediator;

namespace WorkerService.Features.Sincronizacao.Automacao;

public class SincronizarAutomacao(ILogger<SincronizarAutomacao> _logger, IMediator _mediator)
{
    public async Task Executar(CancellationToken cancellationToken)
    {
        await _mediator.Execute(
            new SincronizarPaineisCommand(),
            cancellationToken: cancellationToken
        );
        await _mediator.Execute(
            new SincronizarDispositivosCommand(),
            cancellationToken: cancellationToken
        );
        await _mediator.Execute(
            new SincronizarPortasCommand(),
            cancellationToken: cancellationToken
        );
        await _mediator.Execute(
            new SincronizarInterfacesCommand(),
            cancellationToken: cancellationToken
        );

        _logger.LogInformation("Sincronização dos dados de Automação executada com sucesso");
    }
}
