using WorkerService.Features.Prontidao;

namespace WorkerService.Workers;

public class ProntidaoWorker(ILogger<ProntidaoWorker> _logger, Prontidao _servicoProntidao)
    : BackgroundService
{
    private bool _avisoEmitido = false;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var execucaoCompleta = await _servicoProntidao.PrepararAplicacaoAsync(
                    stoppingToken
                );
                if (execucaoCompleta)
                {
                    _logger.LogInformation("Configuraçõens Obtidas com Sucesso.");
                    break;
                }
                else if (!_avisoEmitido)
                {
                    _logger.LogWarning("Aguardando configurações...");
                    _avisoEmitido = true;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Encerramento normal do serviço
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na preparação da aplicação");
            }
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
