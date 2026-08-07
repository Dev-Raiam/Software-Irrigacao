using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Toolbox.Core.Telemetry;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal class LinuxMetricsProvider : IMetricsProvider
{
    public static bool IsFirstGetHardware = true;
    public static bool IsFirstGetProcess = true;
    public static bool IsFirstGetNetwork = true;
    public static bool IsFirstGetSystem = true;

    public async ValueTask<OperationSystemMetrics> GetSystemAsync(
        CancellationToken cancellationToken
    )
    {
        const string script = """
            LC_ALL=C
            echo "cpu=$(top -bn1 | grep 'Cpu(s)' | awk '{print 100-$8}')"
            free -b | awk '/^Mem:/{print "mem_usage=" ($2-$7)/$2*100; print "mem_total=" $2; print "mem_available=" $7}'
            df -B1 / | awk 'NR==2{print "disk_usage=" ($2-$4)/$2*100; print "disk_total=" $2; print "disk_free=" $4}'
            echo "uptime=$(awk '{print $1}' /proc/uptime)"
            echo "os=$(. /etc/os-release; echo "$PRETTY_NAME")"
            echo "arch=$(uname -m)"
            """;

        var values = ParseKeyValues(await RunScriptAsync(script, cancellationToken));
        try
        {
            return new OperationSystemMetrics
            {
                TimestampUtc = DateTime.UtcNow,
                Uptime = TimeSpan.FromSeconds(ParseDouble(values, "uptime")),
                CpuUsage = ParseDouble(values, "cpu"),
                MemoryUsage = ParseDouble(values, "mem_usage"),
                TotalMemory = ParseLong(values, "mem_total"),
                AvailableMemory = ParseLong(values, "mem_available"),
                DiskUsage = ParseDouble(values, "disk_usage"),
                TotalDisk = ParseLong(values, "disk_total"),
                FreeDisk = ParseLong(values, "disk_free"),
                AdditionalProperties = IsFirstGetSystem
                    ? new Dictionary<string, object>
                    {
                        ["operatingSystem"] = values.GetValueOrDefault(
                            "os",
                            RuntimeInformation.OSDescription
                        ),
                        ["architecture"] = values.GetValueOrDefault(
                            "arch",
                            RuntimeInformation.OSArchitecture.ToString()
                        ),
                    }
                    : null,
            };
        }
        finally
        {
            IsFirstGetSystem = false;
        }
    }

    public async ValueTask<ApplicationProcessMetrics> GetProcessAsync(
        CancellationToken cancellationToken
    )
    {
        var script = """
            LC_ALL=C
            ps -o rss=,vsz=,nlwp=,etimes=,pcpu= -p PID_PLACEHOLDER | awk '{print "working_set=" $1*1024; print "private_memory=" $2*1024; print "threads=" $3; print "running_seconds=" $4; print "cpu=" $5}'
            """.Replace(
            "PID_PLACEHOLDER",
            Environment.ProcessId.ToString(CultureInfo.InvariantCulture)
        );

        var values = ParseKeyValues(await RunScriptAsync(script, cancellationToken));

        try
        {
            return new ApplicationProcessMetrics
            {
                TimestampUtc = DateTime.UtcNow,
                CpuUsage = ParseDouble(values, "cpu"),
                WorkingSet = ParseLong(values, "working_set"),
                ThreadCount = (int)ParseLong(values, "threads"),
                RunningTime = TimeSpan.FromSeconds(ParseLong(values, "running_seconds")),
                PrivateMemory = ParseLong(values, "private_memory"),
                AdditionalProperties = IsFirstGetProcess
                    ? new Dictionary<string, object>
                    {
                        ["Version"] = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0",
                        ["Runtime"] = RuntimeInformation.FrameworkDescription,
                    }
                    : null,
            };
        }
        finally
        {
            IsFirstGetProcess = false;
        }
    }

    public async ValueTask<HardwareMetrics> GetHardwareAsync(CancellationToken cancellationToken)
    {
        const string script = """
            LC_ALL=C
            echo "cpu_temp=$(awk '{print $1/1000}' /sys/class/thermal/thermal_zone0/temp 2>/dev/null || echo 0)"
            echo "board_temp=$(awk '{print $1/1000}' /sys/class/thermal/thermal_zone1/temp 2>/dev/null || echo 0)"
            echo "voltage=$(awk 'NF{print $1/1000; exit}' /sys/class/hwmon/hwmon*/in*_input 2>/dev/null)"
            echo "cpu_freq=$(awk '{print $1/1000}' /sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq 2>/dev/null || awk -F': ' '/cpu MHz/{print $2; exit}' /proc/cpuinfo 2>/dev/null || echo 0)"
            """;

        var values = ParseKeyValues(await RunScriptAsync(script, cancellationToken));
        try
        {
            return new HardwareMetrics
            {
                TimestampUtc = DateTime.UtcNow,
                CpuFrequency = ParseDouble(values, "cpu_freq"),
                CpuTemperature = ParseDouble(values, "cpu_temp"),
                AdditionalProperties = IsFirstGetHardware
                    ? new Dictionary<string, object>
                    {
                        ["boardTemperature"] = ParseDouble(values, "board_temp"),
                        ["voltage"] = ParseDouble(values, "voltage"),
                    }
                    : null,
            };
        }
        finally
        {
            IsFirstGetHardware = false;
        }
    }

    public async ValueTask<NetworkMetrics> GetNetworkAsync(CancellationToken cancellationToken)
    {
        const string script = """
            LC_ALL=C
            echo "hostname=$(hostname)"
            echo "ip=$(ip -o -4 addr show scope global 2>/dev/null | awk '{split($4,a,"/"); print a[1]; exit}')"
            iface=$(ip route get 8.8.8.8 2>/dev/null | awk '{for(i=1;i<=NF;i++) if($i=="dev"){print $(i+1); exit}}')
            [ -z "$iface" ] && iface=$(ip -o link show up 2>/dev/null | awk -F': ' '$2!="lo"{print $2; exit}')
            echo "interface=$iface"
            echo "mac=$(cat /sys/class/net/$iface/address 2>/dev/null)"
            awk -F'[: ]+' '/:/{if($2!="lo"){rx+=$3; tx+=$11}} END{print "bytes_received="rx; print "bytes_sent="tx}' /proc/net/dev
            ping -c1 -W2 8.8.8.8 >/dev/null 2>&1 && echo "internet=true" || echo "internet=false"
            """;

        var values = ParseKeyValues(await RunScriptAsync(script, cancellationToken));
        try
        {
            return new NetworkMetrics
            {
                TimestampUtc = DateTime.UtcNow,
                //BytesSent = ParseLong(values, "bytes_sent"),
                //BytesReceived = ParseLong(values, "bytes_received"),
                InternetAvailable = values.GetValueOrDefault("internet") == "true",
                AdditionalProperties = IsFirstGetNetwork
                    ? new Dictionary<string, object>
                    {
                        ["hostname"] = values.GetValueOrDefault("hostname", string.Empty),
                        ["ipAddress"] = values.GetValueOrDefault("ip", string.Empty),
                        //["macAddress"] = values.GetValueOrDefault("mac", string.Empty),
                        //["interface"] = values.GetValueOrDefault("interface", string.Empty),
                    }
                    : null,
            };
        }
        finally
        {
            IsFirstGetNetwork = false;
        }
    }

    private static async Task<string> RunScriptAsync(
        string script,
        CancellationToken cancellationToken
    )
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        process.StartInfo.ArgumentList.Add("-c");
        process.StartInfo.ArgumentList.Add(script);

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return output;
    }

    private static Dictionary<string, string> ParseKeyValues(string output)
    {
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (
            var line in output.Split(
                '\n',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            )
        )
        {
            var separator = line.IndexOf('=');
            if (separator <= 0)
            {
                continue;
            }

            values[line[..separator]] = line[(separator + 1)..];
        }

        return values;
    }

    private static double ParseDouble(Dictionary<string, string> values, string key)
    {
        return
            values.TryGetValue(key, out var raw)
            && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0.0;
    }

    private static long ParseLong(Dictionary<string, string> values, string key)
    {
        return values.TryGetValue(key, out var raw) && long.TryParse(raw, out var value)
            ? value
            : 0;
    }
}
