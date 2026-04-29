using System.IO;
using General.Admin.EntityFrameworkCore;
using General.Admin.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace General.Admin.Infrastructure;

public class PlatformHealthProbeService
{
    private readonly AdminDbContext _dbContext;
    private readonly IDistributedCache _distributedCache;
    private readonly IOptions<PlatformFileStorageOptions> _fileStorageOptions;
    private readonly ILogger<PlatformHealthProbeService> _logger;

    public PlatformHealthProbeService(
        AdminDbContext dbContext,
        IDistributedCache distributedCache,
        IOptions<PlatformFileStorageOptions> fileStorageOptions,
        ILogger<PlatformHealthProbeService> logger)
    {
        _dbContext = dbContext;
        _distributedCache = distributedCache;
        _fileStorageOptions = fileStorageOptions;
        _logger = logger;
    }

    public Task<PlatformHealthResult> CheckLiveAsync()
    {
        return Task.FromResult(new PlatformHealthResult
        {
            Status = "Healthy",
            Checks =
            [
                new PlatformHealthCheckItem
                {
                    Name = "process",
                    Status = "Healthy",
                    Description = "HTTP host is running."
                }
            ]
        });
    }

    public async Task<PlatformHealthResult> CheckReadyAsync()
    {
        var checks = new List<PlatformHealthCheckItem>
        {
            await CheckDatabaseAsync(),
            await CheckCacheAsync(),
            CheckFileStorage()
        };

        return new PlatformHealthResult
        {
            Status = checks.Any(x => x.Status == "Unhealthy")
                ? "Unhealthy"
                : checks.Any(x => x.Status == "Degraded") ? "Degraded" : "Healthy",
            Checks = checks
        };
    }

    private async Task<PlatformHealthCheckItem> CheckDatabaseAsync()
    {
        try
        {
            return await _dbContext.Database.CanConnectAsync()
                ? Healthy("database", "Database connection is available.")
                : Unhealthy("database", "Database connection failed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Database health check failed.");
            return Unhealthy("database", "Database health check failed.");
        }
    }

    private async Task<PlatformHealthCheckItem> CheckCacheAsync()
    {
        try
        {
            var key = $"platform:health:{Guid.NewGuid():N}";
            await _distributedCache.SetStringAsync(key, "ok", new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
            });
            var value = await _distributedCache.GetStringAsync(key);
            await _distributedCache.RemoveAsync(key);
            return value == "ok"
                ? Healthy("cache", "Distributed cache is available.")
                : Degraded("cache", "Distributed cache read-after-write check failed.");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Distributed cache health check failed.");
            return Degraded("cache", "Distributed cache health check failed.");
        }
    }

    private PlatformHealthCheckItem CheckFileStorage()
    {
        var options = _fileStorageOptions.Value;
        if (!string.Equals(options.Provider, PlatformFileStorageNames.Local, StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(options.Provider)
                ? Degraded("file-storage", "File storage provider is not configured.")
                : Healthy("file-storage", $"File storage provider is {options.Provider}.");
        }

        try
        {
            var rootPath = options.Local.RootPath;
            if (string.IsNullOrWhiteSpace(rootPath))
            {
                return Unhealthy("file-storage", "Local file storage root path is empty.");
            }

            var fullPath = Path.GetFullPath(rootPath);
            Directory.CreateDirectory(fullPath);
            var probeFile = Path.Combine(fullPath, $".health-{Guid.NewGuid():N}");
            File.WriteAllText(probeFile, "ok");
            File.Delete(probeFile);
            return Healthy("file-storage", $"Local file storage is writable: {fullPath}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "File storage health check failed.");
            return Unhealthy("file-storage", "File storage health check failed.");
        }
    }

    private static PlatformHealthCheckItem Healthy(string name, string description)
    {
        return new PlatformHealthCheckItem { Name = name, Status = "Healthy", Description = description };
    }

    private static PlatformHealthCheckItem Degraded(string name, string description)
    {
        return new PlatformHealthCheckItem { Name = name, Status = "Degraded", Description = description };
    }

    private static PlatformHealthCheckItem Unhealthy(string name, string description)
    {
        return new PlatformHealthCheckItem { Name = name, Status = "Unhealthy", Description = description };
    }
}

public class PlatformHealthResult
{
    public List<PlatformHealthCheckItem> Checks { get; set; } = [];

    public string Status { get; set; } = "Healthy";
}

public class PlatformHealthCheckItem
{
    public string Description { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}
