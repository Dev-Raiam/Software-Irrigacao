using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Setup;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

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
                    var response = await CheckUpdate(stoppingToken);

                    if (response != null)
                    {
                        await InstallUpdate(response, stoppingToken);
                        //Directory.Delete(_config.UpdateDirectory, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro inesperado na execução do serviço");
                }

                await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken);
            }
        }

        private async Task<AtualizacaoResposta?> CheckUpdate(CancellationToken cancellationToken)
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
                _logger.LogWarning(response.Exception, response.Error);
                return null;
            }

            if (response.Data == null)
                return null;

            _logger.LogInformation(
                "Atualização Disponivel na Version {version} lançada em {lancamento}",
                response.Data.Versao,
                response.Data.Lancamento
            );

            return response.Data;
        }

        private async Task InstallUpdate(
            AtualizacaoResposta request,
            CancellationToken cancellationToken
        )
        {
            await DownloadReleaseZip(request.UrlDownload, cancellationToken);

            if (ExtractZip())
            {
                if (await StopService())
                {
                    BackupBinary();
                    UpdateBinary();
                    AddPermissionExecution();

                    await _store.UpdateAsync(
                        new Configuracao(
                            id: Entity.Keys.AtualizacaoId,
                            configuracao: request.Id,
                            grupo: Grupo.Api,
                            tipo: Tipo.Config
                        )
                    );

                    await _store.UpdateAsync(
                        new Configuracao(
                            id: Entity.Keys.DataVersaoAtual,
                            configuracao: request.Lancamento,
                            grupo: Grupo.Api,
                            tipo: Tipo.Config
                        )
                    );

                    await _store.UpdateAsync(
                        new Configuracao(
                            id: Entity.Keys.VersaoAtual,
                            configuracao: request.Versao,
                            grupo: Grupo.Api,
                            tipo: Tipo.Config
                        )
                    );

                    await ConfirmUpdate(request.Id, cancellationToken);

                    await StartService();
                }
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

        private bool ExtractZip()
        {
            var downloadPath = Path.Combine(_config.UpdateDirectory, "download");
            var extractedPath = Path.Combine(_config.UpdateDirectory, "extracted");

            Directory.CreateDirectory(extractedPath);

            _logger.LogInformation("Extraindo zip em {extractedPath}", extractedPath);

            ZipFile.ExtractToDirectory(
                Path.Combine(downloadPath, $"{_config.BinaryName}.zip"),
                extractedPath,
                true
            );

            var binaryPath = Path.Combine(extractedPath, _config.BinaryName);
            if (!File.Exists(binaryPath))
            {
                _logger.LogError(
                    "Binário {binaryName} não encontrado no zip extraído",
                    _config.BinaryName
                );
                return false;
            }

            _logger.LogInformation("Zip extraído com sucesso");
            return true;
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

        private void UpdateBinary()
        {
            var updatePath = Path.Combine(_config.UpdateDirectory, "extracted", _config.BinaryName);
            var binaryPath = Path.Combine(_config.BinaryDirectory, _config.BinaryName);

            _logger.LogInformation(
                "Movendo binário de {updatePath} para {binaryPath}",
                updatePath,
                binaryPath
            );

            File.Copy(updatePath, binaryPath, true);
        }

        private bool AddPermissionExecution()
        {
            var binaryPath = Path.Combine(_config.BinaryDirectory, _config.BinaryName);
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
                    "Falha ao adicionar permissão de execução do binario {binaryName} Erro: {erro}",
                    _config.BinaryName,
                    error
                );

                return false;
            }

            return true;
        }

        private async Task<bool> StopService()
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
                var status = await StatusService();

                if (status == "inactive")
                {
                    _logger.LogInformation(
                        "Serviço {serviceName} parado com sucesso",
                        _config.ServiceName
                    );
                    return true;
                }
                else if (status == "active")
                {
                    _logger.LogInformation(
                        "Serviço {serviceName} não foi parado",
                        _config.ServiceName
                    );
                    return false;
                }
            }

            _logger.LogError(
                "Falha ao parar serviço {serviceName}: {error}",
                _config.ServiceName,
                error
            );

            return false;
        }

        private async Task<string?> StatusService()
        {
            _logger.LogInformation(
                "Verificando status do serviço {serviceName}",
                _config.ServiceName
            );
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"systemctl is-active {_config.ServiceName}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
            };

            await Task.Delay(TimeSpan.FromSeconds(5));

            process.Start();

            var output = process.StandardOutput.ReadToEnd().Trim();

            process.WaitForExit();

            _logger.LogInformation(
                "Serviço {serviceName} status: {status}",
                _config.ServiceName,
                output
            );

            return output;
        }

        private async Task<bool> StartService()
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
                var status = await StatusService();

                if (status == "inactive")
                {
                    _logger.LogInformation(
                        "Servico {serviceName} não iniciado",
                        _config.ServiceName
                    );
                    return true;
                }
                else if (status == "active")
                {
                    _logger.LogInformation(
                        "Serviço {serviceName} foi iniciado com sucesso",
                        _config.ServiceName
                    );
                    return false;
                }
            }

            _logger.LogError(
                "Falha ao iniciar serviço {serviceName}: {error}",
                _config.ServiceName,
                error
            );

            return false;
        }

        private async Task ConfirmUpdate(Guid atualizacaoId, CancellationToken cancellationToken)
        {
            var message = new HttpRequestMessage(HttpMethod.Query, _config.UrlConfirm)
            {
                Content = JsonContent.Create(new AtualizacaoConfirmacao(atualizacaoId)),
            };

            var response = await _client.SendAsync<string?>(message, cancellationToken);

            if (!response.Success)
            {
                _logger.LogWarning(response.Exception, response.Error);
            }

            _logger.LogInformation("Confirmação de Atualização enviada com sucesso");
        }
    }
}
