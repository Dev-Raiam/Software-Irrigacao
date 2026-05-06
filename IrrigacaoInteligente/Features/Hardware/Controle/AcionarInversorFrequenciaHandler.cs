using IrrigacaoInteligente.Domain.Entities.Hardware;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;
using Microsoft.Extensions.Options;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace IrrigacaoInteligente.Features.Hardware.Controle;

public class AcionarInversorFrequenciaHandler
    : CommandHandler,
        ICommandHandler<AcionarInversorFrequencia>
{
    private readonly MqttClienteLocal _mqttCliente;
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly MqttConfiguracao _mqttConfiguracao;

    public AcionarInversorFrequenciaHandler(
        MqttClienteLocal mqttCliente,
        IUnitOfWork<IrrigacaoInteligenteContext> uow,
        ArmazenamentoAutomacao armazenamento,
        IOptions<MqttConfiguracao> mqttConfiguracao
    )
        : base(uow)
    {
        _mqttCliente = mqttCliente;
        _armazenamento = armazenamento;
        _mqttConfiguracao = mqttConfiguracao.Value;
    }

    public async Task<ResponseResult> Handle(
        AcionarInversorFrequencia request,
        CancellationToken cancellationToken = default
    )
    {
        var dispositivo = _armazenamento.Dispositivos.FirstOrDefault(d => d.Id == request.Id);

        if (dispositivo is null)
            return NotFound();

        var porta = _armazenamento
            .Portas.Where(p => p.DispositivoConectadoId == dispositivo.Id)
            .FirstOrDefault();

        if (porta is null)
            return NotFound();

        await _mqttCliente.PublicarAsync(
            _mqttConfiguracao.TopicoCmdLocal,
            ComandoDigital.Acionar(porta.EnderecoLogico!),
            cancellationToken
        );

        return Ok<ResponseResult>();
    }
}
