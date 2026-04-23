using Microsoft.AspNetCore.Http;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PhaseOneUserActivityService : ITransientDependency
{
    private static readonly TimeSpan OnlineThreshold = TimeSpan.FromMinutes(20);

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;

    public PhaseOneUserActivityService(
        IHttpContextAccessor httpContextAccessor,
        ICurrentUser currentUser,
        IRepository<AppUserProfile, Guid> userProfileRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
        _userProfileRepository = userProfileRepository;
    }

    public async Task MarkLoginAsync(Guid userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        var now = DateTime.UtcNow;
        var context = _httpContextAccessor.HttpContext;

        profile.ClearForceLogout();
        profile.SetLastLoginTime(now);
        profile.UpdateLastSeen(
            now,
            context?.Connection.RemoteIpAddress?.ToString(),
            ResolveDevice(context),
            ResolveBrowser(context));

        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    public async Task TouchCurrentUserAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return;
        }

        var profile = await GetOrCreateProfileAsync(_currentUser.Id.Value);
        var now = DateTime.UtcNow;
        if (profile.LastSeenAt.HasValue && now - profile.LastSeenAt.Value < TimeSpan.FromSeconds(20))
        {
            return;
        }

        var context = _httpContextAccessor.HttpContext;
        profile.UpdateLastSeen(
            now,
            context?.Connection.RemoteIpAddress?.ToString(),
            ResolveDevice(context),
            ResolveBrowser(context));

        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    public async Task<bool> IsCurrentUserForcedLogoutAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return false;
        }

        var profile = await _userProfileRepository.FindAsync(x => x.UserId == _currentUser.Id.Value);
        return profile?.ForceLogoutAfter.HasValue == true;
    }

    public async Task ForceLogoutAsync(Guid userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        profile.ForceLogout(DateTime.UtcNow);
        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
    }

    public static bool IsOnline(DateTime? lastSeenAt)
    {
        return lastSeenAt.HasValue && DateTime.UtcNow - lastSeenAt.Value <= OnlineThreshold;
    }

    private async Task<AppUserProfile> GetOrCreateProfileAsync(Guid userId)
    {
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);
        if (profile != null)
        {
            return profile;
        }

        profile = new AppUserProfile(Guid.NewGuid(), userId, null, null, null, null, null);
        await _userProfileRepository.InsertAsync(profile, autoSave: true);
        return profile;
    }

    private static string? ResolveBrowser(HttpContext? context)
    {
        return context?.Request.Headers.UserAgent.ToString();
    }

    private static string? ResolveDevice(HttpContext? context)
    {
        var userAgent = ResolveBrowser(context);
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return null;
        }

        if (userAgent.Contains("Mobile", StringComparison.OrdinalIgnoreCase))
        {
            return "Mobile";
        }

        if (userAgent.Contains("Macintosh", StringComparison.OrdinalIgnoreCase))
        {
            return "Mac";
        }

        if (userAgent.Contains("Windows", StringComparison.OrdinalIgnoreCase))
        {
            return "Windows";
        }

        if (userAgent.Contains("Linux", StringComparison.OrdinalIgnoreCase))
        {
            return "Linux";
        }

        return "Web";
    }
}
