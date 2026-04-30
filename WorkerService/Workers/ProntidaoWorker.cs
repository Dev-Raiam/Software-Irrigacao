using WorkerService.State;

namespace WorkerService.Workers;

public class ProntidaoWorker(
    ILogger<ProntidaoWorker> _logger,
    ConfiguracaoInicializacao configuracaoInicializacao
) : BackgroundService
{
    private bool _avisoEmitido = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var execucaoCompleta = await configuracaoInicializacao.Iniciar(stoppingToken);

                if (execucaoCompleta)
                {
                    _logger.LogInformation("Aplicação Configurada!!!");
                    configuracaoInicializacao.ConfiguracaoInicializacaoConcluida();
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
