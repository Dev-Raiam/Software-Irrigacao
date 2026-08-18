using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Setup;

namespace Irrigacao.Atualizador
{
    public class Updater : BackgroundService
    {
        private readonly UpdateInstallationConfig _config;
        private readonly IHttpClientFactory _factoryHttpClient;
        private readonly ILogger<Updater> _logger;
        private readonly IEntityStore _store;
        private readonly IApiClient _client;

        public Updater(
            UpdateInstallationConfig config,
            IHttpClientFactory factoryHttpClient,
            ILogger<Updater> logger,
            IEntityStore store,
            IApiClient client
        )
        {
            _config = config;
            _factoryHttpClient = factoryHttpClient;
            _logger = logger;
            _store = store;
            _client = client;
        }

        private bool _containsRequisition = false;
        private AtualizacaoDisponivel? _request = null;

        private async Task<AtualizacaoDisponivel> ObterModeloRequest()
        {
            var contaId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ContaId);
            var painelId = await _store.ObterConfiguracao<Guid>(Entity.Keys.PainelId);
            var controladorId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ControladorId);
            var versaoAtual = await _store.ObterConfiguracao<string>(Entity.Keys.VersaoAtual);

            return new AtualizacaoDisponivel(
                contaId,
                painelId,
                controladorId,
                null,
                versaoAtual ?? "",
                null,
                (int)RuntimeInformation.OSArchitecture
            );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine($"Worker Iniciado {Application.HasCredentials}");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var url = await CheckUpdate(stoppingToken);

                    if (url != null)
                    {
                        Console.WriteLine($"Install Update Iniciado");
                        await InstallUpdate(url, stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado na execução do serviço");
                }

                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }

        private async Task<string?> CheckUpdate(CancellationToken cancellationToken)
        {
            if (!_containsRequisition)
            {
                if (Application.HasCredentials)
                {
                    _request = await ObterModeloRequest();

                    if (_request == null)
                    {
                        return null;
                    }

                    _containsRequisition = true;
                }
                else
                {
                    return null;
                }
            }

            var message = new HttpRequestMessage(HttpMethod.Query, _config.Url)
            {
                Content = JsonContent.Create(_request),
            };

            var response = await _client.SendAsync<AtualizacaoResposta?>(
                message,
                cancellationToken
            );

            if (!response.Success)
            {
                _logger.LogWarning(response.Error);
                return null;
            }

            if (response.Data == null)
            {
                return null;
            }

            return response.Data.UrlDownload;
        }

        private async Task InstallUpdate(string url, CancellationToken cancellationToken)
        {
            await DownloadReleaseZip(url, cancellationToken);

            ExtractZip();

            if (StopService())
            {
                BackupBinary();
                UpdateBinary();
                StartService();
            }
        }

        private async Task DownloadReleaseZip(string url, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(_config.UpdateDirectory);

            var client = _factoryHttpClient.CreateClient();

            //client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            //client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Software-Irrigacao");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                ""
            );

            _logger.LogInformation("Baixando zip de {url}", url);

            var response = await client.GetAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();

            var zipPath = Path.Combine(_config.UpdateDirectory, $"{_config.BinaryName}.zip");

            await using var fileStream = File.Create(zipPath);
            await response.Content.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("Zip baixado para {updateDirectory}", _config.UpdateDirectory);
        }

        private void ExtractZip()
        {
            Directory.CreateDirectory(_config.UpdateDirectory);

            _logger.LogInformation("Extraindo zip em {updateDirectory}", _config.UpdateDirectory);

            ZipFile.ExtractToDirectory(
                Path.Combine(_config.UpdateDirectory, $"{_config.BinaryName}.zip"),
                _config.UpdateDirectory,
                true
            );

            _logger.LogInformation("Zip extraído com sucesso");
        }

        private bool StopService()
        {
            _logger.LogInformation("Parando serviço {serviceName}", _config.ServiceName);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo systemctl stop {_config.ServiceName}\"",
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
                    "Serviço {serviceName} parado com sucesso",
                    _config.ServiceName
                );
                return true;
            }

            var error = process.StandardError.ReadToEnd();
            _logger.LogError(
                "Falha ao parar serviço {serviceName}: {error}",
                _config.ServiceName,
                error
            );

            return false;
        }

        private void BackupBinary()
        {
            var binaryDirectory = Path.Combine(Directory.GetCurrentDirectory(), _config.BinaryName);
            Directory.CreateDirectory(_config.BackupPath);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(_config.BackupPath, $"{_config.BinaryName}_{timestamp}");

            _logger.LogInformation(
                "Fazendo backup de {sourcePath} para {backupPath}",
                binaryDirectory,
                backupPath
            );

            File.Copy(binaryDirectory, backupPath, true);

            _logger.LogInformation("Backup concluído com sucesso");
        }

        private bool UpdateBinary()
        {
            var binaryUpdate = Path.Combine(_config.UpdateDirectory, _config.BinaryName);
            var destinationPath = Path.Combine(Directory.GetCurrentDirectory(), _config.BinaryName);

            _logger.LogInformation(
                "Movendo binário de {binaryUpdate} para {destinationPath}",
                binaryUpdate,
                destinationPath
            );

            File.Copy(binaryUpdate, destinationPath, true);

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

        private bool StartService()
        {
            _logger.LogInformation("Iniciando serviço {serviceName}", _config.ServiceName);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"sudo systemctl start {_config.ServiceName}\"",
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
                    _config.ServiceName
                );
                return true;
            }

            var error = process.StandardError.ReadToEnd();
            _logger.LogError(
                "Falha ao iniciar serviço {serviceName}: {error}",
                _config.ServiceName,
                error
            );
            return false;
        }
    }
}
