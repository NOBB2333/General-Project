using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PhaseOneUserActivityService : ITransientDependency
{
    private static readonly TimeSpan OnlineThreshold = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan TouchThrottle = TimeSpan.FromSeconds(20);
    private static readonly TimeSpan ActivityCacheTtl = TimeSpan.FromHours(8);
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUser _currentUser;
    private readonly IDistributedCache _distributedCache;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;

    public PhaseOneUserActivityService(
        IHttpContextAccessor httpContextAccessor,
        ICurrentUser currentUser,
        IDistributedCache distributedCache,
        IRepository<AppUserProfile, Guid> userProfileRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
        _distributedCache = distributedCache;
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
        await SetCacheAsync(userId, new PhaseOneUserActivityCacheItem
        {
            ForceLogoutAfter = profile.ForceLogoutAfter,
            LastSeenAt = now
        });
    }

    public async Task TouchCurrentUserAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var cacheItem = await GetCacheAsync(_currentUser.Id.Value) ?? await LoadCacheAsync(_currentUser.Id.Value);
        if (cacheItem.LastSeenAt.HasValue && now - cacheItem.LastSeenAt.Value < TouchThrottle)
        {
            return;
        }

        var context = _httpContextAccessor.HttpContext;
        var profile = await GetOrCreateProfileAsync(_currentUser.Id.Value);
        profile.UpdateLastSeen(
            now,
            context?.Connection.RemoteIpAddress?.ToString(),
            ResolveDevice(context),
            ResolveBrowser(context));

        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
        cacheItem.LastSeenAt = now;
        await SetCacheAsync(_currentUser.Id.Value, cacheItem);
    }

    public async Task<bool> IsCurrentUserForcedLogoutAsync()
    {
        if (!_currentUser.Id.HasValue)
        {
            return false;
        }

        var cacheItem = await GetCacheAsync(_currentUser.Id.Value) ?? await LoadCacheAsync(_currentUser.Id.Value);
        return cacheItem.ForceLogoutAfter.HasValue;
    }

    public async Task ForceLogoutAsync(Guid userId)
    {
        var profile = await GetOrCreateProfileAsync(userId);
        profile.ForceLogout(DateTime.UtcNow);
        await _userProfileRepository.UpdateAsync(profile, autoSave: true);
        await SetCacheAsync(userId, new PhaseOneUserActivityCacheItem
        {
            ForceLogoutAfter = profile.ForceLogoutAfter,
            LastSeenAt = profile.LastSeenAt
        });
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

    private async Task<PhaseOneUserActivityCacheItem?> GetCacheAsync(Guid userId)
    {
        var json = await _distributedCache.GetStringAsync(GetCacheKey(userId));
        return string.IsNullOrWhiteSpace(json)
            ? null
            : JsonSerializer.Deserialize<PhaseOneUserActivityCacheItem>(json, JsonSerializerOptions);
    }

    private async Task<PhaseOneUserActivityCacheItem> LoadCacheAsync(Guid userId)
    {
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == userId);
        var cacheItem = new PhaseOneUserActivityCacheItem
        {
            ForceLogoutAfter = profile?.ForceLogoutAfter,
            LastSeenAt = profile?.LastSeenAt
        };

        await SetCacheAsync(userId, cacheItem);
        return cacheItem;
    }

    private async Task SetCacheAsync(Guid userId, PhaseOneUserActivityCacheItem item)
    {
        var json = JsonSerializer.Serialize(item, JsonSerializerOptions);
        await _distributedCache.SetStringAsync(
            GetCacheKey(userId),
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ActivityCacheTtl
            });
    }

    private static string GetCacheKey(Guid userId)
    {
        return $"phase-one:user-activity:{userId:N}";
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

    private sealed class PhaseOneUserActivityCacheItem
    {
        public DateTime? ForceLogoutAfter { get; set; }

        public DateTime? LastSeenAt { get; set; }
    }
}
