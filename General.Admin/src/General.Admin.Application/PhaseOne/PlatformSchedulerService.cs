using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.PhaseOne;

public class PlatformSchedulerService : ITransientDependency
{
    private readonly IRepository<PlatformScheduledJob, Guid> _jobRepository;

    public PlatformSchedulerService(IRepository<PlatformScheduledJob, Guid> jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task<List<PlatformScheduledJobDto>> GetListAsync()
    {
        return (await _jobRepository.GetListAsync())
            .OrderBy(x => x.JobKey)
            .Select(MapJob)
            .ToList();
    }

    public async Task RunAsync(string jobKey)
    {
        var normalizedKey = jobKey.Trim();
        var job = (await _jobRepository.GetListAsync())
            .FirstOrDefault(x => x.JobKey.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));

        if (job == null)
        {
            throw new InvalidOperationException("定时任务不存在。");
        }

        var now = DateTime.Now;
        job.MarkRun(now, "手动执行成功", job.NextRunTime ?? now.AddDays(1));
        await _jobRepository.UpdateAsync(job, autoSave: true);
    }

    public async Task ToggleAsync(string jobKey, bool isEnabled)
    {
        var normalizedKey = jobKey.Trim();
        var job = (await _jobRepository.GetListAsync())
            .FirstOrDefault(x => x.JobKey.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase));

        if (job == null)
        {
            throw new InvalidOperationException("定时任务不存在。");
        }

        job.Toggle(isEnabled);
        await _jobRepository.UpdateAsync(job, autoSave: true);
    }

    private static PlatformScheduledJobDto MapJob(PlatformScheduledJob item)
    {
        return new PlatformScheduledJobDto
        {
            CronExpression = item.CronExpression,
            Description = item.Description,
            IsEnabled = item.IsEnabled,
            JobKey = item.JobKey,
            LastRunResult = item.LastRunResult,
            LastRunTime = item.LastRunTime,
            NextRunTime = item.NextRunTime,
            Title = item.Title
        };
    }
}
