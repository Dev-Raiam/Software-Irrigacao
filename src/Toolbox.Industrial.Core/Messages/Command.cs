using System.Diagnostics;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Messages.Integration.Events;

namespace Toolbox.Industrial.Core.Messages
{
    public abstract class RemoteCommand : Toolbox.Core.Messages.Command
    {
        [Newtonsoft.Json.JsonProperty("data", Order = -99)]
        [System.Text.Json.Serialization.JsonPropertyOrder(-99)]
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        private DateTimeOffset _createdAt = DateTimeOffset.UtcNow;

        private DateTimeOffset _received = DateTimeOffset.UtcNow;

        protected RemoteCommand()
        {
            var old = _received;
            _received = DateTimeOffset.UtcNow;
            CorrelationId = Id;
        }

        public static TRemoteCommand From<TRemoteCommand>(TRemoteCommand origin)
            where TRemoteCommand : RemoteCommand, new()
        {
            var result = new TRemoteCommand()
            {
                Mqtt = origin.Mqtt,
                Topic = origin.Topic,
                Timeout = origin.Timeout,
                Stopwatch = Stopwatch.StartNew(),
                CorrelationId = origin.CorrelationId,
                AdditionalProperties = new Dictionary<string, object>(
                    origin.AdditionalProperties ?? [],
                    StringComparer.OrdinalIgnoreCase
                ),
                Timeouts = new Dictionary<Guid, TimeSpan> 
                { 
                    [origin.CorrelationId] = origin.Timeout,
                    [origin.Id] = origin.Timeout 
                }
            };
            result._createdAt = origin._createdAt;
            result._received = origin._received;
            return result;
        }

        internal Guid CorrelationId { get; set; }
        internal Mqtt Mqtt { get; set; } = null!;
        internal string Topic { get; set; } = null!;

        [Newtonsoft.Json.JsonProperty("timeout", Order = -98)]
        [System.Text.Json.Serialization.JsonPropertyOrder(-98)]
        [System.Text.Json.Serialization.JsonPropertyName("timeout")]
        internal TimeSpan Timeout { get; init; } = ResponseRequest.DefaultTimeout;

        [Newtonsoft.Json.JsonProperty("timeouts", Order = -97)]
        [System.Text.Json.Serialization.JsonPropertyOrder(-97)]
        [System.Text.Json.Serialization.JsonPropertyName("timeouts")]
        internal Dictionary<Guid, TimeSpan>? Timeouts { get; set; }

        internal TimeSpan Latency => _received - _createdAt;

        internal Stopwatch Stopwatch { get; init; } = Stopwatch.StartNew();

        [Newtonsoft.Json.JsonProperty(Order = -100)]
        [System.Text.Json.Serialization.JsonPropertyOrder(-100)]
        public virtual Guid Id { get; init; } = SequentialGuid.NewGuid();

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public virtual bool HasAdditionalProperties => AdditionalProperties?.Count > 0;

        [Newtonsoft.Json.JsonExtensionData]
        [System.Text.Json.Serialization.JsonExtensionData]
        public virtual Dictionary<string, object>? AdditionalProperties { get; set; }
    }

    public abstract class InternalCommand : Toolbox.Core.Messages.Command { }
}
