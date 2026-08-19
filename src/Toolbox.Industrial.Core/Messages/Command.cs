using System.Diagnostics;
using Newtonsoft.Json;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace Toolbox.Industrial.Core.Messages
{
    public abstract class RemoteCommand : Toolbox.Core.Messages.Command
    {
        private readonly DateTimeOffset _timestamp;

        protected RemoteCommand()
        {
            _timestamp = DateTimeOffset.UtcNow;
        }

        internal Mqtt Mqtt { get; set; } = null!;
        internal string Topic { get; set; } = null!;
        internal TimeSpan Latency => _timestamp - Timestamp;
        internal Stopwatch Stopwatch { get; init; } = Stopwatch.StartNew();

        public Guid Id { get; init; } = SequentialGuid.NewGuid();
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public string ProcessId => $"{Id}-{Mqtt?.BrokerKey}";

        [JsonIgnore]
        public bool HasAdditionalProperties => AdditionalProperties?.Count > 0;

        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public abstract class InternalCommand : Toolbox.Core.Messages.Command { }
}
