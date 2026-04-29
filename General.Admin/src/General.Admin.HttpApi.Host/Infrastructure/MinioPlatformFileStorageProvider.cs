using System.IO;
using System.Threading;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Infrastructure;

public class MinioPlatformFileStorageProvider : IPlatformFileStorageProvider, ISingletonDependency
{
    private readonly Lazy<IMinioClient> _client;
    private readonly PlatformMinioStorageOptions _options;

    public MinioPlatformFileStorageProvider(IOptions<PlatformFileStorageOptions> options)
    {
        _options = options.Value.MinIO;
        _client = new Lazy<IMinioClient>(CreateClient);
    }

    public string ProviderName => PlatformFileStorageNames.Minio;

    public async Task<PlatformFileStorageSaveResult> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        string category,
        string? parentPath,
        CancellationToken cancellationToken = default)
    {
        var client = _client.Value;
        await EnsureBucketAsync(client, cancellationToken);

        var originalName = Path.GetFileName(fileName);
        var fileKey = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(originalName)}";
        var objectName = CloudPlatformFileStoragePathHelper.BuildObjectName(_options.PathTemplate, fileKey, DateTime.Now);
        var uploadStream = await CloudPlatformFileStoragePathHelper.EnsureSeekableAsync(stream, cancellationToken);

        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(objectName)
                .WithStreamData(uploadStream)
                .WithObjectSize(uploadStream.Length)
                .WithContentType(contentType),
            cancellationToken);

        return new PlatformFileStorageSaveResult(ProviderName, fileKey, objectName);
    }

    public async Task<Stream> OpenReadAsync(
        string fileKey,
        string storageLocation,
        CancellationToken cancellationToken = default)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"general-admin-minio-{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = File.Create(tempPath))
            {
                await _client.Value.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(storageLocation)
                        .WithCallbackStream(source => source.CopyTo(output)),
                    cancellationToken);
            }

            return new TemporaryFileReadStream(tempPath);
        }
        catch
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        string fileKey,
        string storageLocation,
        CancellationToken cancellationToken = default)
    {
        await _client.Value.RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(_options.BucketName)
                .WithObject(storageLocation),
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

        var seconds = Math.Clamp((int)expiry.TotalSeconds, 1, 604_800);
        return _client.Value
            .PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(_options.BucketName)
                    .WithObject(storageLocation)
                    .WithExpiry(seconds))!;
    }

    private IMinioClient CreateClient()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint) ||
            string.IsNullOrWhiteSpace(_options.AccessKey) ||
            string.IsNullOrWhiteSpace(_options.SecretKey) ||
            string.IsNullOrWhiteSpace(_options.BucketName))
        {
            throw new InvalidOperationException("MinIO 文件存储配置不完整。");
        }

        return new MinioClient()
            .WithEndpoint(_options.Endpoint)
            .WithCredentials(_options.AccessKey, _options.SecretKey)
            .WithSSL(_options.UseSSL)
            .Build();
    }

    private async Task EnsureBucketAsync(IMinioClient client, CancellationToken cancellationToken)
    {
        var exists = await client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(_options.BucketName),
            cancellationToken);
        if (!exists)
        {
            await client.MakeBucketAsync(
                new MakeBucketArgs()
                    .WithBucket(_options.BucketName)
                    .WithLocation(_options.Region),
                cancellationToken);
        }
    }
}
