using IrrigacaoInteligente.Domain.Enums;

namespace IrrigacaoInteligente.Domain.Entities;

public class Dispositivo
{
    protected Dispositivo() { }

    public Dispositivo(
        Guid id,
        Guid painelId,
        bool arquivado,
        DispositivoTipo tipo,
        Parametros? parametros,
        string descricao
    )
    {
        Id = id;
        PainelId = painelId;
        Arquivado = arquivado;
        Tipo = tipo;
        Parametros = parametros;
        Descricao = descricao;
    }

    public Guid Id { get; private set; }
    public Guid PainelId { get; private set; }
    public Painel Painel { get; private set; } = null!;
    public bool Arquivado { get; private set; }
    public DispositivoTipo Tipo { get; private set; }
    public Parametros? Parametros { get; private set; }
    public Porta? Porta { get; private set; }
    public ICollection<Interface>? Interfaces { get; private set; }
    public string Descricao { get; private set; } = null!;

    public Dispositivo Atualizar(
        bool arquivado,
        DispositivoTipo tipo,
        Parametros? parametros,
        string descricao
    )
    {
        Arquivado = arquivado;
        Tipo = tipo;
        Parametros = parametros;
        Descricao = descricao;
        return this;
    }
}

public class Parametros
{
    public DispositivoUnidadeMedida UnidadeMedida { get; set; }
    public double? ValorMinimo { get; set; }
    public double? ValorMaximo { get; set; }
    public double? Precisao { get; set; }
    public double? TempoResposta { get; set; }
    public double? TemperaturaOperacaoMin { get; set; }
    public double? TemperaturaOperacaoMax { get; set; }
}
