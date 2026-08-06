using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Toolbox.Industrial.Core.Telemetry.Services;

internal class LinuxMetricsProvider : IMetricsProvider
{
    public async ValueTask<SystemMetrics> GetSystemAsync(CancellationToken cancellationToken)
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

        return new SystemMetrics(
            ParseDouble(values, "cpu"),
            ParseDouble(values, "mem_usage"),
            ParseLong(values, "mem_total"),
            ParseLong(values, "mem_available"),
            ParseDouble(values, "disk_usage"),
            ParseLong(values, "disk_total"),
            ParseLong(values, "disk_free"),
            TimeSpan.FromSeconds(ParseDouble(values, "uptime")),
            values.GetValueOrDefault("os", RuntimeInformation.OSDescription),
            values.GetValueOrDefault("arch", RuntimeInformation.OSArchitecture.ToString())
        );
    }

    public async ValueTask<ProcessMetrics> GetProcessAsync(CancellationToken cancellationToken)
    {
        var script = """
            LC_ALL=C
            ps -o rss=,vsz=,nlwp=,etimes=,pcpu= -p PID_PLACEHOLDER | awk '{print "working_set=" $1*1024; print "private_memory=" $2*1024; print "threads=" $3; print "running_seconds=" $4; print "cpu=" $5}'
            """.Replace(
            "PID_PLACEHOLDER",
            Environment.ProcessId.ToString(CultureInfo.InvariantCulture)
        );

        var values = ParseKeyValues(await RunScriptAsync(script, cancellationToken));

        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString() ?? "0.0.0.0";

        return new ProcessMetrics(
            version,
            RuntimeInformation.FrameworkDescription,
            ParseLong(values, "working_set"),
            ParseLong(values, "private_memory"),
            (int)ParseLong(values, "threads"),
            ParseDouble(values, "cpu"),
            TimeSpan.FromSeconds(ParseLong(values, "running_seconds"))
        );
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

        return new HardwareMetrics(
            ParseDouble(values, "cpu_temp"),
            ParseDouble(values, "board_temp"),
            ParseDouble(values, "voltage"),
            ParseDouble(values, "cpu_freq")
        );
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

        return new NetworkMetrics(
            values.GetValueOrDefault("hostname", string.Empty),
            values.GetValueOrDefault("ip", string.Empty),
            values.GetValueOrDefault("mac", string.Empty),
            values.GetValueOrDefault("interface", string.Empty),
            ParseLong(values, "bytes_sent"),
            ParseLong(values, "bytes_received"),
            values.GetValueOrDefault("internet") == "true"
        );
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
