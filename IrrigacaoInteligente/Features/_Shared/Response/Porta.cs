using IrrigacaoInteligente.Domain.Enums;

namespace IrrigacaoInteligente.Features.Shared.Response
{
    public class Porta
    {
        public Guid Id { get; set; }
        public DispositivoConectado Dispositivo { get; set; } = null!;
        public string Nome { get; set; } = string.Empty;
        public PortaTipo Tipo { get; set; }
        public PortaSinal Sinal { get; set; }
        public string? EnderecoBorne { get; set; }
        public PortaStatus Status { get; set; }
        public string? EnderecoLogico { get; set; }
    }

    public class DispositivoConectado
    {
        public Guid? Id { get; init; }
    }
}
