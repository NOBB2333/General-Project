using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI MCP Server 配置。支持 stdio、SSE 和 Streamable HTTP 三类传输。
/// </summary>
public class AiMcpServer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string ArgumentsJson { get; private set; }

    public string Command { get; private set; }

    public string ConfigJson { get; private set; }

    public string Endpoint { get; private set; }

    public string EnvironmentJson { get; private set; }

    public string HeadersJson { get; private set; }

    public bool IsEnabled { get; private set; }

    public DateTime? LastDiscoveryTime { get; private set; }

    public string LastErrorMessage { get; private set; }

    public string Name { get; private set; }

    public string PermissionCode { get; private set; }

    public string ProtocolVersion { get; private set; }

    public string Remark { get; private set; }

    public string ServerCode { get; private set; }

    public int Sort { get; private set; }

    public Guid? TenantId { get; private set; }

    public int TimeoutSeconds { get; private set; }

    public string TransportType { get; private set; }

    protected AiMcpServer()
    {
        ArgumentsJson = "[]";
        Command = string.Empty;
        ConfigJson = "{}";
        Endpoint = string.Empty;
        EnvironmentJson = "{}";
        HeadersJson = "{}";
        LastErrorMessage = string.Empty;
        Name = string.Empty;
        PermissionCode = string.Empty;
        ProtocolVersion = "2024-11-05";
        Remark = string.Empty;
        ServerCode = string.Empty;
        TransportType = "StreamableHttp";
    }

    public AiMcpServer(
        Guid id,
        string serverCode,
        string name,
        string transportType,
        Guid? tenantId,
        string? endpoint = null,
        string? command = null,
        string? argumentsJson = null,
        string? headersJson = null,
        string? environmentJson = null,
        string? configJson = null,
        string? protocolVersion = null,
        string? permissionCode = null,
        bool isEnabled = true,
        int timeoutSeconds = 30,
        int sort = 0,
        string? remark = null) : base(id)
    {
        ServerCode = Check.NotNullOrWhiteSpace(serverCode, nameof(serverCode), 64).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        TransportType = NormalizeTransport(transportType);
        TenantId = tenantId;
        Endpoint = Normalize(endpoint, 512);
        Command = Normalize(command, 512);
        ArgumentsJson = NormalizeJsonArray(argumentsJson);
        HeadersJson = NormalizeJson(headersJson);
        EnvironmentJson = NormalizeJson(environmentJson);
        ConfigJson = NormalizeJson(configJson);
        ProtocolVersion = Normalize(protocolVersion, 32, "2024-11-05");
        PermissionCode = Normalize(permissionCode, 128);
        IsEnabled = isEnabled;
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 180);
        Sort = sort;
        Remark = Normalize(remark, 512);
        LastErrorMessage = string.Empty;
    }

    public void Update(
        string name,
        string transportType,
        string? endpoint,
        string? command,
        string? argumentsJson,
        string? headersJson,
        string? environmentJson,
        string? configJson,
        string? protocolVersion,
        string? permissionCode,
        bool isEnabled,
        int timeoutSeconds,
        int sort,
        string? remark)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        TransportType = NormalizeTransport(transportType);
        Endpoint = Normalize(endpoint, 512);
        Command = Normalize(command, 512);
        ArgumentsJson = NormalizeJsonArray(argumentsJson);
        HeadersJson = NormalizeJson(headersJson);
        EnvironmentJson = NormalizeJson(environmentJson);
        ConfigJson = NormalizeJson(configJson);
        ProtocolVersion = Normalize(protocolVersion, 32, "2024-11-05");
        PermissionCode = Normalize(permissionCode, 128);
        IsEnabled = isEnabled;
        TimeoutSeconds = Math.Clamp(timeoutSeconds, 1, 180);
        Sort = sort;
        Remark = Normalize(remark, 512);
    }

    public void MarkDiscovery(DateTime discoveryTime)
    {
        LastDiscoveryTime = discoveryTime;
        LastErrorMessage = string.Empty;
    }

    public void MarkError(string? errorMessage)
    {
        LastErrorMessage = Normalize(errorMessage, 2000);
    }

    private static string NormalizeTransport(string? value)
    {
        var normalized = Normalize(value, 32, "StreamableHttp");
        return normalized.Equals("stdio", StringComparison.OrdinalIgnoreCase) ? "Stdio" :
            normalized.Equals("sse", StringComparison.OrdinalIgnoreCase) ? "Sse" :
            normalized.Equals("streamablehttp", StringComparison.OrdinalIgnoreCase) ? "StreamableHttp" :
            throw new UserFriendlyException("MCP 传输类型只支持 Stdio、Sse、StreamableHttp。");
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

    private static string NormalizeJsonArray(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "[]" : value.Trim();
        return normalized.Length <= 16_000 ? normalized : normalized[..16_000];
    }
}
