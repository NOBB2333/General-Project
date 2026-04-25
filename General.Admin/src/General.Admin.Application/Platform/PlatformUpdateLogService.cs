using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace General.Admin.Platform;

public class PlatformUpdateLogService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppUpdateLog, Guid> _updateLogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PlatformUpdateLogService(
        ICurrentUser currentUser,
        IRepository<AppUpdateLog, Guid> updateLogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _updateLogRepository = updateLogRepository;
        _userRepository = userRepository;
    }

    public async Task<List<PlatformUpdateLogDto>> GetListAsync()
    {
        var logs = (await _updateLogRepository.GetListAsync())
            .OrderByDescending(x => x.PublishedAt)
            .ToList();
        var users = (await _userRepository.GetListAsync())
            .ToDictionary(x => x.Id);

        return logs
            .Select(item => new PlatformUpdateLogDto
            {
                AuthorName = ResolveDisplayName(users.GetValueOrDefault(item.AuthorUserId)),
                CreationTime = item.CreationTime,
                Id = item.Id,
                ImpactScope = item.ImpactScope,
                PublishedAt = item.PublishedAt,
                Summary = item.Summary,
                Title = item.Title,
                Version = item.Version
            })
            .ToList();
    }

    public async Task CreateAsync(PlatformUpdateLogSaveInput input)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new InvalidOperationException("Current user is not available.");
        }

        var log = new AppUpdateLog(
            Guid.NewGuid(),
            _currentUser.Id.Value,
            input.Version.Trim(),
            input.Title.Trim(),
            input.Summary.Trim(),
            input.PublishedAt,
            input.ImpactScope);
        await _updateLogRepository.InsertAsync(log, autoSave: true);
    }

    public async Task UpdateAsync(Guid id, PlatformUpdateLogSaveInput input)
    {
        var log = await _updateLogRepository.GetAsync(id);
        log.Update(
            input.Version.Trim(),
            input.Title.Trim(),
            input.Summary.Trim(),
            input.PublishedAt,
            input.ImpactScope);
        await _updateLogRepository.UpdateAsync(log, autoSave: true);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _updateLogRepository.DeleteAsync(id, autoSave: true);
    }

    private static string ResolveDisplayName(IdentityUser? user)
    {
        if (user == null)
        {
            return "-";
        }

        var fullName = $"{user.Name}{user.Surname}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName ?? "-" : fullName;
    }
}
