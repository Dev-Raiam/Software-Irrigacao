using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Toolbox.Automacao.Core.Data;
using Toolbox.Automacao.Core.Models;

namespace Toolbox.Automacao.Core.Services.Automacao;
public class AuthenticationHandler : DelegatingHandler
{
    private readonly IAuthenticationService _auth;
    private readonly IGerenciadorConfiguracao _gerenciadorConfiguracao;
    private readonly ILogger<AuthenticationHandler> _logger;
    private readonly Token _token;

    public AuthenticationHandler(
        IAuthenticationService autenticacao,
        IGerenciadorConfiguracao gerenciadorConfiguracao,
        ILogger<AuthenticationHandler> logger,
        Token token
    )
    {
        _auth = autenticacao;
        _gerenciadorConfiguracao = gerenciadorConfiguracao;
        _logger = logger;
        _token = token;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (_token.Expired)
        {
            var credentials = _gerenciadorConfiguracao.ObterCredenciaisIntegracao();

            if (credentials == null)
            {
                _logger.LogError("Credenciais de integração não encontradas.");
                return await base.SendAsync(request, cancellationToken);
            }

            var response = await _auth.Authenticate(credentials, cancellationToken);

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
