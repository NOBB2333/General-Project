namespace General.Admin.Platform;

public class PlatformCacheAreaDto
{
    public string Area { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;
}

public class PlatformCacheRefreshInput
{
    public string Area { get; set; } = string.Empty;
}
