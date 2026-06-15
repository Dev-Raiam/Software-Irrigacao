using Autenticacao.Models;
using IrrigacaoInteligente.Core;

namespace Autenticacao.Services;

public interface IAutenticacaoApi
{
    Task<Result<Token>> Autenticar(Credencial credencial, CancellationToken cancellationToken);
}
