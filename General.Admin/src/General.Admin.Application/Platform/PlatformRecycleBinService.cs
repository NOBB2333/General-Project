using System.Reflection;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class PlatformRecycleBinService : ITransientDependency
{
    private readonly IRepository<AppDictData, Guid> _dictDataRepository;
    private readonly IRepository<AppDictType, Guid> _dictTypeRepository;
    private readonly IDistributedCache _distributedCache;
    private readonly IDataFilter<ISoftDelete> _softDeleteFilter;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<AppMenu, Guid> _menuRepository;
    private readonly IRepository<AppOpenApiApplication, Guid> _openApiRepository;
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly PlatformCacheService _platformCacheService;
    private readonly IRepository<PlatformScheduledJob, Guid> _scheduledJobRepository;
    private readonly IRepository<PlatformScheduledJobTrigger, Guid> _scheduledJobTriggerRepository;
    private readonly IRepository<AppUpdateLog, Guid> _updateLogRepository;

    public PlatformRecycleBinService(
        IRepository<AppDictData, Guid> dictDataRepository,
        IRepository<AppDictType, Guid> dictTypeRepository,
        IDistributedCache distributedCache,
        IDataFilter<ISoftDelete> softDeleteFilter,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IRepository<AppMenu, Guid> menuRepository,
        IRepository<AppOpenApiApplication, Guid> openApiRepository,
        IRepository<AppPlatformFile, Guid> fileRepository,
        PlatformCacheService platformCacheService,
        IRepository<PlatformScheduledJob, Guid> scheduledJobRepository,
        IRepository<PlatformScheduledJobTrigger, Guid> scheduledJobTriggerRepository,
        IRepository<AppUpdateLog, Guid> updateLogRepository)
    {
        _dictDataRepository = dictDataRepository;
        _dictTypeRepository = dictTypeRepository;
        _distributedCache = distributedCache;
        _softDeleteFilter = softDeleteFilter;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _menuRepository = menuRepository;
        _openApiRepository = openApiRepository;
        _fileRepository = fileRepository;
        _platformCacheService = platformCacheService;
        _scheduledJobRepository = scheduledJobRepository;
        _scheduledJobTriggerRepository = scheduledJobTriggerRepository;
        _updateLogRepository = updateLogRepository;
    }

    public async Task<List<PlatformRecycleBinItemDto>> GetListAsync(string? entityType = null)
    {
        using (_softDeleteFilter.Disable())
        {
            var result = new List<PlatformRecycleBinItemDto>();
            await AppendAsync(result, "menu", _menuRepository, x => x.Title, entityType);
            await AppendAsync(result, "dict-type", _dictTypeRepository, x => x.Name, entityType);
            await AppendAsync(result, "dict-data", _dictDataRepository, x => $"{x.Label}({x.Value})", entityType);
            await AppendAsync(result, "file", _fileRepository, x => x.FileName, entityType);
            await AppendAsync(result, "scheduled-job", _scheduledJobRepository, x => x.Title, entityType);
            await AppendAsync(result, "scheduled-trigger", _scheduledJobTriggerRepository, x => x.Title, entityType);
            await AppendAsync(result, "update-log", _updateLogRepository, x => x.Title, entityType);
            await AppendAsync(result, "open-api", _openApiRepository, x => x.Name, entityType);

            return result
                .OrderByDescending(x => x.DeletionTime)
                .ThenBy(x => x.EntityType)
                .ToList();
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

    private async Task AppendAsync<TEntity>(
        ICollection<PlatformRecycleBinItemDto> result,
        string entityType,
        IRepository<TEntity, Guid> repository,
        Func<TEntity, string> displayNameResolver,
        string? filterEntityType)
        where TEntity : class, IEntity<Guid>, ISoftDelete
    {
        if (!ShouldInclude(entityType, filterEntityType))
        {
            return;
        }

        var queryable = await repository.GetQueryableAsync();
        var deletedItems = await _asyncQueryableExecuter.ToListAsync(queryable.Where(x => x.IsDeleted));

        foreach (var item in deletedItems)
        {
            result.Add(new PlatformRecycleBinItemDto
            {
                DeletionTime = ResolveDateTime(item, "DeletionTime"),
                DisplayName = displayNameResolver(item),
                EntityType = entityType,
                Id = item.Id
            });
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
