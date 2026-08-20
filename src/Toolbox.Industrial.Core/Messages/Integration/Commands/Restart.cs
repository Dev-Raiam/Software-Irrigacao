using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public Guid? ControladorId { get; internal set; }
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
        var topic = request.Topic;
        var pendings = new List<PendingProcess<Restart>>();
        var controladorId = request.ControladorId;
        var controladores = Controlador.Master ? Application.Controladores : [];
        if (Controlador.Master)
        {
            var slaves = controladores
                .Where(x =>
                    x.Id != Controlador.ControladorId
                    && (controladorId == null || x.Id == controladorId)
                )
                .ToList();

            foreach (var slave in slaves)
            {
                request.ControladorId = slave.Id;
                request.Topic = $"controladores/{slave.Id}/comando";
                request.AdditionalProperties = null;
                var result = await _mqttInterno.Current!.PublishAsync(request.Topic, request);
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
                var start = DateTimeOffset.UtcNow;
                while (
                    !cancellationToken.IsCancellationRequested
                    && pendings.Any(p => !p.Completion.Task.IsCompleted)
                    && (DateTimeOffset.UtcNow - start).TotalMilliseconds
                        < ResponseRequest.Timeout.TotalMilliseconds
                )
                {
                    await Task.Delay(20);
                }
                //await Task.WhenAll(pendings.Select(x => x.Completion.Task))
                //    .WaitAsync(ResponseRequest.Timeout, cancellationToken);
            }
            catch {}
            var timeout = RequestTimeout()
                .AddError(
                    "timeout",
                    $"A operação excedeu o tempo limite de espera pela resposta. ({ResponseRequest.Timeout})"
                );
            foreach (var pendingResponse in pendings)
            {
                if (!pendingResponse.Completion.Task.IsCompleted)
                {
                    var response = ResponseRequest.From(request, timeout);
                    response.AdditionalProperties?.Remove(
                        nameof(request.Mqtt.BrokerKey).ToLowerFirst()
                    );
                    await request.Mqtt.PublishAsync(
                        $"{pendingResponse.Topic}/resposta",
                        response
                    );
                    MqttManager.Process.Completed(request.ProcessId, response);
                }
                else
                {
                    //var result = pendingResponse.Completion.Task.Result;
                    //if (result != null) { }
                }
            }

        }

        if (controladorId == null || controladorId == Controlador.ControladorId)
        {
            //request.ControladorId = controladorId ?? Controlador.ControladorId;
            //request.Topic = $"controladores/{controladorId}/comando";
            request.Topic = topic;
            var response = ResponseRequest.From(request);
            response.AdditionalProperties?.Remove(nameof(request.Mqtt.BrokerKey).ToLowerFirst());
            await request.Mqtt.PublishAsync($"{request.Topic}/resposta", response);
            return await _mediator.Execute(
                new Messages.Commands.Restart(),
                cancellationToken: cancellationToken
            );
        }
        return NoContent();
    }
}
