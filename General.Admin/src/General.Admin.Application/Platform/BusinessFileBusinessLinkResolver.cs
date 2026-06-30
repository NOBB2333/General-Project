using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Platform;

[ExposeServices(typeof(IFileBusinessLinkResolver))]
public class BusinessFileBusinessLinkResolver : IFileBusinessLinkResolver
{
    private static readonly HashSet<string> SupportedBusinessTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "BusinessAcceptance",
        "BusinessBudget",
        "BusinessChain",
        "BusinessContract",
        "BusinessInvoice",
        "BusinessProcurement",
        "BusinessReceivable",
        "Acceptance",
        "Budget",
        "Chain",
        "Contract",
        "Invoice",
        "Procurement",
        "Receivable"
    };

    private readonly IRepository<BusinessAcceptance, Guid> _businessAcceptanceRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _businessBudgetRepository;
    private readonly IRepository<BusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<BusinessInvoice, Guid> _businessInvoiceRepository;
    private readonly IRepository<BusinessProcurement, Guid> _businessProcurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _businessReceivableRepository;

    public BusinessFileBusinessLinkResolver(
        IRepository<BusinessAcceptance, Guid> businessAcceptanceRepository,
        IRepository<BusinessBudgetExecution, Guid> businessBudgetRepository,
        IRepository<BusinessChain, Guid> businessChainRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<BusinessInvoice, Guid> businessInvoiceRepository,
        IRepository<BusinessProcurement, Guid> businessProcurementRepository,
        IRepository<BusinessReceivable, Guid> businessReceivableRepository)
    {
        _businessAcceptanceRepository = businessAcceptanceRepository;
        _businessBudgetRepository = businessBudgetRepository;
        _businessChainRepository = businessChainRepository;
        _businessContractRepository = businessContractRepository;
        _businessInvoiceRepository = businessInvoiceRepository;
        _businessProcurementRepository = businessProcurementRepository;
        _businessReceivableRepository = businessReceivableRepository;
    }

    public bool CanResolve(string businessType)
    {
        return SupportedBusinessTypes.Contains(businessType.Trim());
    }

    public async Task<Guid?> ResolveProjectIdAsync(string businessType, string businessId)
    {
        if (!Guid.TryParse(businessId, out var id))
        {
            return null;
        }

        return businessType.Trim() switch
        {
            var type when type.Equals("BusinessAcceptance", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Acceptance", StringComparison.OrdinalIgnoreCase) => (await _businessAcceptanceRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("BusinessBudget", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Budget", StringComparison.OrdinalIgnoreCase) => (await _businessBudgetRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("BusinessChain", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Chain", StringComparison.OrdinalIgnoreCase) => (await _businessChainRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("BusinessContract", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Contract", StringComparison.OrdinalIgnoreCase) => (await _businessContractRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("BusinessInvoice", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Invoice", StringComparison.OrdinalIgnoreCase) => (await _businessInvoiceRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("BusinessProcurement", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Procurement", StringComparison.OrdinalIgnoreCase) => (await _businessProcurementRepository.FindAsync(id))?.ProjectId,
            var type when type.Equals("BusinessReceivable", StringComparison.OrdinalIgnoreCase) ||
                          type.Equals("Receivable", StringComparison.OrdinalIgnoreCase) => (await _businessReceivableRepository.FindAsync(id))?.ProjectId,
            _ => null
        };
    }
}
