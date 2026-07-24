namespace Toolbox.Modulo.Tekon.Models
{
    public class DispositivoContextoLeitura
    {
        public ushort[] HoldingRegisters { get; init; } = null!;
        public bool[] CoilRegisters { get; init; } = null!;
    }
}
