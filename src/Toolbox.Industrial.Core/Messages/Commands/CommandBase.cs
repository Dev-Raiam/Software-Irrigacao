using Toolbox.Core.Messages;

namespace Toolbox.Automacao.Core.Messages
{
    public abstract class CommandBase : Command
    {
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
    }
}
