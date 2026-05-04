using Microsoft.Extensions.Options;
using Toolbox.Automacao.Irrigacao.Comandos.Sensores;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using IrrigacaoInteligente.Features.Hardware;
using IrrigacaoInteligente.Infrastructure.Data;
using IrrigacaoInteligente.Infrastructure.Mqtt;
using IrrigacaoInteligente.State;

namespace IrrigacaoInteligente.Features.Roteadores.Sensores;

public class LerSensorDistanciaHandler : CommandHandler, ICommandHandler<LerSensorDistancia>
{
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly MqttClienteLocal _mqttClient;
    private readonly MqttConfiguracao _mqttConfiguracao;

    public LerSensorDistanciaHandler(
        ArmazenamentoAutomacao armazenamento,
        MqttClienteLocal mqttClient,
        IOptions<MqttConfiguracao> mqttConfiguracao,
        IUnitOfWork<IrrigacaoInteligenteContext> uow
    )
        : base(uow)
    {
        _armazenamento = armazenamento;
        _mqttClient = mqttClient;
        _mqttConfiguracao = mqttConfiguracao.Value;
    }

    public async Task<ResponseResult> Handle(
        LerSensorDistancia request,
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

        await _mqttClient.PublicarAsync(
            _mqttConfiguracao.TopicoLocal,
            ControleAnalogico.Ler(porta.EnderecoLogico!),
            cancellationToken
        );

        return Ok<ResponseResult>();
    }
}
