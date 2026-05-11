using System.Text.Json;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;
using Toolbox.Automacao.Irrigacao.Comandos.Sincronizacao;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Configuracao;

public class ValidarEstadoAplicacao : Command { }

public class ValidarEstadoAplicacaoHandler : CommandHandler, ICommandHandler<ValidarEstadoAplicacao>
{
    private readonly Aplicacao _aplicacao;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;
    private readonly ArmazenamentoAutomacao _armazenamentoAutomacao;
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ValidarEstadoAplicacaoHandler> _logger;

    public ValidarEstadoAplicacaoHandler(
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        Aplicacao aplicacao,
        CredenciaisAplicacao credenciaisAplicacao,
        ArmazenamentoAutomacao armazenamentoAutomacao,
        IMediator mediator,
        IServiceProvider serviceProvider,
        ILogger<ValidarEstadoAplicacaoHandler> logger
    )
        : base(uow)
    {
        _aplicacao = aplicacao;
        _credenciaisAplicacao = credenciaisAplicacao;
        _armazenamentoAutomacao = armazenamentoAutomacao;
        _mediator = mediator;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<ResponseResult> Handle(
        ValidarEstadoAplicacao request,
        CancellationToken cancellationToken
    )
    {
        using var scope = _serviceProvider.CreateScope();

        var mqttClienteLocal = scope.ServiceProvider.GetRequiredService<MqttClienteLocal>();
        var mqttClienteRemoto = scope.ServiceProvider.GetRequiredService<MqttClienteRemoto>();

        if (_credenciaisAplicacao.Invalida)
        {
            if (!_aplicacao.AvisoCredenciaisEmitido)
            {
                _logger.LogWarning("Credenciais da Aplicação não foram inseridas.");

                _aplicacao.AvisoCredenciaisEmitido = true;
            }
        }
        else
        {
            _aplicacao.AvisoCredenciaisEmitido = false;

            if (!_credenciaisAplicacao.Invalida && !_aplicacao.MqttLiberado)
            {
                _logger.LogInformation("Credenciais Carregadas com Sucesso !!!");

                _aplicacao.LiberarMqtt();
                _aplicacao.MqttLiberado = true;

                await Task.Delay(1000, cancellationToken);
            }

            if (_armazenamentoAutomacao.Invalido)
            {
                _logger.LogInformation("Sincronizando Controladores...");

                await _mediator.Execute(
                    new SincronizarControladores(),
                    cancellationToken: cancellationToken
                );
            }

            if (!mqttClienteLocal.Conectado || !mqttClienteRemoto.Conectado)
            {
                if (!_aplicacao.AvisoMqttEmitido)
                {
                    _logger.LogError(
                        "Conexão MQTT {mqttCliente} não está estabelecida.",
                        mqttClienteLocal.Conectado != true ? "Local" : "Remoto"
                    );
                    _aplicacao.AvisoMqttEmitido = true;
                }
            }
            else
            {
                _aplicacao.AvisoMqttEmitido = false;
            }
        }

        if (
            !_credenciaisAplicacao.Invalida
            && !_armazenamentoAutomacao.Invalido
            && mqttClienteLocal.Conectado
            && mqttClienteRemoto.Conectado
        )
        {
            return Ok<ResponseResult>();
        }

        return NotFound();
    }
}
