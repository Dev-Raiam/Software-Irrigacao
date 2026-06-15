using System.Net.Http.Json;
using System.Text.Json;
using Autenticacao.Configurations;
using Autenticacao.Models;
using IrrigacaoInteligente.Core;
using Microsoft.Extensions.Options;

namespace Autenticacao.Services
{
    internal sealed class AutenticacaoApi : BaseApi, IAutenticacaoApi
    {
        private readonly HttpClient _httpClient;
        private readonly AppOptions _appOptions;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public AutenticacaoApi(HttpClient httpClient, IOptions<AppOptions> appOptions)
        {
            _httpClient = httpClient;
            _appOptions = appOptions.Value;

            _httpClient.BaseAddress = new Uri(_appOptions.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_appOptions.TimeoutSeconds);
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
