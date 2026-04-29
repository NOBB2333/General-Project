namespace General.Admin.Platform;

public class PlatformScheduledJobClusterNodeDto
{
    public string Description { get; set; } = string.Empty;

    public string HostName { get; set; } = string.Empty;

    public string InstanceId { get; set; } = string.Empty;

    public DateTime LastHeartbeatTime { get; set; }

    public string ProcessId { get; set; } = string.Empty;

    public DateTime StartedAt { get; set; }

    public string Status { get; set; } = string.Empty;
}
