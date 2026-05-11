using IrrigacaoInteligente.Features.Telemetria;
using IrrigacaoInteligente.State;
using Toolbox.Core.Mediator;

namespace IrrigacaoInteligente.Workers;

public class TelemetriaWorker : BackgroundService
{
    private readonly Aplicacao _aplicacao;
    private readonly IServiceProvider _serivceProvider;
    private readonly ILogger<TelemetriaWorker> _logger;

    public TelemetriaWorker(
        Aplicacao aplicacao,
        IServiceProvider serivceProvider,
        ILogger<TelemetriaWorker> logger
    )
    {
        _aplicacao = aplicacao;
        _serivceProvider = serivceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _aplicacao.AguardarLiberacaoAplicacao(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serivceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            try
            {
                await mediator.Execute(new PublicarTelemetria(), cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar telemetria");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
