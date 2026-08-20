using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Toolbox.Core.Mediator;
using Toolbox.Core.Messages;

namespace Toolbox.Industrial.Core.Messages.Commands;

internal class Reboot : InternalCommand { }

internal class RebootHandler : CommandHandler, ICommandHandler<Reboot>
{
    public async Task<ResponseResult> Handle(Reboot request, CancellationToken cancellationToken)
    {
        await Task.Delay(1000);
        try
        {
            return NoContent();
        }
        finally
        {
            Process? process = null;
            if (OperatingSystem.IsWindows())
            {
                process = Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = "shutdown",
                        ArgumentList =
                        {
                            "/r", // Restart
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
                        ArgumentList = { "reboot" },
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
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
