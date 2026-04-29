using WorkerService.Features.Shared.Abstractions;
using WorkerService.State;

namespace WorkerService.Infrastructure.Auth;

public interface IAutenticacaoApi
{
    Task<Token?> Autenticar(
        string chave,
        string segredo,
        Guid contextoId,
        CancellationToken cancellationToken
    );
}
