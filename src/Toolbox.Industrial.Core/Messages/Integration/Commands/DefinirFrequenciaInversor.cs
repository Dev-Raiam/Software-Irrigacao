namespace Toolbox.Automacao.Core.Messages.Integration
{
    public class DefinirFrequenciaInversor : CommandBase
    {
        public Guid InversorId { get; init; }
        public double Frequencia { get; set; } = 0.0;
    }
}
