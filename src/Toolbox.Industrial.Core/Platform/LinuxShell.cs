using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Platform
{
    //Interface
    internal class LinuxShell : IShell
    {
        private readonly ILogger<LinuxShell> _logger;

        public LinuxShell(ILogger<LinuxShell> logger) => _logger = logger;

        private TimeSpan _timeout = TimeSpan.FromSeconds(5);

        public async Task<(string output, string error, int exitCode)> Run(string command)
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

                if (status is not ("deactivating" or "activating"))
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
            await Run($"systemctl start {serviceName}");
            return await WaitForStatus(serviceName, "active", timeout ?? _timeout);
        }

        public async Task<string?> Status(string serviceName, TimeSpan? timeout = null)
        {
            var (output, _, _) = await Run($"systemctl is-active {serviceName}");
            return output.Trim();
        }

        public async Task<bool> Stop(string serviceName, TimeSpan? timeout = null)
        {
            await Run($"systemctl stop {serviceName}");
            return await WaitForStatus(serviceName, "inactive", timeout ?? _timeout);
        }
    }
}
