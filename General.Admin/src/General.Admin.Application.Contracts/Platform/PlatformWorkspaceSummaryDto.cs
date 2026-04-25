namespace General.Admin.Platform;

public class PlatformWorkspaceSummaryDto
{
    public int ExceptionLogCount { get; set; }

    public int FileCount { get; set; }

    public int OnlineUserCount { get; set; }

    public int OrganizationCount { get; set; }

    public int RoleCount { get; set; }

    public int TenantCount { get; set; }

    public List<PlatformWorkspaceAttentionItemDto> AttentionItems { get; set; } = [];

    public int UpdateLogCount { get; set; }

    public int UserCount { get; set; }
}

public class PlatformWorkspaceAttentionItemDto
{
    public string Detail { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    public string Level { get; set; } = "info";

    public string? Link { get; set; }

    public string Title { get; set; } = string.Empty;
}
