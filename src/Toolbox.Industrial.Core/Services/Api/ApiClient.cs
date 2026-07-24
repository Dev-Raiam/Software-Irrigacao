using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Services.Api;

public sealed record Configuration(string BaseUrl);

public interface IApiClient
{
    HttpClient HttpClient { get; }

    Task<Result<T>> GetAsync<T>(
        string url,
        CancellationToken cancellationToken,
        HttpClient? httpClient = null
    );

    Task<Result<T>> PostAsync<T>(
        string url,
        HttpContent content,
        CancellationToken cancellationToken,
        HttpClient? httpClient = null
    );

    Task<Result<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        HttpClient? httpClient = null
    );
}

public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    protected ApiClient(
        [FromKeyedServices(HttpClientNames.Automacao)] HttpClient httpClient,
        ILogger<ApiClient> logger
    )
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public HttpClient HttpClient => _httpClient;

    public async Task<Result<T>> GetAsync<T>(
        string url,
        CancellationToken cancellationToken,
        HttpClient? httpClient = null
    )
    {
        try
        {
            httpClient ??= _httpClient;

            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => Result<T>.Fail("Não Autorizado"),
                    System.Net.HttpStatusCode.Forbidden => Result<T>.Fail("Sem Permissão"),
                    System.Net.HttpStatusCode.NotFound => Result<T>.Fail("Recurso não encontrado"),
                    _ => Result<T>.Fail($"Erro HTTP: {response.StatusCode}"),
                };
            }

            var data = await response.Content.ReadFromJsonAsync<T>();

            if (data == null)
                return Result<T>.Fail("Resposta nula");

            return Result<T>.Ok(data);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout ao chamar {Url}", url);
            return Result<T>.Fail($"Timeout {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Falha de conexão ao chamar {Url}", url);
            return Result<T>.Fail($"Erro de conexão {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Erro ao converter JSON de {Url}", url);
            return Result<T>.Fail($"Erro ao converter JSON {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao chamar {Url}", url);
            return Result<T>.Fail($"Erro inesperado {ex.Message}");
        }
    }

    public async Task<Result<T>> PostAsync<T>(
        string url,
        HttpContent content,
        CancellationToken cancellationToken,
        HttpClient? httpClient = null
    )
    {
        try
        {
            httpClient ??= _httpClient;
            var response = await httpClient.PostAsync(url, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => Result<T>.Fail("Não Autorizado"),
                    System.Net.HttpStatusCode.Forbidden => Result<T>.Fail("Sem Permissão"),
                    System.Net.HttpStatusCode.NotFound => Result<T>.Fail("Recurso não encontrado"),
                    _ => Result<T>.Fail($"Erro HTTP: {response.StatusCode}"),
                };
            }

            var data = await response.Content.ReadFromJsonAsync<T>();

            if (data == null)
                return Result<T>.Fail("Resposta nula");

            return Result<T>.Ok(data);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout ao chamar {Url}", url);
            return Result<T>.Fail($"Timeout {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Falha de conexão ao chamar {Url}", url);
            return Result<T>.Fail($"Erro de conexão {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Erro ao converter JSON de {Url}", url);
            return Result<T>.Fail($"Erro ao converter JSON {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao chamar {Url}", url);
            return Result<T>.Fail($"Erro inesperado {ex.Message}");
        }
    }

    public async Task<Result<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        HttpClient? httpClient = null
    )
    {
        try
        {
            httpClient ??= _httpClient;

            var response = await httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => Result<T>.Fail("Não Autorizado"),
                    System.Net.HttpStatusCode.Forbidden => Result<T>.Fail("Sem Permissão"),
                    System.Net.HttpStatusCode.NotFound => Result<T>.Fail("Recurso não encontrado"),
                    _ => Result<T>.Fail($"Erro HTTP: {response.StatusCode}"),
                };
            }

            var data = await response.Content.ReadFromJsonAsync<T>();

            if (data == null)
                return Result<T>.Fail("Resposta nula");

            return Result<T>.Ok(data);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout ao chamar {Url}", request.RequestUri);
            return Result<T>.Fail($"Timeout {ex.Message}");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Falha de conexão ao chamar {Url}", request.RequestUri);
            return Result<T>.Fail($"Erro de conexão {ex.Message}");
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Erro ao converter JSON de {Url}", request.RequestUri);
            return Result<T>.Fail($"Erro ao converter JSON {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao chamar {Url}", request.RequestUri);
            return Result<T>.Fail($"Erro inesperado {ex.Message}");
        }
    }
}
