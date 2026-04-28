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

public class AcionarBombaHandler : CommandHandler, ICommandHandler<AcionarBomba>
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

    public AcionarBombaHandler(MqttClienteRemoto mqttRemoto, IUnitOfWork<WorkerServiceContext> uow)
        : base(uow)
    {
        _mqttRemoto = mqttRemoto;
    }

    public async Task<ResponseResult> Handle(
        AcionarBomba request,
        CancellationToken cancellationToken = default
    )
    {
        Console.WriteLine($"Executando {nameof(AcionarBomba)}");
        var comando = JsonConvert.SerializeObject(
            new AcionarBomba { Id = Guid.NewGuid() },
            _settings
        );
        await _mqttRemoto.Publicar("comando/acionar-bomba", comando, cancellationToken);
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
