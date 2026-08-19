using Newtonsoft.Json;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace Toolbox.Industrial.Core.Messages.Integration.Events
{
    public sealed class ResponseRequest : Toolbox.Core.Messages.NotificationEvent
    {
        internal Mqtt Mqtt { get; set; } = null!;
        internal string Topic { get; set; } = null!;

        [JsonProperty(Order = -96)]
        public TimeSpan Latency { get; init; }

        public static ResponseRequest From(Command request, ResponseResult? response = null)
        {
            request.Stopwatch.Stop();
            var result = new ResponseRequest
            {
                CorrelationId = request.Id,
                Latency = request.Latency,
                Payload = response?.PayLoad,
                Duration = request.Stopwatch.Elapsed,
                Success = response?.IsSuccessful ?? true,
                StatusCode = ((int?)response?.HttpStatusCode ?? 0),
            };
            result.AdditionalProperties ??= new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            result.AdditionalProperties[nameof(request.Mqtt.BrokerKey).ToLowerFirst()] = request.Mqtt.BrokerKey;
            if (response?.Errors.Count > 0)
            {
                result.AdditionalProperties[nameof(response.Errors).ToLowerFirst()] = response!.Errors;
            }
            return result;
        }

    }

    public interface IPendingResponse 
    {
        void SetResult(ResponseRequest response);
    }

    public sealed class PendingResponse<TPayload> : IPendingResponse
    {
        public required string BrokerKey { get; init; }
        public required TPayload Command { get; init; }
        public required string Topic { get; init; }
        public required TaskCompletionSource<ResponseRequest> Completion { get; init; }

        public void SetResult(ResponseRequest response)
        {
            Completion.TrySetResult(response);
        }
    }
}
