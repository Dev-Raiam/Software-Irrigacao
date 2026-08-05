using Microsoft.Extensions.Hosting;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal interface ISystemMetricsCollector
{
    MetricsSnapshot Current { get; }
}

internal sealed class SystemMetricsCollector : BackgroundService, ISystemMetricsCollector
{
    private static readonly MetricsSnapshot _current = new();
    private readonly IMetricsProvider _provider;

    public MetricsSnapshot Current => _current;

    public SystemMetricsCollector(IMetricsProvider provider)
    {
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.WhenAll(
            RunSystemLoop(stoppingToken),
            RunProcessLoop(stoppingToken),
            RunHardwareLoop(stoppingToken),
            RunNetworkLoop(stoppingToken)
        );
    }

    private async Task RunSystemLoop(CancellationToken token)
    {
        using var timer = new PeriodicTimer(Heartbeat.Options.IntervalSystemMetrics);

        do
        {
            var metrics = await _provider.GetSystemAsync(token);
            Current.Update(metrics);
            if (timer.Period.CompareTo(Heartbeat.Options.IntervalSystemMetrics) != 0)
            {
                timer.Period = Heartbeat.Options.IntervalSystemMetrics;
            }
        } while (await timer.WaitForNextTickAsync(token));
    }

    private async Task RunProcessLoop(CancellationToken token)
    {
        using var timer = new PeriodicTimer(Heartbeat.Options.IntervalProcessMetrics);

        do
        {
            var metrics = await _provider.GetProcessAsync(token);
            Current.Update(metrics);
            if (timer.Period.CompareTo(Heartbeat.Options.IntervalProcessMetrics) != 0)
            {
                timer.Period = Heartbeat.Options.IntervalProcessMetrics;
            }
        } while (await timer.WaitForNextTickAsync(token));
    }

    private async Task RunHardwareLoop(CancellationToken token)
    {
        using var timer = new PeriodicTimer(Heartbeat.Options.IntervalHardwareMetrics);

        do
        {
            var metrics = await _provider.GetHardwareAsync(token);
            Current.Update(metrics);
            if (timer.Period.CompareTo(Heartbeat.Options.IntervalHardwareMetrics) != 0)
            {
                timer.Period = Heartbeat.Options.IntervalHardwareMetrics;
            }
        } while (await timer.WaitForNextTickAsync(token));
    }

    private async Task RunNetworkLoop(CancellationToken token)
    {
        using var timer = new PeriodicTimer(Heartbeat.Options.IntervalNetworkMetrics);

        do
        {
            var metrics = await _provider.GetNetworkAsync(token);
            Current.Update(metrics);
            if (timer.Period.CompareTo(Heartbeat.Options.IntervalNetworkMetrics) != 0)
            {
                timer.Period = Heartbeat.Options.IntervalNetworkMetrics;
            }
        } while (await timer.WaitForNextTickAsync(token));
    }
}
