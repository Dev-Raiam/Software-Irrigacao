namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarInversorFrequencia : RemoteCommand
    {
        public Guid InversorId { get; init; }
    }
}
