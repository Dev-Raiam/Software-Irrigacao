using WorkerService.Features.Shared.Abstractions;

namespace WorkerService.Features.Infrastructure.Auth;

public interface IAutenticacaoApi
{
    Task<Token?> Autenticar(
        string chave,
        string segredo,
        Guid contextoId,
        CancellationToken cancellationToken
    );
}
