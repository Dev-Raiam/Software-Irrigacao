using System.Text.Json.Serialization;

namespace Toolbox.Automacao.Core.Models;

public class Token
{
    [JsonPropertyName("tokenAcesso")]
    public string? AccessToken { get; private set; }
    [JsonPropertyName("tokenAtualizacao")]
    public string? RefreshToken { get; private set; }
    [JsonPropertyName("emitido")]
    public DateTime IssuedAt { get; private set; }
    [JsonPropertyName("expira")]
    public DateTime Expire { get; private set; }

    public Token()
    {
        AccessToken = null;
        RefreshToken = null;
        IssuedAt = DateTime.MinValue;
        Expire = DateTime.MinValue;
    }

    [JsonConstructor]
    public Token(string? tokenAcesso, string? tokenAtualizacao, DateTime emitido, DateTime expira)
    {
        AccessToken = tokenAcesso;
        RefreshToken = tokenAtualizacao;
        IssuedAt = emitido;
        Expire = expira.AddSeconds(-15);
    }

    public void Update(Token token)
    {
        AccessToken = token.AccessToken;
        RefreshToken = token.RefreshToken;
        IssuedAt = token.IssuedAt;
        Expire = token.Expire;
    }

    public bool Expired => Expire <= DateTime.UtcNow;
}
