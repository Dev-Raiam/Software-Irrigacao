using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Context;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Setup;
using CommandHandler = Toolbox.Industrial.Core.Messages.Commands.Handlers.CommandHandler;

namespace Toolbox.Industrial.Core.Messages.Integration.Commands.Handlers;

internal class SincronizarAutomacaoHandler : CommandHandler, ICommandHandler<SincronizarAutomacao>
{
    private readonly MqttManager _mqttInterno;
    private readonly IEntityStore _store;
    private readonly IApiClient _apiClient;
    private readonly ILogger<SincronizarAutomacaoHandler> _logger;

    public SincronizarAutomacaoHandler(
        IMediator mediator,
        IEntityStore store,
        IApiClient apiClient,
        ILogger<SincronizarAutomacaoHandler> logger,
        [FromKeyedServices(Mqtt.Interno)] MqttManager mqttInterno
    )
    {
        _store = store;
        _logger = logger;
        _apiClient = apiClient;
        _mqttInterno = mqttInterno;
    }

    public async Task<ResponseResult> Handle(
        SincronizarAutomacao request,
        CancellationToken cancellationToken
    )
    {
        if (Controlador.PainelId == Guid.Empty)
        {
            _logger.LogWarning("Sincronização cancelada por ausência de configuração.");
            return BadRequest()
                .AddError(
                    nameof(Controlador.PainelId),
                    "Sincronização cancelada por ausência de configuração."
                );
        }
        var restart = false;
        var controladores = Controlador.Master ? Application.Controladores : [];
        if (request.ControladorId == null || request.ControladorId == Controlador.ControladorId)
        {
            await Sincronizar(Controlador.PainelId, cancellationToken);
            restart = true;
        }
        if (!request.Interno)
        {
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
                    request.Topic = $"controladores/{slave.Id}/comando";
                    await _mqttInterno.Current!.PublishAsync(
                        request.Topic,
                        JsonConvert.SerializeObject(request, Mqtt.Serializer)
                    );
                }
            }
            if (restart)
            {
                if (request.Mqtt != null)
                {
                    await request.Mqtt!.PublishAsync($"{request.Topic}/resposta", JsonConvert.SerializeObject(new Response(request), Mqtt.Serializer));
                }
                _logger.LogWarning(
                    "A aplicação será finalizada para completar o ciclo de sincronização de dados."
                );
                await Application.Restart();
            }
        }

        return NoContent();
    }

    private async Task<Result<List<Communication.Api.Contracts.Controlador>>> ObterControladores(
        Guid painelId,
        CancellationToken cancellationToken
    )
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"automacao/v1/paineis/{painelId}/controladores?status=todos"
        );

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypes.Industrial.V1));

        var response = await _apiClient.SendAsync<List<Communication.Api.Contracts.Controlador>>(
            request,
            cancellationToken
        );

        return response;
    }

    private async Task Sincronizar(Guid painelId, CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("PainelId", painelId))
        {
            if (!Application.HasCredentials)
            {
                _logger.LogWarning("Sincronização cancelada por ausência de configuração.");
                return;
            }

            var result = await ObterControladores(painelId, cancellationToken);

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
    }

    //public Models.Controlador? ObterControlador(CancellationToken cancellationToken = default)
    //{
    //    var controlador = ObterControladorMaster(cancellationToken);
    //    return controlador;
    //}

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
