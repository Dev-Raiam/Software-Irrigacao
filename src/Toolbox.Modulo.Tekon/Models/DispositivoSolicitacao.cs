namespace Toolbox.Modulo.Tekon.Models
{
    public class DispositivoSolicitacaoLeitura
    {
        public string Modelo { get; private set; } = null!;

        public int SlaveId { get; private set; }

        public int? Index { get; private set; }

        public DispositivoSolicitacaoLeitura(string modelo, int slaveId, int? index = null) 
        {
            Modelo = modelo;
            SlaveId = slaveId;
            Index = index;
        }
    }
    public class DispositivoSolicitacaoEscrita
    {
        public string Modelo { get; init; } = null!;

        public int SlaveId { get; init; }

        public int? Index { get; init; }
        public bool? Valor { get; init; }
    }
}
