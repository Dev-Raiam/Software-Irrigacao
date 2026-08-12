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

        private const string UrlZip =
            "https://github.com/Dev-Raiam/Software-Irrigacao/releases/latest/download/irrigacao.zip";
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
                // Busque a vers�o atual do binario que esta rodando no Linux
                var currentVersion = BinaryVersion();

                // Compare vers�o (L�gica Completa)
                if (versionRelease > currentVersion)
                {
                    // Baixar o zip do Git hub em uma pasta separada
                    // Extrair o zip
                    // verificar se o binario existe
                    // Parar o servico de irrigacao
                    // Mover o binario antigo para um diretorio temporario
                    // Mover o Binario novo para o diretorio opt/edg-plc
                    // Start o servico
                }

                //var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(
                //    stoppingToken
                //);
                //Version version = Version.Parse(release.TagName.Replace("v", ""));

                //Version versionLocal = new Version(1, 3, 4);

                //if (version > versionLocal)
                //{
                //    _logger.LogInformation($"Version {version}");
                //}

                //if(release != null)
                //{
                //    var version = release.TagName;
                //}
                await Task.Delay(1000, stoppingToken);
            }
        }

        private Version BinaryVersion()
        {
            // Logica de pegar a verss�o do binario do Linux
            return new Version(1, 3, 4);
        }
    }
}
