using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Integration
{
    public class LerMonitorPosicao : Command
    {
        public Guid MonitorId { get; init; }
    }
}
