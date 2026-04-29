using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class PlatformScheduledJobClusterNode : FullAuditedAggregateRoot<Guid>
{
    public string Description { get; private set; }

    public string HostName { get; private set; }

    public string InstanceId { get; private set; }

    public DateTime LastHeartbeatTime { get; private set; }

    public string ProcessId { get; private set; }

    public DateTime StartedAt { get; private set; }

    public string Status { get; private set; }

    protected PlatformScheduledJobClusterNode()
    {
        Description = string.Empty;
        HostName = string.Empty;
        InstanceId = string.Empty;
        ProcessId = string.Empty;
        Status = string.Empty;
    }

    public PlatformScheduledJobClusterNode(
        Guid id,
        string instanceId,
        string hostName,
        string processId,
        DateTime startedAt,
        DateTime lastHeartbeatTime,
        string status,
        string? description = null) : base(id)
    {
        InstanceId = Check.NotNullOrWhiteSpace(instanceId, nameof(instanceId), 128);
        HostName = Check.NotNullOrWhiteSpace(hostName, nameof(hostName), 128);
        ProcessId = Check.NotNullOrWhiteSpace(processId, nameof(processId), 64);
        StartedAt = startedAt;
        LastHeartbeatTime = lastHeartbeatTime;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Description = description?.Trim() ?? string.Empty;
    }

    public void Heartbeat(DateTime heartbeatTime, string status, string? description = null)
    {
        LastHeartbeatTime = heartbeatTime;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Description = description?.Trim() ?? string.Empty;
    }
}
