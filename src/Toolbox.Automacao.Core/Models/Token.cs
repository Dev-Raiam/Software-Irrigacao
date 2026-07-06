using System.Text.Json.Serialization;

namespace Toolbox.Automacao.Core.Models;

public class Token
{
    public string? TokenAcesso { get; private set; }
    public string? TokenAtualizacao { get; private set; }
    public DateTime Emitido { get; private set; }
    public DateTime Expira { get; private set; }

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
        Expira = expira;
        // Validações
    }

    public void Atualizar(
        string tokenAcesso,
        string tokenAtualizacao,
        DateTime emitido,
        DateTime expira
    )
    {
        TokenAcesso = tokenAcesso;
        TokenAtualizacao = tokenAtualizacao;
        Emitido = emitido;
        Expira = expira;
    }

    public Token DecrementarExpiracao()
    {
        Expira = Expira.AddSeconds(-15);
        return this;
    }

    public bool Expirado => Expira <= DateTime.UtcNow;
}
