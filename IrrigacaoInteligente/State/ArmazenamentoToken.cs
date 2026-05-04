namespace IrrigacaoInteligente.State;

public sealed class ArmazenamentoToken
{
    public Token? TokenResponse { get; private set; }

    public void AdicionarToken(Token token) => TokenResponse = token;
}

public sealed record Token(
    string TokenAcesso,
    string TokenAtualizacao,
    DateTime Emitido,
    DateTime Expira
);
