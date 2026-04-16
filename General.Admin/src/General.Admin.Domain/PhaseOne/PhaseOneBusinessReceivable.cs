using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneBusinessReceivable : FullAuditedAggregateRoot<Guid>
{
    public string InvoiceCode { get; private set; }

    public string LinkedContractCode { get; private set; }

    public DateTime PlannedDate { get; private set; }

    public Guid ProjectId { get; private set; }

    public decimal ReceivedAmount { get; private set; }

    public string ReceivableCode { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    public decimal TotalAmount { get; private set; }

    protected PhaseOneBusinessReceivable()
    {
        InvoiceCode = string.Empty;
        LinkedContractCode = string.Empty;
        ReceivableCode = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
    }

    public PhaseOneBusinessReceivable(
        Guid id,
        Guid projectId,
        string receivableCode,
        string title,
        DateTime plannedDate,
        decimal totalAmount,
        decimal receivedAmount,
        string status,
        string? linkedContractCode = null,
        string? invoiceCode = null) : base(id)
    {
        ProjectId = projectId;
        ReceivableCode = Check.NotNullOrWhiteSpace(receivableCode, nameof(receivableCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        PlannedDate = plannedDate;
        TotalAmount = totalAmount;
        ReceivedAmount = receivedAmount;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        LinkedContractCode = linkedContractCode?.Trim() ?? string.Empty;
        InvoiceCode = invoiceCode?.Trim() ?? string.Empty;
    }
}
