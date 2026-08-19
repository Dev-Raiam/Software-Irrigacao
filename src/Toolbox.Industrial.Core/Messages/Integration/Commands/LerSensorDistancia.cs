namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerSensorDistancia : RemoteCommand
    {
        public Guid SensorId { get; init; }
    }
}
