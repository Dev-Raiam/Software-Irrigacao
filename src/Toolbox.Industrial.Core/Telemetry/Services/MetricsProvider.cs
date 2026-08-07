using Toolbox.Core.Telemetry;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal interface IMetricsProvider
{
    ValueTask<OperationSystemMetrics> GetSystemAsync(CancellationToken cancellationToken);

    ValueTask<ApplicationProcessMetrics> GetProcessAsync(CancellationToken cancellationToken);

    ValueTask<HardwareMetrics> GetHardwareAsync(CancellationToken cancellationToken);

    ValueTask<NetworkMetrics> GetNetworkAsync(CancellationToken cancellationToken);
}
