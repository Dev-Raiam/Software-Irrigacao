using LiteDB;

namespace Toolbox.Automacao.Core.Models;

public class Configuracao
{
    [BsonId]
    public string Chave { get; private set; } = null!;
    public string Valor { get; private set; } = null!;

    protected Configuracao() { }
    public Configuracao(string chave, string valor)
    {
        Chave = chave;
        Valor = valor;
    }
}
