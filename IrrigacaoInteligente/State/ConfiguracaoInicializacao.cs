using IrrigacaoInteligente.Features.Configuracao.ConfiguracaoSistema;
using Toolbox.Core.Mediator;

namespace IrrigacaoInteligente.State;

public class ConfiguracaoInicializacao
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TaskCompletionSource _pronto = new("Task-Prontidão");

    public ConfiguracaoInicializacao(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<bool> Iniciar(CancellationToken cancellationToken)
    {
        if (_pronto.Task.IsCompleted)
            return true;

        var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var responseResult = await mediator.Execute(
            new IniciarConfiguracaoInicializacao(),
            cancellationToken: cancellationToken
        );

        return responseResult.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }

    public Task AguardarConfiguracaoInicializacaoAsync(CancellationToken cancellationToken) =>
        _pronto.Task.WaitAsync(cancellationToken);

    public void ConfiguracaoInicializacaoConcluida() => _pronto.TrySetResult();
}
