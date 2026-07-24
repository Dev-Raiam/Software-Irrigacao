namespace Toolbox.Industrial.Driver.Tekon.Models
{
    public class DispositivoContextoLeitura
    {
        public ushort[] HoldingRegisters { get; init; } = null!;
        public bool[] CoilRegisters { get; init; } = null!;
    }
}
