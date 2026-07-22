using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Text.Json;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services.Automacao
{
    public record Credentials(string chave, string segredo, Guid contextoId);

    public interface IAuthenticationService
    {
        Task<Result<Token>> Authenticate(Credentials credentials, CancellationToken cancellationToken);
    }

    internal sealed class AuthenticationService : BaseApi, IAuthenticationService
    {
        public AuthenticationService(HttpClient httpClient, ILogger<BaseApi> logger)
            : base(httpClient, logger)
        {
        }

        public async Task<Result<Token>> Authenticate(
            Credentials credentials,
            CancellationToken cancellationToken
        )
        {
            HttpContent content = new StringContent(
                JsonSerializer.Serialize(credentials),
                System.Text.Encoding.UTF8,
                MediaTypeNames.Application.Json
            );

            var response = await PostAsync<Token>(
                "/autenticacao/v1/autenticar-cliente",
                content,
                cancellationToken
            );

            return response;
        }
    }
}
