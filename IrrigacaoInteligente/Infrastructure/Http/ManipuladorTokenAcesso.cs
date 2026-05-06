using System.Net.Http.Headers;
using IrrigacaoInteligente.Infrastructure.Auth;

namespace IrrigacaoInteligente.Infrastructure.Http;

public class ManipuladorTokenAcesso : DelegatingHandler
{
    private readonly GerenciadorToken _gerenciadorToken;

    public ManipuladorTokenAcesso(GerenciadorToken gerenciadorToken)
    {
        _gerenciadorToken = gerenciadorToken;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = await _gerenciadorToken.ObterTokenValido(cancellationToken);

        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token.TokenAcesso
            );

        return await base.SendAsync(request, cancellationToken);
    }
}
