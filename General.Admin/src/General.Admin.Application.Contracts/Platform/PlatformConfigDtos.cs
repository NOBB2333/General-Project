using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformConfigDto
{
    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string GroupCode { get; set; } = string.Empty;

    public bool IsReadonly { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public string ValueType { get; set; } = string.Empty;
}

public class PlatformConfigSaveInput
{
    [Required]
    [MaxLength(512)]
    public string Value { get; set; } = string.Empty;
}
