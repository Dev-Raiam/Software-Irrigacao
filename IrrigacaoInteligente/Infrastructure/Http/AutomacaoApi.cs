using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using System.Text.Json;
using IrrigacaoInteligente.Features.Sincronizacao.Interfaces;
using IrrigacaoInteligente.State;
using Microsoft.Extensions.Options;

namespace IrrigacaoInteligente.Infrastructure.Http;

public sealed class AutomacaoApi : IAutomacaoApi
{
    private readonly HttpClient _httpClient;
    private readonly ApiOptions _apiOptions;
    private readonly ILogger<AutomacaoApi> _logger;
    private readonly JsonSerializerOptions? _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public AutomacaoApi(
        HttpClient httpClient,
        IOptions<ApiOptions> apiOptions,
        ILogger<AutomacaoApi> logger
    )
    {
        _httpClient = httpClient;
        _apiOptions = apiOptions.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_apiOptions.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_apiOptions.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue(_apiOptions.MediaType)
        );
    }

    public async Task<List<Dictionary<string, dynamic>>?> ObterControladoresPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/controladores?status=todos",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter Controladores por Painel: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var controladores = await response.Content.ReadFromJsonAsync<
                List<Dictionary<string, dynamic>>
            >(cancellationToken: cancellationToken, options: _jsonSerializerOptions);

            return controladores;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Controladores por Painel");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Controladores por Painel");
            return null;
        }
    }
}
