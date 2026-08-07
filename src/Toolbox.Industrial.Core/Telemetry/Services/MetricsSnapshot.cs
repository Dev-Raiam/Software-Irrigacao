using Toolbox.Core.Telemetry;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed class MetricsSnapshot
{
    private static readonly HeartbeatRequest _lastTake = new();
    private ApplicationProcessMetrics? _process;
    private OperationSystemMetrics? _system;
    private HealthCheckMetrics? _status;
    private HardwareMetrics? _hardware;
    private NetworkMetrics? _network;

    public void Update(OperationSystemMetrics metrics) => Interlocked.Exchange(ref _system, metrics);

    public void Update(ApplicationProcessMetrics metrics) => Interlocked.Exchange(ref _process, metrics);

    public void Update(HardwareMetrics metrics) => Interlocked.Exchange(ref _hardware, metrics);

    public void Update(NetworkMetrics metrics) => Interlocked.Exchange(ref _network, metrics);

    public void Update(HealthCheckMetrics metrics) => Interlocked.Exchange(ref _status, metrics);

    public HeartbeatRequest Take()
    {
        var result = new HeartbeatRequest
        {
            System = Interlocked.Exchange(ref _system, null),
            Process = Interlocked.Exchange(ref _process, null),
            Network = Interlocked.Exchange(ref _network, null),
            Hardware = Interlocked.Exchange(ref _hardware, null),
            Status = Interlocked.Exchange(ref _status, null)
        };

        if (result.System != null)
        {
            if (result.System.Equals(_lastTake.System))
                result.System = null;
            else
                _lastTake.System = result.System;
        }
        if (result.Process != null)
        {
            if (result.Process.Equals(_lastTake.Process))
                result.Process = null;
            else
                _lastTake.Process = result.Process; 
        }
        if (result.Network != null)
        {
            if (result.Network.Equals(_lastTake.Network))
                result.Network = null;
            else
                _lastTake.Network = result.Network;
        }
        if (result.Hardware != null)
        {
            if (result.Hardware.Equals(_lastTake.Hardware))
                result.Hardware = null;
            else
                _lastTake.Hardware = result.Hardware;
        }
        if (result.Status != null)
        {
            if (result.Status.Equals(_lastTake.Status))
                result.Status = null;
            else
                _lastTake.Status = result.Status;
        }
        return result;
    }
}
