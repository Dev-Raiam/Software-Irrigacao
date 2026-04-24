using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using WorkerService.Configurations;
using WorkerService.Features.Shared.Response;
using WorkerService.Features.Sincronizacao.Automacao;

namespace WorkerService.Infrastructure.Http;

public sealed class AutomacaoApi : IAutomacaoApi
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguracao _apiConfiguracao;
    private readonly JsonSerializerSettings _jsonSettings;
    private readonly ILogger<AutomacaoApi> _logger;

    public AutomacaoApi(
        HttpClient httpClient,
        IOptions<ApiConfiguracao> apiConfiguracao,
        ILogger<AutomacaoApi> logger
    )
    {
        _httpClient = httpClient;
        _apiConfiguracao = apiConfiguracao.Value;
        _logger = logger;

        _httpClient.BaseAddress = new Uri(_apiConfiguracao.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(_apiConfiguracao.TimeoutSeconds);
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.data.full.v1+json")
        );

        _jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = { new StringEnumConverter() },
        };
    }

    public async Task<List<Painel>?> ObterPaineisAsync(
        Guid contaId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis?contaId={contaId}&categoria=todos&status=todos",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter painéis: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<Painel>>(json, _jsonSettings);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter painéis");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter painéis");
            return null;
        }
    }

    public async Task<List<Dispositivo>?> ObterDispositivosPorPainelAsync(
        Guid painelId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/dispositivos",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao obter dispositivos: {StatusCode}", response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<Dispositivo>>(json, _jsonSettings);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Dispositivos");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Dispositivos");
            return null;
        }
    }

    // /// Nao Precisa
    // public async Task<List<ModuloResponse>?> ObterControladoresPorPainelAsync(
    //     Guid painelId,
    //     CancellationToken cancellationToken = default
    // )
    // {
    //     var response = await _httpClient.GetAsync(
    //         $"/automacao/v1/paineis/{painelId}/controladores",
    //         cancellationToken
    //     );

    //     if (!response.IsSuccessStatusCode)
    //     {
    //         _logger.LogError("Erro ao obter controladores: {StatusCode}", response.StatusCode);
    //         return null;
    //     }

    //     var json = await response.Content.ReadAsStringAsync(cancellationToken);
    //     return JsonConvert.DeserializeObject<List<ModuloResponse>>(json, _jsonSettings);
    // }

    public async Task<List<Porta>?> ObterPortasPorControladorAsync(
        Guid painelId,
        Guid controladorId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/controladores/{controladorId}/portas?status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter portas por Controlador: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<Porta>>(json, _jsonSettings);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Portas por Controlador");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Portas por Controlador");
            return null;
        }
    }

    public async Task<List<Porta>?> ObterPortasPorModuloAsync(
        Guid painelId,
        Guid moduloId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/modulos/{moduloId}/portas?status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter portas por Modulo: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<Porta>>(json, _jsonSettings);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Portas por Modulo");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Portas por Modulo");
            return null;
        }
    }

    public async Task<List<Interface>?> ObterInterfacesPorControladorAsync(
        Guid painelId,
        Guid controladorId,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/controladores/{controladorId}/interfaces?categoria=todas&status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter Interfaces por Controlador: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<Interface>>(json, _jsonSettings);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Interfaces por Controlador");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Interfaces por Controlador");
            return null;
        }
    }

    public async Task<List<Interface>?> ObterInterfacesPorModuloAsync(
        Guid painelId,
        Guid moduloId,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/automacao/v1/paineis/{painelId}/modulos/{moduloId}/interfaces?categoria=todas&status=todas",
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Erro ao obter Interfaces por Modulos: {StatusCode}",
                    response.StatusCode
                );
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonConvert.DeserializeObject<List<Interface>>(json, _jsonSettings);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao obter Interfaces por Modulo");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao obter Interfaces por Modulo");
            return null;
        }
    }
}
