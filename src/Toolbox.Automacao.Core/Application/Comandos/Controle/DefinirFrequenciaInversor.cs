namespace Toolbox.Automacao.Core.Application.Comandos
{
    public class DefinirFrequenciaInversor : CommandBase
    {
        public Guid InversorId { get; init; }
        public double Frequencia { get; set; } = 0.0;
    }
}
