using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Toolbox.Industrial.Core.Communication.Api;

public interface IInternetMonitor
{
    bool IsOnline { get; }

    DateTimeOffset LastCheck { get; }

    event EventHandler<bool>? StatusChanged;
}

public sealed class InternetMonitor : BackgroundService, IInternetMonitor
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(5) };

    public event EventHandler<bool>? StatusChanged;
    private readonly ILogger<InternetMonitor> _logger;
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

    public InternetMonitor(ILogger<InternetMonitor> logger)
    {
        _logger = logger;
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
        _logger.LogInformation("Internet ONLINE.");

        StatusChanged?.Invoke(this, true);
    }

    private void SetOffline()
    {
        IsOnline = false;
        _logger.LogWarning("Internet OFFLINE.");

        StatusChanged?.Invoke(this, false);
    }

    private static async Task<bool> CheckAsync()
    {
        try
        {
            string? requestUri = null;
            if (ApiClient.BaseAddress != null)
            {
                requestUri = $"{ApiClient.BaseAddress}/health";
            }
            //if (ApiClient.BaseAddress != null)
            //{
            //    requestUri = $"{ApiClient.BaseAddress}/automacao/v1/paineis/{painelIl}/controladores/{controladorId}/sinal-vida";
            //}
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
