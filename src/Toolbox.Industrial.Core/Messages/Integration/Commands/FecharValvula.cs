using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class FecharValvula : Command
    {
        public Guid ValvulaId { get; init; }
    }
}
