using System.Diagnostics;
using System.IO.Compression;
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
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "tokenClassic");

            _httpClient = httpClient;

            _client = client;

            _logger = logger;
        }

        //private const string UrlZip =
        //    "https://github.com/Dev-Raiam/Software-Irrigacao/releases/latest/download/irrigacao.zip";
        private const string UrlVersion =
            "https://api.github.com/repos/Dev-Raiam/Software-Irrigacao/releases/latest";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Busque a ultima vers�o no Git Hub
                // Busque a vers�o atual do binario que esta rodando no Linux
                // Compare vers�o (L�gica Completa)

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // Busque a ultima vers�o no Git Hub
                var message = new HttpRequestMessage(HttpMethod.Get, UrlVersion);
                var response = await _client.SendAsync<GitHubRelease>(
                    message,
                    stoppingToken,
                    _httpClient
                );

                if (!response.Success)
                    return;

                var release = response.Data;

                if (release == null)
                    return;

                var versionRelease = Version.Parse(release.TagName.Replace("v", ""));
                // Busque a versão atual do binario que esta rodando no Linux
                var currentVersion = BinaryVersion();

                // Compare versão (Lógica Completa)
                if (versionRelease > currentVersion)
                {
                    _logger.LogInformation(
                        "Nova versão disponível: {versionRelease}",
                        versionRelease
                    );

                    // Baixar o zip do Git hub em uma pasta separada /tmp/irrigacao-update
                    var zipPath = await DownloadReleaseZip(release, stoppingToken);
                    if (zipPath == null)
                    {
                        _logger.LogError("Falha ao baixar o zip da release");
                        return;
                    }

                    // Extrair o zip /tmp/irrigacao-update/extract
                    var extractPath = ExtractZip(zipPath);
                    if (extractPath == null)
                    {
                        _logger.LogError("Falha ao extrair o zip");
                        return;
                    }

                    // verificar se o binario existe em /opt/edge-plc
                    var binaryPath = "/opt/edge-plc/irrigacao";
                    if (!File.Exists(binaryPath))
                    {
                        _logger.LogError("Binário não encontrado em {binaryPath}", binaryPath);
                        return;
                    }

                    // Parar o servico de irrigacao
                    if (!StopService("irrigacao"))
                    {
                        _logger.LogError("Falha ao parar o serviço de irrigação");
                        return;
                    }

                    // Mover o binario antigo para um diretorio temporário /tmp/irrigacao-update/extract opt/edge-plc e o bin antigo em opt/edge-plc-backup
                    var backupPath = "/opt/edge-plc-backup";
                    if (!BackupBinary(binaryPath, backupPath))
                    {
                        _logger.LogError("Falha ao fazer backup do binário");
                        return;
                    }

                    // Mover o Binario novo para o diretorio /opt/edge-plc
                    var newBinaryPath = Path.Combine(extractPath, "irrigacao");
                    if (!MoveNewBinary(newBinaryPath, binaryPath))
                    {
                        _logger.LogError("Falha ao mover o novo binário");
                        return;
                    }

                    // Start o serviço
                    if (!StartService("irrigacao"))
                    {
                        _logger.LogError("Falha ao iniciar o serviço de irrigação");
                        return;
                    }

                    _logger.LogInformation(
                        "Atualização concluída com sucesso para versão {versionRelease}",
                        versionRelease
                    );
                }

                await Task.Delay(1000, stoppingToken);
            }
        }

        private Version BinaryVersion()
        {
            // Logica de pegar a verssão do binario do Linux
            return new Version(1, 3, 4);
        }

        private async Task<string?> DownloadReleaseZip(
            GitHubRelease release,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var updateDir = "/tmp/irrigacao-update";
                Directory.CreateDirectory(updateDir);

                var zipPath = Path.Combine(updateDir, "irrigacao.zip");

                // Encontrar a URL do asset correto (irrigacao.zip)
                var assetUrl = release
                    .Assets.FirstOrDefault(a => a.BrowserDownloadUrl.EndsWith("irrigacao.zip"))
                    ?.BrowserDownloadUrl;
                if (assetUrl == null)
                {
                    _logger.LogError("Asset irrigacao.zip não encontrado na release");
                    return null;
                }

                _logger.LogInformation("Baixando zip de {assetUrl}", assetUrl);
                var response = await _httpClient.GetAsync(assetUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                await using var fileStream = File.Create(zipPath);
                await response.Content.CopyToAsync(fileStream, cancellationToken);

                _logger.LogInformation("Zip baixado para {zipPath}", zipPath);
                return zipPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao baixar o zip da release");
                return null;
            }
        }

        private string? ExtractZip(string zipPath)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao extrair o zip");
                return null;
            }
        }

        private bool StopService(string serviceName)
        {
            try
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
                _logger.LogError(
                    "Falha ao parar serviço {serviceName}: {error}",
                    serviceName,
                    error
                );
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao parar serviço {serviceName}", serviceName);
                return false;
            }
        }

        private bool BackupBinary(string sourcePath, string backupDir)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao fazer backup do binário");
                return false;
            }
        }

        private bool MoveNewBinary(string sourcePath, string destinationPath)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao mover o novo binário");
                return false;
            }
        }

        private bool StartService(string serviceName)
        {
            try
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
                    _logger.LogInformation(
                        "Serviço {serviceName} iniciado com sucesso",
                        serviceName
                    );
                    return true;
                }

                var error = process.StandardError.ReadToEnd();
                _logger.LogError(
                    "Falha ao iniciar serviço {serviceName}: {error}",
                    serviceName,
                    error
                );
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao iniciar serviço {serviceName}", serviceName);
                return false;
            }
        }
    }
}
