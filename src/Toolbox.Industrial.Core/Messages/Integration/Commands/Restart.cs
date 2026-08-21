using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages.Integration.Events;
using Toolbox.Industrial.Core.Setup;

namespace Toolbox.Industrial.Core.Messages.Integration.Commands;

public class Restart : RemoteCommand
{
    /// <summary>
    /// Se informado, somente o controlador especificado realizará a sincronização.
    /// Caso contrário, todos os controladores, Master e Slave, realizarão a sincronização.
    /// Após a sincronização, a aplicação poderá ser reiniciada automaticamente para aplicar a nova configuração.
    /// </summary>
    public Guid? ControladorId { get; set; }
}

internal class RestartHandler : CommandHandler, ICommandHandler<Restart>
{
    private readonly IMediator _mediator;
    private readonly MqttManager _mqttInterno;
    private readonly ILogger<RestartHandler> _logger;

    public RestartHandler(
        IMediator mediator,
        ILogger<RestartHandler> logger,
        [FromKeyedServices(Mqtt.Interno)] MqttManager mqttInterno
    )
    {
        _logger = logger;
        _mediator = mediator;
        _mqttInterno = mqttInterno;
    }

    public async Task<ResponseResult> Handle(Restart request, CancellationToken cancellationToken)
    {
        var pendings = new List<PendingProcess<Restart>>();
        var controladores = Controlador.Master ? Application.Controladores : [];
        if (Controlador.Master)
        {
            var slaves = controladores
                .Where(x =>
                    x.Id != Controlador.ControladorId
                    && (request.ControladorId == null || x.Id == request.ControladorId)
                )
                .ToList();

            foreach (var slave in slaves)
            {
                var requestSlave = RemoteCommand.From(request);
                requestSlave.ControladorId = slave.Id;
                requestSlave.Topic = $"controladores/{slave.Id}/comando";
                requestSlave.AdditionalProperties = null;
                var result = await _mqttInterno.Current!.PublishAsync(
                    requestSlave.Topic,
                    requestSlave
                );
                if (result != null)
                {
                    pendings.Add(result);
                }
            }
        }
        if (pendings.Count > 0)
        {
            try
            {
                //foreach (var pendingResponse in pendings)
                //{
                //    Console.WriteLine(
                //        $"Aguardando processo [{pendingResponse.Id}] => {JsonConvert.SerializeObject(pendingResponse.Content, Formatting.Indented)}"
                //    );
                //}
                await Task.WhenAll(pendings.Select(x => x.Completion.Task))
                    .WaitAsync(ResponseRequest.DefaultTimeout, cancellationToken);
            }
            catch { }
            var timeout = RequestTimeout()
                .AddError(
                    "timeout",
                    $"A operação excedeu o tempo limite de espera pela resposta. ({ResponseRequest.DefaultTimeout})"
                );
            foreach (var pendingResponse in pendings)
            {
                if (!pendingResponse.Completion.Task.IsCompleted)
                {
                    var response = ResponseRequest.From(pendingResponse.Content, timeout);
                    response.AdditionalProperties?.Remove(
                        nameof(request.Mqtt.BrokerKey).ToLowerFirst()
                    );
                    await request.Mqtt.PublishAsync($"{pendingResponse.Topic}/resposta", response);
                    //Verificar se precisa chamar Completed pois já foi realizado dentro de PublishAsync
                    MqttManager.Process.Completed(pendingResponse.Content.Id, response);
                }
                else
                {
                    var response = pendingResponse.Completion.Task.Result;
                    response.AdditionalProperties?.Remove(
                        nameof(request.Mqtt.BrokerKey).ToLowerFirst()
                    );
                    await request.Mqtt.PublishAsync($"{pendingResponse.Topic}/resposta", response);
                }
            }
        }

        if (request.ControladorId == null || request.ControladorId == Controlador.ControladorId)
        {
            request.ControladorId ??= Controlador.ControladorId;
            request.Topic = $"controladores/{request.ControladorId}/comando";
            var response = ResponseRequest.From(request);
            response.AdditionalProperties?.Remove(nameof(request.Mqtt.BrokerKey).ToLowerFirst());
            await request.Mqtt.PublishAsync($"{request.Topic}/resposta", response);
            _logger.LogWarning(
                "Aplicação será reiniciada através de uma solicitação remota."
            );
            return await _mediator.Execute(
                new Messages.Commands.Restart(),
                cancellationToken: cancellationToken
            );
        }
        return NoContent();
    }
}
