using IrrigacaoInteligente.State;

namespace IrrigacaoInteligente.Workers;

public class ProntidaoWorker : BackgroundService
{
    private readonly Aplicacao _aplicacao;
    private readonly ILogger<ProntidaoWorker> _logger;

    public ProntidaoWorker(Aplicacao aplicacao, ILogger<ProntidaoWorker> logger)
    {
        _aplicacao = aplicacao;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var aplicacaoEstado = await _aplicacao.ValidarEstadoAplicacao(stoppingToken);

                if (aplicacaoEstado)
                {
                    _logger.LogInformation("Aplicação pronta.");
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    _aplicacao.LiberarAplicacao();

                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na preparação do serviço");
            }
        }
    }
}
