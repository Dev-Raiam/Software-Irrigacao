using WorkerService.Features.Shared.Response;

namespace WorkerService.Features.Sincronizacao.Automacao;

public interface IAutomacaoApi
{
    Task<List<Painel>?> ObterPaineisAsync(Guid contaId, CancellationToken cancellationToken);
    Task<List<Dispositivo>?> ObterDispositivosPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    );

    // Task<List<ModuloResponse>?> ObterControladoresPorPainelAsync(
    //     Guid painelId,
    //     CancellationToken cancellationToken
    // );
    Task<List<Porta>?> ObterPortasPorControladorAsync(
        Guid painelId,
        Guid controladorId,
        CancellationToken cancellationToken
    );
    Task<List<Porta>?> ObterPortasPorModuloAsync(
        Guid painelId,
        Guid moduloId,
        CancellationToken cancellationToken
    );
    Task<List<Interface>?> ObterInterfacesPorControladorAsync(
        Guid painelId,
        Guid controladorId,
        CancellationToken cancellationToken
    );
    Task<List<Interface>?> ObterInterfacesPorModuloAsync(
        Guid painelId,
        Guid moduloId,
        CancellationToken cancellationToken
    );
}
