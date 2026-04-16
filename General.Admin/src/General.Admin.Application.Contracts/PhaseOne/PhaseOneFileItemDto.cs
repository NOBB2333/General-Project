namespace General.Admin.PhaseOne;

public class PhaseOneFileItemDto
{
    public string ContentType { get; set; } = string.Empty;

    public string FileKey { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public long Size { get; set; }

    public DateTime UploadedAt { get; set; }
}
