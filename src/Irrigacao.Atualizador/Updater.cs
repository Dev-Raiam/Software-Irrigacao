using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Setup;
using static Toolbox.Industrial.Core.Data.Configuracao;

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

        private AtualizacaoDisponivel? _credenciaisCache;
        private bool _credenciais = false;

        private async Task<AtualizacaoDisponivel> ObterCredenciais()
        {
            var contaId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ContaId);
            var painelId = await _store.ObterConfiguracao<Guid>(Entity.Keys.PainelId);
            var controladorId = await _store.ObterConfiguracao<Guid>(Entity.Keys.ControladorId);
            var versaoAtual = await _store.ObterConfiguracao<string>(Entity.Keys.VersaoAtual);

            var atualizacaoId = await _store.ObterConfiguracao<Guid>(Entity.Keys.AtualizacaoId);
            var dataVersaoAtual = await _store.ObterConfiguracao<DateTime>(
                Entity.Keys.DataVersaoAtual
            );

            return new AtualizacaoDisponivel(
                contaId,
                painelId,
                controladorId,
                atualizacaoId != Guid.Empty ? atualizacaoId : null,
                versaoAtual ?? "",
                dataVersaoAtual != default ? dataVersaoAtual : null,
                (int)RuntimeInformation.OSArchitecture
            );
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var url = await CheckUpdate(stoppingToken);

                    if (url != null)
                    {
                        await InstallUpdate(url, stoppingToken);
                        Directory.Delete(_config.UpdateDirectory, true);
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
            if (!_credenciais)
            {
                if (Application.HasCredentials)
                {
                    _credenciaisCache = await ObterCredenciais();

                    if (_credenciaisCache == null)
                        return null;

                    _credenciais = true;
                }
                else
                {
                    return null;
                }
            }

            var message = new HttpRequestMessage(HttpMethod.Query, _config.Url)
            {
                Content = JsonContent.Create(_credenciaisCache),
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
                return null;

            //_store.UpdateAsync<Configuracao>(new Configuracao(
            //        id: Entity.Keys.AtualizacaoId,
            //        configuracao: response.Data.AtualizacaoId,
            //        grupo: grupo.Api,
            //        tipo: tipo.Config
            //    ));

            _logger.LogInformation(
                "Atualização Disponivel na Version {version} lançada em {lancamento}",
                response.Data.Versao,
                response.Data.Lancamento
            );

            return response.Data.UrlDownload;
        }

        private async Task InstallUpdate(string url, CancellationToken cancellationToken)
        {
            await DownloadReleaseZip(url, cancellationToken);

            ExtractZip();

            if (StopService())
            {
                BackupBinary();
                if (!UpdateBinary())
                {
                    _logger.LogError("Atualização falhou, restaurando backup");
                    // restaurar backup
                    StartService();
                    return;
                }
                StartService();
            }
        }

        private async Task DownloadReleaseZip(string url, CancellationToken cancellationToken)
        {
            var downloadPath = Path.Combine(_config.UpdateDirectory, "download");

            Directory.CreateDirectory(downloadPath);

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

            var zipPath = Path.Combine(downloadPath, $"{_config.BinaryName}.zip");

            await using var fileStream = File.Create(zipPath);
            await response.Content.CopyToAsync(fileStream, cancellationToken);

            _logger.LogInformation("Zip baixado para {updateDirectory}", _config.UpdateDirectory);
        }

        private void ExtractZip()
        {
            var downloadPath = Path.Combine(_config.UpdateDirectory, "download");
            var extractedPath = Path.Combine(_config.UpdateDirectory, "extracted");

            Directory.CreateDirectory(extractedPath);

            _logger.LogInformation("Extraindo zip em {extractedPath }", extractedPath);

            ZipFile.ExtractToDirectory(
                Path.Combine(downloadPath, $"{_config.BinaryName}.zip"),
                extractedPath,
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
                    Arguments = $"-c \"systemctl stop {_config.ServiceName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation(
                    "Serviço {serviceName} parado com sucesso",
                    _config.ServiceName
                );
                return true;
            }

            _logger.LogError(
                "Falha ao parar serviço {serviceName}: {error}",
                _config.ServiceName,
                error
            );

            return false;
        }

        private void BackupBinary()
        {
            Directory.CreateDirectory(_config.BackupPath);

            //var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            //var backupPath = Path.Combine(_config.BackupPath, $"{_config.BinaryName}_{timestamp}");

            _logger.LogInformation(
                "Fazendo backup de {binaryDirectory} para {backupPath}",
                _config.BinaryDirectory,
                _config.BackupPath
            );

            var binaryPath = Path.Combine(_config.BinaryDirectory, _config.BinaryName);
            var backupPath = Path.Combine(_config.BackupPath, _config.BinaryName);

            File.Copy(binaryPath, backupPath, true);

            _logger.LogInformation("Backup concluído com sucesso");
        }

        private bool UpdateBinary()
        {
            var updatePath = Path.Combine(_config.UpdateDirectory, "extracted", _config.BinaryName);
            var binaryPath = Path.Combine(_config.BinaryDirectory, _config.BinaryName);

            _logger.LogInformation(
                "Movendo binário de {updatePath} para {binaryPath}",
                updatePath,
                binaryPath
            );

            File.Copy(updatePath, binaryPath, true);

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x {binaryPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                _logger.LogError(
                    "Falha ao atualizar binario {binaryName} Erro: {erro}",
                    _config.BinaryName,
                    error
                );

                return false;
            }

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
                    Arguments = $"-c \"systemctl start {_config.ServiceName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            process.Start();

            var error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation(
                    "Serviço {serviceName} iniciado com sucesso",
                    _config.ServiceName
                );
                return true;
            }

            _logger.LogError(
                "Falha ao iniciar serviço {serviceName}: {error}",
                _config.ServiceName,
                error
            );

            return false;
        }
    }
}
