using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Toolbox.Industrial.Core.Platform
{
    internal class WindowsShell : IShell
    {
        private readonly ILogger<WindowsShell> _logger;

        public WindowsShell(ILogger<WindowsShell> logger)
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
