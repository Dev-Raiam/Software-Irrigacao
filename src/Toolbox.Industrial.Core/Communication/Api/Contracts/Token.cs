using Newtonsoft.Json;

namespace Toolbox.Industrial.Core.Communication.Api.Contracts;

internal sealed record Token
{
    public string? TokenAcesso { get; private set; }
    public DateTime Emitido { get; private set; }
    public DateTime Expira { get; private set; }
    public bool Expirado => Expira <= DateTime.UtcNow;

    public Token()
    {
        TokenAcesso = null;
        Expira = DateTime.MinValue;
        Emitido = DateTime.MinValue;
    }

    [JsonConstructor]
    public Token(string? tokenAcesso, DateTime emitido, DateTime expira)
    {
        Emitido = emitido;
        TokenAcesso = tokenAcesso;
        Expira = expira.AddSeconds(-15);
    }

    public void Update(Token token)
    {
        Expira = token.Expira;
        Emitido = token.Emitido;
        TokenAcesso = token.TokenAcesso;
    }
}
