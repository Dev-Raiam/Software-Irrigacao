using System.Net;
using Toolbox.Core.Mediator;
using Toolbox.Industrial.Core.Communication.Mqtt;

namespace Toolbox.Industrial.Core.Messages
{
    public abstract class Command : Toolbox.Core.Messages.Command
    {
        internal IMqtt Mqtt { get; set; } = null!;
        internal string Topic { get; set; } = null!;

        public Guid Id { get; init; } = SequentialGuid.NewGuid();
        internal DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }

    public class Response : Toolbox.Core.Messages.ResponseEvent
    {
        public Response(Command command, ResponseResult? response = null) : base()
        {
            CommandId = command.Id;
            Success = response?.IsSuccessful ?? true;
            Payload = response?.PayLoad;
            Messages = response?.Errors;
            StatusCode = ((int?)response?.HttpStatusCode ?? 0);
            try
            {
                Duration = Timestamp - command.Timestamp;
            }
            catch
            {
                Duration = TimeSpan.Zero;
            }
        }

        public bool Success { get; }
        public TimeSpan Duration { get; }
        public Guid CommandId { get; }
        public int StatusCode { get; }
        public IDictionary<string, object[]>? Messages { get; }
        public dynamic? Payload { get; }
    }
}
