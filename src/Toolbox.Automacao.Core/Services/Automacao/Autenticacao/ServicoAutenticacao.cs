using Microsoft.Extensions.Options;
using System.Text.Json;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Setup;

namespace Toolbox.Automacao.Core.Services
{
    internal sealed class ServicoAutenticacao : BaseApi, IServicoAutenticacao
    {
        private readonly HttpClient _httpClient;
        private readonly ApiConfiguracao _apiConfiguration;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public ServicoAutenticacao(HttpClient httpClient, IOptions<ApiConfiguracao> apiConfiguration)
        {
            _httpClient = httpClient;
            _apiConfiguration = apiConfiguration.Value;

            _httpClient.BaseAddress = new Uri(_apiConfiguration.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds(_apiConfiguration.TimeoutSeconds);
        }

        public async Task<Result<Token>> Autenticar(
            string chave,
            string segredo,
            Guid contextoId,
            CancellationToken cancellationToken
        )
        {
            HttpContent content = new StringContent(
                JsonSerializer.Serialize(new {Chave = chave, Segredo = segredo, ContextoId = contextoId}, _jsonOptions),
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
