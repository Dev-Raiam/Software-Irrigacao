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

                await sincronizarAutomacao.Executar(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado na sincronização");
            }
        }
    }
}
