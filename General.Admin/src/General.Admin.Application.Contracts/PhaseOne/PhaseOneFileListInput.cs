namespace General.Admin.PhaseOne;

public class PhaseOneFileListInput
{
    public string? Category { get; set; }

    public string? Keyword { get; set; }

    public DateTime? UploadedFrom { get; set; }

    public DateTime? UploadedTo { get; set; }

    public string? UploadedBy { get; set; }
}
