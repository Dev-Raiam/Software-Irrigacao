using Toolbox.Automacao.Autenticacao.Dtos;
using Toolbox.Automacao.Core.Api;

namespace Toolbox.Automacao.Autenticacao;

public interface IAutenticacaoApi
{
    Task<Result<Token>> Autenticar(Credencial credencial, CancellationToken cancellationToken);
}
