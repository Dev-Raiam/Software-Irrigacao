namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerSensorPressao : RemoteCommand
    {
        public Guid SensorId { get; init; }
    }
}
