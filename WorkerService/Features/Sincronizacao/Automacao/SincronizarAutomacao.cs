using Toolbox.Core.Mediator;
using WorkerService.Configurations;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Mqtt;

namespace WorkerService.Features.Sincronizacao.Automacao;

public class SincronizarAutomacao(
    ILogger<SincronizarAutomacao> _logger,
    MqttClienteRemoto _mqttClienteRemoto,
    IMediator _mediator
)
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
        await _mqttClienteRemoto.Publicar(
            "topico-request",
            new { acionar = true },
            cancellationToken
        );
        _logger.LogInformation("Sincronização dos dados de Automação executada com sucesso");
    }
}
