using Volo.Abp.Domain.Services;
using Volo.Abp.Security.Encryption;

namespace General.Admin.Platform;

public class PlatformFileStorageSourceManager : DomainService
{
    private readonly IStringEncryptionService _encryptionService;

    public PlatformFileStorageSourceManager(IStringEncryptionService encryptionService)
    {
        _encryptionService = encryptionService;
    }

    public string? EncryptSecret(string? secret)
    {
        return string.IsNullOrWhiteSpace(secret)
            ? null
            : _encryptionService.Encrypt(secret.Trim());
    }

    public PlatformFileStorageSourceDescriptor ToDescriptor(AppFileStorageSource source)
    {
        return new PlatformFileStorageSourceDescriptor
        {
            AccessKeyId = source.AccessKeyId,
            AccessKeySecret = DecryptSecret(source.EncryptedSecret),
            BucketName = source.BucketName,
            CustomDomain = source.CustomDomain,
            Endpoint = source.Endpoint,
            IsPublic = source.IsPublic,
            Name = source.Name,
            PathTemplate = source.PathTemplate,
            ProviderName = source.ProviderName,
            Region = source.Region,
            RootPath = source.RootPath,
            SourceId = source.Id,
            UseSsl = source.UseSsl
        };
    }

    private string DecryptSecret(string? encryptedSecret)
    {
        return string.IsNullOrWhiteSpace(encryptedSecret)
            ? string.Empty
            : _encryptionService.Decrypt(encryptedSecret) ?? string.Empty;
    }
}
