using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class PlatformFileVersionService : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<ProjectAcceptanceCriteria, Guid> _projectAcceptanceCriteriaRepository;
    private readonly IRepository<ProjectCharter, Guid> _projectCharterRepository;
    private readonly IRepository<AppPlatformFile, Guid> _platformFileRepository;

    public PlatformFileVersionService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IRepository<ProjectAcceptanceCriteria, Guid> projectAcceptanceCriteriaRepository,
        IRepository<ProjectCharter, Guid> projectCharterRepository,
        IRepository<AppPlatformFile, Guid> platformFileRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _projectAcceptanceCriteriaRepository = projectAcceptanceCriteriaRepository;
        _projectCharterRepository = projectCharterRepository;
        _platformFileRepository = platformFileRepository;
    }

    public async Task ApplyVersionChainAsync(AppPlatformFile file)
    {
        if (!ShouldVersionFile(file))
        {
            return;
        }

        var queryable = await _platformFileRepository.GetQueryableAsync();
        var currentFile = await _asyncQueryableExecuter.FirstOrDefaultAsync(
            queryable
                .Where(x =>
                    x.Id != file.Id &&
                    x.BusinessType == file.BusinessType &&
                    x.BusinessId == file.BusinessId &&
                    x.Category == file.Category &&
                    x.IsCurrentVersion)
                .OrderByDescending(x => x.VersionNo)
                .ThenByDescending(x => x.CreationTime));
        if (currentFile == null)
        {
            file.MarkAsVersion(1, null);
            return;
        }

        await EnsureCurrentVersionCanBeReplacedAsync(file);
        currentFile.MarkReplacedBy(file.Id);
        await _platformFileRepository.UpdateAsync(currentFile, autoSave: false);
        file.MarkAsVersion(currentFile.VersionNo + 1, currentFile.Id);
    }

    public async Task RestorePreviousCurrentVersionAsync(AppPlatformFile file)
    {
        if (!ShouldVersionFile(file) || !file.IsCurrentVersion || file.PreviousVersionFileId == null)
        {
            return;
        }

        var previousFile = await _platformFileRepository.FindAsync(file.PreviousVersionFileId.Value);
        if (previousFile == null)
        {
            return;
        }

        previousFile.RestoreAsCurrentVersion();
        await _platformFileRepository.UpdateAsync(previousFile, autoSave: false);
    }

    private static bool ShouldVersionFile(AppPlatformFile file)
    {
        if (file.BusinessType?.Equals("Project", StringComparison.OrdinalIgnoreCase) != true ||
            string.IsNullOrWhiteSpace(file.BusinessId))
        {
            return false;
        }

        return file.Category.Equals("Project/Charter", StringComparison.OrdinalIgnoreCase) ||
               file.Category.Equals("Project/AcceptanceCriteria", StringComparison.OrdinalIgnoreCase);
    }

    private async Task EnsureCurrentVersionCanBeReplacedAsync(AppPlatformFile file)
    {
        if (!Guid.TryParse(file.BusinessId, out var projectId))
        {
            return;
        }

        if (file.Category.Equals("Project/Charter", StringComparison.OrdinalIgnoreCase))
        {
            var approvedCharter = await _projectCharterRepository.AnyAsync(item =>
                item.ProjectId == projectId && item.ApprovalStatus == "已批准");
            if (approvedCharter)
            {
                throw new BusinessException("已批准的项目章程文件不能直接替换，请创建新版本并重新提交审批。");
            }
        }

        if (file.Category.Equals("Project/AcceptanceCriteria", StringComparison.OrdinalIgnoreCase))
        {
            var confirmedCriteria = await _projectAcceptanceCriteriaRepository.AnyAsync(item =>
                item.ProjectId == projectId && (item.Status == "已确认" || item.Status == "已验收"));
            if (confirmedCriteria)
            {
                throw new BusinessException("已确认的验收标准文件不能直接替换，请创建新版本或重新确认。");
            }
        }
    }
}
