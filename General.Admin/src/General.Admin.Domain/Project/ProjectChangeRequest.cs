using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Project;

/// <summary>
/// 项目基线变更申请：只承接范围、进度、成本基线变更。
/// </summary>
public class ProjectChangeRequest : FullAuditedAggregateRoot<Guid>
{
    public Guid ApplicantUserId { get; private set; }

    public string ChangeType { get; private set; }

    public string ExpectedResult { get; private set; }

    public string ImpactScope { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Reason { get; private set; }

    public string RequestCode { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    protected ProjectChangeRequest()
    {
        ChangeType = string.Empty;
        ExpectedResult = string.Empty;
        ImpactScope = string.Empty;
        Reason = string.Empty;
        RequestCode = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
    }

    public ProjectChangeRequest(
        Guid id,
        Guid projectId,
        string requestCode,
        string title,
        string changeType,
        string reason,
        string impactScope,
        string expectedResult,
        Guid applicantUserId,
        string status = "待评审") : base(id)
    {
        ProjectId = projectId;
        RequestCode = Check.NotNullOrWhiteSpace(requestCode, nameof(requestCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        ChangeType = Check.NotNullOrWhiteSpace(changeType, nameof(changeType), 32);
        Reason = Check.NotNullOrWhiteSpace(reason, nameof(reason), 2048);
        ImpactScope = Check.NotNullOrWhiteSpace(impactScope, nameof(impactScope), 2048);
        ExpectedResult = Check.NotNullOrWhiteSpace(expectedResult, nameof(expectedResult), 2048);
        ApplicantUserId = applicantUserId;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
    }

    public void Approve()
    {
        Status = "已批准";
    }

    public void Submit()
    {
        Status = "审批中";
    }

    public void Reject()
    {
        Status = "已驳回";
    }

    public void Withdraw()
    {
        Status = "待评审";
    }

    public bool IsApproved() => Status == "已批准";
}
