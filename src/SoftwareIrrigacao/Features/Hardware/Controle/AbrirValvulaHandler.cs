//using SoftwareIrrigacao.Infrastructure.Data;
//using SoftwareIrrigacao.Infrastructure.Mqtt;
//using SoftwareIrrigacao.State;
//using Microsoft.Extensions.Options;
//using Org.BouncyCastle.Asn1;
//using Toolbox.Automacao.Irrigacao.Comandos.Controle;
//using Toolbox.Core.Api.Data;
//using Toolbox.Core.Data;
//using Toolbox.Core.Mediator;
//using Toolbox.Core.Messages;

//namespace SoftwareIrrigacao.Features.Hardware.Controle;

//public class AbrirValvulaHandler : CommandHandler, ICommandHandler<AbrirValvula>
//{
//    private readonly ArmazenamentoAutomacao _armazenamento;
//    private readonly MqttClienteLocal _mqttClient;
//    private readonly MqttConfiguracao _mqttConfiguracao;
//    private readonly SoftwareIrrigacaoContext _context;

//    public AbrirValvulaHandler(
//        ArmazenamentoAutomacao armazenamento,
//        MqttClienteLocal mqttClient,
//        IOptions<MqttConfiguracao> mqttConfiguracao,
//        SoftwareIrrigacaoContext context
//    )
//    {
//        _armazenamento = armazenamento;
//        _mqttClient = mqttClient;
//        _mqttConfiguracao = mqttConfiguracao.Value;
//        _context = context;
//    }

//    public async Task<ResponseResult> Handle(
//        AbrirValvula request,
//        CancellationToken cancellationToken
//    )
//    {
//        // var dispositivo = _armazenamento.Dispositivos.FirstOrDefault(d => d.Id == request.Id);

//        // if (dispositivo is null)
//        //     return NotFound();

//        // var porta = _armazenamento
//        //     .Portas.Where(p => p.DispositivoConectadoId == dispositivo.Id)
//        //     .FirstOrDefault();

//        // if (porta is null)
//        //     return NotFound();

//        // await _mqttClient.PublicarAsync(
//        //     _mqttConfiguracao.TopicoCmdLocal,
//        //     ComandoDigital.Acionar(porta.EnderecoLogico!),
//        //     cancellationToken
//        // );

//        return Ok<ResponseResult>();
//    }
//}
