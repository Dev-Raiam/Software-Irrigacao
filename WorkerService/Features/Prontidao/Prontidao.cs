using WorkerService.Configurations;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais;
using WorkerService.Features.Shared.Abstractions;
using WorkerService.Infrastructure.Mqtt;

namespace WorkerService.Features.Prontidao;

public class Prontidao(
    IntegracaoConfiguracao _integracaoConfiguracao,
    ContaConfiguracao _contaConfiguracao,
    IServiceProvider _serviceProvider,
    MqttClienteRemoto _mqttClienteRemoto
)
{
    private readonly TaskCompletionSource _pronto = new("Task-Prontidão");

    public async Task<bool> PrepararAplicacaoAsync(CancellationToken cancellationToken)
    {
        // if (_pronto.Task.IsCompleted)
        //     return true;

        if (_integracaoConfiguracao.Invalida || _contaConfiguracao.Invalida)
        {
            using var scope = _serviceProvider.CreateScope();
            var armazenamento = scope.ServiceProvider.GetRequiredService<GerenciadorCredenciais>();
            await armazenamento.ObterCredencialTopicoConfiguracao(cancellationToken);
            await armazenamento.ObterCredencialIntegracao(cancellationToken);
            await armazenamento.ObterContaId(cancellationToken);
        }

        if (!_integracaoConfiguracao.Invalida && !_contaConfiguracao.Invalida)
        {
            if (_mqttClienteRemoto.Conectado)
            {
                await _mqttClienteRemoto.Publicar(
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
