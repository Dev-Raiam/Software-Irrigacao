using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarInversorFrequencia : Command
    {
        public Guid InversorId { get; init; }
    }
}
