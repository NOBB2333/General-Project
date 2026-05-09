using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class PlatformRecycleBinService : ITransientDependency
{
    private const int MaxRecycleBinResultCount = 100;
    private const int MaxRecycleBinSkipCount = 10_000;

    private readonly IRepository<AppDictData, Guid> _dictDataRepository;
    private readonly IRepository<AppDictType, Guid> _dictTypeRepository;
    private readonly IDistributedCache _distributedCache;
    private readonly IDataFilter<ISoftDelete> _softDeleteFilter;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IPlatformFileStorageProviderResolver _fileStorageProviderResolver;
    private readonly IRepository<AppMenu, Guid> _menuRepository;
    private readonly IRepository<AppOpenApiApplication, Guid> _openApiRepository;
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly PlatformFileStorageSourceService _fileStorageSourceService;
    private readonly PlatformCacheService _platformCacheService;
    private readonly IRepository<PlatformScheduledJob, Guid> _scheduledJobRepository;
    private readonly IRepository<PlatformScheduledJobTrigger, Guid> _scheduledJobTriggerRepository;
    private readonly IRepository<AppUpdateLog, Guid> _updateLogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PlatformRecycleBinService(
        IRepository<AppDictData, Guid> dictDataRepository,
        IRepository<AppDictType, Guid> dictTypeRepository,
        IDistributedCache distributedCache,
        IDataFilter<ISoftDelete> softDeleteFilter,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IPlatformFileStorageProviderResolver fileStorageProviderResolver,
        IRepository<AppMenu, Guid> menuRepository,
        IRepository<AppOpenApiApplication, Guid> openApiRepository,
        IRepository<AppPlatformFile, Guid> fileRepository,
        PlatformFileStorageSourceService fileStorageSourceService,
        PlatformCacheService platformCacheService,
        IRepository<PlatformScheduledJob, Guid> scheduledJobRepository,
        IRepository<PlatformScheduledJobTrigger, Guid> scheduledJobTriggerRepository,
        IRepository<AppUpdateLog, Guid> updateLogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _dictDataRepository = dictDataRepository;
        _dictTypeRepository = dictTypeRepository;
        _distributedCache = distributedCache;
        _softDeleteFilter = softDeleteFilter;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _fileStorageProviderResolver = fileStorageProviderResolver;
        _menuRepository = menuRepository;
        _openApiRepository = openApiRepository;
        _fileRepository = fileRepository;
        _fileStorageSourceService = fileStorageSourceService;
        _platformCacheService = platformCacheService;
        _scheduledJobRepository = scheduledJobRepository;
        _scheduledJobTriggerRepository = scheduledJobTriggerRepository;
        _updateLogRepository = updateLogRepository;
        _userRepository = userRepository;
    }

    public async Task<PlatformPagedResultDto<PlatformRecycleBinItemDto>> GetListAsync(PlatformRecycleBinListInput input)
    {
        using (_softDeleteFilter.Disable())
        {
            var entityType = input.EntityType;
            var maxResultCount = Math.Clamp(input.MaxResultCount, 1, MaxRecycleBinResultCount);
            var skipCount = Math.Clamp(input.SkipCount, 0, MaxRecycleBinSkipCount);
            var fetchCount = skipCount + maxResultCount;
            var totalCount = 0;
            var result = new List<PlatformRecycleBinItemDto>();
            totalCount += await AppendAsync(result, "menu", _menuRepository, x => x.Title, ResolveMenuLocation, entityType, fetchCount);
            totalCount += await AppendAsync(result, "dict-type", _dictTypeRepository, x => x.Name, x => x.Code, entityType, fetchCount);
            totalCount += await AppendAsync(result, "dict-data", _dictDataRepository, x => $"{x.Label}({x.Value})", x => x.Value, entityType, fetchCount);
            totalCount += await AppendAsync(result, "file", _fileRepository, x => x.FileName, ResolveFileLocation, entityType, fetchCount);
            totalCount += await AppendAsync(result, "scheduled-job", _scheduledJobRepository, x => x.Title, x => x.JobKey, entityType, fetchCount);
            totalCount += await AppendAsync(result, "scheduled-trigger", _scheduledJobTriggerRepository, x => x.Title, x => x.TriggerKey, entityType, fetchCount);
            totalCount += await AppendAsync(result, "update-log", _updateLogRepository, x => x.Title, x => x.Version, entityType, fetchCount);
            totalCount += await AppendAsync(result, "open-api", _openApiRepository, x => x.Name, x => x.AppId, entityType, fetchCount);

            await FillDeleterNamesAsync(result);

            var ordered = result
                .OrderByDescending(x => x.DeletionTime)
                .ThenBy(x => x.EntityType)
                .ToList();
            return new PlatformPagedResultDto<PlatformRecycleBinItemDto>
            {
                Items = ordered.Skip(skipCount).Take(maxResultCount).ToList(),
                TotalCount = totalCount
            };
        }
    }

    public async Task RestoreAsync(string entityType, Guid id)
    {
        using (_softDeleteFilter.Disable())
        {
            switch (NormalizeEntityType(entityType))
            {
                case "menu":
                    await RestoreAsync(_menuRepository, id);
                    await _platformCacheService.InvalidateAsync("menu");
                    break;
                case "dict-type":
                    var dictType = await _dictTypeRepository.GetAsync(id);
                    await RestoreAsync(_dictTypeRepository, id);
                    await RemoveDictCacheAsync(dictType.Code);
                    break;
                case "dict-data":
                    var dictData = await _dictDataRepository.GetAsync(id);
                    var ownerDictType = await _dictTypeRepository.GetAsync(dictData.DictTypeId);
                    await RestoreAsync(_dictDataRepository, id);
                    await RemoveDictCacheAsync(ownerDictType.Code);
                    break;
                case "file":
                    await RestoreAsync(_fileRepository, id);
                    break;
                case "scheduled-job":
                    await RestoreAsync(_scheduledJobRepository, id);
                    break;
                case "scheduled-trigger":
                    await RestoreAsync(_scheduledJobTriggerRepository, id);
                    break;
                case "update-log":
                    await RestoreAsync(_updateLogRepository, id);
                    break;
                case "open-api":
                    await RestoreAsync(_openApiRepository, id);
                    break;
                default:
                    throw new InvalidOperationException("不支持的回收站实体类型。");
            }
        }
    }

    public async Task DeletePermanentlyAsync(string entityType, Guid id, CancellationToken cancellationToken = default)
    {
        using (_softDeleteFilter.Disable())
        {
            switch (NormalizeEntityType(entityType))
            {
                case "file":
                    await DeleteFilePermanentlyAsync(id, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException("当前仅支持彻底删除文件类型。");
            }
        }
    }

    private async Task<int> AppendAsync<TEntity>(
        ICollection<PlatformRecycleBinItemDto> result,
        string entityType,
        IRepository<TEntity, Guid> repository,
        Func<TEntity, string> displayNameResolver,
        Func<TEntity, string> originalLocationResolver,
        string? filterEntityType,
        int fetchCount)
        where TEntity : class, IEntity<Guid>, ISoftDelete
    {
        if (!ShouldInclude(entityType, filterEntityType))
        {
            return 0;
        }

        var queryable = await repository.GetQueryableAsync();
        var deletedQuery = queryable.Where(x => x.IsDeleted);
        var totalCount = await _asyncQueryableExecuter.CountAsync(deletedQuery);
        var deletedItems = await _asyncQueryableExecuter.ToListAsync(
            deletedQuery
                .Take(fetchCount));

        foreach (var item in deletedItems.OrderByDescending(x => ResolveDateTime(x, "DeletionTime") ?? DateTime.MinValue))
        {
            result.Add(new PlatformRecycleBinItemDto
            {
                DeleterId = ResolveGuid(item, "DeleterId"),
                DeletionTime = ResolveDateTime(item, "DeletionTime"),
                DisplayName = displayNameResolver(item),
                EntityType = entityType,
                Id = item.Id,
                OriginalLocation = originalLocationResolver(item)
            });
        }

        return totalCount;
    }

    private async Task FillDeleterNamesAsync(ICollection<PlatformRecycleBinItemDto> result)
    {
        var deleterIds = result
            .Where(x => x.DeleterId.HasValue)
            .Select(x => x.DeleterId!.Value)
            .Distinct()
            .ToList();
        if (deleterIds.Count == 0)
        {
            return;
        }

        var userQueryable = await _userRepository.GetQueryableAsync();
        var users = (await _asyncQueryableExecuter.ToListAsync(
                userQueryable.Where(x => deleterIds.Contains(x.Id))))
            .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.UserName) ? x.Name : x.UserName);

        foreach (var item in result.Where(x => x.DeleterId.HasValue))
        {
            item.DeleterName = users.TryGetValue(item.DeleterId!.Value, out var userName)
                ? userName
                : null;
        }
    }

    private static async Task RestoreAsync<TEntity>(IRepository<TEntity, Guid> repository, Guid id)
        where TEntity : class, IEntity<Guid>, ISoftDelete
    {
        var entity = await repository.GetAsync(id);
        SetProperty(entity, "IsDeleted", false);
        SetProperty(entity, "DeletionTime", null);
        SetProperty(entity, "DeleterId", null);
        await repository.UpdateAsync(entity, autoSave: true);
    }

    private async Task DeleteFilePermanentlyAsync(Guid id, CancellationToken cancellationToken)
    {
        var file = await _fileRepository.GetAsync(id, cancellationToken: cancellationToken);
        var storageSource = file.StorageSourceId.HasValue
            ? await _fileStorageSourceService.ResolveDescriptorAsync(file.StorageSourceId, requireEnabled: false)
            : null;
        var storageProvider = _fileStorageProviderResolver.Resolve(storageSource?.ProviderName ?? file.StorageProvider);

        await storageProvider.DeleteAsync(
            file.FileKey,
            file.StorageLocation,
            storageSource,
            cancellationToken);
        await _fileRepository.DeleteDirectAsync(x => x.Id == id, cancellationToken);
    }

    private static bool ShouldInclude(string entityType, string? filterEntityType)
    {
        return string.IsNullOrWhiteSpace(filterEntityType) ||
               string.Equals(entityType, NormalizeEntityType(filterEntityType), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeEntityType(string entityType)
    {
        return entityType.Trim().ToLowerInvariant();
    }

    private static DateTime? ResolveDateTime(object entity, string propertyName)
    {
        return entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)?.GetValue(entity) as DateTime?;
    }

    private static Guid? ResolveGuid(object entity, string propertyName)
    {
        return entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)?.GetValue(entity) as Guid?;
    }

    private static string ResolveFileLocation(AppPlatformFile file)
    {
        return PlatformFilePathPolicy.BuildRelativePath(file.Category, file.ParentPath, file.FileName);
    }

    private static string ResolveMenuLocation(AppMenu menu)
    {
        return string.IsNullOrWhiteSpace(menu.PermissionCode)
            ? menu.Path
            : $"{menu.Path} / {menu.PermissionCode}";
    }

    private static void SetProperty(object entity, string propertyName, object? value)
    {
        var property = entity.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)
            ?? throw new InvalidOperationException($"实体缺少属性 {propertyName}。");
        property.SetValue(entity, value);
    }

    private Task RemoveDictCacheAsync(string dictTypeCode)
    {
        return _distributedCache.RemoveAsync($"platform:dict:{AppDictType.NormalizeCode(dictTypeCode)}");
    }
}
