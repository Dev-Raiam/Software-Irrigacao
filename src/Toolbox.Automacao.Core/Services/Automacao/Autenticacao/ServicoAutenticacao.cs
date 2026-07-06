using Microsoft.Extensions.Options;
using System.Text.Json;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Setup;

namespace Toolbox.Automacao.Core.Services
{
    public interface IServicoAutenticacao
    {
        Task<Result<Token>> Autenticar(Credencial credencial, CancellationToken cancellationToken);
    }
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

            if (!response.Sucesso || response.Dado == null)
                return response;

            response.Dado.DecrementarExpiracao();

            return response;
        }
    }
}
