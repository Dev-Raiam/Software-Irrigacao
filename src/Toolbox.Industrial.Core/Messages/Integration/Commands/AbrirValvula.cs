namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AbrirValvula : RemoteCommand
    {
        public Guid ValvulaId { get; init; }
    }
}
