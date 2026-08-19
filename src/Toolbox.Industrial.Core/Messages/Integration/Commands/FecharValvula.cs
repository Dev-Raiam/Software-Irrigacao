namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class FecharValvula : RemoteCommand
    {
        public Guid ValvulaId { get; init; }
    }
}
