using System.Diagnostics;
using Toolbox.Core.Telemetry;
using Toolbox.Industrial.Core.Communication.Api;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal class WindowsMetricsProvider : IMetricsProvider
{
    public async ValueTask<HardwareMetrics> GetHardwareAsync(CancellationToken cancellationToken)
    {
        var result = new HardwareMetrics
        {
            TimestampUtc = DateTime.UtcNow,
            CpuFrequency = Random.Shared.Next(1000, 4000),
            CpuTemperature = Random.Shared.Next(30, 90),
        };
        return await new ValueTask<HardwareMetrics>(result);
    }

    public async ValueTask<NetworkMetrics> GetNetworkAsync(CancellationToken cancellationToken)
    {
        var result = new NetworkMetrics
        {
            TimestampUtc = DateTime.UtcNow,
            InternetAvailable = ApiClient.IsOnline,
        };
        return await new ValueTask<NetworkMetrics>(result);
    }

    public async ValueTask<ApplicationProcessMetrics> GetProcessAsync(
        CancellationToken cancellationToken
    )
    {
        var process = Process.GetCurrentProcess();
        var runningTime = DateTime.UtcNow - process.StartTime.ToUniversalTime();
        var result = new ApplicationProcessMetrics
        {
            TimestampUtc = DateTime.UtcNow,
            ThreadCount = process.Threads.Count,
            CpuUsage =
                process.TotalProcessorTime.TotalMilliseconds / runningTime.TotalMilliseconds * 100,
            RunningTime = runningTime,
            WorkingSet = process.WorkingSet64,
            PrivateMemory = process.PrivateMemorySize64,
        };
        return await new ValueTask<ApplicationProcessMetrics>(result);
    }

    public async ValueTask<OperationSystemMetrics> GetSystemAsync(
        CancellationToken cancellationToken
    )
    {
        var result = new OperationSystemMetrics
        {
            TimestampUtc = DateTime.UtcNow,
            DiskUsage = Random.Shared.Next(0, 100),
            TotalDisk = 500_000_000_000, // 500 GB
            FreeDisk = 200_000_000_000, // 200 GB
            MemoryUsage = Random.Shared.Next(0, 100),
            TotalMemory = 16_000_000_000, // 16 GB
            AvailableMemory = 8_000_000_000, // 8 GB
            Uptime = TimeSpan.FromHours(Environment.TickCount),
            CpuUsage = Random.Shared.Next(0, 100),
        };
        return await new ValueTask<OperationSystemMetrics>(result);
    }
}
