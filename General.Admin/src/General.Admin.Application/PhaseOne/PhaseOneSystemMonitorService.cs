using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
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
        var gcMemoryInfo = GC.GetGCMemoryInfo();
        var now = DateTime.Now;
        var startTime = currentProcess.StartTime;

        var result = new PhaseOneSystemMonitorDto
        {
            AvailableMemoryBytes = gcMemoryInfo.TotalAvailableMemoryBytes,
            CpuTimeSeconds = currentProcess.TotalProcessorTime.TotalSeconds,
            Disks = GetDiskInfos(),
            EnvironmentName = _hostEnvironment.EnvironmentName,
            FrameworkDescription = RuntimeInformation.FrameworkDescription,
            HandleCount = GetHandleCountSafely(currentProcess),
            MachineName = Environment.MachineName,
            ManagedMemoryBytes = GC.GetTotalMemory(forceFullCollection: false),
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            OsDescription = RuntimeInformation.OSDescription,
            ProcessorCount = Environment.ProcessorCount,
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            ProcessStartTime = startTime,
            ThreadCount = currentProcess.Threads.Count,
            UptimeMinutes = Math.Max(0, (now - startTime).TotalMinutes),
            WorkingSetBytes = currentProcess.WorkingSet64
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
}
