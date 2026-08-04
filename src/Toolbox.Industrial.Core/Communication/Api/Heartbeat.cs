using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Mime;
using Toolbox.Industrial.Core.Data;

namespace Toolbox.Industrial.Core.Communication.Api;

public interface IHeartbeat
{
    bool IsOnline { get; }

    DateTimeOffset LastCheck { get; }

    event EventHandler<bool>? StatusChanged;
}

public sealed class Heartbeat : BackgroundService, IHeartbeat
{
    internal sealed record HeartbeatInfo(Dictionary<string, object>? Metrics);

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(5) };
    public event EventHandler<bool>? StatusChanged;
    private readonly ILogger<Heartbeat> _logger;
    private readonly IApiClient _apiClient;
    private int _successCount;
    private int _failureCount;
    public bool IsOnline { get; private set; }
    public DateTimeOffset LastCheck { get; private set; }

    /// <summary>
    /// Quantidade de sucessos consecutivos para considerar online.
    /// </summary>
    public int SuccessThreshold { get; init; } = 2;

    /// <summary>
    /// Quantidade de falhas consecutivas para considerar offline.
    /// </summary>
    public int FailureThreshold { get; init; } = 3;

    public Heartbeat(ILogger<Heartbeat> logger, IApiClient apiClient)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await CheckAsync();
                LastCheck = DateTimeOffset.UtcNow;
                if (result)
                {
                    _failureCount = 0;
                    _successCount++;

                    if (!IsOnline && _successCount >= SuccessThreshold)
                    {
                        SetOnline();
                    }
                }
                else
                {
                    _successCount = 0;
                    _failureCount++;

                    if (IsOnline && _failureCount >= FailureThreshold)
                    {
                        SetOffline();
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private void SetOnline()
    {
        IsOnline = true;
        _logger.LogInformation("Controlador ONLINE.");

        StatusChanged?.Invoke(this, true);
    }

    private void SetOffline()
    {
        IsOnline = false;
        _logger.LogWarning("Controlador OFFLINE.");

        StatusChanged?.Invoke(this, false);
    }

    private async Task<bool> CheckAsync()
    {
        try
        {
            string? requestUri = null;
            if (ApiClient.BaseAddress != null)
            {
                requestUri = $"{ApiClient.BaseAddress}/health";
                if (Controlador.PainelId != Guid.Empty && Controlador.ControladorId != Guid.Empty)
                {
                    using var heartbeat = new HttpRequestMessage(
                        HttpMethod.Post,
                        $"automacao/v1/paineis/{Controlador.PainelId}/controladores/{Controlador.ControladorId}/sinal-vida"
                    );

                    heartbeat.Content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(new HeartbeatInfo(
                            Metrics: new Dictionary<string, object>
                            {
                                { "timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
                                { "controladorId", Controlador.ControladorId },
                                { "painelId", Controlador.PainelId }
                            })
                        ),
                        System.Text.Encoding.UTF8,
                        MediaTypeNames.Application.Json
                    );
                    var result = await _apiClient.SendAsync<object?>(
                        heartbeat,
                        default
                    );
                    ApiClient.Online = result.Success;
                    return result.Success;
                }
            }
            using var request = new HttpRequestMessage(
                HttpMethod.Head,
                requestUri ?? "https://clients3.google.com/generate_204"
            );

            using var response = await Http.SendAsync(request);

            ApiClient.Online = response.IsSuccessStatusCode;
            return response.IsSuccessStatusCode;
        }
        catch
        {
            ApiClient.Online = false;
            return false;
        }
    }
}
