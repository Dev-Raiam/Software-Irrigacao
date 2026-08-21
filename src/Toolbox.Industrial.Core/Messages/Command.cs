using System.Diagnostics;
using Newtonsoft.Json;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace Toolbox.Industrial.Core.Messages
{
    public abstract class RemoteCommand : Toolbox.Core.Messages.Command
    {
        private DateTimeOffset _timestamp;

        protected RemoteCommand()
        {
            _timestamp = DateTimeOffset.UtcNow;
            CorrelationId = Id;
        }

        public static TRemoteCommand From<TRemoteCommand>(TRemoteCommand origin) 
            where TRemoteCommand : RemoteCommand, new()
        { 
            var result = new TRemoteCommand() 
            {
                
                Mqtt = origin.Mqtt,
                Topic = origin.Topic,
                Timestamp = origin.Timestamp,
                Stopwatch = Stopwatch.StartNew(),
                CorrelationId = origin.Id,
                AdditionalProperties = new Dictionary<string, object>(origin.AdditionalProperties ?? [],
                    StringComparer.OrdinalIgnoreCase
                )

            };
            result._timestamp = origin._timestamp;

            return result;
        }

        internal Guid CorrelationId { get; set; }
        internal Mqtt Mqtt { get; set; } = null!;
        internal string Topic { get; set; } = null!;
        internal TimeSpan Latency => _timestamp - Timestamp;
        internal Stopwatch Stopwatch { get; init; } = Stopwatch.StartNew();

        public virtual Guid Id { get; init; } = SequentialGuid.NewGuid();
        public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;

        [JsonIgnore]
        public virtual string ProcessId => $"{Id}-{Mqtt?.BrokerKey}";

        [JsonIgnore]
        public bool HasAdditionalProperties => AdditionalProperties?.Count > 0;

        [JsonExtensionData]
        public virtual Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public abstract class InternalCommand : Toolbox.Core.Messages.Command { }
}
