using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目需求：表达业务能力、范围变更和验收口径，不承载日常执行任务状态。
/// </summary>
public class ProjectRequirement : FullAuditedAggregateRoot<Guid>
{
    public string AcceptanceCriteria { get; private set; }

    public Guid? BusinessOwnerUserId { get; private set; }

    public string ChangeReason { get; private set; }

    public string Description { get; private set; }

    public string ImpactSummary { get; private set; }

    public bool IsCurrent { get; private set; }

    public Guid? MilestoneId { get; private set; }

    public string Priority { get; private set; }

    public Guid? ProductOwnerUserId { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid? ReplacesRequirementId { get; private set; }

    public string RequirementCode { get; private set; }

    public double? SizeEstimate { get; private set; }

    public string Source { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; }

    public int VersionNo { get; private set; }

    public Guid? WbsNodeId { get; private set; }

    protected ProjectRequirement()
    {
        AcceptanceCriteria = string.Empty;
        ChangeReason = string.Empty;
        Description = string.Empty;
        ImpactSummary = string.Empty;
        Priority = string.Empty;
        RequirementCode = string.Empty;
        Source = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
        Type = string.Empty;
    }

    public ProjectRequirement(
        Guid id,
        Guid projectId,
        string requirementCode,
        string title,
        string description,
        string status,
        int versionNo,
        string priority,
        string source,
        string type,
        Guid? wbsNodeId = null,
        Guid? milestoneId = null,
        string? acceptanceCriteria = null,
        Guid? replacesRequirementId = null,
        double? sizeEstimate = null,
        Guid? businessOwnerUserId = null,
        Guid? productOwnerUserId = null,
        bool isCurrent = true,
        string? changeReason = null,
        string? impactSummary = null) : base(id)
    {
        ProjectId = projectId;
        RequirementCode = Check.NotNullOrWhiteSpace(requirementCode, nameof(requirementCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256);
        Description = description?.Trim() ?? string.Empty;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        VersionNo = versionNo;
        Priority = Check.NotNullOrWhiteSpace(priority, nameof(priority), 32);
        Source = Check.NotNullOrWhiteSpace(source, nameof(source), 32);
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        WbsNodeId = wbsNodeId;
        MilestoneId = milestoneId;
        AcceptanceCriteria = acceptanceCriteria?.Trim() ?? string.Empty;
        ReplacesRequirementId = replacesRequirementId;
        SizeEstimate = sizeEstimate;
        BusinessOwnerUserId = businessOwnerUserId;
        ProductOwnerUserId = productOwnerUserId;
        IsCurrent = isCurrent;
        ChangeReason = changeReason?.Trim() ?? string.Empty;
        ImpactSummary = impactSummary?.Trim() ?? string.Empty;
    }

    public void Update(
        string requirementCode,
        string title,
        string description,
        string status,
        int versionNo,
        string priority,
        string source,
        string type,
        Guid? wbsNodeId,
        Guid? milestoneId,
        string? acceptanceCriteria,
        Guid? replacesRequirementId,
        double? sizeEstimate,
        Guid? businessOwnerUserId,
        Guid? productOwnerUserId,
        bool isCurrent,
        string? changeReason,
        string? impactSummary)
    {
        RequirementCode = Check.NotNullOrWhiteSpace(requirementCode, nameof(requirementCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 256);
        Description = description?.Trim() ?? string.Empty;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        VersionNo = versionNo;
        Priority = Check.NotNullOrWhiteSpace(priority, nameof(priority), 32);
        Source = Check.NotNullOrWhiteSpace(source, nameof(source), 32);
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        WbsNodeId = wbsNodeId;
        MilestoneId = milestoneId;
        AcceptanceCriteria = acceptanceCriteria?.Trim() ?? string.Empty;
        ReplacesRequirementId = replacesRequirementId;
        SizeEstimate = sizeEstimate;
        BusinessOwnerUserId = businessOwnerUserId;
        ProductOwnerUserId = productOwnerUserId;
        IsCurrent = isCurrent;
        ChangeReason = changeReason?.Trim() ?? string.Empty;
        ImpactSummary = impactSummary?.Trim() ?? string.Empty;
    }

    public void MarkNotCurrent()
    {
        IsCurrent = false;
    }
}
