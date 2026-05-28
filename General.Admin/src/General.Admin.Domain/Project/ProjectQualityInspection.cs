using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 质量检查记录：记录实际检查结果、发现摘要和整改要求，关联质量检查单。
/// </summary>
public class ProjectQualityInspection : FullAuditedAggregateRoot<Guid>
{
    public string FindingSummary { get; private set; }

    public string InspectionResult { get; private set; }

    public DateTime InspectionTime { get; private set; }

    public Guid InspectorUserId { get; private set; }

    public Guid ProjectId { get; private set; }

    public Guid QualityChecklistId { get; private set; }

    public string RectificationRequirement { get; private set; }

    protected ProjectQualityInspection()
    {
        FindingSummary = string.Empty;
        InspectionResult = string.Empty;
        RectificationRequirement = string.Empty;
    }

    public ProjectQualityInspection(
        Guid id,
        Guid projectId,
        Guid qualityChecklistId,
        Guid inspectorUserId,
        DateTime inspectionTime,
        string inspectionResult,
        string findingSummary,
        string rectificationRequirement) : base(id)
    {
        ProjectId = projectId;
        QualityChecklistId = qualityChecklistId;
        InspectorUserId = inspectorUserId;
        InspectionTime = inspectionTime;
        InspectionResult = Check.NotNullOrWhiteSpace(inspectionResult, nameof(inspectionResult), 32);
        FindingSummary = findingSummary.Trim();
        RectificationRequirement = rectificationRequirement.Trim();
    }
}
