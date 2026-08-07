using Newtonsoft.Json.Linq;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed record HeartbeatResponse
{
    public DateTime ServerTimeUtc { get; init; }
    public HeartbeatOptions? HeartbeatOptions { get; init; }
    public IReadOnlyList<DeviceCommand> Commands { get; init; } = [];
}

internal sealed record DeviceCommand
{
    public required Guid Id { get; init; }
    public required type Type { get; init; }
    public JObject? Parameters { get; init; }

    internal enum type : int
    {
        ReloadConfig = 1,
        SystemReboot = 2,
        SystemRestart = 3,
        SystemShutdown = 4,
        UpdateFirmware = 5,
        SynchronizeClock = 6,
        PublishDiagnostics = 7,
    }
}
