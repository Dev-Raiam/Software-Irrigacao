using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerSensorTensao : Command
    {
        public Guid SensorId { get; init; }
    }
}
