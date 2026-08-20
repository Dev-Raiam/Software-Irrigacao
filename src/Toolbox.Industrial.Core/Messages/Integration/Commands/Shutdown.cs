using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages.Integration.Events;
using Toolbox.Industrial.Core.Setup;

namespace Toolbox.Industrial.Core.Messages.Integration.Commands;

public class Shutdown : RemoteCommand 
{
    /// <summary>
    /// Se informado, somente o controlador especificado realizará a sincronização.
    /// Caso contrário, todos os controladores, Master e Slave, realizarão a sincronização.
    /// Após a sincronização, a aplicação poderá ser reiniciada automaticamente para aplicar a nova configuração.
    /// </summary>
    public Guid? ControladorId { get; internal set; }
}

internal class ShutdownHandler : CommandHandler, ICommandHandler<Shutdown>
{
    private readonly IMediator _mediator;
    private readonly MqttManager _mqttInterno;
    private readonly ILogger<RebootHandler> _logger;

    public ShutdownHandler(
        IMediator mediator,
        ILogger<RebootHandler> logger,
        [FromKeyedServices(Mqtt.Interno)] MqttManager mqttInterno
    )
    {
        _logger = logger;
        _mediator = mediator;
        _mqttInterno = mqttInterno;
    }

    public async Task<ResponseResult> Handle(
        Shutdown request,
        CancellationToken cancellationToken
    )
    {
        var pendings = new List<PendingProcess<Shutdown>>();
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
                await Task.WhenAll(pendings.Select(x => x.Completion.Task))
                    .WaitAsync(ResponseRequest.Timeout, cancellationToken);
            }
            catch (TimeoutException)
            {
                var timeout = RequestTimeout()
                    .AddError(
                        "timeout",
                        $"A operação excedeu o tempo limite de espera pela resposta. ({ResponseRequest.Timeout})"
                    );
                foreach (var pendingResponse in pendings)
                {
                    var result = pendingResponse.Completion.Task.Result;
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
                }
            }
        }
        if (controladorId == null || controladorId == Controlador.ControladorId)
        {
            request.ControladorId = controladorId;
            request.Topic = $"controladores/{controladorId}/comando";
            var response = ResponseRequest.From(request);
            response.AdditionalProperties?.Remove(nameof(request.Mqtt.BrokerKey).ToLowerFirst());
            await request.Mqtt.PublishAsync($"{request.Topic}/resposta", response);
            return await _mediator.Execute(new Messages.Commands.Shutdown(), cancellationToken: cancellationToken);
        }

        return NoContent();
    }
}
