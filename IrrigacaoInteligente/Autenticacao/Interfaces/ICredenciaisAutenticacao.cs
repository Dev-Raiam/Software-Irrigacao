using Autenticacao.Models;

namespace Autenticacao.Interfaces;

public interface ICredenciaisAutenticacao
{
    Task<Credencial> ObterCredencial(CancellationToken cancellationToken);
}
