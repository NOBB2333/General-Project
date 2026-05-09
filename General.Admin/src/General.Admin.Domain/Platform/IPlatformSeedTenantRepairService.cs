namespace General.Admin.Platform;

public interface IPlatformSeedTenantRepairService
{
    Task RepairDefaultTenantSeedDataAsync(
        Guid tenantId,
        IReadOnlyCollection<Guid> userIds,
        IReadOnlyCollection<string> userNames,
        IReadOnlyCollection<Guid> organizationUnitIds,
        IReadOnlyCollection<string> organizationUnitNames,
        IReadOnlyCollection<string> roleNames);
}
