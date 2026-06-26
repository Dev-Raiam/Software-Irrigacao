using Autenticacao.Dtos;
using SoftwareIrrigacao.Shared;

namespace SoftwareIrrigacao.Autenticacao;

public interface IAutenticacaoApi
{
    Task<Result<Token>> Autenticar(Credencial credencial, CancellationToken cancellationToken);
}
