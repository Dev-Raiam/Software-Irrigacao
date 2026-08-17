using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Context;
using System.Net.Http.Headers;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Setup;
using Toolbox.Industrial.Core.Telemetry;

namespace Toolbox.Industrial.Core.Messages.Commands.Handlers;

internal class SincronizarAutomacaoHandler : CommandHandler, ICommandHandler<SincronizarAutomacao>
{
    private readonly IMqtt _mqtt;
    private readonly IEntityStore _store;
    private readonly IApiClient _apiClient;
    private readonly ILogger<SincronizarAutomacaoHandler> _logger;

    public SincronizarAutomacaoHandler(
        IEntityStore store,
        IApiClient apiClient,
        [FromKeyedServices(Mqtt.Interno)] MqttManager mqttInterno,
        ILogger<SincronizarAutomacaoHandler> logger
    )
    {
        _mqtt = mqttInterno.Current!;
        _store = store;
        _logger = logger;
        _apiClient = apiClient;
    }

    public async Task<ResponseResult> Handle(
        SincronizarAutomacao request,
        CancellationToken cancellationToken
    )
    {
        var painelId = await _store.ObterConfiguracao<Guid>(Entity.Keys.PainelId);
        if (painelId == Guid.Empty)
        {
            _logger.LogWarning("Sincronização cancelada por ausência de configuração.");
            return BadRequest();
        }
        var controladores = Controlador.Master ? _store.Query<Controlador>().ToList() : [];
        await Sincronizar(painelId, cancellationToken);
        if (request.Reiniciar)
        {
            if (Controlador.Master)
            {
                foreach (var controlador in controladores.Where(x => x.Id != Controlador.ControladorId))
                {
                    var serializer = JsonConvert.DefaultSettings!.Invoke();
                    serializer.Formatting = Formatting.Indented;
                    serializer.TypeNameHandling = TypeNameHandling.Objects;
                    var sincronizar = JsonConvert.SerializeObject(
                        request,
                        serializer
                    );
                    await _mqtt.PublishAsync($"controladores/{controlador.Id}/comando", sincronizar);
                }
            }
            _logger.LogWarning(
                "A aplicação será finalizada para completar o ciclo de sincronização de dados."
            );
            await Application.Restart();
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
                foreach (var controlador in result.Data)
                {
                    await _store.UpsertAsync(new Controlador(controlador.Id, controlador));
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
