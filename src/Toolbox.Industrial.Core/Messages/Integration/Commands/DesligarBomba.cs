using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DesligarBomba : Command
    {
        public Guid BombaId { get; init; }
    }
}
