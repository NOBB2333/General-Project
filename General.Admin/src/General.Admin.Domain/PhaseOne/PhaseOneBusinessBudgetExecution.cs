using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneBusinessBudgetExecution : FullAuditedAggregateRoot<Guid>
{
    public decimal AdjustedAmount { get; private set; }

    public string BudgetCode { get; private set; }

    public string Category { get; private set; }

    public decimal ExecutedAmount { get; private set; }

    public Guid ProjectId { get; private set; }

    public int SortOrder { get; private set; }

    public decimal VarianceAmount { get; private set; }

    protected PhaseOneBusinessBudgetExecution()
    {
        BudgetCode = string.Empty;
        Category = string.Empty;
    }

    public PhaseOneBusinessBudgetExecution(
        Guid id,
        Guid projectId,
        string budgetCode,
        string category,
        decimal adjustedAmount,
        decimal executedAmount,
        decimal varianceAmount,
        int sortOrder) : base(id)
    {
        ProjectId = projectId;
        BudgetCode = Check.NotNullOrWhiteSpace(budgetCode, nameof(budgetCode), 64);
        Category = Check.NotNullOrWhiteSpace(category, nameof(category), 64);
        AdjustedAmount = adjustedAmount;
        ExecutedAmount = executedAmount;
        VarianceAmount = varianceAmount;
        SortOrder = sortOrder;
    }
}
