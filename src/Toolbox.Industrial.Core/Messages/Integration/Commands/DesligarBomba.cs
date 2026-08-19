namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarBomba : RemoteCommand
    {
        public Guid BombaId { get; init; }
    }
}
