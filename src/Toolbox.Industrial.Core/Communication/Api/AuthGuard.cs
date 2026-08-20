using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Industrial.Core.Communication.Api.Contracts;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Extensions;
using Toolbox.Industrial.Core.Security.Cryptography;

namespace Toolbox.Industrial.Core.Communication.Api;

internal sealed record Credentials(string chave, string segredo, Guid contextoId);

internal class AuthGuard : DelegatingHandler
{
    private readonly Token _token;
    private readonly IEntityStore _store;
    private readonly IApiClient _apiClient;
    private readonly ICryptography _cryptography;
    private readonly ILogger<AuthGuard> _logger;

    public Token Token => _token;

    public AuthGuard(
        Token token,
        IEntityStore store,
        ICryptography cryptography,
        ILogger<AuthGuard> logger,
        [FromKeyedServices(ApiClient.Anonymous)] IApiClient apiClient
    )
    {
        _token = token;
        _store = store;
        _logger = logger;
        _apiClient = apiClient;
        _cryptography = cryptography;
    }

    public async Task<Credentials?> GetCredentials()
    {
        var chave = (await _store.ObterConfiguracao<string>(Entity.Keys.Auth.Chave));
        var segredo = (await _store.ObterConfiguracao<string>(Entity.Keys.Auth.Segredo));
        var contextoId = (await _store.ObterConfiguracao<string>(Entity.Keys.Auth.ContextoId));

        if (chave == null || segredo == null || contextoId == null)
            return null;

        return new Credentials(
            _cryptography.Decrypt(chave),
            _cryptography.Decrypt(segredo),
            Guid.Parse(contextoId)
        );
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
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    RequestMessage = request,
                    Content = new StringContent("Credenciais de integração não encontradas."),
                };
            }

            var response = await _apiClient.Authenticate(credentials, cancellationToken);

            if (!response.Success || response.Data == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    RequestMessage = request,
                    Content = new StringContent(response.Error ?? "Falha na autenticação"),
                };
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
