namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerSensorTensao : RemoteCommand
    {
        public Guid SensorId { get; init; }
    }
}
