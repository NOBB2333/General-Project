using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Platform;

public class PlatformCacheService : ITransientDependency
{
    private static readonly IReadOnlyDictionary<string, string> ManagedAreas = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["authorization"] = "角色、租户和接口访问授权缓存",
        ["dict"] = "开放字典项缓存",
        ["menu"] = "菜单、路由和权限树缓存",
        ["notification"] = "通知相关缓存",
        ["organization"] = "组织架构与成员范围缓存",
        ["role"] = "角色与用户关系缓存",
        ["tenant"] = "租户授权与租户状态缓存"
    };

    private readonly IDistributedCache _distributedCache;

    public PlatformCacheService(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<string> BuildVersionedKeyAsync(string area, string key)
    {
        return $"platform:{area}:{await GetVersionAsync(area)}:{key}";
    }

    public async Task<List<PlatformCacheAreaDto>> GetAreasAsync()
    {
        var result = new List<PlatformCacheAreaDto>();
        foreach (var area in ManagedAreas.OrderBy(x => x.Key))
        {
            result.Add(new PlatformCacheAreaDto
            {
                Area = area.Key,
                Description = area.Value,
                Version = await GetVersionAsync(area.Key)
            });
        }

        return result;
    }

    public Task InvalidateAsync(string area)
    {
        EnsureManagedArea(area);
        return _distributedCache.SetStringAsync(BuildVersionKey(area), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString());
    }

    public Task RefreshAsync(string area)
    {
        return InvalidateAsync(area);
    }

    private static void EnsureManagedArea(string area)
    {
        if (!ManagedAreas.ContainsKey(area))
        {
            throw new InvalidOperationException("缓存域不存在或不允许后台刷新。");
        }
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
