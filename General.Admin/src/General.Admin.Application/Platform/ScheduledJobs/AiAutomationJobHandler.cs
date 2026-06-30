using System.Threading;
using General.Admin.Ai;

namespace General.Admin.Platform;

public class AiAutomationJobHandler : IPlatformScheduledJobHandler
{
    private readonly AiJobService _aiJobService;
    private readonly PlatformSchedulerExecutionLogWriter _executionLogWriter;

    public AiAutomationJobHandler(
        AiJobService aiJobService,
        PlatformSchedulerExecutionLogWriter executionLogWriter)
    {
        _aiJobService = aiJobService;
        _executionLogWriter = executionLogWriter;
    }

    public string JobKey => AiJobConstants.SchedulerJobKey;

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await _aiJobService.RunEnabledJobsAsync("Scheduled", cancellationToken);
        await _executionLogWriter.WriteOperationAsync(JobKey, "AI 自动化任务", result, cancellationToken: cancellationToken);
        return $"{result} 可在 AI 自动化任务页面查看草稿记录。";
    }
}
