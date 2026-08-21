using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Platform
{
    internal class LinuxShell : IShell
    {
        private readonly ILogger<LinuxShell> _logger;

        public LinuxShell(ILogger<LinuxShell> logger) => _logger = logger;

        private async Task<(string output, string error, int exitCode)> Run(string command,CancellationToken cancellationToken)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
                _logger.LogError("Comando falhou: {command} — {error}", command, error);

            return (output, error, process.ExitCode);
        }

        public async Task<bool> StartService(string serviceName, CancellationToken cancellationToken)
        {
            var result = await Run($"systemctl start {serviceName}",cancellationToken);
            return result.exitCode == 0 ? true : false;
        }

        public async Task<ServiceStatus> StatusService(string serviceName, CancellationToken cancellationToken)
        {
            var (output, _, exitCode) = await Run($"systemctl is-active {serviceName}", cancellationToken);
            var status = output.Trim();

            return status switch
            {
                "active" => ServiceStatus.Running,
                "inactive" => ServiceStatus.Stopped,
                "activating" => ServiceStatus.Starting,
                "deactivating" => ServiceStatus.Stopping,
                "failed" => ServiceStatus.Failed,
                _ => ServiceStatus.Unknown,
            };
        }

        public async Task<bool> StopService(string serviceName, CancellationToken cancellationToken)
        {
            var result = await Run($"systemctl stop {serviceName}",cancellationToken);
            return result.exitCode == 0 ? true : false;
        }
    }
}
