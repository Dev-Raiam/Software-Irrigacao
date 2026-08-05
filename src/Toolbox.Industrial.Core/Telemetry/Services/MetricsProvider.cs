namespace Toolbox.Industrial.Core.Telemetry.Services;

internal interface IMetricsProvider
{
    ValueTask<SystemMetrics> GetSystemAsync(CancellationToken cancellationToken);

    ValueTask<ProcessMetrics> GetProcessAsync(CancellationToken cancellationToken);

    ValueTask<HardwareMetrics> GetHardwareAsync(CancellationToken cancellationToken);

    ValueTask<NetworkMetrics> GetNetworkAsync(CancellationToken cancellationToken);
}
