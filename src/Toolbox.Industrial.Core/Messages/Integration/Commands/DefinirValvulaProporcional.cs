using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class DefinirValvulaProporcional : Command
    {
        public Guid ValvulaId { get; init; }
        public int Abertura { get; set; } = 0;
    }
}
