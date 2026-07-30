using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NetDevPack.Security.Jwt.Core.Interfaces;
using NetDevPack.Security.Jwt.Core.Model;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Security.Cryptography;

namespace Toolbox.Industrial.Core.Messages.Commands.Handlers;

internal class RegistrarCredenciaisHandler : CommandHandler, ICommandHandler<RegistrarCredenciais>
{
    private readonly AuthGuard _auth;
    private readonly IMediator _mediator;
    private readonly IEntityStore _store;
    private readonly IJsonWebKeyStore _keyStore;
    private readonly ICryptography _cryptography;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<RegistrarCredenciaisHandler> _logger;
    public RegistrarCredenciaisHandler(
        AuthGuard auth,
        IMediator mediator,
        IEntityStore store,
        IJsonWebKeyStore keyStore,
        ICryptography cryptography,
        IHostApplicationLifetime lifetime,
        ILogger<RegistrarCredenciaisHandler> logger
    )
    {
        _auth = auth;
        _store = store;
        _logger = logger;
        _mediator = mediator;
        _lifetime = lifetime;
        _keyStore = keyStore;
        _cryptography = cryptography;
    }

    public async Task<ResponseResult> Handle(
        RegistrarCredenciais request,
        CancellationToken cancellationToken
    )
    {
        #region Validar requisição

        if (
            string.IsNullOrWhiteSpace(request.Segredo)
            || string.IsNullOrWhiteSpace(request.Chave)
            || request.ContextoId == Guid.Empty
            || request.PainelId == Guid.Empty
            || request.ContaId == Guid.Empty
        )
        {
            _logger.LogError(
                "Falha ao configurar credenciais: {Error}",
                "Dados inválidos na requisição"
            );
            return BadRequest().AddError(nameof(request), "Dados inválidos");
        }

        var credentials = new Credentials(request.Chave, request.Segredo, request.ContextoId, Entity.Keys.Api.Jwt.KId);

        var response = await _auth.Authenticate(credentials, cancellationToken);

        if (!response.Success || response.Data == null)
        {
            _logger.LogError("Falha na validação das credenciais: {Error}", response.Error);
            return BadRequest().AddError(nameof(request), "Dados inválidos");
        }

        #endregion Validar requisição

        #region Verificar reconfiguração

        var restart = false;
        Guid.TryParse(
            (
                await _store.FirstOrDefaultAsync<Configuracao>(x =>
                    x.Id == Entity.Keys.Auth.ContextoId
                )
            )?.Valor.ToString(),
            out var contextoId
        );

        if (contextoId != Guid.Empty)
        {
            Guid.TryParse(
                (
                    await _store.FirstOrDefaultAsync<Configuracao>(x =>
                        x.Id == Entity.Keys.PainelId
                    )
                )?.Valor.ToString(),
                out var painelId
            );

            Guid.TryParse(
                (
                    await _store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.ContaId)
                )?.Valor.ToString(),
                out var contaId
            );

            if (
                contextoId != request.ContextoId
                || painelId != request.PainelId
                || contaId != request.ContaId
            )
            {
                restart = await _store.DeleteAllDataCollectionsAsync();
                _logger.LogWarning(
                    "Configuração foi redefinida e todos os dados armazenados anteriormente foram descartados."
                );
            }
        }

        #endregion Verificar reconfiguração

        #region Salvar configurações

        var chave = _cryptography.Encrypt(request.Chave);
        var segredo = _cryptography.Encrypt(request.Segredo);
        Configuracao[] configuracoes =
        [
            new(Entity.Keys.Auth.Chave, chave!),
            new(Entity.Keys.Auth.Segredo, segredo!),
            new(Entity.Keys.Auth.ContextoId, request.ContextoId.ToString()),
            new(Entity.Keys.ContaId, request.ContaId.ToString()),
            new(Entity.Keys.PainelId, request.PainelId.ToString()),
        ];
        foreach (var configuracao in configuracoes)
        {
            await _store.UpsertAsync(configuracao);
        }
        if (!string.IsNullOrWhiteSpace(response.Data.KId))
        {
            await _store.UpsertAsync(new Configuracao(Entity.Keys.Api.Jwt.KId, response.Data.KId));
        }
        _auth.Token.Update(response.Data);
        
        #endregion Salvar configurações

        //Disparar sincronia
        await _mediator.Execute(
            new SincronizarAutomacao { PainelId = request.PainelId },
            cancellationToken: cancellationToken
        );

        if (restart)
        {
            _logger.LogWarning(
                "A aplicação será finalizada para completar o ciclo de reconfiguração."
            );
            _lifetime.StopApplication();
        }

        return NoContent();
    }
}
