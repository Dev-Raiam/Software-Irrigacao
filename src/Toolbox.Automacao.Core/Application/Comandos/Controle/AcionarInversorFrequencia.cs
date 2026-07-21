namespace Toolbox.Automacao.Core.Application.Comandos
{
    public class AcionarInversorFrequencia : CommandBase
    {
        public Guid InversorId { get; init; }
        public double FrequenciaHz { get; set; } = 0.0;
    }
}
