namespace General.Admin.Platform;

public class PlatformFileStorageOptions
{
    public const string SectionName = "FileStorage";

    public string Provider { get; set; } = PlatformFileStorageNames.Local;

    public long MaxFileSizeBytes { get; set; } = 104_857_600;

    public string[] AllowedContentTypes { get; set; } =
    [
        "image/jpeg",
        "image/png",
        "application/pdf",
        "text/plain",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    ];

    public PlatformLocalFileStorageOptions Local { get; set; } = new();

    public PlatformAliyunOssStorageOptions AliyunOSS { get; set; } = new();

    public PlatformMinioStorageOptions MinIO { get; set; } = new();
}

public class PlatformLocalFileStorageOptions
{
    public string RootPath { get; set; } = "App_Data/upload-files";

    public string PathTemplate { get; set; } = string.Empty;
}

public class PlatformAliyunOssStorageOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKeyId { get; set; } = string.Empty;

    public string AccessKeySecret { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string CustomDomain { get; set; } = string.Empty;

    public bool UseHttps { get; set; } = true;

    public string PathTemplate { get; set; } = "{yyyy}/{MM}/{dd}";
}

public class PlatformMinioStorageOptions
{
    public string Endpoint { get; set; } = string.Empty;

    public string AccessKey { get; set; } = string.Empty;

    public string SecretKey { get; set; } = string.Empty;

    public string BucketName { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string CustomDomain { get; set; } = string.Empty;

    public bool UseSSL { get; set; }

    public string PathTemplate { get; set; } = "{yyyy}/{MM}/{dd}";
}
