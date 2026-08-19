namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarMotoBomba : RemoteCommand
    {
        public Guid MotoBombaId { get; init; }
    }
}
