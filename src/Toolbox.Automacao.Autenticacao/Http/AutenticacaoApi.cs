using System.Text.Json;
using Toolbox.Automacao.Autenticacao.Dtos;
using Toolbox.Automacao.Autenticacao.Setup;
using Microsoft.Extensions.Options;
using Toolbox.Automacao.Core.Api;

namespace Toolbox.Automacao.Autenticacao.Http
{
    internal sealed class AutenticacaoApi : BaseApi, IAutenticacaoApi
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguracao _apiConfiguration;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public AutenticacaoApi(HttpClient httpClient, IOptions<ApiConfiguracao> apiConfiguration)
        {
            _httpClient = httpClient;
            _apiConfiguration = apiConfiguration.Value;

            _httpClient.BaseAddress = new Uri(_apiConfiguration.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_apiConfiguration.TimeoutSeconds);
        }

        public async Task<Result<Token>> Autenticar(
            Credencial credencial,
            CancellationToken cancellationToken
        )
        {
            HttpContent content = new StringContent(
                JsonSerializer.Serialize(credencial, _jsonOptions),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await PostAsync<Token>(
                _httpClient,
                "autenticacao/v1/autenticar-cliente",
                content,
                cancellationToken
            );

            return response;
        }
    }
}
