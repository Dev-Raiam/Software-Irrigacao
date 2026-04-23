using WorkerService.Features.Prontidao;
using WorkerService.Features.Sincronizacao.Automacao;

namespace WorkerService.Workers;

public class SincronizacaoWorker(
    ILogger<SincronizacaoWorker> _logger,
    IServiceProvider _serviceProvider,
    Prontidao _servicoProntidao
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _servicoProntidao.AguardarAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var sincronizarAutomacao =
                    scope.ServiceProvider.GetRequiredService<SincronizarAutomacao>();

                var sucessoSincronizacao = await sincronizarAutomacao.Executar(stoppingToken);

                if (sucessoSincronizacao)
                    _logger.LogInformation("Sincronização executada com sucesso");
                else
                    _logger.LogWarning("Sincronização não foi executada com sucesso");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Encerramento normal do serviço
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Erro ao comunicar com a API de automação");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na sincronização");
            }
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
