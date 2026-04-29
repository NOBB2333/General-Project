namespace General.Admin.Platform;

public class PlatformFileStorageSaveResult
{
    public PlatformFileStorageSaveResult(string provider, string fileKey, string storageLocation)
    {
        Provider = Check.NotNullOrWhiteSpace(provider, nameof(provider), 32);
        FileKey = Check.NotNullOrWhiteSpace(fileKey, nameof(fileKey), 256);
        StorageLocation = Check.NotNullOrWhiteSpace(storageLocation, nameof(storageLocation), 512);
    }

    public string Provider { get; }

    public string FileKey { get; }

    public string StorageLocation { get; }
}
