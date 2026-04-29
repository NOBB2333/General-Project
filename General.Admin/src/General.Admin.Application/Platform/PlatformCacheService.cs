using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Platform;

public class PlatformCacheService : ITransientDependency
{
    private readonly IDistributedCache _distributedCache;

    public PlatformCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<string> BuildVersionedKeyAsync(string area, string key)
    {
        return $"platform:{area}:{await GetVersionAsync(area)}:{key}";
    }

    public Task InvalidateAsync(string area)
    {
        return _distributedCache.SetStringAsync(BuildVersionKey(area), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
    }

    private async Task<string> GetVersionAsync(string area)
    {
        var versionKey = BuildVersionKey(area);
        var version = await _distributedCache.GetStringAsync(versionKey);
        if (!string.IsNullOrWhiteSpace(version))
        {
            return version;
        }

        version = "1";
        await _distributedCache.SetStringAsync(versionKey, version);
        return version;
    }

    private static string BuildVersionKey(string area)
    {
        return $"platform:{area}:version";
    }
}
