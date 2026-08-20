namespace Toolbox.Industrial.Core.Communication.Api;

public sealed record Configuration(string BaseUrl);

public interface IApiClient : IDisposable
{
    Task<Result<T>> SendAsync<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken,
        HttpClient? httpClient = null
    );
}

public sealed class ApiClient : IApiClient
{
    internal static bool Online = true;
    internal static string? BaseAddress;

    internal const string Anonymous = "anonymous";

    public static bool IsOnline => Online;

    private readonly HttpClient _httpClient;
    private bool _disposed = false;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
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
                var content = await response.Content.ReadAsStringAsync();
                return response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Unauthorized => Result<T>.Fail(
                        $"Não autorizado => {content}"
                    ),
                    System.Net.HttpStatusCode.Forbidden => Result<T>.Fail(
                        $"Sem permissão => {content}"
                    ),
                    System.Net.HttpStatusCode.NotFound => Result<T>.Fail(
                        $"Recurso não encontrado => {content}"
                    ),
                    _ => Result<T>.Fail($"Erro HTTP: {response.StatusCode} => {content}"),
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(json))
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);
                return Result<T>.Ok(data);
            }

            return Result<T>.Ok();
        }
        catch (TaskCanceledException ex)
        {
            return Result<T>.Fail($"Timeout {ex.Message}", ex);
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Fail($"Erro de conexão {ex.Message}", ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return Result<T>.Fail($"Erro ao converter JSON {ex.Message}", ex);
        }
        catch (Newtonsoft.Json.JsonException ex)
        {
            return Result<T>.Fail($"Erro ao converter JSON {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail($"Erro inesperado {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _httpClient.Dispose();
    }
}
