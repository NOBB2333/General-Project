using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Business;

public class BusinessContract : FullAuditedAggregateRoot<Guid>
{
    public decimal Amount { get; private set; }

    public string ContractCode { get; private set; }

    public string CounterpartyName { get; private set; }

    public bool IsRevenueContract { get; private set; }

    public string ParentContractCode { get; private set; }

    public Guid ProjectId { get; private set; }

    public DateTime SignDate { get; private set; }

    public string SourceChangeCode { get; private set; }

    public string Status { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; }

    protected BusinessContract()
    {
        ContractCode = string.Empty;
        CounterpartyName = string.Empty;
        ParentContractCode = string.Empty;
        SourceChangeCode = string.Empty;
        Status = string.Empty;
        Title = string.Empty;
        Type = string.Empty;
    }

    public BusinessContract(
        Guid id,
        Guid projectId,
        string contractCode,
        string title,
        string type,
        string counterpartyName,
        decimal amount,
        DateTime signDate,
        string status,
        bool isRevenueContract,
        string? parentContractCode = null,
        string? sourceChangeCode = null) : base(id)
    {
        ProjectId = projectId;
        ContractCode = Check.NotNullOrWhiteSpace(contractCode, nameof(contractCode), 64);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        CounterpartyName = Check.NotNullOrWhiteSpace(counterpartyName, nameof(counterpartyName), 128);
        Amount = amount;
        SignDate = signDate;
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        IsRevenueContract = isRevenueContract;
        ParentContractCode = parentContractCode?.Trim() ?? string.Empty;
        SourceChangeCode = sourceChangeCode?.Trim() ?? string.Empty;
    }
}
