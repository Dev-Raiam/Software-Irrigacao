namespace WorkerService.Features.Shared.Abstractions;

public sealed class ArmazenamentoToken
{
    public Token? TokenResponse { get; private set; }

    public void AdicionarToken(Token token) => TokenResponse = token;
}
