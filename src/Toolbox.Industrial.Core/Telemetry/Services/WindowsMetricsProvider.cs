namespace Toolbox.Industrial.Core.Telemetry.Services;

internal class WindowsMetricsProvider : IMetricsProvider
{
    public ValueTask<HardwareMetrics> GetHardwareAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<HardwareMetrics>();
    }

    public ValueTask<NetworkMetrics> GetNetworkAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<NetworkMetrics>();
    }

    public ValueTask<ProcessMetrics> GetProcessAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<ProcessMetrics>();
    }

    public ValueTask<SystemMetrics> GetSystemAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<SystemMetrics>();
    }
}
