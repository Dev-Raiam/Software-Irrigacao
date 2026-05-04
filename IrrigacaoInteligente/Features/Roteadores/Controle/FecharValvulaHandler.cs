using Microsoft.Extensions.Options;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using IrrigacaoInteligente.Features.Hardware;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;

namespace IrrigacaoInteligente.Features.Roteadores.Controle;

public class FecharValvulaHandler : CommandHandler, ICommandHandler<FecharValvula>
{
    private readonly MqttClienteLocal _mqttCliente;
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly MqttConfiguracao _mqttConfiguracao;

    public FecharValvulaHandler(
        MqttClienteLocal mqttCliente,
        ArmazenamentoAutomacao armazenamento,
        IOptions<MqttConfiguracao> mqttConfiguracao,
        IUnitOfWork<IrrigacaoInteligenteContext> uow
    )
        : base(uow)
    {
        _mqttCliente = mqttCliente;
        _armazenamento = armazenamento;
        _mqttConfiguracao = mqttConfiguracao.Value;
    }

    public async Task<ResponseResult> Handle(
        FecharValvula request,
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
            _mqttConfiguracao.TopicoLocal,
            ControleDigital.Desligar(porta.EnderecoLogico!),
            cancellationToken
        );

        return Ok<ResponseResult>();
    }
}
