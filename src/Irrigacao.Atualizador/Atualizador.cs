using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Irrigacao.Atualizador
{
    public record GitHubRelease
    {
        [JsonPropertyName("name")]
        public string TagName { get; set; } = string.Empty;
    }
    public class Atualizador : BackgroundService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<Atualizador> _logger;

        public Atualizador(HttpClient httpClient, ILogger<Atualizador> logger)
        {
            httpClient.BaseAddress = new Uri("https://github.com/Dev-Raiam/Software-Irrigacao/");
            _httpClient = httpClient;
            _logger = logger;
        }

        private const string UrlZip = "releases/latest/download/irrigacao.zip";
        private const string UrlVersion = "releases/latest";
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                    var response = await _httpClient.GetAsync(UrlVersion, stoppingToken);

                    var release = await response.Content.ReadFromJsonAsync<string>();
                    //if(release != null) 
                    //{
                    //    var version = release.TagName;
                    //}
                }
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
