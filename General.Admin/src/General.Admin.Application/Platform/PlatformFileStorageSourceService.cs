using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class PlatformFileStorageSourceService : ITransientDependency
{
    private const string DefaultSourceLockName = "GeneralAdmin:FileStorageSource:Default";

    private static readonly HashSet<string> SupportedProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        PlatformFileStorageNames.Local,
        PlatformFileStorageNames.AliyunOss,
        PlatformFileStorageNames.Minio
    };

    private readonly IAbpDistributedLock _distributedLock;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly PlatformFileStorageSourceManager _sourceManager;
    private readonly IRepository<AppFileStorageSource, Guid> _sourceRepository;
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;

    public PlatformFileStorageSourceService(
        IAbpDistributedLock distributedLock,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        PlatformFileStorageSourceManager sourceManager,
        IRepository<AppFileStorageSource, Guid> sourceRepository,
        IRepository<AppPlatformFile, Guid> fileRepository)
    {
        _distributedLock = distributedLock;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _sourceManager = sourceManager;
        _sourceRepository = sourceRepository;
        _fileRepository = fileRepository;
    }

    public async Task<List<PlatformFileStorageSourceDto>> GetListAsync(bool enabledOnly = false)
    {
        var queryable = await _sourceRepository.GetQueryableAsync();
        if (enabledOnly)
        {
            queryable = queryable.Where(x => x.IsEnabled);
        }

        var sources = await _asyncQueryableExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.Name));

        return sources.Select(Map).ToList();
    }

    public async Task<PlatformFileStorageSourceDto> CreateAsync(PlatformFileStorageSourceSaveInput input)
    {
        ValidateInput(input, creating: true);

        await using var defaultLock = await AcquireDefaultSourceLockAsync();
        await EnsureNameNotExistsAsync(input.Name.Trim());

        var shouldBeDefault = input.IsDefault || !await _sourceRepository.AnyAsync();
        if (shouldBeDefault)
        {
            await ClearDefaultAsync();
        }

        var entity = new AppFileStorageSource(
            Guid.NewGuid(),
            input.Name,
            input.ProviderName,
            input.Endpoint,
            input.RootPath,
            input.AccessKeyId,
            _sourceManager.EncryptSecret(input.SecretKey),
            input.BucketName,
            input.Region,
            input.CustomDomain,
            input.PathTemplate,
            input.UseSsl,
            shouldBeDefault,
            input.IsEnabled,
            input.IsPublic,
            input.Remark);

        await _sourceRepository.InsertAsync(entity, autoSave: true);
        return Map(entity);
    }

    public async Task<PlatformFileStorageSourceDto> UpdateAsync(Guid id, PlatformFileStorageSourceSaveInput input)
    {
        ValidateInput(input, creating: false);

        await using var defaultLock = await AcquireDefaultSourceLockAsync();
        var entity = await _sourceRepository.GetAsync(id);
        await EnsureNameNotExistsAsync(input.Name.Trim(), id);

        if (input.IsDefault)
        {
            await ClearDefaultAsync(id);
            entity.SetDefault(true);
        }
        else if (entity.IsDefault)
        {
            var otherDefaultExists = await _sourceRepository.AnyAsync(x => x.Id != id && x.IsDefault);
            entity.SetDefault(!otherDefaultExists);
        }

        entity.Update(
            input.Name,
            input.ProviderName,
            input.Endpoint,
            input.RootPath,
            input.AccessKeyId,
            _sourceManager.EncryptSecret(input.SecretKey),
            input.BucketName,
            input.Region,
            input.CustomDomain,
            input.PathTemplate,
            input.UseSsl,
            input.IsEnabled,
            input.IsPublic,
            input.Remark);

        await _sourceRepository.UpdateAsync(entity, autoSave: true);
        return Map(entity);
    }

    public async Task ToggleAsync(Guid id, bool isEnabled)
    {
        var entity = await _sourceRepository.GetAsync(id);
        if (entity.IsDefault && !isEnabled)
        {
            throw new InvalidOperationException("默认文件存储源不能停用。");
        }

        entity.Toggle(isEnabled);
        await _sourceRepository.UpdateAsync(entity, autoSave: true);
    }

    public async Task SetDefaultAsync(Guid id)
    {
        await using var defaultLock = await AcquireDefaultSourceLockAsync();
        var entity = await _sourceRepository.GetAsync(id);
        if (!entity.IsEnabled)
        {
            throw new InvalidOperationException("停用的文件存储源不能设为默认。");
        }

        await ClearDefaultAsync(id);
        entity.SetDefault(true);
        await _sourceRepository.UpdateAsync(entity, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _sourceRepository.GetAsync(id);
        if (entity.IsDefault)
        {
            throw new InvalidOperationException("默认文件存储源不能删除。");
        }

        if (await _fileRepository.AnyAsync(x => x.StorageSourceId == id))
        {
            throw new InvalidOperationException("该存储源已被文件使用，不能删除。");
        }

        await _sourceRepository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<PlatformFileStorageSourceDescriptor?> ResolveDescriptorAsync(
        Guid? sourceId,
        bool requireEnabled)
    {
        AppFileStorageSource? source;
        if (sourceId.HasValue)
        {
            source = await _sourceRepository.FindAsync(sourceId.Value);
            if (source == null)
            {
                throw new InvalidOperationException("文件存储源不存在。");
            }
        }
        else
        {
            var queryable = await _sourceRepository.GetQueryableAsync();
            source = await _asyncQueryableExecuter.FirstOrDefaultAsync(
                queryable
                    .Where(x => x.IsDefault)
                    .OrderBy(x => x.Name));
            if (source == null)
            {
                return null;
            }
        }

        if (requireEnabled && !source.IsEnabled)
        {
            throw new InvalidOperationException("文件存储源已停用。");
        }

        return _sourceManager.ToDescriptor(source);
    }

    public async Task<string?> ResolveSourceNameAsync(Guid? sourceId)
    {
        if (!sourceId.HasValue)
        {
            return null;
        }

        return (await _sourceRepository.FindAsync(sourceId.Value))?.Name;
    }

    public async Task<Dictionary<Guid, string>> ResolveSourceNameMapAsync(IReadOnlyCollection<Guid> sourceIds)
    {
        if (sourceIds.Count == 0)
        {
            return [];
        }

        var queryable = await _sourceRepository.GetQueryableAsync();
        var sources = await _asyncQueryableExecuter.ToListAsync(
            queryable
                .Where(x => sourceIds.Contains(x.Id))
                .Select(x => new { x.Id, x.Name }));
        return sources.ToDictionary(x => x.Id, x => x.Name);
    }

    private async Task<IAbpDistributedLockHandle> AcquireDefaultSourceLockAsync()
    {
        var handle = await _distributedLock.TryAcquireAsync(DefaultSourceLockName, TimeSpan.FromSeconds(10));
        return handle ?? throw new InvalidOperationException("文件存储默认源正在被其他请求修改，请稍后重试。");
    }

    private async Task ClearDefaultAsync(Guid? exceptId = null)
    {
        var queryable = await _sourceRepository.GetQueryableAsync();
        var defaults = await _asyncQueryableExecuter.ToListAsync(
            queryable.Where(x => x.IsDefault && (!exceptId.HasValue || x.Id != exceptId.Value)));
        foreach (var source in defaults)
        {
            source.SetDefault(false);
        }

        if (defaults.Count > 0)
        {
            await _sourceRepository.UpdateManyAsync(defaults, autoSave: true);
        }
    }

    private async Task EnsureNameNotExistsAsync(string name, Guid? currentId = null)
    {
        var exists = await _sourceRepository.AnyAsync(x => x.Name == name && (!currentId.HasValue || x.Id != currentId.Value));
        if (exists)
        {
            throw new InvalidOperationException("文件存储源名称已存在。");
        }
    }

    private static void ValidateInput(PlatformFileStorageSourceSaveInput input, bool creating)
    {
        var provider = input.ProviderName?.Trim() ?? string.Empty;
        if (!SupportedProviders.Contains(provider))
        {
            throw new InvalidOperationException("文件存储源类型不支持。");
        }

        if (provider.Equals(PlatformFileStorageNames.Local, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(input.Endpoint) ||
            string.IsNullOrWhiteSpace(input.AccessKeyId) ||
            string.IsNullOrWhiteSpace(input.BucketName) ||
            (creating && string.IsNullOrWhiteSpace(input.SecretKey)))
        {
            throw new InvalidOperationException("云文件存储源配置不完整。");
        }
    }

    private static PlatformFileStorageSourceDto Map(AppFileStorageSource item)
    {
        return new PlatformFileStorageSourceDto
        {
            AccessKeyId = item.AccessKeyId,
            BucketName = item.BucketName,
            CustomDomain = item.CustomDomain,
            Endpoint = item.Endpoint,
            Id = item.Id,
            IsDefault = item.IsDefault,
            IsEnabled = item.IsEnabled,
            IsPublic = item.IsPublic,
            Name = item.Name,
            PathTemplate = item.PathTemplate,
            ProviderName = item.ProviderName,
            Region = item.Region,
            Remark = item.Remark,
            RootPath = item.RootPath,
            UseSsl = item.UseSsl
        };
    }
}
