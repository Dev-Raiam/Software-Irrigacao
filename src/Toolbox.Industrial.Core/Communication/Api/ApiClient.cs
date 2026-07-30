using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace Toolbox.Industrial.Core.Communication.Api;

public sealed record Configuration(string BaseUrl);

public interface IApiClient
{
    Task<Result<T>> SendAsync<T>(HttpRequestMessage request, CancellationToken cancellationToken);
}

public class ApiClient : IApiClient
{
    internal static string? JwtJwksUrl;
    internal static string? JwtIssuers;
    internal static string? BaseAddress;
    internal static SigningCredentials? Credentials = null;
    public const string Anonymous = "anonymous";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiClient> _logger;

    public ApiClient(HttpClient httpClient, ILogger<ApiClient> logger)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<Result<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => Result<T>.Fail("Não autorizado"),
                    System.Net.HttpStatusCode.Forbidden => Result<T>.Fail("Sem permissão"),
                    System.Net.HttpStatusCode.NotFound => Result<T>.Fail("Recurso não encontrado"),
                    _ => Result<T>.Fail($"Erro HTTP: {response.StatusCode}"),
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var data = JsonConvert.DeserializeObject<T>(json);
                return Result<T>.Ok(data);
            }

            return Result<T>.Ok();
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Timeout ao chamar {Url}", request.RequestUri);
            return Result<T>.Fail($"Timeout {ex.Message}", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Falha de conexão ao chamar {Url}", request.RequestUri);
            return Result<T>.Fail($"Erro de conexão {ex.Message}", ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogError(ex, "Erro ao converter JSON de {Url}", request.RequestUri);
            return Result<T>.Fail($"Erro ao converter JSON {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao chamar {Url}", request.RequestUri);
            return Result<T>.Fail($"Erro inesperado {ex.Message}", ex);
        }
    }
}
