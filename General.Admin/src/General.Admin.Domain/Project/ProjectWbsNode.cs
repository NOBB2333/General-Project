using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目 WBS 节点：表达范围和交付结构，不承担日常任务状态流转。
/// </summary>
public class ProjectWbsNode : FullAuditedAggregateRoot<Guid>
{
    public Guid? SprintId { get; private set; }

    public string DeliveryDescription { get; private set; }

    public string EstimateSummary { get; private set; }

    public Guid? MilestoneId { get; private set; }

    public string Name { get; private set; }

    public Guid? OwnerUserId { get; private set; }

    public Guid? ParentId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string ScopeDescription { get; private set; }

    public int SortOrder { get; private set; }

    public string Status { get; private set; }

    public string Type { get; private set; }

    public string WbsCode { get; private set; }

    protected ProjectWbsNode()
    {
        DeliveryDescription = string.Empty;
        EstimateSummary = string.Empty;
        Name = string.Empty;
        ScopeDescription = string.Empty;
        Status = string.Empty;
        Type = string.Empty;
        WbsCode = string.Empty;
    }

    public ProjectWbsNode(
        Guid id,
        Guid projectId,
        string wbsCode,
        string name,
        string type,
        string status,
        Guid? parentId = null,
        Guid? milestoneId = null,
        Guid? sprintId = null,
        Guid? ownerUserId = null,
        int sortOrder = 0,
        string? scopeDescription = null,
        string? deliveryDescription = null,
        string? estimateSummary = null) : base(id)
    {
        ProjectId = projectId;
        WbsCode = Check.NotNullOrWhiteSpace(wbsCode, nameof(wbsCode), 64);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        ParentId = parentId;
        MilestoneId = milestoneId;
        SprintId = sprintId;
        OwnerUserId = ownerUserId;
        SortOrder = sortOrder;
        ScopeDescription = scopeDescription?.Trim() ?? string.Empty;
        DeliveryDescription = deliveryDescription?.Trim() ?? string.Empty;
        EstimateSummary = estimateSummary?.Trim() ?? string.Empty;
    }

    public void Update(
        string wbsCode,
        string name,
        string type,
        string status,
        Guid? parentId,
        Guid? milestoneId,
        Guid? sprintId,
        Guid? ownerUserId,
        int sortOrder,
        string? scopeDescription,
        string? deliveryDescription,
        string? estimateSummary)
    {
        WbsCode = Check.NotNullOrWhiteSpace(wbsCode, nameof(wbsCode), 64);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128);
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        ParentId = parentId;
        MilestoneId = milestoneId;
        SprintId = sprintId;
        OwnerUserId = ownerUserId;
        SortOrder = sortOrder;
        ScopeDescription = scopeDescription?.Trim() ?? string.Empty;
        DeliveryDescription = deliveryDescription?.Trim() ?? string.Empty;
        EstimateSummary = estimateSummary?.Trim() ?? string.Empty;
    }
}
