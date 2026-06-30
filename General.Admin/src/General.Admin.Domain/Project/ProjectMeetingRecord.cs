using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目会议记录：承载会议纪要、决议和后续行动项。
/// </summary>
public class ProjectMeetingRecord : FullAuditedAggregateRoot<Guid>
{
    public Guid? AttachmentFileId { get; private set; }

    public string Decisions { get; private set; }

    public Guid FacilitatorUserId { get; private set; }

    public string Minutes { get; private set; }

    public DateTime MeetingTime { get; private set; }

    public string ParticipantUserIds { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Status { get; private set; }

    public string Topic { get; private set; }

    protected ProjectMeetingRecord()
    {
        Decisions = string.Empty;
        Minutes = string.Empty;
        ParticipantUserIds = string.Empty;
        Status = string.Empty;
        Topic = string.Empty;
    }

    public ProjectMeetingRecord(
        Guid id,
        Guid projectId,
        string topic,
        DateTime meetingTime,
        Guid facilitatorUserId,
        IEnumerable<Guid>? participantUserIds,
        string status,
        string? minutes = null,
        string? decisions = null,
        Guid? attachmentFileId = null) : base(id)
    {
        ProjectId = projectId;
        Topic = Check.NotNullOrWhiteSpace(topic, nameof(topic), 128);
        MeetingTime = meetingTime;
        FacilitatorUserId = facilitatorUserId;
        ParticipantUserIds = SerializeParticipantUserIds(participantUserIds);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        Minutes = minutes?.Trim() ?? string.Empty;
        Decisions = decisions?.Trim() ?? string.Empty;
        AttachmentFileId = attachmentFileId;
    }

    public List<Guid> GetParticipantUserIds()
    {
        return ParticipantUserIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(item => Guid.TryParse(item, out _))
            .Select(Guid.Parse)
            .ToList();
    }

    private static string SerializeParticipantUserIds(IEnumerable<Guid>? participantUserIds)
    {
        return participantUserIds == null
            ? string.Empty
            : string.Join(",", participantUserIds.Distinct());
    }
}
