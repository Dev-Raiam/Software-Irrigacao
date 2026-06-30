using Toolbox.Automacao.Autenticacao.Dtos;

namespace Toolbox.Automacao.Autenticacao;

public interface ICredenciaisAutenticacao
{
    Task<Credencial> ObterCredencial(CancellationToken cancellationToken);
}
