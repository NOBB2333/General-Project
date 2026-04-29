namespace General.Admin.Platform;

public class PlatformRecycleBinItemDto
{
    public DateTime? DeletionTime { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;
}
