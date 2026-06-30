using System.Text.Json;

namespace General.Admin.Platform;

public partial class PlatformDataSeedContributor
{
    private async Task SeedPlatformScheduledJobsAsync()
    {
        var existingJobs = (await _platformScheduledJobRepository.GetListAsync())
            .ToDictionary(x => x.JobKey);
        var scheduledJobs = new List<PlatformScheduledJob>
        {
            new(Guid.Parse("59000000-0000-0000-0000-000000000001"), "log-cleanup", "清理老旧日志", "0 0 2 * * ?", "每日凌晨清理过期系统日志、操作日志与临时文件。", true, DateTime.Today.AddDays(1).AddHours(2), DateTime.Today.AddHours(-6), "最近一次执行成功"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000002"), "weekly-report-reminder", "周报填写通知", "0 0 17 ? * FRI", "每周五下午提醒项目成员回填周报与工时。", true, DateTime.Today.AddDays(4).AddHours(17), DateTime.Today.AddDays(-3).AddHours(17), "最近一次发送 18 条提醒"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000003"), "tenant-health-check", "租户健康巡检", "0 0/30 * * * ?", "巡检数据库连通性、后台任务积压和磁盘占用。", false, DateTime.Today.AddMinutes(30), DateTime.Today.AddHours(-1), "当前暂停"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000004"), "project-performance-snapshot", "项目绩效快照", "0 0 8 ? * MON", "每周一生成项目 PV、EV、AC、SPI、CPI 快照。", true, DateTime.Today.AddDays(7).AddHours(8), null, "等待首次执行"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000005"), AiJobConstants.SchedulerJobKey, "AI 自动化任务", "0 0 8 ? * MON-FRI", "工作日上午触发已启用的 AI 自动化任务，AI 任务内部根据上下文范围判断每日、每周或每月执行。", true, DateTime.Today.AddDays(1).AddHours(8), null, "等待首次执行"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000006"), IntegrationSyncJobStatus.SchedulerJobKey, "外部系统同步调度", "0 0/5 * * * ?", "每 5 分钟扫描外部系统同步任务，并按任务自身 Cron 执行到期的组织、用户、群和会议同步。", true, DateTime.Today.AddMinutes(5), null, "等待首次执行"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000007"), "project-reminder-notification", "项目到期与风险提醒", "0 0 9 * * ?", "每日早上扫描任务到期、任务逾期、风险到期、高风险项、会议提醒和会议纪要分发，并触发外部系统通知规则。", true, DateTime.Today.AddDays(1).AddHours(9), null, "等待首次执行"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000008"), "business-reminder-notification", "经营提醒与预警推送", "0 0 10 * * ?", "每日扫描回款到期、回款逾期、合同收尾、发票异常、预算偏差和经营预警，并触发外部系统通知规则。", true, DateTime.Today.AddDays(1).AddHours(10), null, "等待首次执行"),
            new(Guid.Parse("59000000-0000-0000-0000-000000000009"), "business-report-distribution", "经营报表分发", "0 30 8 ? * MON", "每周一生成项目经营、回款记录和经营预警报表摘要，并触发外部系统通知规则。", true, DateTime.Today.AddDays(7).AddHours(8).AddMinutes(30), null, "等待首次执行")
        };
        var missingJobs = scheduledJobs
            .Where(x => !existingJobs.ContainsKey(x.JobKey))
            .ToList();
        if (missingJobs.Count > 0)
        {
            await _platformScheduledJobRepository.InsertManyAsync(missingJobs, autoSave: true);
        }

        var jobs = (await _platformScheduledJobRepository.GetListAsync())
            .OrderBy(x => x.JobKey)
            .ToList();
        var existingDefaultTriggerJobIds = (await _platformScheduledJobTriggerRepository.GetListAsync())
            .Where(x => x.TriggerKey == "default")
            .Select(x => x.JobId)
            .ToHashSet();
        var missingDefaultTriggers = jobs
            .Where(x => !existingDefaultTriggerJobIds.Contains(x.Id))
            .ToList();
        if (missingDefaultTriggers.Count == 0)
        {
            return;
        }

        await _platformScheduledJobTriggerRepository.InsertManyAsync(
            missingDefaultTriggers.Select(x => new PlatformScheduledJobTrigger(
                Guid.NewGuid(),
                x.Id,
                "default",
                "默认触发器",
                x.CronExpression,
                x.Description,
                x.IsEnabled,
                x.NextRunTime,
                x.LastRunTime,
                x.LastRunResult)).ToList(),
            autoSave: true);
    }

    private async Task SeedAiAutomationJobsAsync()
    {
        var existingJobs = (await _aiJobRepository.GetListAsync())
            .ToDictionary(x => x.JobCode);
        var outputTargetJson = JsonSerializer.Serialize(new
        {
            targetType = "Draft",
            channel = "AiJobRun",
            allowWriteBack = false,
            manualReviewRequired = true
        });
        var jobs = new List<AiJob>
        {
            new(
                Guid.Parse("71000000-0000-0000-0000-000000000001"),
                "ai-weekly-project-summary",
                "每周项目状态摘要",
                "ProjectWeeklySummary",
                _currentTenant.Id,
                "每周生成项目组合状态摘要，覆盖进度、健康度、SPI/CPI、风险和经营预警。",
                "project-weekly-report",
                "pmo-assistant",
                contextScopeJson: JsonSerializer.Serialize(new
                {
                    scope = "ProjectPortfolio",
                    scheduleCadence = "WeeklyMonday",
                    maxItems = 12,
                    includeBusiness = true,
                    includeEarnedValue = true
                }),
                outputTargetJson: outputTargetJson,
                schedulerJobKey: AiJobConstants.SchedulerJobKey,
                isEnabled: true),
            new(
                Guid.Parse("71000000-0000-0000-0000-000000000002"),
                "ai-delayed-task-identification",
                "延期任务识别",
                "DelayedTaskIdentification",
                _currentTenant.Id,
                "识别项目组合中的延期和临期任务，形成 PM 优先处理草稿。",
                "risk-analysis",
                "pm-assistant",
                contextScopeJson: JsonSerializer.Serialize(new
                {
                    scope = "ProjectPortfolio",
                    scheduleCadence = "WeeklyMonday",
                    maxItems = 20,
                    sourceTypes = new[] { "task" }
                }),
                outputTargetJson: outputTargetJson,
                schedulerJobKey: AiJobConstants.SchedulerJobKey,
                isEnabled: true),
            new(
                Guid.Parse("71000000-0000-0000-0000-000000000003"),
                "ai-performance-exception-identification",
                "SPI/CPI 异常识别",
                "PerformanceExceptionIdentification",
                _currentTenant.Id,
                "识别 SPI、CPI 低于阈值的项目，形成治理复核草稿。",
                "risk-analysis",
                "pmo-assistant",
                contextScopeJson: JsonSerializer.Serialize(new
                {
                    scope = "ProjectPortfolio",
                    scheduleCadence = "WeeklyMonday",
                    maxItems = 20,
                    spiThreshold = 0.9m,
                    cpiThreshold = 0.9m
                }),
                outputTargetJson: outputTargetJson,
                schedulerJobKey: AiJobConstants.SchedulerJobKey,
                isEnabled: true),
            new(
                Guid.Parse("71000000-0000-0000-0000-000000000004"),
                "ai-high-risk-due-identification",
                "高风险到期识别",
                "HighRiskDueIdentification",
                _currentTenant.Id,
                "每日识别 P0/P1 风险、问题、里程碑到期和逾期项，形成 PMO 治理行动草稿。",
                "risk-analysis",
                "pmo-assistant",
                contextScopeJson: JsonSerializer.Serialize(new
                {
                    scope = "ProjectPortfolio",
                    scheduleCadence = "Weekday",
                    maxItems = 20,
                    dueWithinDays = 7
                }),
                outputTargetJson: outputTargetJson,
                schedulerJobKey: AiJobConstants.SchedulerJobKey,
                isEnabled: true),
            new(
                Guid.Parse("71000000-0000-0000-0000-000000000005"),
                "ai-overdue-receivable-identification",
                "逾期回款识别",
                "OverdueReceivableIdentification",
                _currentTenant.Id,
                "每日识别回款风险、待回款收尾项目和经营闭环阻塞，形成经营跟进行动草稿。",
                "receivable-collection-suggestion",
                "business-assistant",
                contextScopeJson: JsonSerializer.Serialize(new
                {
                    scope = "BusinessPortfolio",
                    scheduleCadence = "Weekday",
                    maxItems = 20,
                    includeReceivable = true
                }),
                outputTargetJson: outputTargetJson,
                schedulerJobKey: AiJobConstants.SchedulerJobKey,
                isEnabled: true),
            new(
                Guid.Parse("71000000-0000-0000-0000-000000000006"),
                "ai-business-monthly-summary",
                "经营管理月度摘要",
                "BusinessMonthlySummary",
                _currentTenant.Id,
                "每月生成经营管理摘要，覆盖合同、成本、毛利、回款、收尾和重点项目。",
                "business-risk-explanation",
                "business-assistant",
                contextScopeJson: JsonSerializer.Serialize(new
                {
                    scope = "BusinessPortfolio",
                    scheduleCadence = "MonthlyFirstWorkday",
                    maxItems = 12,
                    includeProfit = true,
                    includeReceivable = true
                }),
                outputTargetJson: outputTargetJson,
                schedulerJobKey: AiJobConstants.SchedulerJobKey,
                isEnabled: true)
        };
        var missingJobs = jobs
            .Where(x => !existingJobs.ContainsKey(x.JobCode))
            .ToList();
        if (missingJobs.Count > 0)
        {
            await _aiJobRepository.InsertManyAsync(missingJobs, autoSave: true);
        }
    }
}
