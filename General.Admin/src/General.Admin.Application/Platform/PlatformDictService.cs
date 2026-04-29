using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class PlatformDictService : ITransientDependency
{
    private static readonly DistributedCacheEntryOptions DictCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
    };

    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IDistributedCache _distributedCache;
    private readonly IRepository<AppDictData, Guid> _dictDataRepository;
    private readonly IRepository<AppDictType, Guid> _dictTypeRepository;

    public PlatformDictService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IDistributedCache distributedCache,
        IRepository<AppDictData, Guid> dictDataRepository,
        IRepository<AppDictType, Guid> dictTypeRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _distributedCache = distributedCache;
        _dictDataRepository = dictDataRepository;
        _dictTypeRepository = dictTypeRepository;
    }

    public async Task<List<PlatformDictTypeDto>> GetTypesAsync()
    {
        return (await _dictTypeRepository.GetListAsync())
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Code)
            .Select(MapType)
            .ToList();
    }

    public async Task<PlatformDictTypeDto> CreateTypeAsync(PlatformDictTypeSaveInput input)
    {
        var code = AppDictType.NormalizeCode(input.Code);
        if (await _dictTypeRepository.AnyAsync(x => x.Code == code))
        {
            throw new InvalidOperationException("字典编码已存在。");
        }

        var entity = new AppDictType(Guid.NewGuid(), code, input.Name, input.Sort, input.Remark);
        await _dictTypeRepository.InsertAsync(entity, autoSave: true);
        return MapType(entity);
    }

    public async Task<PlatformDictTypeDto> UpdateTypeAsync(Guid id, PlatformDictTypeSaveInput input)
    {
        var entity = await _dictTypeRepository.GetAsync(id);
        entity.Update(input.Name, input.Sort, input.Remark);
        await _dictTypeRepository.UpdateAsync(entity, autoSave: true);
        await RemoveDictCacheAsync(entity.Code);
        return MapType(entity);
    }

    public async Task DeleteTypeAsync(Guid id)
    {
        var entity = await _dictTypeRepository.GetAsync(id);
        var items = (await _dictDataRepository.GetListAsync())
            .Where(x => x.DictTypeId == id)
            .ToList();
        if (items.Count > 0)
        {
            await _dictDataRepository.DeleteManyAsync(items, autoSave: true);
        }

        await _dictTypeRepository.DeleteAsync(entity, autoSave: true);
        await RemoveDictCacheAsync(entity.Code);
    }

    public async Task<List<PlatformDictDataDto>> GetDataAsync(string dictTypeCode)
    {
        var dictType = await FindTypeByCodeAsync(dictTypeCode);
        var queryable = await _dictDataRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(
            queryable
                .Where(x => x.DictTypeId == dictType.Id)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Value));

        return items.Select(MapData).ToList();
    }

    public async Task<List<PlatformDictItemDto>> GetItemsAsync(string dictTypeCode)
    {
        var cacheKey = BuildDictCacheKey(dictTypeCode);
        var cached = await _distributedCache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            return JsonSerializer.Deserialize<List<PlatformDictItemDto>>(cached) ?? [];
        }

        var dictType = await FindTypeByCodeAsync(dictTypeCode);
        var queryable = await _dictDataRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(
            queryable
                .Where(x => x.DictTypeId == dictType.Id && x.IsEnabled)
                .OrderBy(x => x.Sort)
                .ThenBy(x => x.Value));

        var result = items.Select(x => new PlatformDictItemDto
        {
            Label = x.Label,
            TagColor = x.TagColor,
            Value = x.Value
        }).ToList();
        await _distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(result), DictCacheOptions);
        return result;
    }

    public async Task<Dictionary<string, List<PlatformDictItemDto>>> GetBatchItemsAsync(string codes)
    {
        var result = new Dictionary<string, List<PlatformDictItemDto>>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(codes))
        {
            return result;
        }

        foreach (var code in codes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            result[code] = await GetItemsAsync(code);
        }

        return result;
    }

    public async Task<PlatformDictDataDto> CreateDataAsync(string dictTypeCode, PlatformDictDataSaveInput input)
    {
        var dictType = await FindTypeByCodeAsync(dictTypeCode);
        var value = input.Value.Trim();
        if (await _dictDataRepository.AnyAsync(x => x.DictTypeId == dictType.Id && x.Value == value))
        {
            throw new InvalidOperationException("字典值已存在。");
        }

        var entity = new AppDictData(
            Guid.NewGuid(),
            dictType.Id,
            input.Label,
            value,
            input.Sort,
            input.IsEnabled,
            input.TagColor,
            input.Remark);
        await _dictDataRepository.InsertAsync(entity, autoSave: true);
        await RemoveDictCacheAsync(dictType.Code);
        return MapData(entity);
    }

    public async Task<PlatformDictDataDto> UpdateDataAsync(Guid id, PlatformDictDataSaveInput input)
    {
        var entity = await _dictDataRepository.GetAsync(id);
        var dictType = await _dictTypeRepository.GetAsync(entity.DictTypeId);
        var value = input.Value.Trim();
        if (await _dictDataRepository.AnyAsync(x => x.DictTypeId == dictType.Id && x.Value == value && x.Id != id))
        {
            throw new InvalidOperationException("字典值已存在。");
        }

        entity.Update(input.Label, value, input.Sort, input.IsEnabled, input.TagColor, input.Remark);
        await _dictDataRepository.UpdateAsync(entity, autoSave: true);
        await RemoveDictCacheAsync(dictType.Code);
        return MapData(entity);
    }

    public async Task DeleteDataAsync(Guid id)
    {
        var entity = await _dictDataRepository.GetAsync(id);
        var dictType = await _dictTypeRepository.GetAsync(entity.DictTypeId);
        await _dictDataRepository.DeleteAsync(entity, autoSave: true);
        await RemoveDictCacheAsync(dictType.Code);
    }

    private async Task<AppDictType> FindTypeByCodeAsync(string dictTypeCode)
    {
        var code = AppDictType.NormalizeCode(dictTypeCode);
        var dictType = await _dictTypeRepository.FindAsync(x => x.Code == code);
        return dictType ?? throw new InvalidOperationException("字典类型不存在。");
    }

    private Task RemoveDictCacheAsync(string dictTypeCode)
    {
        return _distributedCache.RemoveAsync(BuildDictCacheKey(dictTypeCode));
    }

    private static string BuildDictCacheKey(string dictTypeCode)
    {
        return $"platform:dict:{AppDictType.NormalizeCode(dictTypeCode)}";
    }

    private static PlatformDictTypeDto MapType(AppDictType item)
    {
        return new PlatformDictTypeDto
        {
            Code = item.Code,
            Id = item.Id,
            Name = item.Name,
            Remark = item.Remark,
            Sort = item.Sort
        };
    }

    private static PlatformDictDataDto MapData(AppDictData item)
    {
        return new PlatformDictDataDto
        {
            DictTypeId = item.DictTypeId,
            Id = item.Id,
            IsEnabled = item.IsEnabled,
            Label = item.Label,
            Remark = item.Remark,
            Sort = item.Sort,
            TagColor = item.TagColor,
            Value = item.Value
        };
    }
}
