namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed record HeartbeatRequest(
    SystemMetrics? System,
    ProcessMetrics? Process,
    NetworkMetrics? Network,
    HardwareMetrics? Hardware,
    IndustrialMetrics? Industrial
)
{
    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;
}
