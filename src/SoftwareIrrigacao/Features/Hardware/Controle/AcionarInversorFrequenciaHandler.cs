using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Models.Comandos;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace SoftwareIrrigacao.Features.Hardware.Controle;

public class AcionarInversorFrequenciaHandler : ICommandHandler<AcionarInversorFrequencia>
{
    private readonly IMqtt _mqtt;

    public AcionarInversorFrequenciaHandler([FromKeyedServices(Mqtt.Local)] IMqtt mqtt)
    {
        _mqtt = mqtt;
    }

    public async Task<ResponseResult> Handle(
        AcionarInversorFrequencia request,
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
