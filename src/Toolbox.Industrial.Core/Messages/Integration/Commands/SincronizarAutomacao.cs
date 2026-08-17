using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class SincronizarAutomacao : Command
    {
        public Guid PainelId { get; init; }
        internal bool Reiniciar { get; set; } = true;
    }
}
