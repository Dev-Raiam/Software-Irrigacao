using WorkerService.Configurations;
using WorkerService.Features.Shared.Abstractions;

namespace WorkerService.Features.Infrastructure.GerenciamentoToken;

public sealed class GerenciadorToken(
    ArmazenamentoToken _armazenamentoToken,
    IAutenticacaoApi _autenticacaoApi,
    ILogger<GerenciadorToken> _logger,
    IntegracaoConfiguracao _integracaoConfig
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
                _integracaoConfig!.Chave!,
                _integracaoConfig.Segredo!,
                _integracaoConfig.ContextoId,
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
