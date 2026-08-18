using System.Diagnostics;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace Toolbox.Industrial.Core.Messages
{
    public abstract class Command : Toolbox.Core.Messages.Command
    {
        internal IMqtt Mqtt { get; set; } = null!;
        internal string Topic { get; set; } = null!;
        //internal Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

        public Guid Id { get; init; } = SequentialGuid.NewGuid();
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
    }
}
