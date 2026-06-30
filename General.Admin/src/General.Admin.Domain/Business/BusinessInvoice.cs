using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Business;

public class BusinessInvoice : FullAuditedAggregateRoot<Guid>
{
    public decimal Amount { get; private set; }

    public string InvoiceCode { get; private set; }

    public DateTime InvoiceDate { get; private set; }

    public string LinkedContractCode { get; private set; }

    public Guid ProjectId { get; private set; }

    public string ReceivableCode { get; private set; }

    public string Status { get; private set; }

    public decimal TaxAmount { get; private set; }

    public string Title { get; private set; }

    protected BusinessInvoice()
    {
        InvoiceCode = string.Empty;
        LinkedContractCode = string.Empty;
        ReceivableCode = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
    }

    public BusinessInvoice(
        Guid id,
        Guid projectId,
        string invoiceCode,
        string title,
        DateTime invoiceDate,
        decimal amount,
        decimal taxAmount,
        string status,
        string? linkedContractCode = null,
        string? receivableCode = null) : base(id)
    {
        ProjectId = projectId;
        InvoiceCode = Check.NotNullOrWhiteSpace(invoiceCode, nameof(invoiceCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        InvoiceDate = invoiceDate;
        Amount = amount;
        TaxAmount = taxAmount;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        LinkedContractCode = linkedContractCode?.Trim() ?? string.Empty;
        ReceivableCode = receivableCode?.Trim() ?? string.Empty;
    }

    public void Update(
        string invoiceCode,
        string title,
        DateTime invoiceDate,
        decimal amount,
        decimal taxAmount,
        string status,
        string? linkedContractCode = null,
        string? receivableCode = null)
    {
        InvoiceCode = Check.NotNullOrWhiteSpace(invoiceCode, nameof(invoiceCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        InvoiceDate = invoiceDate;
        Amount = amount;
        TaxAmount = taxAmount;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        LinkedContractCode = linkedContractCode?.Trim() ?? string.Empty;
        ReceivableCode = receivableCode?.Trim() ?? string.Empty;
    }
}
