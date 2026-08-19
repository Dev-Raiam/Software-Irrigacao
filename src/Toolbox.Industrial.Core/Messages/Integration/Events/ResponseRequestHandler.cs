using Microsoft.Extensions.DependencyInjection;
using Toolbox.Core.Extensions;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace Toolbox.Industrial.Core.Messages.Integration.Events
{
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
                MqttManager.Process.Completed(
                    $"{notification.CorrelationId}-{brokerKey}",
                    notification
                );
            }
        }
    }
}
