namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DefinirValvulaProporcional : RemoteCommand
    {
        public Guid ValvulaId { get; init; }
        public int Abertura { get; set; } = 0;
    }
}
