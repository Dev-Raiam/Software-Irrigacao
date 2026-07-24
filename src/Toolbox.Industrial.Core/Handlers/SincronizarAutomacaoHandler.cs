using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Data.Entities;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Services.Api;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Handlers;

internal class SincronizarAutomacaoHandler : CommandHandler, ICommandHandler<SincronizarAutomacao>
{
    private readonly IApiClient _apiClient;
    private readonly IRepository _repository;
    private readonly ILogger<SincronizarAutomacaoHandler> _logger;

    public SincronizarAutomacaoHandler(
        IApiClient apiClient,
        IRepository repository,
        ILogger<SincronizarAutomacaoHandler> logger
    )
    {
        _logger = logger;
        _apiClient = apiClient;
        _repository = repository;
    }

    public async Task<ResponseResult> Handle(
        SincronizarAutomacao request,
        CancellationToken cancellationToken
    )
    {
        if (
            !Guid.TryParse(
                _repository.FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.PainelId)?.Value,
                out var painelId
            )
            || painelId == Guid.Empty
        )
        {
            _logger.LogWarning("Sincronização cancelada: PainelId não configurado");
            return BadRequest();
        }

        _logger.LogInformation("Iniciando sincronização para PainelId: {PainelId}", painelId);

        await Sincronizar(painelId, cancellationToken);

        _logger.LogInformation("Sincronização concluída para PainelId: {PainelId}", painelId);

        return NoContent();
    }

    private bool CredenciaisRegistradas()
    {
        var chave = _repository
            .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Auth.Chave)
            ?.Value;

        var segredo = _repository
            .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Auth.Segredo)
            ?.Value;

        var contextoId = _repository
            .FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Auth.ContextoId)
            ?.Value;

        return chave != null && segredo != null && contextoId != null;
    }

    private async Task<Result<List<Models.Controlador>>> ObterControladores(
        Guid painelId,
        CancellationToken cancellationToken
    )
    {
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"automacao/v1/paineis/{painelId}/controladores?status=todos"
        );

        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue(CustomMediaTypes.AutomacaoV1)
        );

        var response = await _apiClient.SendAsync<List<Models.Controlador>>(
            request,
            cancellationToken
        );

        return response;
    }

    private async Task Sincronizar(Guid painelId, CancellationToken cancellationToken)
    {
        using (LogContext.PushProperty("PainelId", painelId))
        {
            _logger.LogInformation("Sincronização Iniciada...");

            if (!CredenciaisRegistradas())
            {
                _logger.LogWarning(
                    "Sincronização cancelada: credenciais de integracao não configuradas"
                );
                return;
            }

            var result = await ObterControladores(painelId, cancellationToken);

            if (result.Success && result.Data != null)
            {
                foreach (var controlador in result.Data)
                {
                    _repository.Upsert(new Controlador(controlador.Id, controlador));
                }

                var quantidadeControladores = result.Data.Count;
                _logger.LogInformation(
                    "Sincronização concluída: {QuantidadeControladores} controladores",
                    quantidadeControladores
                );
            }
            else
            {
                _logger.LogWarning("Falha ao obter controladores: {Error}", result.Error);
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
