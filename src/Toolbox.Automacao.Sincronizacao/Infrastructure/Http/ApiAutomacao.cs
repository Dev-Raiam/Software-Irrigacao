using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Sincronizacao.Core.Abstractions;
using Toolbox.Automacao.Sincronizacao.Core.Entities;
using Toolbox.Automacao.Sincronizacao.Extensions.Options;

namespace Toolbox.Automacao.Sincronizacao.Infrastructure.Http;

internal class ApiAutomacao : BaseApi, IApiAutomacao
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguracao _apiConfiguracao;

    public ApiAutomacao(HttpClient httpClient, IOptions<ApiConfiguracao> apiConfiguracao)
    {
        _httpClient = httpClient;
        _apiConfiguracao = apiConfiguracao.Value;

        _httpClient.BaseAddress = new Uri(_apiConfiguracao.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_apiConfiguracao.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(_apiConfiguracao.MediaType)
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
