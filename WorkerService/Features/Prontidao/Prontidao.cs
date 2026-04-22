using WorkerService.Configurations;
using WorkerService.Features.Configuracao.GerenciamentoCredenciais;

namespace WorkerService.Features.Prontidao;

public class Prontidao(
    IntegracaoConfiguracao _integracaoConfiguracao,
    ContaConfiguracao _contaConfiguracao,
    IServiceProvider _serviceProvider
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
            MarcarPronto();
            return true;
        }
        return false;
    }

    public Task AguardarAsync(CancellationToken cancellationToken) =>
        _pronto.Task.WaitAsync(cancellationToken);

    public void MarcarPronto() => _pronto.TrySetResult();
}
