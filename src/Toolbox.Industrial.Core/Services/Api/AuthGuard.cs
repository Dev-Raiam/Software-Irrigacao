using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Data.Entities;
using Toolbox.Industrial.Core.Models;
using Toolbox.Industrial.Core.Services.Cryptography;

namespace Toolbox.Industrial.Core.Services.Api;

internal sealed record Credentials(string chave, string segredo, Guid contextoId);

internal class AuthGuard : DelegatingHandler
{
    private readonly Token _token;
    private readonly IApiClient _client;
    private readonly HttpClient _httpClient;
    private readonly IRepository _repository;
    private readonly ICryptography _cryptography;
    private readonly ILogger<AuthGuard> _logger;

    public AuthGuard(
        Token token,
        IApiClient client,
        HttpClient httpClient,
        IRepository repository,
        ICryptography cryptography,
        ILogger<AuthGuard> logger
    )
    {
        _token = token;
        _client = client;
        _logger = logger;
        _httpClient = httpClient;
        _repository = repository;
        _cryptography = cryptography;
    }

    public Credentials? GetCredentials()
    {
        var chave = _repository.FirstOrDefault<Configuracao>(x => x.Id == Entity.Keys.Auth.Chave);
        var segredo = _repository.FirstOrDefault<Configuracao>(x =>
            x.Id == Entity.Keys.Auth.Segredo
        );
        var contextoId = _repository.FirstOrDefault<Configuracao>(x =>
            x.Id == Entity.Keys.Auth.ContextoId
        );

        if (chave == null || segredo == null || contextoId == null)
            return null;

        return new Credentials(
            _cryptography.Decrypt(chave.Value),
            _cryptography.Decrypt(segredo.Value),
            Guid.Parse(contextoId.Value)
        );
    }

    private async Task<Result<Token>> Authenticate(
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
            cancellationToken,
            _httpClient
        );

        return response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (_token.Expired)
        {
            var credentials = GetCredentials();

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

        if (!string.IsNullOrEmpty(_token.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _token.AccessToken
            );
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
