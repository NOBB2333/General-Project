using System.IO;
using System.Collections.Concurrent;
using System.Threading;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Infrastructure;

public class MinioPlatformFileStorageProvider : IPlatformFileStorageProvider, ISingletonDependency
{
    private readonly ConcurrentDictionary<string, Lazy<IMinioClient>> _clients = new(StringComparer.OrdinalIgnoreCase);
    private readonly PlatformMinioStorageOptions _options;

    public MinioPlatformFileStorageProvider(IOptions<PlatformFileStorageOptions> options)
    {
        _options = options.Value.MinIO;
    }

    public string ProviderName => PlatformFileStorageNames.Minio;

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
        await EnsureBucketAsync(client, options, cancellationToken);

        var originalName = Path.GetFileName(fileName);
        var fileKey = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(originalName)}";
        var objectName = CloudPlatformFileStoragePathHelper.BuildObjectName(options.PathTemplate, fileKey, DateTime.Now);
        var uploadStream = await CloudPlatformFileStoragePathHelper.EnsureSeekableAsync(stream, cancellationToken);

        await client.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(options.BucketName)
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
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default)
    {
        var options = ResolveOptions(source);
        var client = GetClient(options, source);
        var tempPath = Path.Combine(Path.GetTempPath(), $"general-admin-minio-{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = File.Create(tempPath))
            {
                await client.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(options.BucketName)
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
        PlatformFileStorageSourceDescriptor? source = null,
        CancellationToken cancellationToken = default)
    {
        var options = ResolveOptions(source);
        await GetClient(options, source).RemoveObjectAsync(
            new RemoveObjectArgs()
                .WithBucket(options.BucketName)
                .WithObject(storageLocation),
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

        var seconds = Math.Clamp((int)expiry.TotalSeconds, 1, 604_800);
        return GetClient(options, source)
            .PresignedGetObjectAsync(
                new PresignedGetObjectArgs()
                    .WithBucket(options.BucketName)
                    .WithObject(storageLocation)
                    .WithExpiry(seconds))!;
    }

    private IMinioClient GetClient(PlatformMinioStorageOptions options, PlatformFileStorageSourceDescriptor? source)
    {
        var key = source == null
            ? "__default__"
            : $"{source.SourceId:N}:{options.Endpoint}:{options.AccessKey}:{options.BucketName}:{options.UseSSL}";
        return _clients.GetOrAdd(key, _ => new Lazy<IMinioClient>(() => CreateClient(options))).Value;
    }

    private IMinioClient CreateClient(PlatformMinioStorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Endpoint) ||
            string.IsNullOrWhiteSpace(options.AccessKey) ||
            string.IsNullOrWhiteSpace(options.SecretKey) ||
            string.IsNullOrWhiteSpace(options.BucketName))
        {
            throw new InvalidOperationException("MinIO 文件存储配置不完整。");
        }

        return new MinioClient()
            .WithEndpoint(options.Endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(options.UseSSL)
            .Build();
    }

    private async Task EnsureBucketAsync(
        IMinioClient client,
        PlatformMinioStorageOptions options,
        CancellationToken cancellationToken)
    {
        var exists = await client.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(options.BucketName),
            cancellationToken);
        if (!exists)
        {
            await client.MakeBucketAsync(
                new MakeBucketArgs()
                    .WithBucket(options.BucketName)
                    .WithLocation(options.Region),
                cancellationToken);
        }
    }

    private PlatformMinioStorageOptions ResolveOptions(PlatformFileStorageSourceDescriptor? source)
    {
        if (source == null)
        {
            return _options;
        }

        return new PlatformMinioStorageOptions
        {
            AccessKey = source.AccessKeyId,
            BucketName = source.BucketName ?? string.Empty,
            CustomDomain = source.CustomDomain ?? string.Empty,
            Endpoint = source.Endpoint ?? string.Empty,
            PathTemplate = source.PathTemplate ?? _options.PathTemplate,
            Region = source.Region ?? string.Empty,
            SecretKey = source.AccessKeySecret,
            UseSSL = source.UseSsl
        };
    }
}
