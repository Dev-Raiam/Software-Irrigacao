using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerSensorCorrente : Command
    {
        public Guid SensorId { get; init; }
    }
}
