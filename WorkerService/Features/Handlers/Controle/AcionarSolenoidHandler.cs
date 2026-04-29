using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;
using WorkerService.Infrastructure.Mqtt;

namespace WorkerService.Features.Handlers.Controle;

public class AcionarSolenoidHandler : CommandHandler, ICommandHandler<AcionarSolenoide>
{
    private readonly MqttClienteRemoto _mqttRemoto;
    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Objects,
    };
    private readonly WorkerServiceContext _context;

    public AcionarSolenoidHandler(
        IUnitOfWork<WorkerServiceContext> uow,
        MqttClienteRemoto mqttRemoto
    )
        : base(uow)
    {
        _context = uow.Context;
        _mqttRemoto = mqttRemoto;
    }

    public async Task<ResponseResult> Handle(
        AcionarSolenoide request,
        CancellationToken cancellationToken = default
    )
    {
        // Console.WriteLine(
        //     $"sssssssssssssssssssssssssssssssssssExecutando {nameof(AcionarSolenoide)}"
        // );

        // var dispositivo = _context.Dispositivos.FirstOrDefault(d => d.Id == request.Id);
        // if (dispositivo == null)
        // {
        //     AddError("Dispositivo não encontrado");
        //     return NotFound();
        // }
        // Console.WriteLine($"Dispositivo encontrado: {dispositivo.Id}");
        // //var comando = JsonConvert.SerializeObject(new AcionarSolenoide(), _settings);
        // await _mqttRemoto.PublicarAsync(
        //     "comando/solenoide",
        //     dispositivo.Descricao,
        //     cancellationToken
        // );
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
