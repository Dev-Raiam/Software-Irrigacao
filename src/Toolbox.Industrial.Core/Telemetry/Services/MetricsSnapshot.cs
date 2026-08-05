namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed class MetricsSnapshot
{
    private SystemMetrics? _system;
    private ProcessMetrics? _process;
    private HardwareMetrics? _hardware;
    private NetworkMetrics? _network;
    private IndustrialMetrics? _industrial;

    public void Update(SystemMetrics metrics) => Interlocked.Exchange(ref _system, metrics);

    public void Update(ProcessMetrics metrics) => Interlocked.Exchange(ref _process, metrics);

    public void Update(HardwareMetrics metrics) => Interlocked.Exchange(ref _hardware, metrics);

    public void Update(NetworkMetrics metrics) => Interlocked.Exchange(ref _network, metrics);

    public void Update(IndustrialMetrics metrics) => Interlocked.Exchange(ref _industrial, metrics);

    public HeartbeatRequest Take()
    {
        return new HeartbeatRequest(
            Interlocked.Exchange(ref _system, null),
            Interlocked.Exchange(ref _process, null),
            Interlocked.Exchange(ref _network, null),
            Interlocked.Exchange(ref _hardware, null),
            Interlocked.Exchange(ref _industrial, null)
        );
    }
}
