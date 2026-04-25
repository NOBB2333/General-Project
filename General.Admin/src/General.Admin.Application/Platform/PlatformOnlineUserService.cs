using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.TenantManagement;

namespace General.Admin.Platform;

public class PlatformOnlineUserService : ITransientDependency
{
    private readonly PlatformUserActivityService _userActivityService;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PlatformOnlineUserService(
        PlatformUserActivityService userActivityService,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        ITenantRepository tenantRepository,
        IRepository<AppUserProfile, Guid> userProfileRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _userActivityService = userActivityService;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _tenantRepository = tenantRepository;
        _userProfileRepository = userProfileRepository;
        _userRepository = userRepository;
    }

    public async Task<List<PlatformOnlineUserDto>> GetListAsync()
    {
        var threshold = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(20));
        var profilesQueryable = await _userProfileRepository.GetQueryableAsync();
        var profiles = await _asyncQueryableExecuter.ToListAsync(
            profilesQueryable
                .Where(x => x.LastSeenAt.HasValue && x.LastSeenAt.Value >= threshold)
                .OrderByDescending(x => x.LastSeenAt ?? x.LastLoginTime));

        if (profiles.Count == 0)
        {
            return [];
        }

        var userIds = profiles.Select(x => x.UserId).Distinct().ToList();
        var usersQueryable = await _userRepository.GetQueryableAsync();
        var users = (await _asyncQueryableExecuter.ToListAsync(usersQueryable.Where(x => userIds.Contains(x.Id))))
            .ToDictionary(x => x.Id);
        var tenantIds = users.Values.Where(x => x.TenantId.HasValue).Select(x => x.TenantId!.Value).Distinct().ToList();
        var tenants = tenantIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _tenantRepository.GetListAsync()).Where(x => tenantIds.Contains(x.Id)).ToDictionary(x => x.Id, x => x.Name);

        return profiles
            .Select(profile =>
            {
                users.TryGetValue(profile.UserId, out var user);
                return new PlatformOnlineUserDto
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
