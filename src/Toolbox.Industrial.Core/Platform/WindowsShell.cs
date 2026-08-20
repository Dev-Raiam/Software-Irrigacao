using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Platform
{
    internal class WindowsShell : IShell
    {
        private readonly ILogger<WindowsShell> _logger;

        public WindowsShell(ILogger<WindowsShell> logger) => _logger = logger;
        
        private TimeSpan _timeout = TimeSpan.FromSeconds(5);

        private async Task<(string output, string error, int exitCode)> Run(string command)
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
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                _logger.LogError("Comando falhou: {command} — {error}", command, error);

            return (output, error, process.ExitCode);
        }

        private async Task<bool> WaitForStatus(
            string serviceName,
            string expectedStatus,
            TimeSpan timeout
        )
        {
            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                var status = await Status(serviceName);

                if (status == expectedStatus)
                    return true;

                if (status is not ("STOP_PENDING" or "START_PENDING"))
                {
                    _logger.LogWarning(
                        "Serviço {serviceName} em estado: {status}",
                        serviceName,
                        status
                    );
                }

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            _logger.LogError(
                "Timeout aguardando serviço {serviceName} atingir estado {expected}",
                serviceName,
                expectedStatus
            );
            return false;
        }

        public async Task<bool> Start(string serviceName, TimeSpan? timeout = null)
        {
            await Run($"sc start {serviceName}");
            return await WaitForStatus(serviceName, "RUNNING", timeout ?? _timeout);
        }

        public async Task<string?> Status(string serviceName, TimeSpan? timeout = null)
        {
            var (output, _, _) = await Run($"sc query {serviceName}");

            var stateLine = output
                .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(l => l.Contains("STATE"));

            if (stateLine == null)
                return null;

            var parts = stateLine.Split(':', StringSplitOptions.TrimEntries);
            return parts.Length > 1 ? parts[1].Split(' ')[0] : null;
        }

        public async Task<bool> Stop(string serviceName, TimeSpan? timeout = null)
        {
            await Run($"sc stop {serviceName}");
            return await WaitForStatus(serviceName, "STOPPED", timeout ?? _timeout);
        }
    }
}
