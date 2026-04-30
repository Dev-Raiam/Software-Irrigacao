using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using WorkerService.Features.Sincronizacao.Automacao;
using WorkerService.State;

namespace WorkerService.Workers;

public class SincronizacaoWorker(
    ILogger<SincronizacaoWorker> _logger,
    IServiceProvider serviceProvider,
    ConfiguracaoInicializacao configuracaoInicializacao,
    ArmazenamentoAutomacao armazenamentoAutomacao
) : BackgroundService
{
    private readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await configuracaoInicializacao.AguardarConfiguracaoInicializacaoAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine(JsonSerializer.Serialize(armazenamentoAutomacao, JsonOptions));

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                using var scope = serviceProvider.CreateScope();
                var sincronizarAutomacao =
                    scope.ServiceProvider.GetRequiredService<SincronizarAutomacao>();

                await sincronizarAutomacao.Executar(stoppingToken);
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
