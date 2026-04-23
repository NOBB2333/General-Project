namespace General.Admin.PhaseOne;

public class PhaseOneSystemMonitorDto
{
    public long AvailableMemoryBytes { get; set; }

    public string? MemoryUsageNote { get; set; }

    public double? ProcessCpuUsagePercent { get; set; }

    public double? SystemCpuUsagePercent { get; set; }

    public double CpuTimeSeconds { get; set; }

    public List<PhaseOneSystemMonitorDiskDto> Disks { get; set; } = [];

    public string EnvironmentName { get; set; } = string.Empty;

    public string FrameworkDescription { get; set; } = string.Empty;

    public int? HandleCount { get; set; }

    public long ManagedMemoryBytes { get; set; }

    public string MachineName { get; set; } = string.Empty;

    public string OsArchitecture { get; set; } = string.Empty;

    public string OsDescription { get; set; } = string.Empty;

    public int ProcessorCount { get; set; }

    public string ProcessArchitecture { get; set; } = string.Empty;

    public long ProcessMemoryDisplayDenominatorBytes { get; set; }

    public DateTime ProcessStartTime { get; set; }

    public double? ProcessMemoryUsagePercent { get; set; }

    public double? SystemMemoryUsagePercent { get; set; }

    public long TotalMemoryBytes { get; set; }

    public int ThreadCount { get; set; }

    public double UptimeMinutes { get; set; }

    public long WorkingSetBytes { get; set; }
}

public class PhaseOneSystemMonitorDiskDto
{
    public string DriveFormat { get; set; } = string.Empty;

    public long FreeSpaceBytes { get; set; }

    public string Name { get; set; } = string.Empty;

    public long TotalSizeBytes { get; set; }

    public string Type { get; set; } = string.Empty;
}
