using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using General.Admin.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace General.Admin.Integration;

public class IntegrationRateLimitService : ITransientDependency
{
    private const int DefaultMessageLimitPerMinute = 60;
    private const int DefaultWebhookLimitPerMinute = 120;
    private static readonly ConcurrentDictionary<string, RateLimitCounter> Counters = new();

    private readonly ICurrentTenant _currentTenant;
    private readonly ISettingProvider _settingProvider;

    public IntegrationRateLimitService(ICurrentTenant currentTenant, ISettingProvider settingProvider)
    {
        _currentTenant = currentTenant;
        _settingProvider = settingProvider;
    }

    public async Task CheckMessageSendAsync(IntegrationMessageSendInput input)
    {
        var limit = await GetLimitAsync(
            AdminSettings.IntegrationMessageSendLimitPerMinute,
            DefaultMessageLimitPerMinute);
        CheckRateLimit(
            "message",
            $"{input.ConnectorId:N}|{input.TargetType}|{input.TargetId}",
            limit,
            "外部系统消息发送");
    }

    public async Task CheckWebhookRequestAsync(IntegrationWebhookReceiveInput input)
    {
        var limit = await GetLimitAsync(
            AdminSettings.IntegrationWebhookRequestLimitPerMinute,
            DefaultWebhookLimitPerMinute);
        CheckRateLimit(
            "webhook",
            $"{input.ProviderType}|{input.ConnectorCode}|{input.EventType}|{input.SourceIp}",
            limit,
            "外部系统回调请求");
    }

    private async Task<int> GetLimitAsync(string settingName, int fallback)
    {
        var value = await _settingProvider.GetOrNullAsync(settingName);
        return int.TryParse(value, out var limit) && limit >= 0 ? limit : fallback;
    }

    private void CheckRateLimit(string scope, string rawKey, int limit, string displayName)
    {
        if (limit <= 0)
        {
            return;
        }

        var window = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 60;
        CleanupExpiredCounters(window);

        var cacheKey = BuildCacheKey(scope, rawKey, window);
        var counter = Counters.AddOrUpdate(
            cacheKey,
            _ => new RateLimitCounter(window, 1),
            (_, current) => current.Window == window
                ? current with { Count = current.Count + 1 }
                : new RateLimitCounter(window, 1));

        if (counter.Count > limit)
        {
            throw new UserFriendlyException($"{displayName}超过频率限制（每分钟 {limit} 次），请稍后重试。");
        }
    }

    private void CleanupExpiredCounters(long currentWindow)
    {
        if (Counters.Count < 5000)
        {
            return;
        }

        foreach (var item in Counters)
        {
            if (item.Value.Window < currentWindow - 2)
            {
                Counters.TryRemove(item.Key, out _);
            }
        }
    }

    private string BuildCacheKey(string scope, string rawKey, long window)
    {
        var tenantKey = _currentTenant.Id?.ToString("N") ?? "host";
        var normalizedKey = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey.Trim())));
        return $"integration-rate:{scope}:{tenantKey}:{window}:{normalizedKey}";
    }

    private sealed record RateLimitCounter(long Window, int Count);
}
