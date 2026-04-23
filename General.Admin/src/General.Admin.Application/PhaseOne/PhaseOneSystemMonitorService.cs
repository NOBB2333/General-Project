using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Hosting;
using Volo.Abp.DependencyInjection;

namespace General.Admin.PhaseOne;

public class PhaseOneSystemMonitorService : ITransientDependency
{
    private readonly IHostEnvironment _hostEnvironment;

    public PhaseOneSystemMonitorService(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public Task<PhaseOneSystemMonitorDto> GetAsync()
    {
        using var currentProcess = Process.GetCurrentProcess();
        var now = DateTime.Now;
        var startTime = currentProcess.StartTime;
        var totalMemoryBytes = GetTotalMemoryBytes();
        var availableMemoryBytes = GetAvailableMemoryBytes();
        var workingSetBytes = currentProcess.WorkingSet64;
        var systemUsedMemoryBytes = GetSystemUsedMemoryBytes(totalMemoryBytes, availableMemoryBytes);

        var result = new PhaseOneSystemMonitorDto
        {
            AvailableMemoryBytes = availableMemoryBytes,
            CpuUsagePercent = GetCpuUsagePercent(currentProcess, now),
            CpuTimeSeconds = currentProcess.TotalProcessorTime.TotalSeconds,
            Disks = GetDiskInfos(),
            EnvironmentName = _hostEnvironment.EnvironmentName,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            HandleCount = GetHandleCountSafely(currentProcess),
            MachineName = Environment.MachineName,
            ManagedMemoryBytes = GC.GetTotalMemory(forceFullCollection: false),
            MemoryUsageNote = OperatingSystem.IsMacOS()
                ? "当前系统内存使用按 macOS PhysMem 口径展示，包含压缩内存。"
                : null,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            OsDescription = RuntimeInformation.OSDescription,
            ProcessorCount = Environment.ProcessorCount,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessMemoryDisplayDenominatorBytes = totalMemoryBytes,
            ProcessStartTime = startTime,
            ProcessMemoryUsagePercent = totalMemoryBytes > 0 ? workingSetBytes * 100d / totalMemoryBytes : null,
            SystemMemoryUsagePercent = GetSystemMemoryUsagePercent(totalMemoryBytes, availableMemoryBytes, systemUsedMemoryBytes),
            TotalMemoryBytes = totalMemoryBytes,
            ThreadCount = currentProcess.Threads.Count,
            UptimeMinutes = Math.Max(0, (now - startTime).TotalMinutes),
            WorkingSetBytes = workingSetBytes
        };

        return Task.FromResult(result);
    }

    private static List<PhaseOneSystemMonitorDiskDto> GetDiskInfos()
    {
        return DriveInfo.GetDrives()
            .Where(drive => drive.IsReady)
            .Select(drive => new PhaseOneSystemMonitorDiskDto
            {
                DriveFormat = drive.DriveFormat,
                FreeSpaceBytes = drive.AvailableFreeSpace,
                Name = drive.Name,
                TotalSizeBytes = drive.TotalSize,
                Type = drive.DriveType.ToString()
            })
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

    private static double? GetCpuUsagePercent(Process currentProcess, DateTime now)
    {
        var uptimeSeconds = (now - currentProcess.StartTime).TotalSeconds;
        if (uptimeSeconds <= 0 || Environment.ProcessorCount <= 0)
        {
            return null;
        }

        var percent = currentProcess.TotalProcessorTime.TotalSeconds
            / uptimeSeconds
            / Environment.ProcessorCount
            * 100d;

        return Math.Round(Math.Clamp(percent, 0, 100), 2);
    }

    private static double? GetSystemMemoryUsagePercent(long totalMemoryBytes, long availableMemoryBytes, long? systemUsedMemoryBytes = null)
    {
        if (totalMemoryBytes <= 0)
        {
            return null;
        }

        var usedBytes = systemUsedMemoryBytes ?? Math.Max(0, totalMemoryBytes - Math.Max(0, availableMemoryBytes));
        return Math.Round(usedBytes * 100d / totalMemoryBytes, 2);
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
}
