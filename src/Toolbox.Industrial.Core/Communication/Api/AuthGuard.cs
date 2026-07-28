using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Security.Cryptography;

namespace Toolbox.Industrial.Core.Communication.Api;

internal sealed record Credentials(string chave, string segredo, Guid contextoId);

internal class AuthGuard : DelegatingHandler
{
    private readonly Token _token;
    private readonly IApiClient _client;
    private readonly IEntityStore _store;

    //private readonly HttpClient _httpClient;
    private readonly ICryptography _cryptography;
    private readonly ILogger<AuthGuard> _logger;

    public AuthGuard(
        Token token,
        IEntityStore store,
        //HttpClient httpClient,
        ICryptography cryptography,
        ILogger<AuthGuard> logger,
        [FromKeyedServices(ApiClient.Anonymous)] IApiClient client
    )
    {
        _token = token;
        _store = store;
        _client = client;
        _logger = logger;
        //_httpClient = httpClient;
        _cryptography = cryptography;
    }

    public async Task<Credentials?> GetCredentials()
    {
        var chave = (
            await _store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.Auth.Chave)
        )?.Value.ToString();
        var segredo = (
            await _store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.Auth.Segredo)
        )?.Value.ToString();
        var contextoId = (
            await _store.FirstOrDefaultAsync<Configuracao>(x => x.Id == Entity.Keys.Auth.ContextoId)
        )?.Value.ToString();

        if (chave == null || segredo == null || contextoId == null)
            return null;

        return new Credentials(
            _cryptography.Decrypt(chave),
            _cryptography.Decrypt(segredo),
            Guid.Parse(contextoId)
        );
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

        var response = await _client.PostAsync<Token>(
            "/autenticacao/v1/autenticar-cliente",
            content,
            cancellationToken
        );

        return response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (_token.Expirado)
        {
            var credentials = await GetCredentials();

            if (credentials == null)
            {
                _logger.LogError("Credenciais de integração não encontradas.");
                return await base.SendAsync(request, cancellationToken);
            }

            var response = await Authenticate(credentials, cancellationToken);

            if (!response.Success || response.Data == null)
            {
                _logger.LogError("Falha na autenticação: {Error}", response.Error);
                return await base.SendAsync(request, cancellationToken);
            }
            _token.Update(response.Data);
        }

        if (!string.IsNullOrEmpty(_token.TokenAcesso))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _token.TokenAcesso
            );
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
