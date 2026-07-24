namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarInversorFrequencia : CommandBase
    {
        public Guid InversorId { get; init; }
    }
}
