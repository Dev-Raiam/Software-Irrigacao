using System.Text.Json.Serialization;

namespace Toolbox.Industrial.Core.Communication.Api.Contracts;

public sealed record Token
{
    public string? TokenAcesso { get; private set; }
    public string? TokenAtualizacao { get; private set; }
    public DateTime Emitido { get; private set; }
    public DateTime Expira { get; private set; }
    public bool Expirado => Expira <= DateTime.UtcNow;

    public Token()
    {
        TokenAcesso = null;
        TokenAtualizacao = null;
        Emitido = DateTime.MinValue;
        Expira = DateTime.MinValue;
    }

    [JsonConstructor]
    public Token(string? tokenAcesso, string? tokenAtualizacao, DateTime emitido, DateTime expira)
    {
        TokenAcesso = tokenAcesso;
        TokenAtualizacao = tokenAtualizacao;
        Emitido = emitido;
        Expira = expira.AddSeconds(-15);
    }

    public void Update(Token token)
    {
        TokenAcesso = token.TokenAcesso;
        TokenAtualizacao = token.TokenAtualizacao;
        Emitido = token.Emitido;
        Expira = token.Expira;
    }

}
