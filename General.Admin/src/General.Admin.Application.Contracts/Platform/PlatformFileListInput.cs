namespace General.Admin.Platform;

public class PlatformFileListInput
{
    public string? Category { get; set; }

    public string? CategoryPrefix { get; set; }

    public string? BusinessType { get; set; }

    public string? BusinessId { get; set; }

    public string? BusinessKeyword { get; set; }

    public Guid? BusinessProjectId { get; set; }

    public string? BusinessScope { get; set; }

    public string? Keyword { get; set; }

    public string? ParentPath { get; set; }

    public Guid? StorageSourceId { get; set; }

    public DateTime? UploadedFrom { get; set; }

    public DateTime? UploadedTo { get; set; }

    public string? UploadedBy { get; set; }

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }
}
