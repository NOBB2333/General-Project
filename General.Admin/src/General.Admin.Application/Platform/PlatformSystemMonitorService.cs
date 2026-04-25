using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Platform;

public class PlatformSystemMonitorService : ITransientDependency
{
    private readonly IHostEnvironment _hostEnvironment;
    private const int CpuSampleDurationMilliseconds = 400;

    public PlatformSystemMonitorService(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public async Task<PlatformSystemMonitorDto> GetAsync()
    {
        using var currentProcess = Process.GetCurrentProcess();
        var now = DateTime.Now;
        var startTime = currentProcess.StartTime;
        var totalMemoryBytes = GetTotalMemoryBytes();
        var availableMemoryBytes = GetAvailableMemoryBytes();
        var workingSetBytes = currentProcess.WorkingSet64;
        var systemUsedMemoryBytes = GetSystemUsedMemoryBytes(totalMemoryBytes, availableMemoryBytes)
            ?? Math.Max(0, totalMemoryBytes - Math.Max(0, availableMemoryBytes));
        var cpuSnapshot = await CaptureCpuUsageAsync(currentProcess);

        var result = new PlatformSystemMonitorDto
        {
            AvailableMemoryBytes = availableMemoryBytes,
            CpuUsageNote = OperatingSystem.IsMacOS()
                ? "当前系统 CPU 使用率按 macOS top 口径展示。"
                : null,
            CpuTimeSeconds = currentProcess.TotalProcessorTime.TotalSeconds,
            Disks = GetDiskInfos(),
            EnvironmentName = _hostEnvironment.EnvironmentName,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            HandleCount = GetHandleCountSafely(currentProcess),
            MachineName = Environment.MachineName,
            ManagedMemoryBytes = GC.GetTotalMemory(forceFullCollection: false),
            MemoryUsageNote = OperatingSystem.IsMacOS()
                ? "当前系统内存使用按 macOS PhysMem 口径展示，已与页面显示的“已用/总量”保持一致。"
                : null,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            OsDescription = RuntimeInformation.OSDescription,
            ProcessorCount = Environment.ProcessorCount,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessCpuUsagePercent = cpuSnapshot.ProcessCpuUsagePercent,
            ProcessMemoryDisplayDenominatorBytes = totalMemoryBytes,
            ProcessStartTime = startTime,
            ProcessMemoryUsagePercent = totalMemoryBytes > 0 ? workingSetBytes * 100d / totalMemoryBytes : null,
            SystemCpuUsagePercent = cpuSnapshot.SystemCpuUsagePercent,
            SystemMemoryUsagePercent = GetSystemMemoryUsagePercent(totalMemoryBytes, systemUsedMemoryBytes),
            SystemUsedMemoryBytes = systemUsedMemoryBytes,
            TotalMemoryBytes = totalMemoryBytes,
            ThreadCount = currentProcess.Threads.Count,
            UptimeMinutes = Math.Max(0, (now - startTime).TotalMinutes),
            WorkingSetBytes = workingSetBytes
        };

        return result;
    }

    private static async Task<PlatformCpuSnapshot> CaptureCpuUsageAsync(Process currentProcess)
    {
        try
        {
            var processCpuStart = currentProcess.TotalProcessorTime;
            var wallClockStart = DateTime.UtcNow;

            await Task.Delay(CpuSampleDurationMilliseconds);
            currentProcess.Refresh();

            var processCpuEnd = currentProcess.TotalProcessorTime;
            var wallClockSeconds = (DateTime.UtcNow - wallClockStart).TotalSeconds;

            return new PlatformCpuSnapshot
            {
                ProcessCpuUsagePercent = GetProcessCpuUsagePercent(processCpuStart, processCpuEnd, wallClockSeconds),
                SystemCpuUsagePercent = GetSystemCpuUsagePercent()
            };
        }
        catch
        {
            return new PlatformCpuSnapshot();
        }
    }

    private static List<PlatformSystemMonitorDiskDto> GetDiskInfos()
    {
        var result = new List<PlatformSystemMonitorDiskDto>();

        foreach (var drive in DriveInfo.GetDrives())
        {
            try
            {
                if (!drive.IsReady)
                {
                    continue;
                }

                result.Add(new PlatformSystemMonitorDiskDto
                {
                    DriveFormat = drive.DriveFormat,
                    FreeSpaceBytes = drive.AvailableFreeSpace,
                    Name = drive.Name,
                    TotalSizeBytes = drive.TotalSize,
                    Type = drive.DriveType.ToString()
                });
            }
            catch
            {
                // Ignore individual drive probe failures so one special mount does not break the page.
            }
        }

        return result
            .OrderBy(drive => drive.Name)
            .ToList();
    }

    private static int? GetHandleCountSafely(Process currentProcess)
    {
        try
        {
            return currentProcess.HandleCount;
        }
        catch
        {
            return null;
        }
    }

    private static double? GetProcessCpuUsagePercent(TimeSpan processCpuStart, TimeSpan processCpuEnd, double wallClockSeconds)
    {
        if (wallClockSeconds <= 0 || Environment.ProcessorCount <= 0)
        {
            return null;
        }

        var consumedCpuSeconds = (processCpuEnd - processCpuStart).TotalSeconds;
        if (consumedCpuSeconds < 0)
        {
            return null;
        }

        var percent = consumedCpuSeconds
            / wallClockSeconds
            / Environment.ProcessorCount
            * 100d;

        return Math.Round(Math.Clamp(percent, 0, 100), 2);
    }

    private static double? GetSystemCpuUsagePercent()
    {
        return OperatingSystem.IsMacOS()
            ? GetMacSystemCpuUsagePercent()
            : GetLinuxSystemCpuUsagePercent();
    }

    private static double? GetSystemMemoryUsagePercent(long totalMemoryBytes, long systemUsedMemoryBytes)
    {
        if (totalMemoryBytes <= 0)
        {
            return null;
        }

        return Math.Round(systemUsedMemoryBytes * 100d / totalMemoryBytes, 2);
    }

    private static long? GetSystemUsedMemoryBytes(long totalMemoryBytes, long availableMemoryBytes)
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                var output = ExecuteCommand("/usr/bin/top", "-l 1");
                if (!string.IsNullOrWhiteSpace(output))
                {
                    var usedMatch = Regex.Match(output, @"PhysMem:\s+(?<used>\d+(?:\.\d+)?)(?<unit>[KMGTP])\s+used", RegexOptions.IgnoreCase);
                    if (usedMatch.Success)
                    {
                        return ConvertToBytes(usedMatch.Groups["used"].Value, usedMatch.Groups["unit"].Value);
                    }
                }
            }
        }
        catch
        {
            // Ignore host-specific metric failures and degrade gracefully.
        }

        if (totalMemoryBytes <= 0)
        {
            return null;
        }

        return Math.Max(0, totalMemoryBytes - Math.Max(0, availableMemoryBytes));
    }

    private static long GetTotalMemoryBytes()
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                var output = ExecuteCommand("/usr/sbin/sysctl", "-n hw.memsize");
                if (long.TryParse(output?.Trim(), out var macMemBytes))
                {
                    return macMemBytes;
                }
            }

            var memInfoPath = "/proc/meminfo";
            if (File.Exists(memInfoPath))
            {
                var memTotalLine = File.ReadLines(memInfoPath)
                    .FirstOrDefault(line => line.StartsWith("MemTotal:", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(memTotalLine))
                {
                    var parts = memTotalLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var kiloBytes))
                    {
                        return kiloBytes * 1024;
                    }
                }
            }
        }
        catch
        {
            // Ignore host-specific metric failures and degrade gracefully.
        }

        return 0;
    }

    private static long GetAvailableMemoryBytes()
    {
        try
        {
            if (OperatingSystem.IsMacOS())
            {
                var output = ExecuteCommand("/usr/bin/vm_stat", string.Empty);
                if (!string.IsNullOrWhiteSpace(output))
                {
                    var pageSizeMatch = Regex.Match(output, @"page size of (?<size>\d+) bytes", RegexOptions.IgnoreCase);
                    var pageSize = pageSizeMatch.Success && long.TryParse(pageSizeMatch.Groups["size"].Value, out var parsedPageSize)
                        ? parsedPageSize
                        : 4096;
                    var freePages = ParseVmStatPages(output, "Pages free");
                    var speculativePages = ParseVmStatPages(output, "Pages speculative");
                    return (freePages + speculativePages) * pageSize;
                }
            }

            var memInfoPath = "/proc/meminfo";
            if (File.Exists(memInfoPath))
            {
                var memAvailableLine = File.ReadLines(memInfoPath)
                    .FirstOrDefault(line => line.StartsWith("MemAvailable:", StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(memAvailableLine))
                {
                    var parts = memAvailableLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 2 && long.TryParse(parts[1], out var kiloBytes))
                    {
                        return kiloBytes * 1024;
                    }
                }
            }
        }
        catch
        {
            // Ignore host-specific metric failures and degrade gracefully.
        }

        return 0;
    }

    private static double? GetLinuxSystemCpuUsagePercent()
    {
        try
        {
            var start = GetLinuxSystemCpuSample();
            if (start is null)
            {
                return null;
            }

            Thread.Sleep(CpuSampleDurationMilliseconds);
            var end = GetLinuxSystemCpuSample();
            if (end is null)
            {
                return null;
            }

            var totalDelta = end.TotalTicks - start.TotalTicks;
            var idleDelta = end.IdleTicks - start.IdleTicks;
            if (totalDelta <= 0)
            {
                return null;
            }

            var usage = (totalDelta - idleDelta) * 100d / totalDelta;
            return Math.Round(Math.Clamp(usage, 0, 100), 2);
        }
        catch
        {
            // Ignore host-specific metric failures and degrade gracefully.
        }

        return null;
    }

    private static SystemCpuSample? GetLinuxSystemCpuSample()
    {
        var statLine = File.ReadLines("/proc/stat").FirstOrDefault();
        if (string.IsNullOrWhiteSpace(statLine) || !statLine.StartsWith("cpu "))
        {
            return null;
        }

        var parts = statLine.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 5)
        {
            return null;
        }

        long totalTicks = 0;
        for (var index = 1; index < parts.Length; index++)
        {
            if (!long.TryParse(parts[index], out var value))
            {
                return null;
            }

            totalTicks += value;
        }

        if (!long.TryParse(parts[4], out var idleTicks))
        {
            return null;
        }

        return new SystemCpuSample(totalTicks, idleTicks);
    }

    private static double? GetMacSystemCpuUsagePercent()
    {
        var output = ExecuteCommand("/usr/bin/top", "-l 1 -n 0");
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        var cpuLine = output.Split('\n')
            .FirstOrDefault(line => line.Contains("CPU usage:", StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrWhiteSpace(cpuLine))
        {
            return null;
        }

        var user = ParseTopCpuSegment(cpuLine, "user");
        var system = ParseTopCpuSegment(cpuLine, "sys");
        var idle = ParseTopCpuSegment(cpuLine, "idle");

        if (user is null || system is null || idle is null)
        {
            return null;
        }

        return Math.Round(Math.Clamp(user.Value + system.Value, 0, 100), 2);
    }

    private static string? ExecuteCommand(string fileName, string arguments)
    {
        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    Arguments = arguments,
                    CreateNoWindow = true,
                    FileName = fileName,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                }
            };
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(2000);
            return output;
        }
        catch
        {
            return null;
        }
    }

    private static long ParseVmStatPages(string output, string label)
    {
        var pattern = $@"{Regex.Escape(label)}:\s+(?<pages>\d+)\.";
        var match = Regex.Match(output, pattern, RegexOptions.IgnoreCase);
        return match.Success && long.TryParse(match.Groups["pages"].Value, out var pages)
            ? pages
            : 0;
    }

    private static double? ParseTopCpuSegment(string cpuLine, string label)
    {
        var pattern = $@"(?<value>\d+(?:\.\d+)?)%\s*{Regex.Escape(label)}";
        var match = Regex.Match(cpuLine, pattern, RegexOptions.IgnoreCase);
        return match.Success && double.TryParse(match.Groups["value"].Value, out var value)
            ? value
            : null;
    }

    private static long ConvertToBytes(string value, string unit)
    {
        if (!double.TryParse(value, out var numericValue))
        {
            return 0;
        }

        var multiplier = unit.Trim().ToUpperInvariant() switch
        {
            "K" => 1024d,
            "M" => 1024d * 1024d,
            "G" => 1024d * 1024d * 1024d,
            "T" => 1024d * 1024d * 1024d * 1024d,
            "P" => 1024d * 1024d * 1024d * 1024d * 1024d,
            _ => 1d
        };

        return (long)Math.Round(numericValue * multiplier);
    }

    private sealed record PlatformCpuSnapshot
    {
        public double? ProcessCpuUsagePercent { get; init; }

        public double? SystemCpuUsagePercent { get; init; }
    }

    private sealed record SystemCpuSample(long TotalTicks, long IdleTicks);
}
