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
        var script = """
            LC_ALL=C
            echo "cpu=$(top -bn1 | grep 'Cpu(s)' | awk '{print 100-$8}')"
            free -m | awk '/^Mem:/{print "mem_usage=" $2-$7; print "mem_total=" $2; print "mem_available=" $7}'
            df -B1M / | awk 'NR==2{print "disk_usage=" $2-$4; print "disk_total=" $2; print "disk_free=" $4}'
            echo "uptime=$(awk '{print $1}' /proc/uptime)"
            """;
        if (IsFirstGetSystem)
        {
            script += """
                echo "os=$(. /etc/os-release; echo "$PRETTY_NAME")"
                echo "arch=$(uname -m)"
                """;
        }
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
            ps -o nlwp=,etimes=,pcpu= -p PID_PLACEHOLDER | awk '{print "threads=" $1; print "running_seconds=" $2; print "cpu=" $3}'
            awk -F':[ \t]*' 'BEGIN{w=0;p=0;hasP=0} /^VmRSS:/{w=int($2/1024)} /^RssAnon:/{p=int($2/1024);hasP=1} END{print "working_set=" w; print "private_memory=" (hasP ? p : w)}' /proc/PID_PLACEHOLDER/status
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
                        ["Version"] =
                            Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0",
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
            echo "cpu_temp=$(awk '{printf "%.1f\n", $1/1000}' /sys/class/thermal/thermal_zone0/temp 2>/dev/null || echo 0)"
            echo "cpu_freq=$(awk '{printf "%.0f\n", $1/1000}' /sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq 2>/dev/null || awk -F': ' '/cpu MHz/{printf "%.0f\n", $2; exit}' /proc/cpuinfo 2>/dev/null || echo 0)"
            """;

        var values = ParseKeyValues(await RunScriptAsync(script, cancellationToken));
        try
        {
            return new HardwareMetrics
            {
                TimestampUtc = DateTime.UtcNow,
                CpuFrequency = ParseDouble(values, "cpu_freq"),
                CpuTemperature = ParseDouble(values, "cpu_temp")
            };
        }
        finally
        {
            IsFirstGetHardware = false;
        }
    }

    public async ValueTask<NetworkMetrics> GetNetworkAsync(CancellationToken cancellationToken)
    {
        var script = """
            LC_ALL=C
            ping -c1 -W2 8.8.8.8 >/dev/null 2>&1 && echo "internet=true" || echo "internet=false"
            """;
        if (IsFirstGetNetwork) 
        {
            script += """
                echo "hostname=$(hostname)"
                echo "ip=$(ip -o -4 addr show scope global 2>/dev/null | awk '{split($4,a,"/"); print a[1]; exit}')"
                """;
        }

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
