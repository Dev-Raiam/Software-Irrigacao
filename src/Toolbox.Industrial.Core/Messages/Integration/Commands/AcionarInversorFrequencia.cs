using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarInversorFrequencia : Command
    {
        public Guid InversorId { get; init; }
        public double FrequenciaHz { get; set; } = 0.0;
    }
}
