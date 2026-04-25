namespace General.Admin.Platform;

public class PlatformEndpointOptionDto
{
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string? CapabilityKey { get; set; }
}

public class PlatformEndpointGroupDto
{
    public string GroupName { get; set; } = string.Empty;

    public List<PlatformEndpointOptionDto> Items { get; set; } = [];
}
