using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Logging;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using Toolbox.Automacao.Core.Api;
using Toolbox.Automacao.Core.Models;
using Toolbox.Automacao.Core.Setup;

namespace Toolbox.Automacao.Core.Services.Automacao;

public interface IServicoAutomacao
{
    Task<Result<List<Controlador>>> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    );
}

internal sealed class ServicoAutomacao : BaseApi, IServicoAutomacao
{
    //private readonly HttpClient _httpClient;

    public ServicoAutomacao(IHttpClientFactory httpClientFactory, [FromKeyedServices(HttpClientNames.Automacao)] HttpClient httpClient, ILogger<BaseApi> logger)
        : base(httpClient, logger)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(CustomMediaTypes.AutomacaoV1)
        );
    }

    public async Task<Result<List<Controlador>>> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    )
    {
        var response = await GetAsync<List<Controlador>>(
            $"automacao/v1/paineis/{painelId}/controladores?status=todos",
            cancellationToken
        );

        return response;
    }
}
