using System.Threading;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace General.Admin.PhaseOne;

public class PlatformWeeklyReportReminderJobHandler : IPlatformScheduledJobHandler
{
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;
    private readonly IRepository<PhaseOneProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<PhaseOneProjectWorklog, Guid> _projectWorklogRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public PlatformWeeklyReportReminderJobHandler(
        PlatformSchedulerExecutionLogWriter executionLogWriter,
        IRepository<PhaseOneProjectMember, Guid> projectMemberRepository,
        IRepository<PhaseOneProjectWorklog, Guid> projectWorklogRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _executionLogWriter = executionLogWriter;
        _projectMemberRepository = projectMemberRepository;
        _projectWorklogRepository = projectWorklogRepository;
        _userRepository = userRepository;
    }

    public string JobKey => "weekly-report-reminder";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var weekStart = GetWeekStart(DateTime.Today);
        var activeMembers = (await _projectMemberRepository.GetListAsync())
            .Where(x => x.IsActive(weekStart))
            .Select(x => x.UserId)
            .Distinct()
            .ToList();

        if (activeMembers.Count == 0)
        {
            const string emptyResult = "演示提醒完成：当前没有需要提醒的项目成员。";
            await _executionLogWriter.WriteOperationAsync(JobKey, "周报填写通知", emptyResult, cancellationToken: cancellationToken);
            return $"{emptyResult} 可在日志中心的操作日志查看执行记录。";
        }

        var submittedUserIds = (await _projectWorklogRepository.GetListAsync())
            .Where(x => x.WeekStartDate == weekStart)
            .Select(x => x.UserId)
            .Distinct()
            .ToHashSet();

        var pendingUserIds = activeMembers
            .Where(userId => !submittedUserIds.Contains(userId))
            .Distinct()
            .ToList();

        var userLookup = (await _userRepository.GetListAsync(cancellationToken: cancellationToken))
            .ToDictionary(x => x.Id);
        var pendingUserNames = pendingUserIds
            .Select(userId => userLookup.TryGetValue(userId, out var user)
                ? string.IsNullOrWhiteSpace($"{user.Name}{user.Surname}".Trim())
                    ? user.UserName ?? userId.ToString("N")[..8]
                    : $"{user.Name}{user.Surname}".Trim()
                : userId.ToString("N")[..8])
            .Take(5)
            .ToList();

        var result = pendingUserIds.Count == 0
            ? $"演示提醒完成：本周周报已全部提交（统计周期起点 {weekStart:yyyy-MM-dd}）。"
            : $"演示提醒完成：待提醒 {pendingUserIds.Count} 名成员（{string.Join("、", pendingUserNames)}{(pendingUserIds.Count > pendingUserNames.Count ? " 等" : string.Empty)}），统计周期起点 {weekStart:yyyy-MM-dd}。";

        await _executionLogWriter.WriteOperationAsync(JobKey, "周报填写通知", result, cancellationToken: cancellationToken);
        return $"{result} 可在日志中心的操作日志查看执行记录。";
    }

    private static DateTime GetWeekStart(DateTime value)
    {
        var diff = ((int)value.DayOfWeek + 6) % 7;
        return value.Date.AddDays(-diff);
    }
}
