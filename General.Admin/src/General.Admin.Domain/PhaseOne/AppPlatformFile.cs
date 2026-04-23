using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.PhaseOne;

public class AppPlatformFile : FullAuditedAggregateRoot<Guid>
{
    public string Category { get; private set; }

    public string ContentType { get; private set; }

    public string FileKey { get; private set; }

    public string FileName { get; private set; }

    public string? ParentPath { get; private set; }

    public long Size { get; private set; }

    public string StorageLocation { get; private set; }

    public Guid? UploadedByUserId { get; private set; }

    protected AppPlatformFile()
    {
        Category = "default";
        ContentType = string.Empty;
        FileKey = string.Empty;
        FileName = string.Empty;
        StorageLocation = string.Empty;
    }

    public AppPlatformFile(
        Guid id,
        string fileKey,
        string fileName,
        string contentType,
        long size,
        string category,
        string? parentPath,
        string storageLocation,
        Guid? uploadedByUserId) : base(id)
    {
        FileKey = Check.NotNullOrWhiteSpace(fileKey, nameof(fileKey), 256);
        FileName = Check.NotNullOrWhiteSpace(fileName, nameof(fileName), 256);
        ContentType = Check.NotNullOrWhiteSpace(contentType, nameof(contentType), 256);
        Size = size;
        Category = Normalize(category, 64) ?? "default";
        ParentPath = Normalize(parentPath, 256);
        StorageLocation = Check.NotNullOrWhiteSpace(storageLocation, nameof(storageLocation), 512);
        UploadedByUserId = uploadedByUserId;
    }

    public void UpdateCategory(string category, string? parentPath)
    {
        Category = Normalize(category, 64) ?? "default";
        ParentPath = Normalize(parentPath, 256);
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
