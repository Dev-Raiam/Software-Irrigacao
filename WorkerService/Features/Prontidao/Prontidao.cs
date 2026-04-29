using WorkerService.Configurations;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Mqtt;
using WorkerService.State;

namespace WorkerService.Features.Prontidao;

public class Prontidao(
    CredenciaisAplicacao _credenciaisAplicacao,
    IServiceProvider _serviceProvider,
    MqttClienteRemoto _mqttClienteRemoto
)
{
    private readonly TaskCompletionSource _pronto = new("Task-Prontidão");

    public async Task<bool> PrepararAplicacaoAsync(CancellationToken cancellationToken)
    {
        // if (_pronto.Task.IsCompleted)
        //     return true;

        if (_credenciaisAplicacao.Invalida)
        {
            using var scope = _serviceProvider.CreateScope();
            var armazenamento = scope.ServiceProvider.GetRequiredService<GerenciadorCredenciais>();
            await armazenamento.ObterPainelId(cancellationToken);
            await armazenamento.ObterCredencialIntegracao(cancellationToken);
            await armazenamento.ObterContaId(cancellationToken);
        }

        if (!_credenciaisAplicacao.Invalida)
        {
            if (_mqttClienteRemoto.Conectado)
            {
                await _mqttClienteRemoto.PublicarAsync(
                    "prontidao",
                    "Aplicacao Liberada para Uso",
                    cancellationToken
                );
            }
            return true;
        }
        return false;
    }

    public Task AguardarAsync(CancellationToken cancellationToken) =>
        _pronto.Task.WaitAsync(cancellationToken);

    public void MarcarPronto() => _pronto.TrySetResult();
}
