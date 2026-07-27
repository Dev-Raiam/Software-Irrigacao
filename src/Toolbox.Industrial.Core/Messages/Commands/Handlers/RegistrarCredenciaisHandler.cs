using Microsoft.Extensions.Hosting;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Messages.Integration;
using Toolbox.Industrial.Core.Security.Cryptography;

namespace Toolbox.Industrial.Core.Messages.Commands.Handlers;

internal class RegistrarCredenciaisHandler : CommandHandler, ICommandHandler<RegistrarCredenciais>
{
    private readonly IMediator _mediator;
    private readonly IEntityStore _store;
    private readonly ICryptography _cryptography;
    private readonly IHostApplicationLifetime _lifetime;

    public RegistrarCredenciaisHandler(
        IMediator mediator,
        IEntityStore store,
        ICryptography cryptography,
        IHostApplicationLifetime lifetime
    )
    {
        _store = store;
        _mediator = mediator;
        _lifetime = lifetime;
        _cryptography = cryptography;
    }

    public async Task<ResponseResult> Handle(
        RegistrarCredenciais request,
        CancellationToken cancellationToken
    )
    {
        Guid.TryParse(
            (
                await _store.FirstOrDefaultAsync<Configuracao>(x =>
                    x.Id == Entity.Keys.Auth.ContextoId
                )
            )?.Value.ToString(),
            out var contextoId
        );
        Guid.TryParse(
            (
                await _store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.PainelId)
            )?.Value.ToString(),
            out var painelId
        );

        Guid.TryParse(
            (
                await _store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.ContaId)
            )?.Value.ToString(),
            out var contaId
        );

        var restart = false;
        if (contextoId != request.ContextoId ||
            contaId != request.ContaId ||
            painelId != request.PainelId)
        {
            restart = await _store.DeleteAllCollectionsAsync();
        }

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

        await _mediator.Execute(
            new SincronizarAutomacao { PainelId = request.PainelId },
            cancellationToken: cancellationToken
        );
        if (restart)
        {
            _lifetime.StopApplication();
        }

        return NoContent();
    }
}
