using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Toolbox.Core.Extensions;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Extensions;

namespace Toolbox.Industrial.Core.Messages.Integration.Events;

public sealed class ResponseRequest : Toolbox.Core.Messages.NotificationEvent
{
    internal Mqtt Mqtt { get; set; } = null!;
    internal string Topic { get; set; } = null!;

    [JsonProperty(Order = -96)]
    public TimeSpan Latency { get; init; }

    [JsonIgnore]
    public string ProcessId => $"{CorrelationId}-{Mqtt?.BrokerKey}";

    public static TimeSpan Timeout = TimeSpan.FromSeconds(3);

    public static ResponseRequest From(RemoteCommand request, ResponseResult? response = null)
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
        result.AdditionalProperties ??= new Dictionary<string, object>(
            StringComparer.OrdinalIgnoreCase
        );
        result.AdditionalProperties[nameof(request.Mqtt.BrokerKey).ToLowerFirst()] = request
            .Mqtt
            .BrokerKey;
        if (response?.Errors.Count > 0)
        {
            result.AdditionalProperties[nameof(response.Errors).ToLowerFirst()] =
                response!.GetErrors();
        }
        return result;
    }
}

internal class ResponseRequestHandler : INotificationHandler<ResponseRequest>
{
    private readonly IServiceProvider _provider;

    public ResponseRequestHandler(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task Handle(
        ResponseRequest notification,
        CancellationToken cancellationToken = default
    )
    {
        if (!notification.Success)
        {
            //logar falha
        }
        var property = nameof(notification.Mqtt.BrokerKey).ToLowerFirst();
        if (
            notification.HasAdditionalProperties
            && notification.AdditionalProperties!.TryGetValue(property, out object? brokerKey)
        )
        {
            notification.AdditionalProperties.Remove(property);
            if (notification.Mqtt.BrokerKey != $"{brokerKey}")
            {
                var mqtt = _provider.GetRequiredKeyedService<MqttManager>(brokerKey).Current!;
                if (mqtt != null)
                {
                    await mqtt.PublishAsync(notification.Topic, notification);
                }
            }
            MqttManager.Process.Completed($"{notification.CorrelationId}", notification);
        }
    }
}
