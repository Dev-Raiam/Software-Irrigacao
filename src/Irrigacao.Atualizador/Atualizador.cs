using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Toolbox.Industrial.Core.Communication.Api;

namespace Irrigacao.Atualizador
{
    public record GitHubRelease(
        [property: JsonProperty("tag_name")] string TagName,
        [property: JsonProperty("assets")] GitHubAsset[] Assets
    );

    public record GitHubAsset(
        [property: JsonProperty("browser_download_url")] string BrowserDownloadUrl
    );

    public class Atualizador : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiClient _client;
        private readonly ILogger<Atualizador> _logger;

        public Atualizador(IApiClient client, HttpClient httpClient, ILogger<Atualizador> logger)
        {
            httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Software-Irrigacao");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                "TokenClassic"
            );

            _httpClient = httpClient;

            _client = client;

            _logger = logger;
        }

        private GitHubRelease? _release;
        private const string _binaryName = "irrigacao";
        private string _binaryPath = Path.Combine(Directory.GetCurrentDirectory(), _binaryName);
        private const string UrlVersion =
            "https://api.github.com/repos/Dev-Raiam/Software-Irrigacao/releases/latest";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Atualizador Iniciado: {time}", DateTimeOffset.Now);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var newUpdate = await UpdateChecker(stoppingToken);

                    if (newUpdate)
                        await UpdateInstaller(stoppingToken);

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch
            {
                _logger.LogError("Erro inesperado na execução do serviço");
            }
        }

        private async Task<bool> UpdateChecker(CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, UrlVersion);
            var response = await _client.SendAsync<GitHubRelease>(
                message,
                cancellationToken,
                _httpClient
            );

            if (!response.Success)
            {
                _logger.LogWarning(response.Error);

                return false;
            }

            var release = response.Data;

            if (release == null)
            {
                return false;
            }

            var currentArchitecture = RuntimeInformation.OSArchitecture;
            Version versionRelease = new Version();

            if (currentArchitecture == Architecture.X64)
            {
                if (!release.TagName.Contains("vx"))
                {
                    return false;
                }

                versionRelease = Version.Parse(release.TagName.Replace("vx", ""));
            }

            if (currentArchitecture == Architecture.Arm64)
            {
                if (!release.TagName.Contains("v"))
                {
                    return false;
                }

                versionRelease = Version.Parse(release.TagName.Replace("v", ""));
            }

            var currentVersion = BinaryVersion();

            if (currentVersion < versionRelease!)
            {
                _logger.LogInformation("Nova versão disponível: {versionRelease}", versionRelease);

                _release = release;

                return true;
            }

            return false;
        }

        private async Task UpdateInstaller(CancellationToken cancellationToken)
        {
            var zipPath = await DownloadReleaseZip(cancellationToken);

            var extractPath = ExtractZip(zipPath);

            var binaryCompatible = await ValidateBinaryArchitecture(extractPath);

            if (!binaryCompatible)
                return;

            //if (!File.Exists(_binaryPath))
            //{
            //    _logger.LogError("Binário não encontrado em {binaryPath}", _binaryPath);
            //    return;
            //}

            if (StopService("irrigacao"))
            {
                var backupPath = "/opt/edge-plc-backup";

                if (BackupBinary(_binaryPath, backupPath))
                {
                    var newBinaryPath = Path.Combine(extractPath, _binaryName);

                    if (MoveNewBinary(newBinaryPath, _binaryPath))
                    {
                        if (StartService("irrigacao"))
                        {
                            _logger.LogInformation("Atualização concluída com sucesso para versão");
                        }
                    }
                }
            }
            else
            {
                _logger.LogInformation("Falha na Atualização");
            }
        }

        private Version BinaryVersion()
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = _binaryPath,
                ArgumentList = { "--version" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = process.StandardError.ReadToEnd();
                _logger.LogError("Falha ao obter versão do binário: {error}", error);
                return new Version(1, 0, 0);
            }

            var versionString = output
                .Replace("v", "", StringComparison.OrdinalIgnoreCase)
                .Replace("version:", "", StringComparison.OrdinalIgnoreCase)
                .Trim();

            if (Version.TryParse(versionString, out var version))
            {
                _logger.LogInformation("Versão do binário: {version}", version);
                return version;
            }

            _logger.LogError("Não foi possível converter a versão: {output}", output);
            return new Version(1, 0, 0);
        }

        private async Task<string> DownloadReleaseZip(CancellationToken cancellationToken)
        {
            var updateDir = "/tmp/irrigacao-update";
            Directory.CreateDirectory(updateDir);

            var zipPath = Path.Combine(updateDir, "irrigacao.zip");

            var assetUrl = _release!
                .Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith("irrigacao.zip"))
                ?.BrowserDownloadUrl;

            if (assetUrl != null)
            {
                _logger.LogInformation("Baixando zip de {assetUrl}", assetUrl);
                var response = await _httpClient.GetAsync(assetUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var fileStream = File.Create(zipPath);
                await response.Content.CopyToAsync(fileStream, cancellationToken);

                _logger.LogInformation("Zip baixado para {zipPath}", zipPath);
            }

            return zipPath;
        }

        private string ExtractZip(string zipPath)
        {
            var extractDir = "/tmp/irrigacao-update/extract";

            Directory.CreateDirectory(extractDir);

            _logger.LogInformation(
                "Extraindo zip de {zipPath} para {extractDir}",
                zipPath,
                extractDir
            );

            ZipFile.ExtractToDirectory(zipPath, extractDir, true);

            _logger.LogInformation("Zip extraído com sucesso");

            return extractDir;
        }

        private async Task<bool> ValidateBinaryArchitecture(string extractDir)
        {
            var binaryExtract = Path.Combine(extractDir, "irrigacao");
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = "file",
                ArgumentList = { binaryExtract },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            process.Start();
            var output = process.StandardOutput.ReadToEnd().Trim();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Falha ao obter arquitetura do binario");
                return false;
            }

            var currentArchitecture = RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "amd64",
                Architecture.Arm64 => "arm64",
                _ => "",
            };

            var binaryArchitecture = output switch
            {
                var s
                    when s.Contains("x86-64", StringComparison.OrdinalIgnoreCase)
                        || s.Contains("x86_64", StringComparison.OrdinalIgnoreCase) => "amd64",
                var s
                    when s.Contains("aarch64", StringComparison.OrdinalIgnoreCase)
                        || s.Contains("arm64", StringComparison.OrdinalIgnoreCase) => "arm64",
                _ => "unknown",
            };

            if (binaryArchitecture != currentArchitecture)
            {
                _logger.LogError(
                    "Arquitetura {output} do binario, incompativel com a arquitetura atual {currentArchitecture}",
                    output,
                    currentArchitecture
                );
                return false;
            }

            return true;
        }

        private bool StopService(string serviceName)
        {
            _logger.LogInformation("Parando serviço {serviceName}", serviceName);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo systemctl stop {serviceName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Serviço {serviceName} parado com sucesso", serviceName);
                return true;
            }

            var error = process.StandardError.ReadToEnd();
            _logger.LogError("Falha ao parar serviço {serviceName}: {error}", serviceName, error);
            return false;
        }

        private bool BackupBinary(string sourcePath, string backupDir)
        {
            Directory.CreateDirectory(backupDir);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(backupDir, $"irrigacao_{timestamp}");

            _logger.LogInformation(
                "Fazendo backup de {sourcePath} para {backupPath}",
                sourcePath,
                backupPath
            );
            File.Copy(sourcePath, backupPath, true);

            _logger.LogInformation("Backup concluído com sucesso");
            return true;
        }

        private bool MoveNewBinary(string sourcePath, string destinationPath)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);

            _logger.LogInformation(
                "Movendo binário de {sourcePath} para {destinationPath}",
                sourcePath,
                destinationPath
            );
            File.Copy(sourcePath, destinationPath, true);

            // Dar permissão de execução
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo chmod +x {destinationPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            process.WaitForExit();

            _logger.LogInformation("Binário movido com sucesso");
            return true;
        }

        private bool StartService(string serviceName)
        {
            _logger.LogInformation("Iniciando serviço {serviceName}", serviceName);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo systemctl start {serviceName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation("Serviço {serviceName} iniciado com sucesso", serviceName);
                return true;
            }

            var error = process.StandardError.ReadToEnd();
            _logger.LogError("Falha ao iniciar serviço {serviceName}: {error}", serviceName, error);
            return false;
        }
    }
}
