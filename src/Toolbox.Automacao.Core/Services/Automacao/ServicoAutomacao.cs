using System.Net.Http.Headers;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Toolbox.Automacao.Core.Services;

internal sealed class ServicoAutomacao : BaseApi, IServicoAutomacao
{
    private readonly HttpClient _httpClient;
    private readonly IHttpClientFactory _httpClientFactory;

    public ServicoAutomacao(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;

        _httpClient = _httpClientFactory.CreateClient("Toolbox");

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.data.automacao.v1+json")
        );
    }

    public async Task<Result<List<Controlador>>> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    )
    {
        var response = await GetAsync<List<Controlador>>(
            _httpClient,
            $"automacao/v1/paineis/{painelId}/controladores?status=todos",
            cancellationToken
        );

        return response;
    }
}
