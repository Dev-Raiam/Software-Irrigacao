using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Toolbox.Automacao.Core.Api;

public class BaseApi
{
    private readonly ILogger<BaseApi> _logger;

    protected BaseApi(ILogger<BaseApi> logger)
    {
        _logger = logger;
    }

    protected async Task<Result<T>> GetAsync<T>(
        HttpClient http,
        string url,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await http.GetAsync(url, cancellationToken);

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

    protected async Task<Result<T>> PostAsync<T>(
        HttpClient http,
        string url,
        HttpContent content,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await http.PostAsync(url, content, cancellationToken);

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
}
