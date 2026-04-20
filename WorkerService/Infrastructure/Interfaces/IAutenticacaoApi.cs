using WorkerService.Infrastructure.Services;

namespace WorkerService.Infrastructure.Interfaces;

public interface IAutenticacaoApi
{
    Task<Token?> Autenticar(
        string chave,
        string segredo,
        Guid contextoId,
        CancellationToken cancellationToken
    );
}
