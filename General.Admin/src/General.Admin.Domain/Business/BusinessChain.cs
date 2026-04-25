using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Business;

public class BusinessChain : FullAuditedAggregateRoot<Guid>
{
    public string ChainCode { get; private set; }

    public string LinkedContractCode { get; private set; }

    public Guid OwnerUserId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string SourceChangeCode { get; private set; }

    public string Stage { get; private set; }

    public string Status { get; private set; }

    public string Summary { get; private set; }

    public string Title { get; private set; }

    public string Type { get; private set; }

    protected BusinessChain()
    {
        ChainCode = string.Empty;
        LinkedContractCode = string.Empty;
        SourceChangeCode = string.Empty;
        Stage = string.Empty;
        Status = string.Empty;
        Summary = string.Empty;
        Title = string.Empty;
        Type = string.Empty;
    }

    public BusinessChain(
        Guid id,
        Guid projectId,
        string chainCode,
        string stage,
        string type,
        string title,
        string status,
        Guid ownerUserId,
        string? summary = null,
        string? linkedContractCode = null,
        string? sourceChangeCode = null) : base(id)
    {
        ProjectId = projectId;
        ChainCode = Check.NotNullOrWhiteSpace(chainCode, nameof(chainCode), 64);
        Stage = Check.NotNullOrWhiteSpace(stage, nameof(stage), 32);
        Type = Check.NotNullOrWhiteSpace(type, nameof(type), 32);
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), 128);
        Status = Check.NotNullOrWhiteSpace(status, nameof(status), 32);
        OwnerUserId = ownerUserId;
        Summary = summary?.Trim() ?? string.Empty;
        LinkedContractCode = linkedContractCode?.Trim() ?? string.Empty;
        SourceChangeCode = sourceChangeCode?.Trim() ?? string.Empty;
    }
}
