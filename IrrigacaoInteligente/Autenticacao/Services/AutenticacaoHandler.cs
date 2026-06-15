using System.Net.Http.Headers;
using Autenticacao.Interfaces;
using Autenticacao.Models;

namespace Autenticacao.Services;

public class AutenticacaoHandler : DelegatingHandler
{
    private readonly IAutenticacaoApi _autenticacaoApi;
    private readonly ICredenciaisAutenticacao _credenciaisAutenticacao;
    private readonly ILogger<AutenticacaoHandler> _logger;
    private readonly Token _token;

    public AutenticacaoHandler(
        IAutenticacaoApi autenticacaoApi,
        ICredenciaisAutenticacao credenciaisAutenticacao,
        ILogger<AutenticacaoHandler> logger,
        Token token
    )
    {
        _autenticacaoApi = autenticacaoApi;
        _credenciaisAutenticacao = credenciaisAutenticacao;
        _logger = logger;
        _token = token;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (_token.Expirado)
        {
            var credencial = await _credenciaisAutenticacao.ObterCredencial(cancellationToken);

            var result = await _autenticacaoApi.Autenticar(credencial, cancellationToken);

            if (result.Sucesso && result.Dado != null)
            {
                result.Dado.Expira = result.Dado.Expira.AddSeconds(-15);

                _token.Atualizar(result.Dado);
            }
            else
            {
                _logger.LogError("Falha ao autenticar: {Error}", result.Error);

                if (result.Exception != null)
                {
                    _logger.LogError("Exception: {Exception}", result.Exception.Message);
                }
            }
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
