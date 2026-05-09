namespace General.Admin.Platform;

public class PlatformRecycleBinItemDto
{
    public DateTime? DeletionTime { get; set; }

    public Guid? DeleterId { get; set; }

    public string? DeleterName { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public string OriginalLocation { get; set; } = string.Empty;
}

public class PlatformRecycleBinListInput
{
    public string? EntityType { get; set; }

    public int MaxResultCount { get; set; } = 20;

    public int SkipCount { get; set; }
}
