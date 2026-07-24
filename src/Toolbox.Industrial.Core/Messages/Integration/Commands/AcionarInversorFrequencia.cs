namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarInversorFrequencia : CommandBase
    {
        public Guid InversorId { get; init; }
        public double FrequenciaHz { get; set; } = 0.0;
    }
}
