namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerSensorTemperatura : RemoteCommand
    {
        public Guid SensorId { get; init; }
    }
}
