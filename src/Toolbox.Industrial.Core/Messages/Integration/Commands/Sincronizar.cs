using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Messages.Integration.Events;
using Toolbox.Industrial.Core.Setup;

namespace Toolbox.Industrial.Core.Messages.Integration.Commands;

public class Sincronizar : RemoteCommand
{
    /// <summary>
    /// Se informado, somente o controlador especificado realizará a sincronização.
    /// Caso contrário, todos os controladores, Master e Slave, realizarão a sincronização.
    /// Após a sincronização, a aplicação poderá ser reiniciada automaticamente para aplicar a nova configuração.
    /// </summary>
    public Guid? ControladorId { get; set; }
}

internal class SincronizarHandler : CommandHandler, ICommandHandler<Sincronizar>
{
    private readonly MqttManager _mqttInterno;
    private readonly IMediator _mediator;
    private readonly IEntityStore _store;
    private readonly IApiClient _apiClient;
    private readonly ILogger<SincronizarHandler> _logger;

    public SincronizarHandler(
        IMediator mediator,
        IEntityStore store,
        IApiClient apiClient,
        ILogger<SincronizarHandler> logger,
        [FromKeyedServices(Mqtt.Interno)] MqttManager mqttInterno
    )
    {
        _store = store;
        _logger = logger;
        _mediator = mediator;
        _apiClient = apiClient;
        _mqttInterno = mqttInterno;
    }

    public async Task<ResponseResult> Handle(
        Sincronizar request,
        CancellationToken cancellationToken
    )
    {
        if (Controlador.PainelId == Guid.Empty)
        {
            _logger.LogWarning("Sincronização cancelada por ausência de configuração.");
            var result = BadRequest()
                .AddError(
                    nameof(Controlador.PainelId),
                    "Sincronização cancelada por ausência de configuração."
                );

            var response = ResponseRequest.From(request, result);
            response.AdditionalProperties?.Remove(nameof(request.Mqtt.BrokerKey).ToLowerFirst());
            await request.Mqtt.PublishAsync($"{request.Topic}/resposta", response);
            //Coloquei dentro do Mqtt.PublishAsync
            //MqttManager.Process.Completed(request.ProcessId, response);
            return result;
        }
        var restart = false;
        //var topic = request.Topic;
        //var controladorId = request.ControladorId;
        var controladores = Controlador.Master ? Application.Controladores : [];
        if (request.ControladorId == null || request.ControladorId == Controlador.ControladorId)
        {
            await Sincronizar(Controlador.PainelId, cancellationToken);
            restart = true;
        }

        var pendings = new List<PendingProcess<Sincronizar>>();
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
        if (restart)
        {
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
                        await request.Mqtt.PublishAsync(
                            $"{pendingResponse.Topic}/resposta",
                            response
                        );
                        //Verificar se precisa chamar Completed pois já foi realizado dentro de PublishAsync
                        MqttManager.Process.Completed(pendingResponse.Content.Id, response);
                    }
                    else
                    {
                        var response = pendingResponse.Completion.Task.Result;
                        response.AdditionalProperties?.Remove(
                            nameof(request.Mqtt.BrokerKey).ToLowerFirst()
                        );
                        await request.Mqtt.PublishAsync(
                            $"{pendingResponse.Topic}/resposta",
                            response
                        );
                    }
                }
            }
            request.ControladorId ??= Controlador.ControladorId;
            request.Topic = $"controladores/{request.ControladorId}/comando";
            var resposta = ResponseRequest.From(request);
            resposta.AdditionalProperties?.Remove(nameof(request.Mqtt.BrokerKey).ToLowerFirst());
            await request.Mqtt.PublishAsync($"{request.Topic}/resposta", resposta);
            //await _mqttInterno.Current!.PublishAsync(
            //    $"{request.Topic}/resposta",
            //    ResponseRequest.From(request)
            //);
            _logger.LogWarning(
                "A aplicação será finalizada para completar o ciclo remoto de sincronização de dados."
            );
            return await _mediator.Execute(
                new Messages.Commands.Restart(),
                cancellationToken: cancellationToken
            );
        }

        return NoContent();
    }

    private async Task Sincronizar(Guid painelId, CancellationToken cancellationToken)
    {
        if (!Application.HasCredentials)
        {
            _logger.LogWarning("Sincronização cancelada por ausência de configuração.");
            return;
        }

        var result = await _apiClient.ObterControladores(painelId, cancellationToken);

        if (result.Success && result.Data != null)
        {
            Application._controladores.Clear();

            foreach (var controlador in result.Data)
            {
                var ctrl = new Controlador(controlador.Id, controlador);
                await _store.UpsertAsync(ctrl);
                Application._controladores.Add(ctrl);
            }
            var controladores = _store.Query<Controlador>().ToList();
            foreach (var controlador in controladores)
            {
                if (Application._controladores.NotAny(c => c.Id == controlador.Id))
                {
                    await _store.DeleteAsync(controlador);
                }
            }
        }
        else
        {
            _logger.LogWarning(
                exception: result.Exception,
                "Falha ao obter controladores: {Error}",
                result.Error
            );
        }
    }

    //public List<Dispositivo> ObterDispositivos(CancellationToken cancellationToken = default)
    //{
    //    var controlador = ObterControladorMaster(cancellationToken);

    //    List<Dispositivo> dispositivos = new List<Dispositivo>();

    //    if (controlador == null)
    //        return dispositivos;

    //    foreach (var dispositivo in controlador.Dispositivos)
    //    {
    //        dispositivos.Add(dispositivo);
    //    }

    //    return dispositivos;
    //}

    //public List<Modulo> ObterModulos(CancellationToken cancellationToken = default)
    //{
    //    var controlador = ObterControladorMaster(cancellationToken);

    //    List<Modulo> modulos = new List<Modulo>();

    //    if (controlador == null)
    //        return modulos;

    //    foreach (var modulo in controlador.Modulos)
    //    {
    //        modulos.Add(modulo);
    //    }

    //    return modulos;
    //}

    //private Models.Controlador? ObterControladorMaster(CancellationToken cancellationToken = default)
    //{
    //    var colecao = _database.GetCollection<Controlador>(Entity.GetCollection<Controlador>());

    //    var configuracao = colecao.FindOne(c => c.Value.Master);

    //    var controlador = configuracao == null ? null : configuracao.Value;

    //    return controlador;
    //}
}
