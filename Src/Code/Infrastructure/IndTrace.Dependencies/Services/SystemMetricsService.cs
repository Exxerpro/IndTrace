// <copyright file="SystemMetricsService.cs" company="Exxerpro Solutions SA de CV">
// Copyright (c) Exxerpro Solutions SA de CV. Licensed under the GNU Affero General Public License v3.0 or later.
// </copyright>
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Globalization;
using System.Net.NetworkInformation;

namespace IndTrace.Dependencies.Services;

/// <summary>
/// Service for collecting system performance metrics including CPU, memory, and network utilization.
/// </summary>
public class SystemMetricsService
{
    private readonly PerformanceCounter? cpuCounter; // Made nullable to address CS8618
    private readonly PerformanceCounter? ramCounter; // Made nullable to address CS8618

    // Guards every read/write of the mutable sample state below. The service is registered as a
    // singleton and can be queried from concurrent dashboard requests, so the last-sample counters
    // and timestamps must not be torn across threads.
    private readonly object sampleLock = new();

    private long previousBytesSent;
    private long previousBytesReceived;
    private DateTime lastNetworkCheck;

    // Linux CPU is sampled as a delta between two /proc/stat reads, not a since-boot average.
    private long previousCpuIdle;
    private long previousCpuTotal;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemMetricsService"/> class.
    /// </summary>
    public SystemMetricsService()
    {
        if (OperatingSystem.IsWindows())
        {
            this.cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            this.ramCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        this.previousBytesSent = this.GetTotalBytesSent();
        this.previousBytesReceived = this.GetTotalBytesReceived();
        this.lastNetworkCheck = DateTime.UtcNow;

        if (!OperatingSystem.IsWindows())
        {
            var (idle, total) = ReadLinuxCpuTimes();
            this.previousCpuIdle = idle;
            this.previousCpuTotal = total;
        }
    }

    /// <summary>
    /// Gets the current CPU usage percentage.
    /// </summary>
    /// <returns>The CPU usage as a percentage.</returns>
    public float GetCpuUsage()
    {
        return OperatingSystem.IsWindows() ? this.cpuCounter?.NextValue() ?? 0f : this.GetLinuxCpuUsage();
    }

    /// <summary>
    /// Gets the current memory usage.
    /// </summary>
    /// <returns>The memory usage in MB (Windows) or percentage (Linux).</returns>
    public float GetMemoryUsage()
    {
        if (OperatingSystem.IsWindows())
        {
            return this.ramCounter?.NextValue() ?? 0f;
        }
        return this.GetLinuxMemoryUsage();
    }

    /// <summary>
    /// Gets the network utilization in bytes per second for sent and received data.
    /// </summary>
    /// <returns>A tuple containing the bytes sent per second and bytes received per second.</returns>
    public (double sent, double received) GetNetworkUtilization()
    {
        long totalSent = this.GetTotalBytesSent();
        long totalReceived = this.GetTotalBytesReceived();
        DateTime now = DateTime.UtcNow;

        lock (this.sampleLock)
        {
            double seconds = (now - this.lastNetworkCheck).TotalSeconds;

            // Same-tick sampling would divide by zero and push Infinity/NaN to the dashboard.
            double sentPerSec = SystemMetricsCalculations.PerSecondRate(totalSent, this.previousBytesSent, seconds);
            double receivedPerSec = SystemMetricsCalculations.PerSecondRate(totalReceived, this.previousBytesReceived, seconds);

            this.previousBytesSent = totalSent;
            this.previousBytesReceived = totalReceived;
            this.lastNetworkCheck = now;

            return (sentPerSec, receivedPerSec);
        }
    }

    /// <summary>
    /// Reads the aggregate idle and total CPU jiffies from the first line of /proc/stat.
    /// </summary>
    /// <returns>A tuple of (idle jiffies, total jiffies).</returns>
    private static (long Idle, long Total) ReadLinuxCpuTimes()
    {
        string[] cpuInfo = System.IO.File.ReadAllLines("/proc/stat")[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);

        long total = 0;
        for (int i = 1; i < cpuInfo.Length; i++)
        {
            total += long.Parse(cpuInfo[i], CultureInfo.InvariantCulture);
        }

        // Field layout: cpu user nice system idle iowait irq softirq ... => idle is index 4.
        long idle = long.Parse(cpuInfo[4], CultureInfo.InvariantCulture);
        return (idle, total);
    }

    /// <summary>
    /// Gets the CPU usage on Linux systems as the delta since the previous read of /proc/stat,
    /// reflecting recent utilization rather than a since-boot average.
    /// </summary>
    /// <returns>The CPU usage as a percentage.</returns>
    private float GetLinuxCpuUsage()
    {
        var (idle, total) = ReadLinuxCpuTimes();

        lock (this.sampleLock)
        {
            double idleDelta = idle - this.previousCpuIdle;
            double totalDelta = total - this.previousCpuTotal;

            this.previousCpuIdle = idle;
            this.previousCpuTotal = total;

            return SystemMetricsCalculations.CpuUsagePercent(idleDelta, totalDelta);
        }
    }

    /// <summary>
    /// Gets the memory usage on Linux systems by reading from /proc/meminfo.
    /// </summary>
    /// <returns>The memory usage as a percentage.</returns>
    private float GetLinuxMemoryUsage()
    {
        var memInfo = System.IO.File.ReadAllLines("/proc/meminfo");
        float totalMemory = float.Parse(memInfo[0].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
        float availableMemory = float.Parse(memInfo[2].Split(' ', StringSplitOptions.RemoveEmptyEntries)[1], CultureInfo.InvariantCulture);
        return SystemMetricsCalculations.UsedPercent(availableMemory, totalMemory);
    }

    /// <summary>
    /// Gets the total bytes sent across all active network interfaces.
    /// </summary>
    /// <returns>The total number of bytes sent.</returns>
    private long GetTotalBytesSent()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .Sum(n => n.GetIPv4Statistics().BytesSent);
    }

    /// <summary>
    /// Gets the total bytes received across all active network interfaces.
    /// </summary>
    /// <returns>The total number of bytes received.</returns>
    private long GetTotalBytesReceived()
    {
        return NetworkInterface.GetAllNetworkInterfaces()
            .Where(n => n.OperationalStatus == OperationalStatus.Up)
            .Sum(n => n.GetIPv4Statistics().BytesReceived);
    }
}

/// <summary>
/// Pure, side-effect-free calculations behind <see cref="SystemMetricsService"/>. Kept separate so the
/// division-by-zero and percentage guards are unit-testable without OS coupling.
/// </summary>
public static class SystemMetricsCalculations
{
    /// <summary>
    /// Computes a per-second rate from two cumulative counter samples, guarding against a
    /// non-positive elapsed interval (same-tick sampling) that would otherwise yield Infinity/NaN.
    /// </summary>
    /// <param name="current">The current cumulative counter value.</param>
    /// <param name="previous">The previous cumulative counter value.</param>
    /// <param name="elapsedSeconds">Seconds elapsed between the two samples.</param>
    /// <returns>The rate per second, or 0 when <paramref name="elapsedSeconds"/> is not positive.</returns>
    public static double PerSecondRate(long current, long previous, double elapsedSeconds)
    {
        if (elapsedSeconds <= 0 || double.IsNaN(elapsedSeconds))
        {
            return 0d;
        }

        return (current - previous) / elapsedSeconds;
    }

    /// <summary>
    /// Computes CPU usage percentage from idle/total jiffie deltas, guarding against a non-positive
    /// total delta and clamping the result to the 0..100 range.
    /// </summary>
    /// <param name="idleDelta">Idle jiffies elapsed between samples.</param>
    /// <param name="totalDelta">Total jiffies elapsed between samples.</param>
    /// <returns>The CPU usage as a percentage in the range 0..100.</returns>
    public static float CpuUsagePercent(double idleDelta, double totalDelta)
    {
        if (totalDelta <= 0 || double.IsNaN(totalDelta))
        {
            return 0f;
        }

        double usage = 100d * (1d - (idleDelta / totalDelta));
        return (float)Math.Clamp(usage, 0d, 100d);
    }

    /// <summary>
    /// Computes the used percentage from an available and total amount, guarding against a
    /// non-positive total and clamping the result to the 0..100 range.
    /// </summary>
    /// <param name="available">The available amount.</param>
    /// <param name="total">The total amount.</param>
    /// <returns>The used percentage in the range 0..100.</returns>
    public static float UsedPercent(float available, float total)
    {
        if (total <= 0 || float.IsNaN(total))
        {
            return 0f;
        }

        double used = (1d - (available / total)) * 100d;
        return (float)Math.Clamp(used, 0d, 100d);
    }
}
