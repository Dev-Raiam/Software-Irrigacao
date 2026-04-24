using WorkerService.Features.Prontidao;

namespace WorkerService.Workers;

public class ProntidaoWorker(ILogger<ProntidaoWorker> _logger, Prontidao _servicoProntidao)
    : BackgroundService
{
    private bool _avisoEmitido = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var execucaoCompleta = await _servicoProntidao.PrepararAplicacaoAsync(
                    stoppingToken
                );
                if (execucaoCompleta)
                {
                    _servicoProntidao.MarcarPronto();
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
