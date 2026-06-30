using System.Net.Http.Json;

namespace Toolbox.Automacao.Core.Api;

public class BaseApi
{
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
            return Result<T>.Fail("Timeout", ex);
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Fail("Erro de conexão", ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return Result<T>.Fail("Erro ao converter JSON", ex);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail($"Erro inesperado", ex);
        }
    }

    protected static async Task<Result<T>> PostAsync<T>(
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
            return Result<T>.Fail("Timeout", ex);
        }
        catch (HttpRequestException ex)
        {
            return Result<T>.Fail("Erro de conexão", ex);
        }
        catch (System.Text.Json.JsonException ex)
        {
            return Result<T>.Fail("Erro ao converter JSON", ex);
        }
        catch (Exception ex)
        {
            return Result<T>.Fail($"Erro inesperado", ex);
        }
    }
}
