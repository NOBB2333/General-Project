using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneBusinessProcurement : FullAuditedAggregateRoot<Guid>
{
    public decimal Amount { get; private set; }

    public string LinkedContractCode { get; private set; }

    public Guid ProjectId { get; private set; }

    public string ProcurementCode { get; private set; }

    public DateTime SignDate { get; private set; }

    public string SourceChangeCode { get; private set; }

    public string Stage { get; private set; }

    public string Status { get; private set; }

    public string SupplierName { get; private set; }

    public string Title { get; private set; }

    protected PhaseOneBusinessProcurement()
    {
        LinkedContractCode = string.Empty;
        ProcurementCode = string.Empty;
        SourceChangeCode = string.Empty;
        Stage = string.Empty;
        Status = string.Empty;
        SupplierName = string.Empty;
        Title = string.Empty;
    }

    public PhaseOneBusinessProcurement(
        Guid id,
        Guid projectId,
        string procurementCode,
        string title,
        string supplierName,
        decimal amount,
        DateTime signDate,
        string stage,
        string status,
        string? linkedContractCode = null,
        string? sourceChangeCode = null) : base(id)
    {
        ProjectId = projectId;
        ProcurementCode = Check.NotNullOrWhiteSpace(procurementCode, nameof(procurementCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        SupplierName = Check.NotNullOrWhiteSpace(supplierName, nameof(supplierName), 128);
        Amount = amount;
        SignDate = signDate;
        Stage = Check.NotNullOrWhiteSpace(stage, nameof(stage), 32);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        LinkedContractCode = linkedContractCode?.Trim() ?? string.Empty;
        SourceChangeCode = sourceChangeCode?.Trim() ?? string.Empty;
    }
}
