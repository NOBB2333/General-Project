using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class PhaseOneProjectMember : FullAuditedAggregateRoot<Guid>
{
    public bool AllowHistoricalRead { get; private set; }

    public DateTime JoinDate { get; private set; }

    public DateTime? LeaveDate { get; private set; }

    public Guid OrganizationUnitId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string RoleNames { get; private set; }

    public Guid UserId { get; private set; }

    protected PhaseOneProjectMember()
    {
        RoleNames = string.Empty;
    }

    public PhaseOneProjectMember(
        Guid id,
        Guid projectId,
        Guid userId,
        Guid organizationUnitId,
        IEnumerable<string> roleNames,
        DateTime joinDate,
        DateTime? leaveDate = null,
        bool allowHistoricalRead = true) : base(id)
    {
        ProjectId = projectId;
        UserId = userId;
        OrganizationUnitId = organizationUnitId;
        RoleNames = NormalizeRoles(roleNames);
        JoinDate = joinDate;
        LeaveDate = leaveDate;
        AllowHistoricalRead = allowHistoricalRead;
    }

    public bool IsActive(DateTime? today = null)
    {
        var value = today ?? DateTime.Today;
        return !LeaveDate.HasValue || LeaveDate.Value.Date >= value.Date;
    }

    public IReadOnlyList<string> GetRoleList()
    {
        return RoleNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string NormalizeRoles(IEnumerable<string> roleNames)
    {
        return string.Join(
            ",",
            roleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase));
    }
}
