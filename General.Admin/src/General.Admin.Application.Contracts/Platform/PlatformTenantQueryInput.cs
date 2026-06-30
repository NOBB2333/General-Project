namespace General.Admin.Platform;

public class PlatformTenantQueryInput
{
    public int SkipCount { get; set; }

    public int MaxResultCount { get; set; } = 20;
}
