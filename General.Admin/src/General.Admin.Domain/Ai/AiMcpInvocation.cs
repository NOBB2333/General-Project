using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// MCP 工具调用日志。用于追踪外部 MCP 工具的输入、输出、耗时和错误。
/// </summary>
public class AiMcpInvocation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public long ElapsedMilliseconds { get; private set; }

    public string ErrorMessage { get; private set; }

    public string InputJson { get; private set; }

    public string OutputJson { get; private set; }

    public Guid ServerId { get; private set; }

    public string Status { get; private set; }

    public Guid? TenantId { get; private set; }

    public int TimeoutSeconds { get; private set; }

    public Guid? ToolId { get; private set; }

    public string ToolName { get; private set; }

    protected AiMcpInvocation()
    {
        ErrorMessage = string.Empty;
        InputJson = "{}";
        OutputJson = "{}";
        Status = string.Empty;
        ToolName = string.Empty;
    }

    public AiMcpInvocation(
        Guid id,
        Guid serverId,
        Guid? toolId,
        string toolName,
        string? inputJson,
        int timeoutSeconds,
        Guid? tenantId) : base(id)
    {
        ServerId = serverId;
        ToolId = toolId;
        ToolName = Check.NotNullOrWhiteSpace(toolName, nameof(toolName), 128).Trim();
        InputJson = NormalizeJson(inputJson, "{}");
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 180);
        TenantId = tenantId;
        OutputJson = "{}";
        ErrorMessage = string.Empty;
        Status = "Running";
    }

    public void Complete(string? outputJson, long elapsedMilliseconds)
    {
        OutputJson = NormalizeJson(outputJson, "{}");
        ErrorMessage = string.Empty;
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
        Status = "Completed";
    }

    public void Fail(string status, string? errorMessage, long elapsedMilliseconds)
    {
        Status = Normalize(status, 32, "Failed");
        ErrorMessage = Normalize(errorMessage, 4000);
        ElapsedMilliseconds = Math.Max(0, elapsedMilliseconds);
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJson(string? value, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= 64_000 ? normalized : normalized[..64_000];
    }
}
