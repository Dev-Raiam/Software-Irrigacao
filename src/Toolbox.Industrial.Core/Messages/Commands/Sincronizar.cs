using Microsoft.Extensions.Logging;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Setup;

namespace Toolbox.Industrial.Core.Messages.Commands;

internal class Sincronizar : InternalCommand { }

internal class SincronizarHandler : CommandHandler, ICommandHandler<Sincronizar>
{
    private readonly IEntityStore _store;
    private readonly IApiClient _apiClient;
    private readonly ILogger<SincronizarHandler> _logger;

    public SincronizarHandler(
        IEntityStore store,
        IApiClient apiClient,
        ILogger<SincronizarHandler> logger
    )
    {
        _store = store;
        _logger = logger;
        _apiClient = apiClient;
    }

    public async Task<ResponseResult> Handle(
        Sincronizar request,
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
        await Sincronizar(Controlador.PainelId, cancellationToken);
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
