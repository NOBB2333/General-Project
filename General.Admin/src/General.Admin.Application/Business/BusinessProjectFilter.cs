namespace General.Admin.Business;

internal static class BusinessProjectFilter
{
    public static IEnumerable<BusinessProjectSummaryDto> Apply(
        IEnumerable<BusinessProjectSummaryDto> items,
        string? keyword,
        string? projectCode = null,
        string? counterpartyName = null,
        string? businessCloseStatus = null)
    {
        keyword = keyword?.Trim();
        projectCode = projectCode?.Trim();
        counterpartyName = counterpartyName?.Trim();
        businessCloseStatus = businessCloseStatus?.Trim();

        return items
            .Where(item => BusinessProjectionMapper.MatchKeyword(keyword, item.ProjectCode, item.Name, item.CustomerName, item.ManagerName))
            .Where(item => string.IsNullOrWhiteSpace(projectCode) || item.ProjectCode.Contains(projectCode, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(counterpartyName) || item.CustomerName.Contains(counterpartyName, StringComparison.OrdinalIgnoreCase))
            .Where(item => string.IsNullOrWhiteSpace(businessCloseStatus) || item.BusinessCloseStatus.Equals(businessCloseStatus, StringComparison.OrdinalIgnoreCase));
    }
}
