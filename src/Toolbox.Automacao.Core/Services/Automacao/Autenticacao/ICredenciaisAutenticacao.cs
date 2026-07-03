using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public interface ICredenciaisAutenticacao
{
    Task<Credencial> ObterCredencial(CancellationToken cancellationToken);
}
