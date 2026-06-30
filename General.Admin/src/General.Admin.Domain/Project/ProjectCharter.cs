using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目章程：记录商业论证、目标、范围边界、假设、制约、成功标准及审批状态（PMP 启动过程组）。
/// </summary>
public class ProjectCharter : FullAuditedAggregateRoot<Guid>
{
    public string ApprovalStatus { get; private set; }

    public string Assumptions { get; private set; }

    public string BusinessCase { get; private set; }

    public string Constraints { get; private set; }

    public string Objectives { get; private set; }

    public Guid ProjectId { get; private set; }

    public string ScopeBoundary { get; private set; }

    public string SuccessCriteria { get; private set; }

    protected ProjectCharter()
    {
        ApprovalStatus = string.Empty;
        Assumptions = string.Empty;
        BusinessCase = string.Empty;
        Constraints = string.Empty;
        Objectives = string.Empty;
        ScopeBoundary = string.Empty;
        SuccessCriteria = string.Empty;
    }

    public ProjectCharter(
        Guid id,
        Guid projectId,
        string businessCase,
        string objectives,
        string scopeBoundary,
        string assumptions,
        string constraints,
        string successCriteria,
        string approvalStatus) : base(id)
    {
        ProjectId = projectId;
        BusinessCase = NormalizeSummary(businessCase, nameof(businessCase));
        Objectives = NormalizeSummary(objectives, nameof(objectives));
        ScopeBoundary = NormalizeSummary(scopeBoundary, nameof(scopeBoundary));
        Assumptions = NormalizeSummary(assumptions, nameof(assumptions));
        Constraints = NormalizeSummary(constraints, nameof(constraints));
        SuccessCriteria = NormalizeSummary(successCriteria, nameof(successCriteria));
        ApprovalStatus = Check.NotNullOrWhiteSpace(approvalStatus, nameof(approvalStatus), 32);
    }

    public void Update(
        string businessCase,
        string objectives,
        string scopeBoundary,
        string assumptions,
        string constraints,
        string successCriteria,
        string approvalStatus)
    {
        BusinessCase = NormalizeSummary(businessCase, nameof(businessCase));
        Objectives = NormalizeSummary(objectives, nameof(objectives));
        ScopeBoundary = NormalizeSummary(scopeBoundary, nameof(scopeBoundary));
        Assumptions = NormalizeSummary(assumptions, nameof(assumptions));
        Constraints = NormalizeSummary(constraints, nameof(constraints));
        SuccessCriteria = NormalizeSummary(successCriteria, nameof(successCriteria));
        ApprovalStatus = Check.NotNullOrWhiteSpace(approvalStatus, nameof(approvalStatus), 32);
    }

    public void UpdateApprovalStatus(string approvalStatus)
    {
        ApprovalStatus = Check.NotNullOrWhiteSpace(approvalStatus, nameof(approvalStatus), 32);
    }

    private static string NormalizeSummary(string? value, string parameterName)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return Check.Length(normalized, parameterName, maxLength: 2048) ?? string.Empty;
    }
}
