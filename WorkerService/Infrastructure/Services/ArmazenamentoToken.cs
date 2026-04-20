namespace WorkerService.Infrastructure.Services;

public sealed class ArmazenamentoToken
{
    public Token? TokenResponse { get; private set; }

    public void AdicionarToken(Token token) => TokenResponse = token;
}
