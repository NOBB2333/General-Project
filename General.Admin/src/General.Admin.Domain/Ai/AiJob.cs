using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

/// <summary>
/// AI 自动化任务定义。任务触发仍由 Platform Scheduler 承接，本实体只保存 AI 执行配置。
/// </summary>
public class AiJob : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public string AssistantCode { get; private set; }

    public string ContextScopeJson { get; private set; }

    public string Description { get; private set; }

    public bool IsEnabled { get; private set; }

    public string JobCode { get; private set; }

    public string JobType { get; private set; }

    public DateTime? LastRunTime { get; private set; }

    public string LastRunStatus { get; private set; }

    public string LastRunSummary { get; private set; }

    public Guid? ModelId { get; private set; }

    public string Name { get; private set; }

    public string OutputTargetJson { get; private set; }

    public string PromptTemplateCode { get; private set; }

    public string SchedulerJobKey { get; private set; }

    public Guid? TenantId { get; private set; }

    protected AiJob()
    {
        AssistantCode = string.Empty;
        ContextScopeJson = "{}";
        Description = string.Empty;
        JobCode = string.Empty;
        JobType = string.Empty;
        LastRunStatus = "Pending";
        LastRunSummary = string.Empty;
        Name = string.Empty;
        OutputTargetJson = "{}";
        PromptTemplateCode = string.Empty;
        SchedulerJobKey = AiJobConstants.SchedulerJobKey;
    }

    public AiJob(
        Guid id,
        string jobCode,
        string name,
        string jobType,
        Guid? tenantId,
        string? description = null,
        string? promptTemplateCode = null,
        string? assistantCode = null,
        Guid? modelId = null,
        string? contextScopeJson = null,
        string? outputTargetJson = null,
        string? schedulerJobKey = null,
        bool isEnabled = true) : base(id)
    {
        JobCode = Check.NotNullOrWhiteSpace(jobCode, nameof(jobCode), 64).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        JobType = NormalizeJobType(jobType);
        TenantId = tenantId;
        Description = Normalize(description, 512);
        PromptTemplateCode = Normalize(promptTemplateCode, 128);
        AssistantCode = Normalize(assistantCode, 64);
        ModelId = modelId;
        ContextScopeJson = NormalizeJsonObject(contextScopeJson);
        OutputTargetJson = NormalizeJsonObject(outputTargetJson);
        SchedulerJobKey = Normalize(schedulerJobKey, 64, AiJobConstants.SchedulerJobKey);
        IsEnabled = isEnabled;
        LastRunStatus = "Pending";
        LastRunSummary = string.Empty;
    }

    public void Update(
        string name,
        string jobType,
        string? description,
        string? promptTemplateCode,
        string? assistantCode,
        Guid? modelId,
        string? contextScopeJson,
        string? outputTargetJson,
        string? schedulerJobKey,
        bool isEnabled)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        JobType = NormalizeJobType(jobType);
        Description = Normalize(description, 512);
        PromptTemplateCode = Normalize(promptTemplateCode, 128);
        AssistantCode = Normalize(assistantCode, 64);
        ModelId = modelId;
        ContextScopeJson = NormalizeJsonObject(contextScopeJson);
        OutputTargetJson = NormalizeJsonObject(outputTargetJson);
        SchedulerJobKey = Normalize(schedulerJobKey, 64, AiJobConstants.SchedulerJobKey);
        IsEnabled = isEnabled;
    }

    public void Toggle(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }

    public void MarkRun(DateTime runTime, string status, string? summary)
    {
        LastRunTime = runTime;
        LastRunStatus = NormalizeRunStatus(status);
        LastRunSummary = Normalize(summary, 512);
    }

    private static string NormalizeJobType(string? value)
    {
        var normalized = Normalize(value, 64, "ProjectWeeklySummary");
        return normalized.Equals("projectweeklysummary", StringComparison.OrdinalIgnoreCase) ? "ProjectWeeklySummary" :
            normalized.Equals("delayedtaskidentification", StringComparison.OrdinalIgnoreCase) ? "DelayedTaskIdentification" :
            normalized.Equals("performanceexceptionidentification", StringComparison.OrdinalIgnoreCase) ? "PerformanceExceptionIdentification" :
            normalized.Equals("highriskdueidentification", StringComparison.OrdinalIgnoreCase) ? "HighRiskDueIdentification" :
            normalized.Equals("overduereceivableidentification", StringComparison.OrdinalIgnoreCase) ? "OverdueReceivableIdentification" :
            normalized.Equals("businessmonthlysummary", StringComparison.OrdinalIgnoreCase) ? "BusinessMonthlySummary" :
            throw new UserFriendlyException("AI 自动化任务类型不支持。");
    }

    private static string NormalizeRunStatus(string? value)
    {
        var normalized = Normalize(value, 32, "Pending");
        return normalized.Equals("pending", StringComparison.OrdinalIgnoreCase) ? "Pending" :
            normalized.Equals("running", StringComparison.OrdinalIgnoreCase) ? "Running" :
            normalized.Equals("success", StringComparison.OrdinalIgnoreCase) ? "Success" :
            normalized.Equals("failed", StringComparison.OrdinalIgnoreCase) ? "Failed" :
            normalized.Equals("cancelled", StringComparison.OrdinalIgnoreCase) ? "Cancelled" :
            normalized;
    }

    private static string Normalize(string? value, int maxLength, string defaultValue = "")
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static string NormalizeJsonObject(string? value)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? "{}" : value.Trim();
        return normalized.Length <= 32_000 ? normalized : normalized[..32_000];
    }
}
