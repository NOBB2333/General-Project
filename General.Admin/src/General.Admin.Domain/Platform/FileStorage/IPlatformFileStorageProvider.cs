using System.IO;
using System.Threading;

namespace General.Admin.Platform;

public interface IPlatformFileStorageProvider
{
    string ProviderName { get; }

    Task<PlatformFileStorageSaveResult> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        string category,
        string? parentPath,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string fileKey,
        string storageLocation,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string fileKey,
        string storageLocation,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default);

    Task<string?> GetPublicUrlAsync(
        string fileKey,
        string storageLocation,
        TimeSpan expiry,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default);
}
