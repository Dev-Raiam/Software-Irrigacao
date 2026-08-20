using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;
using Toolbox.Industrial.Core.Setup;

namespace Toolbox.Industrial.Core.Messages.Commands;

internal class Restart : InternalCommand { }

internal class RestartHandler : CommandHandler, ICommandHandler<Restart>
{
    public async Task<ResponseResult> Handle(
        Restart request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return NoContent();
        }
        finally
        {
            await Application.Restart();
        }
    }
}
