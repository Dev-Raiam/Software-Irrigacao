using IrrigacaoInteligente.State;

namespace IrrigacaoInteligente.Workers;

public class ProntidaoWorker : BackgroundService
{
    private readonly ConfiguracaoInicializacao _configuracaoInicializacao;
    private readonly ILogger<ProntidaoWorker> _logger;

    public ProntidaoWorker(
        ConfiguracaoInicializacao configuracaoInicializacao,
        ILogger<ProntidaoWorker> logger
    )
    {
        _configuracaoInicializacao = configuracaoInicializacao;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var execucaoCompleta = await _configuracaoInicializacao.Iniciar(stoppingToken);

                if (execucaoCompleta)
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                    _configuracaoInicializacao.ConfiguracaoInicializacaoConcluida();

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
