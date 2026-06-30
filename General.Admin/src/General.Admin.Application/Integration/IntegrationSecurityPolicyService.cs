using System.Net;
using General.Admin.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace General.Admin.Integration;

public class IntegrationSecurityPolicyService : ITransientDependency
{
    private readonly ISettingProvider _settingProvider;

    public IntegrationSecurityPolicyService(ISettingProvider settingProvider)
    {
        _settingProvider = settingProvider;
    }

    public async Task CheckWebhookSourceAsync(IntegrationWebhookReceiveInput input)
    {
        var whitelist = await _settingProvider.GetOrNullAsync(AdminSettings.IntegrationWebhookIpWhitelist);
        if (string.IsNullOrWhiteSpace(whitelist))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(input.SourceIp) || !IsAllowed(input.SourceIp.Trim(), whitelist))
        {
            throw new UserFriendlyException("Webhook 来源 IP 不在白名单内。");
        }
    }

    private static bool IsAllowed(string sourceIp, string whitelist)
    {
        return whitelist
            .Split([',', ';', '\n', '\r', '\t', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(item =>
                item == "*" ||
                IsExactIpMatch(sourceIp, item) ||
                IsCidrMatch(sourceIp, item));
    }

    private static bool IsExactIpMatch(string sourceIp, string item)
    {
        return IPAddress.TryParse(sourceIp, out var source) &&
               IPAddress.TryParse(item, out var allowed) &&
               source.Equals(allowed);
    }

    private static bool IsCidrMatch(string sourceIp, string item)
    {
        var parts = item.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 ||
            !IPAddress.TryParse(sourceIp, out var source) ||
            !IPAddress.TryParse(parts[0], out var network) ||
            !int.TryParse(parts[1], out var prefixLength))
        {
            return false;
        }

        var sourceBytes = source.GetAddressBytes();
        var networkBytes = network.GetAddressBytes();
        if (sourceBytes.Length != networkBytes.Length || prefixLength < 0 || prefixLength > sourceBytes.Length * 8)
        {
            return false;
        }

        var fullBytes = prefixLength / 8;
        var remainingBits = prefixLength % 8;
        for (var i = 0; i < fullBytes; i++)
        {
            if (sourceBytes[i] != networkBytes[i])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)(0xFF << (8 - remainingBits));
        return (sourceBytes[fullBytes] & mask) == (networkBytes[fullBytes] & mask);
    }
}
