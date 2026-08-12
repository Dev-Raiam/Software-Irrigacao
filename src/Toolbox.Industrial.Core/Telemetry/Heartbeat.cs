using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Toolbox.Industrial.Core.Communication.Api;
using Toolbox.Industrial.Core.Communication.Mqtt;
using Toolbox.Industrial.Core.Data;
using Toolbox.Industrial.Core.Telemetry.Services;

namespace Toolbox.Industrial.Core.Telemetry;

internal sealed class Heartbeat : BackgroundService
{
    private readonly JsonSerializerSettings _mqttSerializer;
    public static HeartbeatOptions Options = new();
    public event EventHandler<bool>? StatusChanged;
    private readonly ILogger<Heartbeat> _logger;
    private readonly IHeartbeatClient _client;
    private readonly IMqtt? _mqtt;
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

    public Heartbeat(
        [FromKeyedServices(Mqtt.Local)] IMqtt? mqtt,
        IHeartbeatClient client,
        ILogger<Heartbeat> logger
    )
    {
        _client = client;
        _logger = logger;
        _mqtt = mqtt;
        _mqttSerializer = JsonConvert.DefaultSettings!.Invoke();
        _mqttSerializer.TypeNameHandling = TypeNameHandling.Objects;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_mqtt != null && !Controlador.Master && Controlador.ControladorId != Guid.Empty)
        {
            await Task.WhenAll(
                RunSlaveHeartbeatLoop(stoppingToken),
                RunInternetHeartbeatLoop(stoppingToken)
            );

            return;
        }
        await RunInternetHeartbeatLoop(stoppingToken);
    }

    private async Task RunInternetHeartbeatLoop(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Options.IntervalHeartbeat);

        do
        {
            try
            {
                var result = await CheckAsync(timer, stoppingToken);
                LastCheck = DateTimeOffset.UtcNow;
                if (result)
                {
                    _failureCount = 0;
                    _successCount++;
                    if (!IsOnline && _successCount >= SuccessThreshold)
                    {
                        IsOnline = true;
                        _logger.LogInformation("Controlador ONLINE.");
                        StatusChanged?.Invoke(this, true);
                    }
                }
                else
                {
                    _successCount = 0;
                    _failureCount++;
                    if (IsOnline && _failureCount >= FailureThreshold)
                    {
                        IsOnline = false;
                        _logger.LogWarning("Controlador OFFLINE.");
                        StatusChanged?.Invoke(this, false);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao enviar o Heartbeat.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunSlaveHeartbeatLoop(CancellationToken stoppingToken)
    {
        if (_mqtt == null)
        {
            return;
        }

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

        do
        {
            try
            {
                //Slave deve enviar um pulso ao Master
                //informando que está em operação.
                if (_mqtt.IsConnected)
                {
                    var heartbeat = JsonConvert.SerializeObject(
                        new SlaveHeartbeat { ControladorId = Controlador.ControladorId },
                        _mqttSerializer
                    );
                    await _mqtt.PublishAsync("heartbeats", heartbeat);
                }
            }
            catch { }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task<bool> CheckAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            string? requestUri = null;
            if (ApiClient.BaseAddress != null)
            {
                requestUri = $"{ApiClient.BaseAddress}/health";
                if (Controlador.PainelId != Guid.Empty && Controlador.ControladorId != Guid.Empty)
                {
                    using var result = await _client.SendAsync(stoppingToken);
                    ApiClient.Online = result.IsSuccessStatusCode;
                    if (timer.Period.CompareTo(Options.IntervalHeartbeat) != 0)
                    {
                        timer.Period = Options.IntervalHeartbeat;
                    }
                    return result.IsSuccessStatusCode;
                }
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Head,
                requestUri ?? "https://clients3.google.com/generate_204"
            );

            using var response = await _client.SendAsync(request, stoppingToken);

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

internal sealed record HeartbeatOptions
{
    public TimeSpan IntervalHeartbeat { get; init; } = TimeSpan.FromSeconds(10);
    public TimeSpan IntervalSystemMetrics { get; init; } = TimeSpan.FromMinutes(2);
    public TimeSpan IntervalProcessMetrics { get; init; } = TimeSpan.FromMinutes(3);
    public TimeSpan IntervalNetworkMetrics { get; init; } = TimeSpan.FromSeconds(10);
    public TimeSpan IntervalHardwareMetrics { get; init; } = TimeSpan.FromSeconds(30);
    public TimeSpan IntervalHealthCheckMetrics { get; init; } = TimeSpan.FromMinutes(1);
}

internal sealed record SlaveHeartbeat
{
    public required Guid ControladorId { get; init; }
}
