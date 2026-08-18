using Toolbox.Core.Mediator;

namespace Toolbox.Industrial.Core.Messages.Integration.Events
{
    public class ResponseCommand : Toolbox.Core.Messages.NotificationEvent
    {
        public ResponseCommand(Command command, ResponseResult? response = null)
        {
            CorrelationId = command.Id;
            Success = response?.IsSuccessful ?? true;
            Payload = response?.PayLoad ?? command;
            Messages = response?.Errors ?? new Dictionary<string, object[]>{ ["teste"] = new[] { "Warning", "Teste" }, };
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
    }
}
