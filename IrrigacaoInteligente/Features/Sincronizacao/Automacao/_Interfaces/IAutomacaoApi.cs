using IrrigacaoInteligente.Features.Shared.Response;

namespace IrrigacaoInteligente.Features.Sincronizacao.Automacao.Interfaces;

public interface IAutomacaoApi
{
    Task<List<Painel>?> ObterPaineisAsync(Guid contaId, CancellationToken cancellationToken);
    Task<List<Dispositivo>?> ObterDispositivosPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    );
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
    Task<string?> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    );
}
