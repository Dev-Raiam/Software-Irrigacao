using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using WorkerService.Configurations;
using WorkerService.Features.Infrastructure.GerenciamentoToken;
using WorkerService.Features.Shared.Abstractions;

namespace WorkerService.Infrastructure.Http;

public sealed class AutenticacaoApi : IAutenticacaoApi
{
    private readonly HttpClient _httpClient;
    private readonly ApiConfiguracao _apiConfiguracao;
    private readonly ILogger<AutenticacaoApi> _logger;

    public AutenticacaoApi(
        HttpClient httpClient,
        IOptions<ApiConfiguracao> apiConfiguracao,
        ILogger<AutenticacaoApi> logger
    )
    {
        _httpClient = httpClient;
        _apiConfiguracao = apiConfiguracao.Value;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(_apiConfiguracao.BaseUrl);
    }

    public async Task<Token?> Autenticar(
        string chave,
        string segredo,
        Guid contextoId,
        CancellationToken cancellationToken
    )
    {
        var body = new
        {
            Chave = chave,
            Segredo = segredo,
            ContextoId = contextoId,
        };

        var json = JsonSerializer.Serialize(body);

        HttpContent content = new StringContent(
            json,
            System.Text.Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync(
                "autenticacao/v1/autenticar-cliente",
                content,
                cancellationToken
            );

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao autenticar: {StatusCode}", response.StatusCode);
            }
            var tokenResponse = await response.Content.ReadFromJsonAsync<Token>(cancellationToken);

            if (tokenResponse is not null)
                tokenResponse = tokenResponse with
                {
                    Expira = tokenResponse.Expira.AddSeconds(-15),
                };

            return tokenResponse;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(ex, "Tempo esgotado ao autenticar");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Erro de rede ao autenticar");
            return null;
        }
    }

    // public async Task<TokenResponse> RefreshTokenAsync(
    //     string tokenAcesso,
    //     string tokenAtualizacao,
    //     Guid contextoId,
    //     CancellationToken cancellationToken
    // )
    // {
    //     var body = new
    //     {
    //         TokenAcesso = tokenAcesso,
    //         TokenAtualizacao = tokenAtualizacao,
    //         ContextoId = contextoId,
    //     };
    //     var json = JsonSerializer.Serialize(body);

    //     HttpContent content = new StringContent(
    //         json,
    //         System.Text.Encoding.UTF8,
    //         "application/json"
    //     );

    //     var response = await _httpClient.PostAsync(
    //         "autenticacao/v1/atualizar-token",
    //         content,
    //         cancellationToken
    //     );

    //     if (!response.IsSuccessStatusCode)
    //     {
    //         throw new HttpRequestException("Falha ao autenticar");
    //     }

    //     var token =
    //         await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken)
    //         ?? throw new InvalidOperationException("Token não encontrado na resposta");

    //     return token;
    // }
}
