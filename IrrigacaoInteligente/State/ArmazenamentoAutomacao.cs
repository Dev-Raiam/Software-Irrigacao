using IrrigacaoInteligente.Domain.Entities;

namespace IrrigacaoInteligente.State
{
    public class ArmazenamentoAutomacao
    {
        public List<Painel> Paineis { get; set; } = [];
        public List<Modulo> Modulos { get; set; } = [];
        public List<Porta> Portas { get; set; } = [];
        public List<Interface> Interfaces { get; set; } = [];
        public List<Dispositivo> Dispositivos { get; set; } = [];
    }
}
