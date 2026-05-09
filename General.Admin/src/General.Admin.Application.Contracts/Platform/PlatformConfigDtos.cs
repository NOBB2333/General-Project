using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformConfigDto
{
    public string Code { get; set; } = string.Empty;

    public string DefaultValue { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string GroupCode { get; set; } = string.Empty;

    public string GroupName { get; set; } = string.Empty;

    public bool HasTenantValue { get; set; }

    public bool IsReadonly { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProviderName { get; set; } = string.Empty;

    public string? ProviderKey { get; set; }

    public string Value { get; set; } = string.Empty;

    public string ValueType { get; set; } = string.Empty;
}

public class PlatformConfigSaveInput
{
    [MaxLength(64)]
    public string? ProviderName { get; set; }

    [MaxLength(128)]
    public string? ProviderKey { get; set; }

    [Required]
    [MaxLength(512)]
    public string Value { get; set; } = string.Empty;
}
