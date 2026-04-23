using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.TenantManagement;

namespace General.Admin.PhaseOne;

public class PhaseOneOnlineUserService : ITransientDependency
{
    private readonly PhaseOneUserActivityService _userActivityService;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PhaseOneOnlineUserService(
        PhaseOneUserActivityService userActivityService,
        ITenantRepository tenantRepository,
        IRepository<AppUserProfile, Guid> userProfileRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _userActivityService = userActivityService;
        _tenantRepository = tenantRepository;
        _userProfileRepository = userProfileRepository;
        _userRepository = userRepository;
    }

    public async Task<List<PhaseOneOnlineUserDto>> GetListAsync()
    {
        var profiles = (await _userProfileRepository.GetListAsync())
            .Where(x => PhaseOneUserActivityService.IsOnline(x.LastSeenAt))
            .OrderByDescending(x => x.LastSeenAt ?? x.LastLoginTime)
            .ToList();
        var users = (await _userRepository.GetListAsync())
            .ToDictionary(x => x.Id);
        var tenants = (await _tenantRepository.GetListAsync())
            .ToDictionary(x => x.Id, x => x.Name);

        return profiles
            .Select(profile =>
            {
                users.TryGetValue(profile.UserId, out var user);
                return new PhaseOneOnlineUserDto
                {
                    Browser = profile.LastSeenBrowser ?? string.Empty,
                    CurrentTenantId = user?.TenantId,
                    Device = profile.LastSeenDevice ?? string.Empty,
                    Id = profile.Id,
                    IpAddress = profile.LastSeenIpAddress ?? string.Empty,
                    LastAccessedAt = profile.LastSeenAt,
                    SignedInAt = profile.LastLoginTime,
                    TenantName = user?.TenantId.HasValue == true ? tenants.GetValueOrDefault(user.TenantId.Value) : null,
                    CanForceLogout = true,
                    UserId = profile.UserId,
                    UserName = user?.UserName ?? profile.UserId.ToString("N")
                };
            })
            .ToList();
    }

    public async Task ForceLogoutAsync(Guid userId)
    {
        await _userActivityService.ForceLogoutAsync(userId);
    }
}
