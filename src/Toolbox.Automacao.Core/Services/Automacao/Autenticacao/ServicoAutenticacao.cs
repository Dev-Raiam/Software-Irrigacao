using System.Text.Json;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Toolbox.Automacao.Core.Services
{
    public interface IServicoAutenticacao
    {
        Task<Result<Token>> Autenticar(Credencial credencial, CancellationToken cancellationToken);
    }
    internal sealed class ServicoAutenticacao : BaseApi, IServicoAutenticacao
    {
        private readonly HttpClient _httpClient;
        public ServicoAutenticacao(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Result<Token>> Autenticar(
            Credencial credencial,
            CancellationToken cancellationToken
        )
        {
            HttpContent content = new StringContent(
                JsonSerializer.Serialize(credencial),
                System.Text.Encoding.UTF8,
                 Application.Json
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
