namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarSolenoide : RemoteCommand
    {
        public Guid SolenoideId { get; init; }
    }
}
