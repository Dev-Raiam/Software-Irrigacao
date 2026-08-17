using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;

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

        private const string UrlAtualizacao =
            "/automacao/v1/integracoes/2eb57304-1df3-4883-8f81-29b3e9426f6c/atualizacao-disponivel";

        private bool _containsRequisition = false;

        private async Task<bool> ExisteCredenciais()
        {
            var contaId = await _store.AnyAsync<Configuracao>(x => x.Id == Entity.Keys.ContaId);
            var painelId = await _store.AnyAsync<Configuracao>(x => x.Id == Entity.Keys.PainelId);
            var controladorId = await _store.AnyAsync<Configuracao>(x =>
                x.Id == Entity.Keys.ControladorId
            );
            var versao = await _store.AnyAsync<Configuracao>(x => x.Id == Entity.Keys.VersaoAtual);

            var chaveExiste = await _store.AnyAsync<Configuracao>(x => x.Id == Entity.Keys.Auth.Chave);
            var segredoExiste = await _store.AnyAsync<Configuracao>(x =>
                x.Id == Entity.Keys.Auth.Segredo
            );
            var contextoIdExiste = await _store.AnyAsync<Configuracao>(x =>
                x.Id == Entity.Keys.Auth.ContextoId
            );

            if (
                contaId
                && painelId
                && controladorId
                && versao
                && chaveExiste
                && segredoExiste
                && contextoIdExiste
            )
                return true;

            return false;
        }

        private async Task<AtualizacaoDisponivel> ObterModeloRequest()
        {
            var contaId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ContaId);
            var painelId = await _store.ObterConfiguracao<Guid>(Entity.Keys.PainelId);
            var controladorId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ControladorId);
            var versaoAtual = await _store.FirstOrDefaultAsync<Configuracao>(x =>
                x.Id == Entity.Keys.VersaoAtual
            );

            return new AtualizacaoDisponivel(
                contaId,
                painelId,
                controladorId,
                null,
                versaoAtual.Valor.ToString()!,
                null,
                (int)RuntimeInformation.OSArchitecture
            );
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            AtualizacaoDisponivel? request = null;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_containsRequisition)
                    {
                        var atualization = await Check(request!, stoppingToken);
                        if (atualization != null)
                            await Install(
                                atualization.UrlDownload,
                                stoppingToken
                            );
                    }
                    else
                    {
                        if (await ExisteCredenciais())
                        {
                            request = await ObterModeloRequest();
                            _containsRequisition = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado na execução do serviço");
                }

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        private async Task<AtualizacaoResposta?> Check(
            AtualizacaoDisponivel request,
            CancellationToken cancellationToken
        )
        {
            var message = new HttpRequestMessage(HttpMethod.Query, UrlAtualizacao)
            {
                Content = JsonContent.Create(request),
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

            return response.Data;
        }
        private async Task Install(string url, CancellationToken cancellationToken)
        {
            var zipPath = Path.Combine(_config.UpdateDirectory, "irrigacao.zip");

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

            var httpClient = _factoryHttpClient.CreateClient("Git");

            httpClient.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
            httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Software-Irrigacao");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                ""
            );

            _logger.LogInformation("Baixando zip de {url}", url);

            var response = await httpClient.GetAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();

            await using var fileStream = File.Create(_config.UpdateDirectory);
            await response.Content.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("Zip baixado para {updateDirectory}", _config.UpdateDirectory);
        }

        private void ExtractZip()
        {
            Directory.CreateDirectory(_config.UpdateDirectory);

            _logger.LogInformation("Extraindo zip em {updateDirectory}", _config.UpdateDirectory);

            ZipFile.ExtractToDirectory(
                Path.Combine(_config.UpdateDirectory, Path.Combine(_config.BinaryName, ".zip")),
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
            var binaryDirectory = Directory.GetCurrentDirectory();
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
            var destinationPath = Directory.GetCurrentDirectory();

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
