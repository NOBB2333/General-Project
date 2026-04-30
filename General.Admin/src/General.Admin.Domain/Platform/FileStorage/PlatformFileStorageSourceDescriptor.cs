namespace General.Admin.Platform;

public class PlatformFileStorageSourceDescriptor
{
    public string AccessKeyId { get; set; } = string.Empty;

    public string AccessKeySecret { get; set; } = string.Empty;

    public string? BucketName { get; set; }

    public string? CustomDomain { get; set; }

    public string? Endpoint { get; set; }

    public bool IsPublic { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? PathTemplate { get; set; }

    public string ProviderName { get; set; } = PlatformFileStorageNames.Local;

    public string? Region { get; set; }

    public string? RootPath { get; set; }

    public Guid SourceId { get; set; }

    public bool UseSsl { get; set; }
}
