using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Text.Json;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services
{
    public interface IServicoAutenticacao
    {
        Task<Result<Token>> Autenticar(Integracao integracao, CancellationToken cancellationToken);
    }

    internal sealed class ServicoAutenticacao : BaseApi, IServicoAutenticacao
    {
        private readonly HttpClient _httpClient;

        public ServicoAutenticacao(HttpClient httpClient, ILogger<BaseApi> logger)
            : base(logger)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<Token>> Autenticar(
            Integracao integracao,
            CancellationToken cancellationToken
        )
        {
            HttpContent content = new StringContent(
                JsonSerializer.Serialize(integracao),
                System.Text.Encoding.UTF8,
                MediaTypeNames.Application.Json
            );

            var response = await PostAsync<Token>(
                _httpClient,
                "/autenticacao/v1/autenticar-cliente",
                content,
                cancellationToken
            );

            if (!response.Sucesso || response.Dado == null)
                return response;

            response.Dado.DecrementarSegundosExpiracao();

            return response;
        }
    }
}
