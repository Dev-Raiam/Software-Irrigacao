namespace Toolbox.Industrial.Driver.TekonBkp.Models
{
    public class DispositivoContextoLeitura
    {
        public ushort[] HoldingRegisters { get; init; } = null!;
        public bool[] CoilRegisters { get; init; } = null!;
    }
}
