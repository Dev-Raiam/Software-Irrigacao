namespace Toolbox.Automacao.Core.Models;

public class Configuracao
{
    public string Chave { get; private set; } = null!;
    public string Valor { get; private set; } = null!;

    public Configuracao(string chave, string valor)
    {
        Chave = chave;
        Valor = valor;
    }

    protected Configuracao() { }

    public void Atualizar(string valor)
    {
        Valor = valor;
    }
}
