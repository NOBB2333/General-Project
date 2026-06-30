using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiMcpServerDto
{
    public string ArgumentsJson { get; set; } = "[]";

    public string Command { get; set; } = string.Empty;

    public string ConfigJson { get; set; } = "{}";

    public DateTime CreationTime { get; set; }

    public string Endpoint { get; set; } = string.Empty;

    public string EnvironmentJson { get; set; } = "{}";

    public string HeadersJson { get; set; } = "{}";

    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? LastDiscoveryTime { get; set; }

    public string LastErrorMessage { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string PermissionCode { get; set; } = string.Empty;

    public string ProtocolVersion { get; set; } = "2024-11-05";

    public string Remark { get; set; } = string.Empty;

    public string ServerCode { get; set; } = string.Empty;

    public int Sort { get; set; }

    public int TimeoutSeconds { get; set; }

    public int ToolCount { get; set; }

    public string TransportType { get; set; } = "StreamableHttp";
}

public class AiMcpServerSaveInput
{
    [MaxLength(16000)]
    public string ArgumentsJson { get; set; } = "[]";

    [MaxLength(512)]
    public string Command { get; set; } = string.Empty;

    [MaxLength(16000)]
    public string ConfigJson { get; set; } = "{}";

    [MaxLength(512)]
    public string Endpoint { get; set; } = string.Empty;

    [MaxLength(16000)]
    public string EnvironmentJson { get; set; } = "{}";

    [MaxLength(16000)]
    public string HeadersJson { get; set; } = "{}";

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string PermissionCode { get; set; } = string.Empty;

    [MaxLength(32)]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string ServerCode { get; set; } = string.Empty;

    public int Sort { get; set; }

    public int TimeoutSeconds { get; set; } = 30;

    [Required]
    [MaxLength(32)]
    public string TransportType { get; set; } = "StreamableHttp";
}

public class AiMcpToolDto
{
    public DateTime CreationTime { get; set; }

    public string Description { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string InputSchemaJson { get; set; } = "{}";

    public bool IsEnabled { get; set; }

    public DateTime LastDiscoveredTime { get; set; }

    public string PermissionCode { get; set; } = string.Empty;

    public Guid ServerId { get; set; }

    public string ServerName { get; set; } = string.Empty;

    public string ToolName { get; set; } = string.Empty;
}

public class AiMcpToolPolicyInput
{
    public bool IsEnabled { get; set; } = true;

    [MaxLength(128)]
    public string PermissionCode { get; set; } = string.Empty;
}

public class AiMcpInvokeInput
{
    [MaxLength(16000)]
    public string ArgumentsJson { get; set; } = "{}";
}

public class AiMcpInvokeResultDto
{
    public long ElapsedMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Guid InvocationId { get; set; }

    public string OutputJson { get; set; } = "{}";

    public string Status { get; set; } = string.Empty;

    public string ToolName { get; set; } = string.Empty;
}

public class AiMcpInvocationDto : AiMcpInvokeResultDto
{
    public DateTime CreationTime { get; set; }

    public string InputJson { get; set; } = "{}";

    public Guid ServerId { get; set; }

    public string ServerName { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; }

    public Guid? ToolId { get; set; }
}

public class AiMcpInvocationQueryInput
{
    public int MaxResultCount { get; set; } = 50;

    public Guid? ServerId { get; set; }

    [MaxLength(32)]
    public string? Status { get; set; }

    public Guid? ToolId { get; set; }

    [MaxLength(128)]
    public string? ToolName { get; set; }
}

public class AiMcpDiscoveryResultDto
{
    public string ErrorMessage { get; set; } = string.Empty;

    public int ToolCount { get; set; }

    public List<AiMcpToolDto> Tools { get; set; } = [];
}

public class AiMcpConnectionTestResultDto
{
    public string ErrorMessage { get; set; } = string.Empty;

    public string RawResponseJson { get; set; } = "{}";

    public bool Success { get; set; }
}
