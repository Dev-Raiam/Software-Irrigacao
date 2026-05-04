using IrrigacaoInteligente.State;

namespace IrrigacaoInteligente.Workers;

public class ProntidaoWorker : BackgroundService
{
    private readonly ILogger<ProntidaoWorker> _logger;
    private readonly ConfiguracaoInicializacao _configuracaoInicializacao;
    private bool _avisoEmitido = false;

    public ProntidaoWorker(
        ILogger<ProntidaoWorker> logger,
        ConfiguracaoInicializacao configuracaoInicializacao
    )
    {
        _logger = logger;
        _configuracaoInicializacao = configuracaoInicializacao;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var execucaoCompleta = await _configuracaoInicializacao.Iniciar(stoppingToken);

                if (execucaoCompleta)
                {
                    _logger.LogInformation("Aplicação Configurada!!!");
                    _configuracaoInicializacao.ConfiguracaoInicializacaoConcluida();
                    break;
                }
                else if (!_avisoEmitido)
                {
                    _logger.LogInformation("Aguardando configurações...");
                    _avisoEmitido = true;
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
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
