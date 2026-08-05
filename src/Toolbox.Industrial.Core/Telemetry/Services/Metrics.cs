namespace Toolbox.Industrial.Core.Telemetry.Services;

internal sealed record SystemMetrics(
    double CpuUsage,
    double MemoryUsage,
    long TotalMemory,
    long AvailableMemory,
    double DiskUsage,
    long TotalDisk,
    long FreeDisk,
    TimeSpan Uptime,
    string OperatingSystem,
    string Architecture
);

internal sealed record ProcessMetrics(
    string Version,
    string Runtime,
    long WorkingSet,
    long PrivateMemory,
    int ThreadCount,
    double CpuUsage,
    TimeSpan RunningTime
);

internal sealed record NetworkMetrics(
    string HostName,
    string IpAddress,
    string MacAddress,
    string @Interface,
    long BytesSent,
    long BytesReceived,
    bool InternetAvailable
);

internal sealed record HardwareMetrics(
    double CpuTemperature,
    double BoardTemperature,
    double Voltage,
    double CpuFrequency
);

internal sealed record IndustrialMetrics(
    bool MqttConnected,
    bool DatabaseConnected,
    int ConnectedDevices,
    int AlarmCount,
    int QueueLength,
    DateTime? LastSynchronizationUtc
);
