using System.Data;

namespace IrrigacaoInteligente.Domain.Entities;

public class Controlador
{
    public Guid Id { get; private set; }
    public string Configuracao { get; private set; } = null!;

    protected Controlador() { }

    public Controlador(Guid id, string configuracao)
    {
        Id = id;
        Configuracao = configuracao;
    }

    public Controlador Atualizar(string configuracao)
    {
        Configuracao = configuracao;
        return this;
    }
}
