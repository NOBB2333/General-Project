using Volo.Abp.Auditing;
using General.Admin.Project;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Business;

public sealed record BusinessContext(
    List<BusinessAcceptance> Acceptances,
    List<BusinessBudgetExecution> BudgetExecutions,
    List<BusinessChain> BusinessChains,
    List<BusinessContract> Contracts,
    List<BusinessForecastHistory> ForecastHistories,
    List<BusinessInvoice> Invoices,
    Dictionary<Guid, string> OrganizationUnitNameMap,
    List<ProjectPerformanceSnapshot> PerformanceSnapshots,
    List<BusinessProcurement> Procurements,
    Dictionary<Guid, string> ProjectCodeMap,
    Dictionary<Guid, string> ProjectNameMap,
    List<ProjectEntity> Projects,
    List<BusinessReceivable> Receivables,
    Dictionary<Guid, string> UserNameMap)
{
    public Dictionary<Guid, ProjectEntity> ProjectById { get; } =
        Projects.ToDictionary(p => p.Id);

    public Dictionary<Guid, List<BusinessContract>> ContractsByProject { get; } =
        Contracts.GroupBy(c => c.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<BusinessInvoice>> InvoicesByProject { get; } =
        Invoices.GroupBy(i => i.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<BusinessProcurement>> ProcurementsByProject { get; } =
        Procurements.GroupBy(p => p.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<BusinessReceivable>> ReceivablesByProject { get; } =
        Receivables.GroupBy(r => r.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<BusinessBudgetExecution>> BudgetExecutionsByProject { get; } =
        BudgetExecutions.GroupBy(b => b.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, ProjectPerformanceSnapshot> LatestPerformanceSnapshotByProject { get; } =
        PerformanceSnapshots
            .GroupBy(item => item.ProjectId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(item => item.SnapshotDate)
                    .ThenByDescending(item => item.CreationTime)
                    .First());

    public Dictionary<Guid, List<BusinessChain>> BusinessChainsByProject { get; } =
        BusinessChains.GroupBy(b => b.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, List<BusinessForecastHistory>> ForecastHistoriesByProject { get; } =
        ForecastHistories.GroupBy(f => f.ProjectId).ToDictionary(g => g.Key, g => g.ToList());

    public Dictionary<Guid, DateTime> LastUpdatedByProject { get; } =
        ComputeLastUpdated(Projects, Acceptances, BudgetExecutions, BusinessChains, Contracts, ForecastHistories, Invoices, Procurements, Receivables);

    private static Dictionary<Guid, DateTime> ComputeLastUpdated(
        List<ProjectEntity> projects,
        List<BusinessAcceptance> acceptances,
        List<BusinessBudgetExecution> budgetExecutions,
        List<BusinessChain> businessChains,
        List<BusinessContract> contracts,
        List<BusinessForecastHistory> forecastHistories,
        List<BusinessInvoice> invoices,
        List<BusinessProcurement> procurements,
        List<BusinessReceivable> receivables)
    {
        var result = projects.ToDictionary(p => p.Id, p => ResolveEntityTimestamp(p));

        void MergeMax<T>(IEnumerable<T> items, Func<T, Guid> getId, Func<T, DateTime> getTime)
        {
            foreach (var item in items)
            {
                var id = getId(item);
                var t = getTime(item);
                if (!result.TryGetValue(id, out var existing) || t > existing)
                {
                    result[id] = t;
                }
            }
        }

        MergeMax(acceptances, i => i.ProjectId, i => ResolveEntityTimestamp(i));
        MergeMax(budgetExecutions, i => i.ProjectId, i => ResolveEntityTimestamp(i));
        MergeMax(businessChains, i => i.ProjectId, i => ResolveEntityTimestamp(i));
        MergeMax(contracts, i => i.ProjectId, i => ResolveEntityTimestamp(i));
        MergeMax(forecastHistories, i => i.ProjectId, i => ResolveEntityTimestamp(i));
        MergeMax(invoices, i => i.ProjectId, i => ResolveEntityTimestamp(i));
        MergeMax(procurements, i => i.ProjectId, i => ResolveEntityTimestamp(i));
        MergeMax(receivables, i => i.ProjectId, i => ResolveEntityTimestamp(i));

        return result;
    }

    private static DateTime ResolveEntityTimestamp<T>(T item)
        where T : IHasCreationTime, IHasModificationTime
        => item.LastModificationTime ?? item.CreationTime;
}
