using Org.BouncyCastle.Bcpg;

namespace IrrigacaoInteligente.Features;

public class TelemetriaResposta
{
    public Guid ControladorId { get; set; }
    public List<Leitura> Leituras { get; set; } = [];

    public class Leitura
    {
        public Guid DispositivoId { get; set; }
        public string Descricao { get; set; } = null!;
        public string? Dados { get; set; }
    }
}
