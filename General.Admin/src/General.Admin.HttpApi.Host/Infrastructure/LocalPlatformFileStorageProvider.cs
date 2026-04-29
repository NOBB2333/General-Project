using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace General.Admin.Infrastructure;

public class LocalPlatformFileStorageProvider : IPlatformFileStorageProvider, ISingletonDependency
{
    private readonly IWebHostEnvironment _environment;
    private readonly PlatformFileStorageOptions _options;

    public LocalPlatformFileStorageProvider(
        IWebHostEnvironment environment,
        IOptions<PlatformFileStorageOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public string ProviderName => PlatformFileStorageNames.Local;

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
        var storageDirectory = GetStorageDirectory(DateTime.Now);
        Directory.CreateDirectory(storageDirectory);

        var filePath = Path.Combine(storageDirectory, fileKey);
        await using var output = File.Create(filePath);
        await stream.CopyToAsync(output, cancellationToken);

        return new PlatformFileStorageSaveResult(ProviderName, fileKey, filePath);
    }

    public Task<Stream> OpenReadAsync(
        string fileKey,
        string storageLocation,
        CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(fileKey, storageLocation);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("文件不存在。", fileKey);
        }

        return Task.FromResult<Stream>(File.OpenRead(filePath));
    }

    public Task<bool> DeleteAsync(
        string fileKey,
        string storageLocation,
        CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath(fileKey, storageLocation);
        if (!File.Exists(filePath))
        {
            return Task.FromResult(false);
        }

        File.Delete(filePath);
        return Task.FromResult(true);
    }

    public Task<string?> GetPublicUrlAsync(
        string fileKey,
        string storageLocation,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(null);
    }

    private string ResolveFilePath(string fileKey, string storageLocation)
    {
        var rootDirectory = GetRootDirectory();
        if (!string.IsNullOrWhiteSpace(storageLocation))
        {
            var candidate = Path.IsPathRooted(storageLocation)
                ? storageLocation
                : Path.Combine(_environment.ContentRootPath, storageLocation);
            if (IsPathUnderRoot(candidate, rootDirectory))
            {
                return Path.GetFullPath(candidate);
            }

            candidate = Path.Combine(rootDirectory, storageLocation);
            if (IsPathUnderRoot(candidate, rootDirectory))
            {
                return Path.GetFullPath(candidate);
            }

            throw new InvalidOperationException("文件存储路径不合法。");
        }

        var filePath = Path.Combine(rootDirectory, Path.GetFileName(fileKey));
        if (!IsPathUnderRoot(filePath, rootDirectory))
        {
            throw new InvalidOperationException("文件存储路径不合法。");
        }

        return Path.GetFullPath(filePath);
    }

    private string GetStorageDirectory(DateTime now)
    {
        var pathTemplate = RenderPathTemplate(_options.Local.PathTemplate, now);
        var storageDirectory = string.IsNullOrWhiteSpace(pathTemplate)
            ? GetRootDirectory()
            : Path.Combine(GetRootDirectory(), pathTemplate);
        if (!IsPathUnderRoot(storageDirectory, GetRootDirectory()))
        {
            throw new InvalidOperationException("文件存储路径配置不合法。");
        }

        return Path.GetFullPath(storageDirectory);
    }

    private string GetRootDirectory()
    {
        var rootPath = string.IsNullOrWhiteSpace(_options.Local.RootPath)
            ? "App_Data/upload-files"
            : _options.Local.RootPath.Trim();

        var rootDirectory = Path.IsPathRooted(rootPath)
            ? rootPath
            : Path.Combine(_environment.ContentRootPath, rootPath);
        return Path.GetFullPath(rootDirectory);
    }

    private static bool IsPathUnderRoot(string path, string rootDirectory)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(rootDirectory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) +
            Path.DirectorySeparatorChar;
        return fullPath.Equals(fullRoot.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) ||
               fullPath.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static string RenderPathTemplate(string? template, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        return template
            .Replace("{yyyy}", now.ToString("yyyy"), StringComparison.OrdinalIgnoreCase)
            .Replace("{MM}", now.ToString("MM"), StringComparison.OrdinalIgnoreCase)
            .Replace("{dd}", now.ToString("dd"), StringComparison.OrdinalIgnoreCase)
            .Trim()
            .Replace('\\', Path.DirectorySeparatorChar)
            .Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
