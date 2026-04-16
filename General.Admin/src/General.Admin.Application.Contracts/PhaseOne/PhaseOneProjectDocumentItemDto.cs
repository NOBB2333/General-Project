namespace General.Admin.PhaseOne;

public class PhaseOneProjectDocumentItemDto
{
    public string Category { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}
