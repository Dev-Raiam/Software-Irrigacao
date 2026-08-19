using System.Text.Json;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Messages;
using Toolbox.Industrial.Core.Messages.Integration;

namespace SoftwareIrrigacao.Features.Hardware.Controle;

public class DefinirValvulaProporcionalHandler : ICommandHandler<DefinirValvulaProporcional>
{
    private readonly IMqtt _mqtt;

    public DefinirValvulaProporcionalHandler([FromKeyedServices(Mqtt.Interno)] IMqtt mqtt)
    {
        _mqtt = mqtt;
    }

    public async Task<ResponseResult> Handle(
        DefinirValvulaProporcional request,
        CancellationToken cancellationToken = default
    )
    {
        // Pegar a porta da interface de dados
        // de sincronizacao ou seja o meu handler ja deverar ter feito
        // a requisicao para buscar as infromacoes de qual é o dispositivo com id que vem no request
        var comando = new ComandoControleAnalogico { Porta = "Q1", Valor = request.Abertura };
        //var payload = JsonSerializer.Serialize(comando);

        // TODO: Implementar lógica de publicação no MQTT
        await _mqtt.PublishAsync("topic", comando);

        return ResponseResult.Result(System.Net.HttpStatusCode.OK);
    }
}
