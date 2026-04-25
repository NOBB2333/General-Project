namespace General.Admin.Platform;

public class PlatformFileListInput
{
    public string? Category { get; set; }

    public string? Keyword { get; set; }

    public DateTime? UploadedFrom { get; set; }

    public DateTime? UploadedTo { get; set; }

    public string? UploadedBy { get; set; }
}
