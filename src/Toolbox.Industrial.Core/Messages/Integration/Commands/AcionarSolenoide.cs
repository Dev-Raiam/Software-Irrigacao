using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class AcionarSolenoide : Command
    {
        public Guid SolenoideId { get; init; }
    }
}
