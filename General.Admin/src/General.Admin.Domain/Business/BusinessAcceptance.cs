using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Business;

public class BusinessAcceptance : FullAuditedAggregateRoot<Guid>
{
    public string AcceptanceCode { get; private set; }

    public DateTime AcceptanceDate { get; private set; }

    public string AcceptanceResult { get; private set; }

    public string LinkedContractCode { get; private set; }

    public Guid? MilestoneId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string QualityInspectionCode { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    protected BusinessAcceptance()
    {
        AcceptanceCode = string.Empty;
        AcceptanceResult = string.Empty;
        LinkedContractCode = string.Empty;
        QualityInspectionCode = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
    }

    public BusinessAcceptance(
        Guid id,
        Guid projectId,
        string acceptanceCode,
        string title,
        DateTime acceptanceDate,
        string acceptanceResult,
        string status,
        string? linkedContractCode = null,
        Guid? milestoneId = null,
        string? qualityInspectionCode = null) : base(id)
    {
        ProjectId = projectId;
        AcceptanceCode = Check.NotNullOrWhiteSpace(acceptanceCode, nameof(acceptanceCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        AcceptanceDate = acceptanceDate;
        AcceptanceResult = Check.NotNullOrWhiteSpace(acceptanceResult, nameof(acceptanceResult), 32);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        LinkedContractCode = linkedContractCode?.Trim() ?? string.Empty;
        MilestoneId = milestoneId;
        QualityInspectionCode = qualityInspectionCode?.Trim() ?? string.Empty;
    }

    public void Update(
        string acceptanceCode,
        string title,
        DateTime acceptanceDate,
        string acceptanceResult,
        string status,
        string? linkedContractCode = null,
        Guid? milestoneId = null,
        string? qualityInspectionCode = null)
    {
        AcceptanceCode = Check.NotNullOrWhiteSpace(acceptanceCode, nameof(acceptanceCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        AcceptanceDate = acceptanceDate;
        AcceptanceResult = Check.NotNullOrWhiteSpace(acceptanceResult, nameof(acceptanceResult), 32);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        LinkedContractCode = linkedContractCode?.Trim() ?? string.Empty;
        MilestoneId = milestoneId;
        QualityInspectionCode = qualityInspectionCode?.Trim() ?? string.Empty;
    }
}
