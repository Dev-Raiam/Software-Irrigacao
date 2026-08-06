//using System.Diagnostics;
//using System.Text;
//using Microsoft.Extensions.Logging;

//namespace Toolbox.Industrial.Core.Platform;

//internal static class SystemdServiceConfigurator
//{
//    public static void Install(
//        string serviceName,
//        string executablePath,
//        string description,
//        string? workingDirectory = null,
//        string? user = null
//    )
//    {
//        var serviceContent = GenerateServiceFile(
//            serviceName,
//            executablePath,
//            description,
//            workingDirectory,
//            user
//        );

//        var serviceFilePath = $"/etc/systemd/system/{serviceName}.service";
//        File.WriteAllText(serviceFilePath, serviceContent);

//        // Dar permissões
//        Chmod("644", serviceFilePath);

//        // Recarregar systemd
//        Execute("systemctl", "daemon-reload");

//        // Habilitar para iniciar no boot
//        Execute("systemctl", "enable", serviceName);

//        // Iniciar o serviço
//        Execute("systemctl", "start", serviceName);
//    }

//    private static string GenerateServiceFile(
//        string serviceName,
//        string executablePath,
//        string description,
//        string? workingDirectory,
//        string? user
//    )
//    {
//        var sb = new StringBuilder();
//        sb.AppendLine("[Unit]");
//        sb.AppendLine($"Description={description}");
//        sb.AppendLine("After=network.target");
//        sb.AppendLine();
//        sb.AppendLine("[Service]");
//        sb.AppendLine("Type=notify");

//        if (!string.IsNullOrEmpty(user))
//            sb.AppendLine($"User={user}");

//        if (!string.IsNullOrEmpty(workingDirectory))
//            sb.AppendLine($"WorkingDirectory={workingDirectory}");

//        sb.AppendLine($"ExecStart={executablePath}");
//        sb.AppendLine("Restart=always");
//        sb.AppendLine("RestartSec=10");
//        sb.AppendLine();
//        sb.AppendLine("[Install]");
//        sb.AppendLine("WantedBy=multi-user.target");

//        return sb.ToString();
//    }

//    private static void Execute(string command, params string[] args)
//    {
//        using var process = new Process
//        {
//            StartInfo = new ProcessStartInfo
//            {
//                FileName = command,
//                ArgumentList = { args },
//                RedirectStandardOutput = true,
//                RedirectStandardError = true,
//                UseShellExecute = false,
//                CreateNoWindow = true,
//            },
//        };

//        process.Start();
//        process.WaitForExit();

//        if (process.ExitCode != 0)
//        {
//            var error = process.StandardError.ReadToEnd();
//            throw new InvalidOperationException(
//                $"Command '{command} {string.Join(" ", args)}' failed: {error}"
//            );
//        }
//    }

//    private static void Chmod(string permissions, string filePath)
//    {
//        Execute("chmod", permissions, filePath);
//    }
//}
