using System.Data;

namespace IrrigacaoInteligente.Domain.Entities;

public class Controlador
{
    public Guid Id { get; private set; }
    public string Configuracao { get; private set; } = null!;

    public Controlador(string configuracao)
    {
        Id = Guid.NewGuid();
        Configuracao = configuracao;
    }

    public Controlador Atualizar(string configuracao)
    {
        Configuracao = configuracao;
        return this;
    }
}
