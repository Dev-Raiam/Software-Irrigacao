using Toolbox.Core.Messages;

namespace Toolbox.Automacao.Core.Application
{
    public abstract class CommandBase : Command
    {
        public Guid Id { get; init; }
        public Guid CorrelationId { get; init; } = Guid.NewGuid();
        public DateTime CriadoEm { get; init; } = DateTime.UtcNow;
    }
}
