using System.Text.Encodings.Web;
using System.Text.Json;
using IrrigacaoInteligente.Features.Sincronizacao.Automacao;
using IrrigacaoInteligente.State;
using Microsoft.AspNetCore.Http.Json;

namespace IrrigacaoInteligente.Workers;

public class SincronizacaoWorker : BackgroundService
{
    private readonly ILogger<SincronizacaoWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConfiguracaoInicializacao _configuracaoInicializacao;
    private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
    private readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public SincronizacaoWorker(
        ILogger<SincronizacaoWorker> logger,
        IServiceProvider serviceProvider,
        ConfiguracaoInicializacao configuracaoInicializacao,
        ArmazenamentoAutomacao armazenamentoAutomacao
    )
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuracaoInicializacao = configuracaoInicializacao;
        _armazenamentoAutomacao = armazenamentoAutomacao;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _configuracaoInicializacao.AguardarConfiguracaoInicializacaoAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine(JsonSerializer.Serialize(_armazenamentoAutomacao, JsonOptions));

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                using var scope = _serviceProvider.CreateScope();
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
