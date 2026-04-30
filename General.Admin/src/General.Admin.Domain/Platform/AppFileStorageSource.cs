using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppFileStorageSource : FullAuditedAggregateRoot<Guid>
{
    public string AccessKeyId { get; private set; }

    public string? BucketName { get; private set; }

    public string? CustomDomain { get; private set; }

    public string? EncryptedSecret { get; private set; }

    public string? Endpoint { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsEnabled { get; private set; }

    public bool IsPublic { get; private set; }

    public string Name { get; private set; }

    public string? PathTemplate { get; private set; }

    public string ProviderName { get; private set; }

    public string? Region { get; private set; }

    public string? Remark { get; private set; }

    public string? RootPath { get; private set; }

    public bool UseSsl { get; private set; }

    protected AppFileStorageSource()
    {
        AccessKeyId = string.Empty;
        Name = string.Empty;
        ProviderName = PlatformFileStorageNames.Local;
        IsEnabled = true;
    }

    public AppFileStorageSource(
        Guid id,
        string name,
        string providerName,
        string? endpoint,
        string? rootPath,
        string accessKeyId,
        string? encryptedSecret,
        string? bucketName,
        string? region,
        string? customDomain,
        string? pathTemplate,
        bool useSsl,
        bool isDefault,
        bool isEnabled,
        bool isPublic,
        string? remark) : base(id)
    {
        Name = NormalizeRequired(name, 128);
        ProviderName = NormalizeRequired(providerName, 32);
        Endpoint = Normalize(endpoint, 512);
        RootPath = Normalize(rootPath, 512);
        AccessKeyId = Normalize(accessKeyId, 256) ?? string.Empty;
        EncryptedSecret = Normalize(encryptedSecret, 2048);
        BucketName = Normalize(bucketName, 128);
        Region = Normalize(region, 128);
        CustomDomain = Normalize(customDomain, 512);
        PathTemplate = Normalize(pathTemplate, 128);
        UseSsl = useSsl;
        IsDefault = isDefault;
        IsEnabled = isEnabled;
        IsPublic = isPublic;
        Remark = Normalize(remark, 256);
    }

    public void Update(
        string name,
        string providerName,
        string? endpoint,
        string? rootPath,
        string accessKeyId,
        string? encryptedSecret,
        string? bucketName,
        string? region,
        string? customDomain,
        string? pathTemplate,
        bool useSsl,
        bool isEnabled,
        bool isPublic,
        string? remark)
    {
        Name = NormalizeRequired(name, 128);
        ProviderName = NormalizeRequired(providerName, 32);
        Endpoint = Normalize(endpoint, 512);
        RootPath = Normalize(rootPath, 512);
        AccessKeyId = Normalize(accessKeyId, 256) ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(encryptedSecret))
        {
            EncryptedSecret = Normalize(encryptedSecret, 2048);
        }
        BucketName = Normalize(bucketName, 128);
        Region = Normalize(region, 128);
        CustomDomain = Normalize(customDomain, 512);
        PathTemplate = Normalize(pathTemplate, 128);
        UseSsl = useSsl;
        IsEnabled = isEnabled;
        IsPublic = isPublic;
        Remark = Normalize(remark, 256);
    }

    public void SetDefault(bool isDefault)
    {
        IsDefault = isDefault;
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    private static string NormalizeRequired(string value, int maxLength)
    {
        return Check.NotNullOrWhiteSpace(value, nameof(value), maxLength).Trim();
    }

    private static string? Normalize(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
