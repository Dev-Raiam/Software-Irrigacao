using Irrigacao.Atualizador.Extensions;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Grupo = Toolbox.Industrial.Core.Data.Configuracao.grupo;
using Tipo = Toolbox.Industrial.Core.Data.Configuracao.tipo;

namespace Irrigacao.Atualizador
{
    // Mudar para o Toolbox.Industrial.Core
    public interface IUpdateInstaller
    {
        Task Run(UpdateResponse request, CancellationToken cancellationToken);
    }

    internal class UpdateInstaller : IUpdateInstaller
    {
        private readonly IEntityStore _store;
        private readonly IApiClient _client;
        private readonly IHttpClientFactory _factoryHttpClient;
        private readonly ILogger<UpdateInstaller> _logger;

        public UpdateInstaller(
            IEntityStore store,
            IApiClient client,
            IHttpClientFactory factoryHttpClient,
            ILogger<UpdateInstaller> logger
        )
        {
            _store = store;
            _client = client;
            _factoryHttpClient = factoryHttpClient;
            _logger = logger;
        }
        // Utilizar o Metodo de Extensão em Toolbox.Industrial.Core AddAtualizadorCore() ja disponibilizando essa interface
        private const string _binaryName = "irrigacao";
        private const string _serviceName = "irrigacao";
        private const string _backupPath = "/var/backups/edge-plc";
        private const string _urlConfirm =
            "/automacao/v1/integracoes/2eb57304-1df3-4883-8f81-29b3e9426f6c/confirmar-download-atualizacao";
        private readonly string _currentDirectory = Directory.GetCurrentDirectory();

        public async Task Run(UpdateResponse data, CancellationToken cancellationToken)
        {
            var downloadSucess = await DownloadZip(data.UrlDownload, cancellationToken);

            if (downloadSucess)
            {
                if (await StopService())
                {
                    BackupBinary();

                    if (ExtractZip())
                    {
                        Configuracao[] configuracoes =
                        [
                            new(
                                id: Entity.Keys.AtualizacaoId,
                                configuracao: data.Id,
                                grupo: Grupo.Api,
                                tipo: Tipo.Config
                            ),
                            new(
                                id: Entity.Keys.VersaoAtual,
                                configuracao: data.Versao.ToString(),
                                grupo: Grupo.Api,
                                tipo: Tipo.Config
                            ),
                            new(
                                id: Entity.Keys.DataVersaoAtual,
                                configuracao: data.Lancamento,
                                grupo: Grupo.Api,
                                tipo: Tipo.Config
                            ),
                        ];

                        foreach (var configuracao in configuracoes)
                        {
                            await _store.UpdateAsync(configuracao);
                        }

                        await _client.UpdateConfirm(
                            _logger,
                            data.Id,
                            _urlConfirm,
                            cancellationToken
                        );

                        await StartService();
                    }
                    else
                    {
                        await StartService();
                    }
                }
            }
        }

        private async Task<bool> DownloadZip(string url, CancellationToken cancellationToken)
        {
            var client = _factoryHttpClient.CreateClient();

            client.DefaultRequestHeaders.UserAgent.ParseAdd("Software-Irrigacao");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                ""
            );

            _logger.LogInformation("Baixando zip de {url}", url);

            try
            {
                var response = await client.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Não foi possível baixar o arquivo {BinaryName}. HTTP {StatusCode}",
                        _binaryName,
                        (int)response.StatusCode
                    );

                    return false;
                }

                var zip = $"{_binaryName}.zip";

                await using var fileStream = File.Create(zip);
                await response.Content.CopyToAsync(fileStream, cancellationToken);

                _logger.LogInformation("Zip baixado com sucesso");

                return true;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Falha de comunicação durante o download.");
                return false;
            }
        }

        private bool ExtractZip()
        {
            var zip = $"{_binaryName}.zip";

            if (!File.Exists(zip))
            {
                _logger.LogError($"Zip {zip} não encontrado");
                return false;
            }

            _logger.LogInformation("Extraindo zip");

            try
            {
                ZipFile.ExtractToDirectory(zip, _currentDirectory, true);

                _logger.LogInformation("Zip extraído com sucesso");
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is IOException)
            {
                _logger.LogError(ex, "Falha ao extrair o zip {zip}", zip);

                return false;
            }

            File.Delete(zip);

            AddPermissionExecution();

            return true;
        }

        private void BackupBinary()
        {
            Directory.CreateDirectory(_backupPath);

            _logger.LogInformation(
                "Fazendo backup de {currentDirectory } para {backupPath}",
                _currentDirectory,
                _backupPath
            );

            var backupPath = Path.Combine(_backupPath, _binaryName);

            File.Copy(_binaryName, backupPath, true);

            _logger.LogInformation("Backup concluído com sucesso");
        }

        private bool AddPermissionExecution()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"chmod +x {_binaryName}\"",
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
                    _binaryName,
                    error
                );

                return false;
            }

            return true;
        }

        private async Task<bool> StopService()
        {
            _logger.LogInformation("Parando serviço {serviceName}", _serviceName);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"systemctl stop {_serviceName}\"",
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
                        _serviceName
                    );
                    return true;
                }
                else if (status == "active")
                {
                    _logger.LogInformation("Serviço {serviceName} não foi parado", _serviceName);
                    return false;
                }

                _logger.LogInformation("Serviço {serviceName} com sucesso", _serviceName);
                return true;
            }

            _logger.LogError("Falha ao parar serviço {serviceName}: {error}", _serviceName, error);

            return false;
        }

        private async Task<string?> StatusService()
        {
            _logger.LogInformation("Verificando status do serviço {serviceName}", _serviceName);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"systemctl is-active {_serviceName}\"",
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

            _logger.LogInformation("Serviço {serviceName} status: {status}", _serviceName, output);

            return output;
        }

        private async Task<bool> StartService()
        {
            _logger.LogInformation("Iniciando serviço {serviceName}", _serviceName);
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"systemctl start {_serviceName}\"",
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
                    _logger.LogInformation("Servico {serviceName} não iniciado", _serviceName);
                    return false;
                }
                else if (status == "active")
                {
                    _logger.LogInformation(
                        "Serviço {serviceName} foi iniciado com sucesso",
                        _serviceName
                    );
                    return true;
                }
            }

            _logger.LogError(
                "Falha ao iniciar serviço {serviceName}: {error}",
                _serviceName,
                error
            );

            return false;
        }
    }
}
