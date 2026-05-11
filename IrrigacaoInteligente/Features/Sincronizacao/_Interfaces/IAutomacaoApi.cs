namespace IrrigacaoInteligente.Features.Sincronizacao.Interfaces;

public interface IAutomacaoApi
{
    Task<List<Dictionary<string, dynamic>>?> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    );
}
