using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class SincronizarAutomacao : Command
    {
        public Guid PainelId { get; init; }
        public Guid? ControladorId { get; init; }
        internal bool Reiniciar { get; set; } = true;
    }
}
