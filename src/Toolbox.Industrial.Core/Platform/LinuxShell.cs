using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Platform
{
    internal class LinuxShell : IShell
    {
        private readonly ILogger<LinuxShell> _logger;

        public LinuxShell(ILogger<LinuxShell> logger)
        {
            _logger = logger;
        }

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
    }
}
