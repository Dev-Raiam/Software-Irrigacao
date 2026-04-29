using System.Net.Http.Headers;
using WorkerService.Infrastructure.Auth;

namespace WorkerService.Infrastructure.Http;

public class ManipuladorTokenAcesso(GerenciadorToken _gerenciadorToken) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = await _gerenciadorToken.ObterTokenValido(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token!.TokenAcesso);

        return await base.SendAsync(request, cancellationToken);
    }
}
