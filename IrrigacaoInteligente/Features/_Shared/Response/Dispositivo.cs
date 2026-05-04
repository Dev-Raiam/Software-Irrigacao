using IrrigacaoInteligente.Domain.Enums;

namespace IrrigacaoInteligente.Features.Shared.Response;

public class Dispositivo
{
    public Guid Id { get; set; }
    public Guid Guid { get; set; }
    public Guid PainelId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public DispositivoTipo Tipo { get; set; }

    // public DispositivoCategoria Categoria { get; set; }
    public bool Arquivado { get; set; }
    public bool Habilitado { get; set; }
    public DispositivoParametros? Parametros { get; set; }
    public List<Porta> Portas { get; set; } = new();
}

public class DispositivoParametros
{
    public double? ValorMinimo { get; set; }
    public double? ValorMaximo { get; set; }
    public DispositivoUnidadeMedida UnidadeMedida { get; set; }
    public double? Precisao { get; set; }
    public double? TempoResposta { get; set; }
    public double? TemperaturaOperacaoMin { get; set; }
    public double? TemperaturaOperacaoMax { get; set; }
}
