using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Platform
{
    internal class WindowsShell : IShell
    {
        private readonly ILogger<WindowsShell> _logger;

        public WindowsShell(ILogger<WindowsShell> logger) => _logger = logger;

        private async Task<(string output, string error, int exitCode)> Run(string command, CancellationToken cancellationToken)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
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
            var result = await Run($"sc start {serviceName}", cancellationToken);
            return result.exitCode == 0 ? true : false;
        }

        public async Task<ServiceStatus> StatusService(string serviceName, CancellationToken cancellationToken)
        {
            var (output, _, _) = await Run($"sc query {serviceName}",cancellationToken);

            var stateLine = output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(l => l.Contains("ESTADO"));

            if (stateLine == null)
                return ServiceStatus.Unknown;

            var parts = stateLine.Split(':', StringSplitOptions.TrimEntries);
            if (parts.Length <= 1)
                return ServiceStatus.Unknown;

            var stateParts = parts[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var state = stateParts.Length > 1 ? stateParts[1] : stateParts[0];

            return state switch
            {
                "RUNNING" => ServiceStatus.Running,
                "STOPPED" => ServiceStatus.Stopped,
                "START_PENDING" => ServiceStatus.Starting,
                "STOP_PENDING" => ServiceStatus.Stopping,
                _ => ServiceStatus.Unknown,
            };
        }

        public async Task<bool> StopService(string serviceName, CancellationToken cancellationToken)
        {
            var result = await Run($"sc stop {serviceName}", cancellationToken);
            return result.exitCode == 0 ? true : false;
        }
    }
}
