namespace General.Admin.PhaseOne;

public class PhaseOneFileItemDto
{
    public string Category { get; set; } = "default";

    public string ContentType { get; set; } = string.Empty;

    public string FileKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? ParentPath { get; set; }

    public long Size { get; set; }

    public string StorageLocation { get; set; } = string.Empty;

    public string? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }
}
