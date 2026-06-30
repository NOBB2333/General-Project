using System.ComponentModel.DataAnnotations;

namespace General.Admin.Ai;

public class AiProviderDto
{
    public DateTime CreationTime { get; set; }

    public string Endpoint { get; set; } = string.Empty;

    public bool HasApiKey { get; set; }

    public Guid Id { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public int ModelCount { get; set; }

    public string Name { get; set; } = string.Empty;

    public string OrganizationId { get; set; } = string.Empty;

    public string ProviderType { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;
}

public class AiProviderSaveInput
{
    [MaxLength(2048)]
    public string ApiKey { get; set; } = string.Empty;

    [MaxLength(8000)]
    public string ConfigJson { get; set; } = "{}";

    [MaxLength(512)]
    public string Endpoint { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string OrganizationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string ProviderType { get; set; } = AiProviderProtocolNames.OpenAIResponses;

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;
}

public class AiProviderTestResultDto
{
    public string Message { get; set; } = string.Empty;

    public bool Success { get; set; }
}

public class AiDiscoveredModelDto
{
    public string DisplayName { get; set; } = string.Empty;

    public string ModelCode { get; set; } = string.Empty;

    public string ModelType { get; set; } = "standard";
}

public class AiProviderModelDiscoveryResultDto
{
    public int CreatedCount { get; set; }

    public List<AiDiscoveredModelDto> Models { get; set; } = [];

    public string Message { get; set; } = string.Empty;

    public bool Success { get; set; }

    public int UpdatedCount { get; set; }
}

public class AiModelDto
{
    public List<string> Capabilities { get; set; } = [];

    public int ContextWindow { get; set; }

    public string Currency { get; set; } = "USD";

    public string DisplayName { get; set; } = string.Empty;

    public Guid? FallbackModelId { get; set; }

    public string FallbackModelName { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public decimal InputTokenPrice { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; }

    public int MaxOutputTokens { get; set; }

    public string ModelCode { get; set; } = string.Empty;

    public string ModelType { get; set; } = string.Empty;

    public decimal OutputTokenPrice { get; set; }

    public Guid ProviderId { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public int Sort { get; set; }
}

public class AiModelSaveInput
{
    public List<string> Capabilities { get; set; } = [];

    public int ContextWindow { get; set; }

    [MaxLength(16)]
    public string Currency { get; set; } = "USD";

    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    public Guid? FallbackModelId { get; set; }

    public decimal InputTokenPrice { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; } = true;

    public int MaxOutputTokens { get; set; }

    [Required]
    [MaxLength(128)]
    public string ModelCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(32)]
    public string ModelType { get; set; } = "standard";

    public decimal OutputTokenPrice { get; set; }

    public Guid ProviderId { get; set; }

    [MaxLength(512)]
    public string Remark { get; set; } = string.Empty;

    public int Sort { get; set; }
}
