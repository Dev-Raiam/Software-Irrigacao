using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Automacao.Core.Application.Comandos;
using Toolbox.Automacao.Core.Models.Comandos;
using Toolbox.Automacao.Core.Services.Mqtt;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace SoftwareIrrigacao.Features.Hardware.Controle;

public class DesligarMotoBombaHandler : ICommandHandler<DesligarMotoBomba>
{
    private readonly IMqtt _mqtt;

    public DesligarMotoBombaHandler([FromKeyedServices("local")] IMqtt mqtt)
    {
        _mqtt = mqtt;
    }

    public async Task<ResponseResult> Handle(
        DesligarMotoBomba request,
        CancellationToken cancellationToken = default
    )
    {
        // Pegar a porta da interface de dados
        // de sincronizacao ou seja o meu handler ja deverar ter feito
        // a requisicao para buscar as infromacoes de qual é o dispositivo com id que vem no request
        var comando = new ComandoControleDigital { Porta = "Q1", Valor = false };
        var payload = JsonSerializer.Serialize(comando);

        // TODO: Implementar lógica de publicação no MQTT
        await _mqtt.PublishAsync("topic", payload);

        return ResponseResult.Result(System.Net.HttpStatusCode.OK);
    }
}
