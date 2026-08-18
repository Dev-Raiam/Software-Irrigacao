using System.Net;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace Toolbox.Industrial.Core.Messages
{
    public abstract class Command : Toolbox.Core.Messages.Command
    {
        internal IMqtt? Mqtt { get; set; }
        internal string Topic { get; set; } = null!;

        public Guid Id { get; init; } = SequentialGuid.NewGuid();
    }

    public class Response : Toolbox.Core.Messages.ResponseEvent
    {
        public Response(Command command, ResponseResult? response = null)
        {
            CommandId = command.Id;
            Success = response?.IsSuccessful ?? true;
            Payload = response?.PayLoad;
            Messages = response?.Errors;
            StatusCode = ((int?)response?.HttpStatusCode ?? 0);
        }

        public bool Success { get; }
        public Guid CommandId { get; }
        public int StatusCode { get; }
        public IDictionary<string, object[]>? Messages { get; }
        public dynamic? Payload { get; }
    }
}
