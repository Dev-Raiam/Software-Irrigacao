namespace IrrigacaoInteligente.Features;

public class TelemetriaResponse
{
    public Guid ControladorId { get; set; }
    public Guid DispositivoId { get; set; }
    public string Descricao { get; set; } = null!;
    public dynamic? Dados { get; set; }
}
