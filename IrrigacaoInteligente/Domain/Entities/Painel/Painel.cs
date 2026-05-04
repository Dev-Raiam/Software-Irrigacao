namespace IrrigacaoInteligente.Domain.Entities;

public class Painel
{
    protected Painel() { }

    public Painel(
        Guid id,
        bool arquivado,
        bool primario,
        string descricao,
        string referencia,
        ICollection<Modulo> modulos
    )
    {
        Id = id;
        Arquivado = arquivado;
        Primario = primario;
        Descricao = descricao;
        Referencia = referencia;
        Modulos = modulos;
    }

    public Guid Id { get; private set; }
    public bool Arquivado { get; private set; }
    public bool Primario { get; private set; }
    public string Descricao { get; private set; } = null!;
    public string Referencia { get; private set; } = null!;
    public ICollection<Modulo> Modulos { get; private set; } = null!;

    public Painel Atualizar(
        bool arquivado,
        bool primario,
        string descricao,
        string referencia,
        ICollection<Modulo> modulos
    )
    {
        Arquivado = arquivado;
        Primario = primario;
        Descricao = descricao;
        Referencia = referencia;
        Modulos = modulos;
        return this;
    }
}
