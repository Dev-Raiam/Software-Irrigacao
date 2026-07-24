using System.Text.Json;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Messages;
using Toolbox.Industrial.Core.Messages.Integration;

namespace SoftwareIrrigacao.Features.Hardware.Controle;

public class AcionarBombaHandler : ICommandHandler<AcionarBomba>
{
    private readonly IMqtt _mqtt;

    public AcionarBombaHandler([FromKeyedServices(Mqtt.Local)] IMqtt mqtt)
    {
        _mqtt = mqtt;
    }

    public async Task<ResponseResult> Handle(
        AcionarBomba request,
        CancellationToken cancellationToken = default
    )
    {
        // Pegar a porta da interface de dados
        // de sincronizacao ou seja o meu handler ja deverar ter feito
        // a requisicao para buscar as infromacoes de qual é o dispositivo com id que vem no request
        var comando = new ComandoControleDigital { Porta = "Q1", Valor = true };
        var payload = JsonSerializer.Serialize(comando);

        // TODO: Implementar lógica de publicação no MQTT
        await _mqtt.PublishAsync("topic", payload);

        return ResponseResult.Result(System.Net.HttpStatusCode.OK);
    }
}
