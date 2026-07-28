using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerSensorPh : Command
    {
        public Guid SensorId { get; init; }
    }
}
