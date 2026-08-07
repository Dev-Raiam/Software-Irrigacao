using Toolbox.Core.Telemetry;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed class MetricsSnapshot
{
    private ApplicationProcessMetrics? _process;
    private ApplicationStatusMetrics? _status;
    private OperationSystemMetrics? _system;
    private HardwareMetrics? _hardware;
    private NetworkMetrics? _network;

    public void Update(OperationSystemMetrics metrics) => Interlocked.Exchange(ref _system, metrics);

    public void Update(ApplicationProcessMetrics metrics) => Interlocked.Exchange(ref _process, metrics);

    public void Update(HardwareMetrics metrics) => Interlocked.Exchange(ref _hardware, metrics);

    public void Update(NetworkMetrics metrics) => Interlocked.Exchange(ref _network, metrics);

    public void Update(ApplicationStatusMetrics metrics) => Interlocked.Exchange(ref _status, metrics);

    public HeartbeatRequest Take()
    {
        return new HeartbeatRequest(
            Interlocked.Exchange(ref _system, null),
            Interlocked.Exchange(ref _process, null),
            Interlocked.Exchange(ref _network, null),
            Interlocked.Exchange(ref _hardware, null),
            Interlocked.Exchange(ref _status, null)
        );
    }
}
