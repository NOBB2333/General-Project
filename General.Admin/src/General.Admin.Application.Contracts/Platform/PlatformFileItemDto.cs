namespace General.Admin.Platform;

public class PlatformFileItemDto
{
    public string Category { get; set; } = "default";

    public string ContentType { get; set; } = string.Empty;

    public string? BusinessId { get; set; }

    public string? BusinessType { get; set; }

    public string? BucketName { get; set; }

    public string FileKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? ParentPath { get; set; }

    public string RelativePath { get; set; } = string.Empty;

    public long Size { get; set; }

    public bool IsPublic { get; set; }

    public string StorageLocation { get; set; } = string.Empty;

    public string StorageProvider { get; set; } = string.Empty;

    public Guid? StorageSourceId { get; set; }

    public string? StorageSourceName { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }
}

public class PlatformFileTreeItemDto
{
    public string Category { get; set; } = "default";

    public string? ParentPath { get; set; }
}

public class PlatformFileMetadataInput
{
    public string FileName { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public string? BusinessId { get; set; }

    public string? BusinessType { get; set; }
}

public class PlatformFileBatchDeleteInput
{
    public List<string> FileKeys { get; set; } = [];
}
