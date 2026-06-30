using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiWebSearchConfigDto
{
    public List<string> AllowDomains { get; set; } = [];

    public string ConfigJson { get; set; } = "{}";

    public DateTime CreationTime { get; set; }

    public List<string> DenyDomains { get; set; } = [];

    public string Endpoint { get; set; } = string.Empty;

    public bool HasApiKey { get; set; }

    public Guid Id { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; }

    public int MaxResults { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; }
}

public class AiWebSearchConfigSaveInput
{
    [MaxLength(8000)]
    public string ApiKey { get; set; } = string.Empty;

    public List<string> AllowDomains { get; set; } = [];

    [MaxLength(8000)]
    public string ConfigJson { get; set; } = "{}";

    public List<string> DenyDomains { get; set; } = [];

    [MaxLength(512)]
    public string Endpoint { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int MaxResults { get; set; } = 5;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(64)]
    public string ProviderType { get; set; } = "Custom";

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    public int TimeoutSeconds { get; set; } = 15;
}

public class AiWebSearchRecordDto
{
    public Guid? ConfigId { get; set; }

    public Guid? ConversationId { get; set; }

    public DateTime CreationTime { get; set; }

    public long ElapsedMilliseconds { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public Guid? MessageId { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string Query { get; set; } = string.Empty;

    public string RequestJson { get; set; } = "{}";

    public string ResponseSummaryJson { get; set; } = "{}";

    public int ResultCount { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class AiWebSearchRecordQueryInput
{
    public Guid? ConfigId { get; set; }

    public Guid? ConversationId { get; set; }

    [MaxLength(64)]
    public string? Keyword { get; set; }

    public int MaxResultCount { get; set; } = 50;

    [MaxLength(32)]
    public string? Status { get; set; }
}

public class AiWebSearchInput
{
    public Guid? ConfigId { get; set; }

    public Guid? ConversationId { get; set; }

    public int MaxResults { get; set; } = 5;

    public Guid? MessageId { get; set; }

    [Required]
    [MaxLength(512)]
    public string Query { get; set; } = string.Empty;
}

public class AiWebSearchResultDto
{
    public string ErrorMessage { get; set; } = string.Empty;

    public Guid RecordId { get; set; }

    public string ResponseSummaryJson { get; set; } = "{}";

    public int ResultCount { get; set; }

    public string Status { get; set; } = string.Empty;
}
