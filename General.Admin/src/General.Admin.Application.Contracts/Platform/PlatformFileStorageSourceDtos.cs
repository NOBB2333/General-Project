using System.ComponentModel.DataAnnotations;

namespace General.Admin.Platform;

public class PlatformFileStorageSourceDto
{
    public string AccessKeyId { get; set; } = string.Empty;

    public string? BucketName { get; set; }

    public string? CustomDomain { get; set; }

    public string? Endpoint { get; set; }

    public Guid Id { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsPublic { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PathTemplate { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string? Region { get; set; }

    public string? Remark { get; set; }

    public string? RootPath { get; set; }

    public bool UseSsl { get; set; }
}

public class PlatformFileStorageSourceSaveInput
{
    [MaxLength(256)]
    public string AccessKeyId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? BucketName { get; set; }

    [MaxLength(512)]
    public string? CustomDomain { get; set; }

    [MaxLength(512)]
    public string? Endpoint { get; set; }

    public bool IsDefault { get; set; }

    public bool IsEnabled { get; set; } = true;

    public bool IsPublic { get; set; }

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(128)]
    public string? PathTemplate { get; set; }

    [Required]
    [MaxLength(32)]
    public string ProviderName { get; set; } = "Local";

    [MaxLength(128)]
    public string? Region { get; set; }

    [MaxLength(256)]
    public string? Remark { get; set; }

    [MaxLength(512)]
    public string? RootPath { get; set; }

    [MaxLength(1024)]
    public string? SecretKey { get; set; }

    public bool UseSsl { get; set; }
}

public class PlatformFileStorageSourceToggleInput
{
    public bool IsEnabled { get; set; }
}
