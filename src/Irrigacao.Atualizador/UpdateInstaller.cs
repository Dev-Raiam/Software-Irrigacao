using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using Irrigacao.Atualizador.Extensions;
using Microsoft.IdentityModel.Tokens;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Platform;
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
        private readonly IShell _shell;
        private readonly IEntityStore _store;
        private readonly IApiClient _client;
        private readonly IHttpClientFactory _factoryHttpClient;
        private readonly ILogger<UpdateInstaller> _logger;

        public UpdateInstaller(
            IShell shell,
            IEntityStore store,
            IApiClient client,
            IHttpClientFactory factoryHttpClient,
            ILogger<UpdateInstaller> logger
        )
        {
            _shell = shell;
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

        private TimeSpan _timeout = TimeSpan.FromSeconds(2);
        private TimeSpan _startTimeout = TimeSpan.FromSeconds(130);

        public async Task Run(UpdateResponse data, CancellationToken cancellationToken)
        {
            if (!await DownloadZip(data.UrlDownload, cancellationToken))
                return;

            if (!await StopAndWait(cancellationToken))
                return;

            BackupBinary();

            if (ExtractZip())
            {
                await UpdateConfigurations(data, cancellationToken);
                await _client.UpdateConfirm(_logger, data.Id, _urlConfirm, cancellationToken);
            }

            await _shell.StartService(_serviceName, cancellationToken);

            if (
                await _shell.WaitForStatus(
                    _serviceName,
                    ServiceStatus.Running,
                    _startTimeout,
                    cancellationToken
                )
            )
                return;

            await Rollback(cancellationToken);
        }

        private async Task<bool> StopAndWait(CancellationToken cancellationToken)
        {
            await _shell.StopService(_serviceName, cancellationToken);
            return await _shell.WaitForStatus(
                _serviceName,
                ServiceStatus.Stopped,
                _timeout,
                cancellationToken
            );
        }

        private async Task UpdateConfigurations(
            UpdateResponse data,
            CancellationToken cancellationToken
        )
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
        }

        private async Task Rollback(CancellationToken cancellationToken)
        {
            _logger.LogWarning("Serviço não atingiu estado Running. Iniciando rollback.");

            if (!await _shell.StopService(_serviceName, cancellationToken))
                return;

            if (
                !await _shell.WaitForStatus(
                    _serviceName,
                    ServiceStatus.Stopped,
                    _timeout,
                    cancellationToken
                )
            )
                return;

            RestoreBinary();

            await _shell.StartService(_serviceName, cancellationToken);
        }

        private void RestoreBinary()
        {
            var backupPath = Path.Combine(_backupPath, _binaryName);

            if (!File.Exists(backupPath))
            {
                _logger.LogError("Backup não encontrado em {backupPath}", backupPath);
                return;
            }

            _logger.LogInformation("Restaurando binário de {backupPath}", backupPath);

            File.Copy(backupPath, _binaryName, true);
            AddPermissionExecution();

            _logger.LogInformation("Binário restaurado com sucesso");
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

                File.Delete(zip);

                AddPermissionExecution();

                return true;
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is IOException)
            {
                _logger.LogError(ex, "Falha ao extrair o zip {zip}", zip);

                return false;
            }
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
    }
}
