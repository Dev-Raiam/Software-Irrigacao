using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Commands;

internal class Shutdown : InternalCommand { }

internal class ShutdownHandler : CommandHandler, ICommandHandler<Shutdown>
{
    public async Task<ResponseResult> Handle(
        Shutdown request,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(1000);
        Process? process = null;
        try
        {
            return NoContent();
        }
        finally
        {
            if (OperatingSystem.IsWindows())
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        ArgumentList =
                        {
                            "/s", // Shutdown
                            "/f", //Força o encerramento dos aplicativos.
                            "/t",
                            "0", // Sem atraso <segundos>
                        },
                        UseShellExecute = false,
                    }
                );
            }
            else if (OperatingSystem.IsLinux())
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "systemctl",
                        ArgumentList = { "poweroff" },
                        UseShellExecute = false,
                    }
                );
            }
            if (process != null)
            {
                await process.WaitForExitAsync();
            }
        }
    }
}
