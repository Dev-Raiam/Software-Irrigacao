using Toolbox.Core.Telemetry;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed record HeartbeatRequest(
    OperationSystemMetrics? System,
    ApplicationProcessMetrics? Process,
    NetworkMetrics? Network,
    HardwareMetrics? Hardware,
    ApplicationStatusMetrics? Status
);
