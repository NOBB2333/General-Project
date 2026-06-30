using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// MCP 工具定义快照。由 MCP Server 发现后落库，可单独启停和绑定权限。
/// </summary>
public class AiMcpTool : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string Description { get; private set; }

    public string InputSchemaJson { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTime LastDiscoveredTime { get; private set; }

    public string PermissionCode { get; private set; }

    public Guid ServerId { get; private set; }

    public Guid? TenantId { get; private set; }

    public string ToolName { get; private set; }

    protected AiMcpTool()
    {
        Description = string.Empty;
        InputSchemaJson = "{}";
        PermissionCode = string.Empty;
        ToolName = string.Empty;
    }

    public AiMcpTool(
        Guid id,
        Guid serverId,
        string toolName,
        string? description,
        string? inputSchemaJson,
        Guid? tenantId,
        string? permissionCode = null,
        bool isEnabled = true) : base(id)
    {
        ServerId = serverId;
        ToolName = Check.NotNullOrWhiteSpace(toolName, nameof(toolName), 128).Trim();
        Description = Normalize(description, 1000);
        InputSchemaJson = NormalizeJson(inputSchemaJson);
        TenantId = tenantId;
        PermissionCode = Normalize(permissionCode, 128);
        IsEnabled = isEnabled;
        LastDiscoveredTime = DateTime.UtcNow;
    }

    public void Refresh(string? description, string? inputSchemaJson)
    {
        Description = Normalize(description, 1000);
        InputSchemaJson = NormalizeJson(inputSchemaJson);
        LastDiscoveredTime = DateTime.UtcNow;
    }

    public void UpdatePolicy(bool isEnabled, string? permissionCode)
    {
        IsEnabled = isEnabled;
        PermissionCode = Normalize(permissionCode, 128);
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
