namespace Toolbox.Automacao.Core.Models;

public class Token
{
    public string TokenAcesso { get; set; } = string.Empty;
    public string TokenAtualizacao { get; set; } = string.Empty;
    public DateTime Emitido { get; set; }
    public DateTime Expira { get; set; }

    public void Atualizar(Token token)
    {
        TokenAcesso = token.TokenAcesso;
        TokenAtualizacao = token.TokenAtualizacao;
        Emitido = token.Emitido;
        Expira = token.Expira;
    }

    public bool Expirado => Expira <= DateTime.UtcNow;
}
