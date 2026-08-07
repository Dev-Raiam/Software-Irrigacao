using Toolbox.Core.Telemetry;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal class WindowsMetricsProvider : IMetricsProvider
{
    public ValueTask<HardwareMetrics> GetHardwareAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<HardwareMetrics>(new HardwareMetrics());
    }

    public ValueTask<NetworkMetrics> GetNetworkAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<NetworkMetrics>(new NetworkMetrics());
    }

    public ValueTask<ApplicationProcessMetrics> GetProcessAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<ApplicationProcessMetrics>(new ApplicationProcessMetrics());
    }

    public ValueTask<OperationSystemMetrics> GetSystemAsync(CancellationToken cancellationToken)
    {
        return new ValueTask<OperationSystemMetrics>(new OperationSystemMetrics());
    }
}
