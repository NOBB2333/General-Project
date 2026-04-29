using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Infrastructure;

public class PlatformFileStorageProviderResolver : IPlatformFileStorageProviderResolver, ITransientDependency
{
    private readonly IReadOnlyDictionary<string, IPlatformFileStorageProvider> _providers;
    private readonly PlatformFileStorageOptions _options;

    public PlatformFileStorageProviderResolver(
        IEnumerable<IPlatformFileStorageProvider> providers,
        IOptions<PlatformFileStorageOptions> options)
    {
        _providers = providers.ToDictionary(x => x.ProviderName, StringComparer.OrdinalIgnoreCase);
        _options = options.Value;
    }

    public IPlatformFileStorageProvider Resolve(string? providerName = null)
    {
        var normalizedProvider = string.IsNullOrWhiteSpace(providerName)
            ? _options.Provider
            : providerName.Trim();

        if (string.IsNullOrWhiteSpace(normalizedProvider))
        {
            normalizedProvider = PlatformFileStorageNames.Local;
        }

        if (_providers.TryGetValue(normalizedProvider, out var provider))
        {
            return provider;
        }

        throw new InvalidOperationException($"文件存储提供器未注册：{normalizedProvider}。");
    }
}
