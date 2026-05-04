using IrrigacaoInteligente.State;

namespace IrrigacaoInteligente.Infrastructure.Auth;

public sealed class GerenciadorToken
{
    private readonly ArmazenamentoToken _armazenamentoToken;
    private readonly IAutenticacaoApi _autenticacaoApi;
    private readonly ILogger<GerenciadorToken> _logger;
    private readonly CredenciaisAplicacao _credenciaisAplicacao;

    public GerenciadorToken(
        ArmazenamentoToken armazenamentoToken,
        IAutenticacaoApi autenticacaoApi,
        ILogger<GerenciadorToken> logger,
        CredenciaisAplicacao credenciaisAplicacao
    )
    {
        _armazenamentoToken = armazenamentoToken;
        _autenticacaoApi = autenticacaoApi;
        _logger = logger;
        _credenciaisAplicacao = credenciaisAplicacao;
    }

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
            if (token is not null)
            {
                _armazenamentoToken.AdicionarToken(token);
                _logger.LogInformation("Token obtido: {Token}", token.TokenAcesso);
            }
        }
        return _armazenamentoToken.TokenResponse;
    }
}
