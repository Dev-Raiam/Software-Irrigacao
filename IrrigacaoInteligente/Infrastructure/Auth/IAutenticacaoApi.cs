using IrrigacaoInteligente.State;

namespace IrrigacaoInteligente.Infrastructure.Auth;

public interface IAutenticacaoApi
{
    Task<Token?> Autenticar(
        string chave,
        string segredo,
        Guid contextoId,
        CancellationToken cancellationToken
    );
}
