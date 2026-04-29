using WorkerService.Features.Shared.Abstractions;
using WorkerService.State;

namespace WorkerService.Infrastructure.Auth;

public sealed class GerenciadorToken(
    ArmazenamentoToken _armazenamentoToken,
    IAutenticacaoApi _autenticacaoApi,
    ILogger<GerenciadorToken> _logger,
    CredenciaisAplicacao _credenciaisAplicacao
)
{
    public async Task<Token?> ObterTokenValido(CancellationToken cancellationToken)
    {
        if (
            _armazenamentoToken.TokenResponse?.TokenAcesso is null
            || _armazenamentoToken.TokenResponse?.Expira <= DateTime.UtcNow
        )
        {
            var token = await _autenticacaoApi.Autenticar(
                _credenciaisAplicacao!.IntegracaoChave!,
                _credenciaisAplicacao.IntegracaoSegredo!,
                _credenciaisAplicacao.IntegracaoContextoId,
                cancellationToken
            );
            if (token != null)
            {
                _armazenamentoToken.AdicionarToken(token);
                _logger.LogInformation("Token obtido: {Token}", token.TokenAcesso);
            }
        }
        return _armazenamentoToken.TokenResponse;
    }
}
