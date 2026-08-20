using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Platform
{
    public interface ISystemctlService
    {
        Task<bool> Stop(string serviceName, TimeSpan? timeout = null);
        Task<bool> Start(string serviceName, TimeSpan? timeout = null);
        Task<string?> GetStatus(string serviceName);
    }

    internal class SystemctlService : ISystemctlService
    {
        private readonly IShell _shell;
        private readonly ILogger<SystemctlService> _logger;
        private TimeSpan _timeout = TimeSpan.FromSeconds(5);

        public SystemctlService(IShell shell, ILogger<SystemctlService> logger)
        {
            _shell = shell;
            _logger = logger;
        }

        public async Task<bool> Stop(string serviceName, TimeSpan? timeout = null)
        {
            await _shell.Run($"systemctl stop {serviceName}");
            return await WaitForStatus(serviceName, "inactive", timeout ?? _timeout);
        }

        public async Task<bool> Start(string serviceName, TimeSpan? timeout = null)
        {
            await _shell.Run($"systemctl start {serviceName}");
            return await WaitForStatus(serviceName, "active", timeout ?? _timeout);
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
                var status = await GetStatus(serviceName);

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

        public async Task<string?> GetStatus(string serviceName)
        {
            var (output, _, _) = await _shell.Run($"systemctl is-active {serviceName}");
            return output.Trim();
        }

        //private async Task<(string output, string error, int exitCode)> Run(string command)
        //{
        //    var process = new Process
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = "/bin/bash",
        //            Arguments = $"-c \"{command}\"",
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true,
        //        },
        //    };

        //    process.Start();
        //    var output = await process.StandardOutput.ReadToEndAsync();
        //    var error = await process.StandardError.ReadToEndAsync();
        //    await process.WaitForExitAsync();

        //    if (process.ExitCode != 0)
        //        _logger.LogError("Comando falhou: {command} — {error}", command, error);

        //    return (output, error, process.ExitCode);
        //}
    }
}
