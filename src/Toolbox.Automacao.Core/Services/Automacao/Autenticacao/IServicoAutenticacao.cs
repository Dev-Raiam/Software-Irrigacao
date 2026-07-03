using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services;

public interface IServicoAutenticacao
{
    Task<Result<Token>> Autenticar(string chave, string segredo, Guid contextoId, CancellationToken cancellationToken);
}
