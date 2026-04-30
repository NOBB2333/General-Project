using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using Aliyun.OSS;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Infrastructure;

public class AliyunOssPlatformFileStorageProvider : IPlatformFileStorageProvider, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, Lazy<IOss>> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly PlatformAliyunOssStorageOptions _options;

    public AliyunOssPlatformFileStorageProvider(IOptions<PlatformFileStorageOptions> options)
    {
        _options = options.Value.AliyunOSS;
    }

    public string ProviderName => PlatformFileStorageNames.AliyunOss;

    public async Task<PlatformFileStorageSaveResult> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        string category,
        string? parentPath,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default)
    {
        var options = ResolveOptions(source);
        var client = GetClient(options, source);
        var originalName = Path.GetFileName(fileName);
        var fileKey = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(originalName)}";
        var objectName = CloudPlatformFileStoragePathHelper.BuildObjectName(options.PathTemplate, fileKey, DateTime.Now);
        var uploadStream = await CloudPlatformFileStoragePathHelper.EnsureSeekableAsync(stream, cancellationToken);
        var metadata = new ObjectMetadata { ContentType = contentType };

        await Task.Run(
            () => client.PutObject(options.BucketName, objectName, uploadStream, metadata),
            cancellationToken);

        return new PlatformFileStorageSaveResult(ProviderName, fileKey, objectName);
    }

    public async Task<Stream> OpenReadAsync(
        string fileKey,
        string storageLocation,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default)
    {
        var options = ResolveOptions(source);
        var ossObject = await Task.Run(
            () => GetClient(options, source).GetObject(options.BucketName, storageLocation),
            cancellationToken);
        return new OwnerDisposingReadStream(ossObject.Content, ossObject);
    }

    public async Task<bool> DeleteAsync(
        string fileKey,
        string storageLocation,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default)
    {
        var options = ResolveOptions(source);
        await Task.Run(
            () => GetClient(options, source).DeleteObject(options.BucketName, storageLocation),
            cancellationToken);
        return true;
    }

    public Task<string?> GetPublicUrlAsync(
        string fileKey,
        string storageLocation,
        TimeSpan expiry,
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default)
    {
        var options = ResolveOptions(source);
        if (!string.IsNullOrWhiteSpace(options.CustomDomain))
        {
            return Task.FromResult<string?>($"{options.CustomDomain.TrimEnd('/')}/{storageLocation}");
        }

        var uri = GetClient(options, source).GeneratePresignedUri(
            options.BucketName,
            storageLocation,
            DateTime.Now.Add(expiry));
        return Task.FromResult<string?>(uri.ToString());
    }

    private IOss GetClient(PlatformAliyunOssStorageOptions options, PlatformFileStorageSourceDescriptor? source)
    {
        var key = source == null
            ? "__default__"
            : $"{source.SourceId:N}:{options.Endpoint}:{options.AccessKeyId}:{options.BucketName}:{options.UseHttps}";
        return _clients.GetOrAdd(key, _ => new Lazy<IOss>(() => CreateClient(options))).Value;
    }

    private IOss CreateClient(PlatformAliyunOssStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint) ||
            string.IsNullOrWhiteSpace(options.AccessKeyId) ||
            string.IsNullOrWhiteSpace(options.AccessKeySecret) ||
            string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException("Aliyun OSS 文件存储配置不完整。");
        }

        var endpoint = options.Endpoint;
        if (!endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            endpoint = $"{(options.UseHttps ? "https" : "http")}://{endpoint}";
        }

        return new OssClient(endpoint, options.AccessKeyId, options.AccessKeySecret);
    }

    private PlatformAliyunOssStorageOptions ResolveOptions(PlatformFileStorageSourceDescriptor? source)
    {
        if (source == null)
        {
            return _options;
        }

        return new PlatformAliyunOssStorageOptions
        {
            AccessKeyId = source.AccessKeyId,
            AccessKeySecret = source.AccessKeySecret,
            BucketName = source.BucketName ?? string.Empty,
            CustomDomain = source.CustomDomain ?? string.Empty,
            Endpoint = source.Endpoint ?? string.Empty,
            PathTemplate = source.PathTemplate ?? _options.PathTemplate,
            Region = source.Region ?? string.Empty,
            UseHttps = source.UseSsl
        };
    }
}
