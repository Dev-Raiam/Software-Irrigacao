namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DefinirFrequenciaInversor : Command
    {
        public Guid InversorId { get; init; }
        public double Frequencia { get; set; } = 0.0;
    }
}
