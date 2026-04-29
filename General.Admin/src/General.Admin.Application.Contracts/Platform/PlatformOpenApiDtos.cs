using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformOpenApiApplicationDto
{
    public string AppId { get; set; } = string.Empty;

    public DateTime CreationTime { get; set; }

    public Guid Id { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? LastModificationTime { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public List<string> Scopes { get; set; } = [];
}

public class PlatformOpenApiApplicationSecretDto : PlatformOpenApiApplicationDto
{
    public string AppSecret { get; set; } = string.Empty;
}

public class PlatformOpenApiApplicationSaveInput
{
    public bool IsEnabled { get; set; } = true;

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(256)]
    public string Remark { get; set; } = string.Empty;

    public List<string> Scopes { get; set; } = [];
}
