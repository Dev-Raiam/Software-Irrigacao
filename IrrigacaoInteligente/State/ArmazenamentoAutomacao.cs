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
        public string? Dados { get; set; }

        // public bool Invalido =>
        //     Paineis.Count == 0
        //     || Modulos.Count == 0
        //     || Portas.Count == 0
        //     || Interfaces.Count == 0
        //     || Dispositivos.Count == 0;
        public bool Invalido => string.IsNullOrEmpty(Dados);
    }
}
