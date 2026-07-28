using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AbrirValvula : Command
    {
        public Guid ValvulaId { get; init; }
    }
}
