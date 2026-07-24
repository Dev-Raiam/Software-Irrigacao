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

    public RegistrarCredenciaisHandler(
        IMediator mediator,
        IEntityStore store,
        ICryptography cryptography
    )
    {
        _mediator = mediator;
        _store = store;
        _cryptography = cryptography;
    }

    public async Task<ResponseResult> Handle(
        RegistrarCredenciais request,
        CancellationToken cancellationToken
    )
    {
        var chave = _cryptography.Encrypt(request.Chave);
        var segredo = _cryptography.Encrypt(request.Segredo);

        Configuracao[] configuracoes =
        [
            new(Entity.Keys.Auth.Chave, chave!),
            new(Entity.Keys.Auth.Segredo, segredo!),
            new(Entity.Keys.Auth.ContextoId, request.ContextoId.ToString()),
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

        return NoContent();
    }
}
