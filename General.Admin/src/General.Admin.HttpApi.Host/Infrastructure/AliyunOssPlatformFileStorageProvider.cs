using System.IO;
using System.Threading;
using Aliyun.OSS;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Infrastructure;

public class AliyunOssPlatformFileStorageProvider : IPlatformFileStorageProvider, ISingletonDependency
{
    private readonly Lazy<IOss> _client;
    private readonly PlatformAliyunOssStorageOptions _options;

    public AliyunOssPlatformFileStorageProvider(IOptions<PlatformFileStorageOptions> options)
    {
        _options = options.Value.AliyunOSS;
        _client = new Lazy<IOss>(CreateClient);
    }

    public string ProviderName => PlatformFileStorageNames.AliyunOss;

    public async Task<PlatformFileStorageSaveResult> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        string category,
        string? parentPath,
        CancellationToken cancellationToken = default)
    {
        var originalName = Path.GetFileName(fileName);
        var fileKey = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(originalName)}";
        var objectName = CloudPlatformFileStoragePathHelper.BuildObjectName(_options.PathTemplate, fileKey, DateTime.Now);
        var uploadStream = await CloudPlatformFileStoragePathHelper.EnsureSeekableAsync(stream, cancellationToken);
        var metadata = new ObjectMetadata { ContentType = contentType };

        await Task.Run(
            () => _client.Value.PutObject(_options.BucketName, objectName, uploadStream, metadata),
            cancellationToken);

        return new PlatformFileStorageSaveResult(ProviderName, fileKey, objectName);
    }

    public async Task<Stream> OpenReadAsync(
        string fileKey,
        string storageLocation,
        CancellationToken cancellationToken = default)
    {
        var ossObject = await Task.Run(
            () => _client.Value.GetObject(_options.BucketName, storageLocation),
            cancellationToken);
        return new OwnerDisposingReadStream(ossObject.Content, ossObject);
    }

    public async Task<bool> DeleteAsync(
        string fileKey,
        string storageLocation,
        CancellationToken cancellationToken = default)
    {
        await Task.Run(
            () => _client.Value.DeleteObject(_options.BucketName, storageLocation),
            cancellationToken);
        return true;
    }

    public Task<string?> GetPublicUrlAsync(
        string fileKey,
        string storageLocation,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(_options.CustomDomain))
        {
            return Task.FromResult<string?>($"{_options.CustomDomain.TrimEnd('/')}/{storageLocation}");
        }

        var uri = _client.Value.GeneratePresignedUri(
            _options.BucketName,
            storageLocation,
            DateTime.Now.Add(expiry));
        return Task.FromResult<string?>(uri.ToString());
    }

    private IOss CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) ||
            string.IsNullOrWhiteSpace(_options.AccessKeyId) ||
            string.IsNullOrWhiteSpace(_options.AccessKeySecret) ||
            string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("Aliyun OSS 文件存储配置不完整。");
        }

        var endpoint = _options.Endpoint;
        if (!endpoint.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            endpoint = $"{(_options.UseHttps ? "https" : "http")}://{endpoint}";
        }

        return new OssClient(endpoint, _options.AccessKeyId, _options.AccessKeySecret);
    }
}
