using Autenticacao.Dtos;

namespace SoftwareIrrigacao.Autenticacao;

public interface ICredenciaisAutenticacao
{
    Task<Credencial> ObterCredencial(CancellationToken cancellationToken);
}
