using System.Text.Json;
using Toolbox.Automacao.Core.Messages;
using Toolbox.Automacao.Core.Messages.Integration;
using Toolbox.Automacao.Core.Services.Mqtt;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace SoftwareIrrigacao.Features.Hardware.Leitura;

public class LerSensorCorrenteHandler : ICommandHandler<LerSensorCorrente>
{
    private readonly IMqtt _mqtt;

    public LerSensorCorrenteHandler([FromKeyedServices(Mqtt.Local)] IMqtt mqtt)
    {
        _mqtt = mqtt;
    }

    public async Task<ResponseResult> Handle(
        LerSensorCorrente request,
        CancellationToken cancellationToken = default
    )
    {
        // Pegar a porta da interface de dados
        // de sincronizacao ou seja o meu handler ja deverar ter feito
        // a requisicao para buscar as infromacoes de qual é o dispositivo com id que vem no request
        var comando = new ComandoLeitura { Porta = "Q1" };
        var payload = JsonSerializer.Serialize(comando);

        // TODO: Implementar lógica de publicação no MQTT
        await _mqtt.PublishAsync("topic", payload);

        return ResponseResult.Result(System.Net.HttpStatusCode.OK);
    }
}
