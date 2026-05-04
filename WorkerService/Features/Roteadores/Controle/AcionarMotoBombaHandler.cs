using Microsoft.Extensions.Options;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Features.Hardware;
using WorkerService.Infrastructure.Data;
using WorkerService.Infrastructure.Mqtt;
using WorkerService.State;

namespace WorkerService.Features.Roteadores.Controle;

public class AcionarMotoBombaHandler : CommandHandler, ICommandHandler<AcionarMotoBomba>
{
    private readonly MqttClienteLocal _mqttCliente;
    private readonly ArmazenamentoAutomacao _armazenamento;
    private readonly MqttConfiguracao _mqttConfiguracao;

    public AcionarMotoBombaHandler(
        MqttClienteLocal mqttCliente,
        IUnitOfWork<WorkerServiceContext> uow,
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
        AcionarMotoBomba request,
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
            ControleDigital.Acionar(porta.EnderecoLogico!),
            cancellationToken
        );

        return Ok<ResponseResult>();
    }
}
