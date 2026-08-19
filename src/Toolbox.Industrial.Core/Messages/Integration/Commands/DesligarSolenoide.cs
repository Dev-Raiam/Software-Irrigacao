namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarSolenoide : RemoteCommand
    {
        public Guid SolenoideId { get; init; }
    }
}
