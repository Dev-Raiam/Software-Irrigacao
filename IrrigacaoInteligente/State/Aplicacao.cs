using IrrigacaoInteligente.Features.Configuracao;
using Toolbox.Core.Mediator;

namespace IrrigacaoInteligente.State;

public class Aplicacao
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TaskCompletionSource _pronto = new("Task-Prontidão");
    private readonly ILogger<Aplicacao> _logger;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private bool _avisoEmitido = false;

    public Aplicacao(
        IServiceProvider serviceProvider,
        ILogger<Aplicacao> logger,
        CredenciaisAplicacao credenciaisAplicacao
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _credenciaisAplicacao = credenciaisAplicacao;
    }

    public async Task<bool> ValidarEstado(CancellationToken cancellationToken)
    {
        if (_pronto.Task.IsCompleted)
            return true;

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (!_avisoEmitido && _credenciaisAplicacao.Invalida)
        {
            _logger.LogInformation("Aguardando configurações...");
            _avisoEmitido = true;
        }

        var responseResult = await mediator.Execute(
            new ValidarEstadoAplicacao(),
            cancellationToken: cancellationToken
        );

        return responseResult.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }

    public Task AguardarConfiguracaoAplicacao(CancellationToken cancellationToken) =>
        _pronto.Task.WaitAsync(cancellationToken);

    public void Configurada() => _pronto.TrySetResult();
}
