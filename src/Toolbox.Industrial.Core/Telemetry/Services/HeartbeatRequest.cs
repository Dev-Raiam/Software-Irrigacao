using Toolbox.Core.Telemetry;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed record HeartbeatRequest
{
    public OperationSystemMetrics? System { get; set; }
    public ApplicationProcessMetrics? Process { get; set; }
    public NetworkMetrics? Network { get; set; }
    public HardwareMetrics? Hardware { get; set; }
    public ApplicationStatusMetrics? Status { get; set; }
}
