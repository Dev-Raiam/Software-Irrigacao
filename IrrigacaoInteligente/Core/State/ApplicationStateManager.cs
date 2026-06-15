using System.Net;
using IrrigacaoInteligente.Core.Cache;
using IrrigacaoInteligente.Core.Mqtt;
using IrrigacaoInteligente.Features.Configuracao;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Mediator;

namespace IrrigacaoInteligente.Core.State;

//public class ApplicationStateManager
//{
//    private readonly IServiceProvider _serviceProvider;
//    private readonly MqttClienteLocal _mqttClienteLocal = null!;
//    private readonly MqttClienteRemoto _mqttClienteRemoto = null!;

//    //private readonly ArmazenamentoAutomacao _armazenamentoAutomacao = null!;
//    private readonly TaskCompletionSource _pronto = new("Task-Aplicacao");
//    private readonly TaskCompletionSource _prontoMqtt = new("Task-Aplicacao-Mqtt");
//    private readonly ILogger<ApplicationStateManager> _logger;
//    private readonly CredenciaisAplicacao _credenciaisAplicacao;
//    private bool _avisoEstadoAplicacao = false;
//    public bool AvisoCredenciaisEmitido { get; set; } = false;
//    public bool AvisoMqttEmitido { get; set; } = false;
//    public bool MqttLiberado { get; set; } = false;

//    public ApplicationStateManager(
//        IServiceProvider serviceProvider,
//        ILogger<ApplicationStateManager> logger,
//        CredenciaisAplicacao credenciaisAplicacao,
//        //ArmazenamentoAutomacao armazenamentoAutomacao,
//        MqttClienteLocal mqttClienteLocal,
//        MqttClienteRemoto mqttClienteRemoto
//    )
//    {
//        _serviceProvider = serviceProvider;
//        _logger = logger;
//        _credenciaisAplicacao = credenciaisAplicacao;
//        _mqttClienteLocal = mqttClienteLocal;
//        _mqttClienteRemoto = mqttClienteRemoto;
//        _credenciaisAplicacao = credenciaisAplicacao;
//    }

//    public async Task<bool> ValidarEstadoAplicacao(CancellationToken cancellationToken)
//    {
//        if (_pronto.Task.IsCompleted)
//            return true;

//        using var scope = _serviceProvider.CreateScope();
//        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

//        if (!_avisoEstadoAplicacao && _credenciaisAplicacao.Invalida)
//        {
//            _logger.LogInformation("Aguardando configurações...");
//            _avisoEstadoAplicacao = true;
//        }

//        return true;
//    }

//    public Task AguardarLiberacaoAplicacao(CancellationToken cancellationToken) =>
//        _pronto.Task.WaitAsync(cancellationToken);

//    public Task AguardarLiberacaoMqtt(CancellationToken cancellationToken) =>
//        _prontoMqtt.Task.WaitAsync(cancellationToken);

//    public void LiberarAplicacao() => _pronto.TrySetResult();

//    public void LiberarMqtt() => _prontoMqtt.TrySetResult();
//}


public class ApplicationStateManager 
{
    private readonly TaskCompletionSource _credenciaisDefinidas = new();
    private readonly TaskCompletionSource _sincronizacaoConcluida = new();

    public void LiberarCredenciais() => _credenciaisDefinidas.TrySetResult();
    public void LiberarSincronizacao() => _sincronizacaoConcluida.TrySetResult();

    public Task AguardarCredenciaisAsync() => _credenciaisDefinidas.Task;
    public Task AguardarSincronizacaoAsync() => _sincronizacaoConcluida.Task;
}