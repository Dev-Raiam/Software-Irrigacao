using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Toolbox.Automacao.Irrigacao.Comandos.Controle;
using Toolbox.Core.Api.Data;
using Toolbox.Core.Data;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using WorkerService.Infrastructure.Data;
using WorkerService.Infrastructure.Mqtt;

namespace WorkerService.Features.Roteadores.Controle;

public class AcionarBombaHandler : CommandHandler, ICommandHandler<AcionarBomba>
{
    private readonly MqttClienteLocal _mqttLocal;
    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        DateFormatHandling = DateFormatHandling.IsoDateFormat,
        DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        NullValueHandling = NullValueHandling.Ignore,
        TypeNameHandling = TypeNameHandling.Objects,
    };

    public AcionarBombaHandler(
        MqttClienteLocal mqttClienteLocal,
        IUnitOfWork<WorkerServiceContext> uow
    )
        : base(uow)
    {
        _mqttLocal = mqttClienteLocal;
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
        await _mqttLocal.PublicarAsync(
            "comando/03800edb-8dff-4e2b-9ad8-00f0af1cdebf",
            comando,
            cancellationToken
        );
        await Task.Delay(1, cancellationToken);
        return Ok<ResponseResult>();
    }
}
