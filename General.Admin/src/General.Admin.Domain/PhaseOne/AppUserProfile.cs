using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class AppUserProfile : FullAuditedAggregateRoot<Guid>
{
    public string? EmployeeNo { get; private set; }

    public string? ExternalSource { get; private set; }

    public string? ExternalUserId { get; private set; }

    public DateTime? LastLoginTime { get; private set; }

    public DateTime? LastSeenAt { get; private set; }

    public string? LastSeenBrowser { get; private set; }

    public string? LastSeenDevice { get; private set; }

    public string? LastSeenIpAddress { get; private set; }

    public string? PhoneNumber { get; private set; }

    public Guid UserId { get; private set; }

    public DateTime? ForceLogoutAfter { get; private set; }

    protected AppUserProfile()
    {
    }

    public AppUserProfile(
        Guid id,
        Guid userId,
        string? employeeNo,
        string? phoneNumber,
        string? externalSource,
        string? externalUserId,
        DateTime? lastLoginTime) : base(id)
    {
        UserId = userId;
        EmployeeNo = Normalize(employeeNo);
        PhoneNumber = Normalize(phoneNumber);
        ExternalSource = Normalize(externalSource);
        ExternalUserId = Normalize(externalUserId);
        LastLoginTime = lastLoginTime;
    }

    public void Update(
        string? employeeNo,
        string? phoneNumber,
        string? externalSource,
        string? externalUserId)
    {
        EmployeeNo = Normalize(employeeNo);
        PhoneNumber = Normalize(phoneNumber);
        ExternalSource = Normalize(externalSource);
        ExternalUserId = Normalize(externalUserId);
    }

    public void SetLastLoginTime(DateTime? value)
    {
        LastLoginTime = value;
    }

    public void UpdateLastSeen(
        DateTime? lastSeenAt,
        string? ipAddress,
        string? device,
        string? browser)
    {
        LastSeenAt = lastSeenAt;
        LastSeenIpAddress = Normalize(ipAddress);
        LastSeenDevice = Normalize(device);
        LastSeenBrowser = Normalize(browser);
    }

    public void ForceLogout(DateTime forceLogoutAfter)
    {
        ForceLogoutAfter = forceLogoutAfter;
        LastSeenAt = forceLogoutAfter.Subtract(TimeSpan.FromHours(1));
    }

    public void ClearForceLogout()
    {
        ForceLogoutAfter = null;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
