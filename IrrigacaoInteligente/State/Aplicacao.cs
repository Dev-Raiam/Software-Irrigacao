using IrrigacaoInteligente.Features.Configuracao;
using Toolbox.Core.Mediator;

namespace IrrigacaoInteligente.State;

public class Aplicacao
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TaskCompletionSource _pronto = new("Task-Aplicacao");
    private readonly TaskCompletionSource _prontoMqtt = new("Task-Aplicacao-Mqtt");
    private readonly ILogger<Aplicacao> _logger;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private bool _avisoEstadoAplicacao = false;
    public bool AvisoCredenciaisEmitido { get; set; } = false;
    public bool AvisoMqttEmitido { get; set; } = false;
    public bool MqttLiberado { get; set; } = false;

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

    public async Task<bool> ValidarEstadoAplicacao(CancellationToken cancellationToken)
    {
        if (_pronto.Task.IsCompleted)
            return true;

        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        if (!_avisoEstadoAplicacao && _credenciaisAplicacao.Invalida)
        {
            _logger.LogInformation("Aguardando configurações...");
            _avisoEstadoAplicacao = true;
        }

        var responseResult = await mediator.Execute(
            new ValidarEstadoAplicacao(),
            cancellationToken: cancellationToken
        );

        return responseResult.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }

    public Task AguardarLiberacaoAplicacao(CancellationToken cancellationToken) =>
        _pronto.Task.WaitAsync(cancellationToken);

    public Task AguardarLiberacaoMqtt(CancellationToken cancellationToken) =>
        _prontoMqtt.Task.WaitAsync(cancellationToken);

    public void LiberarAplicacao() => _pronto.TrySetResult();

    public void LiberarMqtt() => _prontoMqtt.TrySetResult();
}
