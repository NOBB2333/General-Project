using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformDictTypeDto
{
    public string Code { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public int Sort { get; set; }
}

public class PlatformDictTypeSaveInput
{
    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Remark { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Sort { get; set; }
}

public class PlatformDictDataDto
{
    public Guid DictTypeId { get; set; }

    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public int Sort { get; set; }

    public string TagColor { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class PlatformDictDataSaveInput
{
    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Label { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Remark { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int Sort { get; set; }

    [MaxLength(32)]
    public string TagColor { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string Value { get; set; } = string.Empty;
}

public class PlatformDictItemDto
{
    public string Label { get; set; } = string.Empty;

    public string TagColor { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
