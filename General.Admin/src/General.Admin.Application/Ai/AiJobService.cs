using System.Diagnostics;
using System.Threading;
using System.Text.Json;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

public class AiJobService : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly BusinessContextLoader _businessContextLoader;
    private readonly BusinessQueryBuilder _businessQueryBuilder;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiJob, Guid> _jobRepository;
    private readonly IRepository<AiJobRun, Guid> _jobRunRepository;
    private readonly IRepository<AiModel, Guid> _modelRepository;
    private readonly ProjectDashboardService _projectDashboardService;

    public AiJobService(
        BusinessContextLoader businessContextLoader,
        BusinessQueryBuilder businessQueryBuilder,
        ICurrentTenant currentTenant,
        IDataFilter<IMultiTenant> multiTenantFilter,
        IGuidGenerator guidGenerator,
        IRepository<AiJob, Guid> jobRepository,
        IRepository<AiJobRun, Guid> jobRunRepository,
        IRepository<AiModel, Guid> modelRepository,
        ProjectDashboardService projectDashboardService)
    {
        _businessContextLoader = businessContextLoader;
        _businessQueryBuilder = businessQueryBuilder;
        _currentTenant = currentTenant;
        _multiTenantFilter = multiTenantFilter;
        _guidGenerator = guidGenerator;
        _jobRepository = jobRepository;
        _jobRunRepository = jobRunRepository;
        _modelRepository = modelRepository;
        _projectDashboardService = projectDashboardService;
    }

    public async Task<List<AiJobDto>> GetJobsAsync(AiJobQueryInput input)
    {
        var jobs = await _jobRepository.GetListAsync();
        if (!input.IncludeDisabled)
        {
            jobs = jobs.Where(x => x.IsEnabled).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            jobs = jobs
                .Where(x =>
                    x.JobCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.JobType))
        {
            var jobType = input.JobType.Trim();
            jobs = jobs.Where(x => x.JobType.Equals(jobType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.SchedulerJobKey))
        {
            var schedulerJobKey = input.SchedulerJobKey.Trim();
            jobs = jobs.Where(x => x.SchedulerJobKey.Equals(schedulerJobKey, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var modelNames = await ResolveModelNamesAsync(jobs.Select(x => x.ModelId));
        return jobs
            .OrderByDescending(x => x.IsEnabled)
            .ThenBy(x => x.JobCode)
            .Take(Math.Clamp(input.MaxResultCount, 1, 500))
            .Select(item => MapJob(item, modelNames))
            .ToList();
    }

    public async Task<AiJobDto> CreateJobAsync(AiJobSaveInput input)
    {
        ValidateJobInput(input);
        var jobCode = input.JobCode.Trim();
        if (await _jobRepository.AnyAsync(x => x.JobCode == jobCode))
        {
            throw new UserFriendlyException($"AI 自动化任务编码已存在：{jobCode}");
        }

        var entity = new AiJob(
            _guidGenerator.Create(),
            jobCode,
            input.Name,
            input.JobType,
            _currentTenant.Id,
            input.Description,
            input.PromptTemplateCode,
            input.AssistantCode,
            input.ModelId,
            input.ContextScopeJson,
            input.OutputTargetJson,
            input.SchedulerJobKey,
            input.IsEnabled);
        await _jobRepository.InsertAsync(entity, autoSave: true);
        return MapJob(entity, await ResolveModelNamesAsync([entity.ModelId]));
    }

    public async Task<AiJobDto> UpdateJobAsync(Guid id, AiJobSaveInput input)
    {
        ValidateJobInput(input);
        var entity = await _jobRepository.GetAsync(id);
        if (!entity.JobCode.Equals(input.JobCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("AI 自动化任务编码创建后不能修改。");
        }

        entity.Update(
            input.Name,
            input.JobType,
            input.Description,
            input.PromptTemplateCode,
            input.AssistantCode,
            input.ModelId,
            input.ContextScopeJson,
            input.OutputTargetJson,
            input.SchedulerJobKey,
            input.IsEnabled);
        await _jobRepository.UpdateAsync(entity, autoSave: true);
        return MapJob(entity, await ResolveModelNamesAsync([entity.ModelId]));
    }

    public async Task<AiJobDto> ToggleJobAsync(Guid id, bool isEnabled)
    {
        var entity = await _jobRepository.GetAsync(id);
        entity.Toggle(isEnabled);
        await _jobRepository.UpdateAsync(entity, autoSave: true);
        return MapJob(entity, await ResolveModelNamesAsync([entity.ModelId]));
    }

    public async Task DeleteJobAsync(Guid id)
    {
        await _jobRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<AiJobRunResultDto> RunJobAsync(Guid id)
    {
        var entity = await _jobRepository.GetAsync(id);
        return await RunJobInternalAsync(entity, "Manual", CancellationToken.None);
    }

    public async Task<string> RunEnabledJobsAsync(string runMode, CancellationToken cancellationToken = default)
    {
        var normalizedRunMode = NormalizeRunMode(runMode);
        List<AiJob> jobs;
        using (_multiTenantFilter.Disable())
        {
            jobs = (await _jobRepository.GetListAsync())
                .Where(x => x.IsEnabled)
                .Where(x => x.SchedulerJobKey.Equals(AiJobConstants.SchedulerJobKey, StringComparison.OrdinalIgnoreCase))
                .Where(x => ShouldRunForMode(x, normalizedRunMode, DateTime.Today))
                .OrderBy(x => x.TenantId)
                .ThenBy(x => x.JobCode)
                .ToList();
        }

        if (jobs.Count == 0)
        {
            return "未找到需要执行的 AI 自动化任务。";
        }

        var successCount = 0;
        var failedCount = 0;
        foreach (var job in jobs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (_currentTenant.Change(job.TenantId))
            {
                var result = await RunJobInternalAsync(job, normalizedRunMode, cancellationToken);
                if (result.Run.Status == "Success")
                {
                    successCount++;
                }
                else
                {
                    failedCount++;
                }
            }
        }

        return $"AI 自动化任务完成：触发 {jobs.Count} 个，成功 {successCount} 个，失败 {failedCount} 个。";
    }

    public async Task<List<AiJobRunDto>> GetJobRunsAsync(AiJobRunQueryInput input)
    {
        var runs = await _jobRunRepository.GetListAsync();
        if (input.AiJobId.HasValue)
        {
            runs = runs.Where(x => x.AiJobId == input.AiJobId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.JobCode))
        {
            var jobCode = input.JobCode.Trim();
            runs = runs.Where(x => x.JobCode.Equals(jobCode, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.JobType))
        {
            var jobType = input.JobType.Trim();
            runs = runs.Where(x => x.JobType.Equals(jobType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.RunMode))
        {
            var runMode = input.RunMode.Trim();
            runs = runs.Where(x => x.RunMode.Equals(runMode, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var status = input.Status.Trim();
            runs = runs.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (input.StartedFrom.HasValue)
        {
            runs = runs.Where(x => x.StartedTime >= input.StartedFrom.Value).ToList();
        }

        if (input.StartedTo.HasValue)
        {
            var startedTo = input.StartedTo.Value.Date.AddDays(1);
            runs = runs.Where(x => x.StartedTime < startedTo).ToList();
        }

        return runs
            .OrderByDescending(x => x.StartedTime)
            .Take(Math.Clamp(input.MaxResultCount, 1, 500))
            .Select(MapRun)
            .ToList();
    }

    private async Task<AiJobRunResultDto> RunJobInternalAsync(
        AiJob job,
        string runMode,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var run = new AiJobRun(
            _guidGenerator.Create(),
            job.Id,
            job.JobCode,
            job.Name,
            job.JobType,
            runMode,
            startedAt,
            job.TenantId);
        await _jobRunRepository.InsertAsync(run, autoSave: true, cancellationToken: cancellationToken);

        var inputSnapshotJson = "{}";
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pmoDashboard = await _projectDashboardService.GetPmoDashboardAsync();
            var businessOverview = await LoadPrivilegedBusinessOverviewAsync();
            inputSnapshotJson = BuildInputSnapshotJson(job, runMode, pmoDashboard, businessOverview);
            var draft = BuildDraft(job, pmoDashboard, businessOverview);
            stopwatch.Stop();
            run.Complete(
                inputSnapshotJson,
                draft.OutputDraftJson,
                draft.ItemCount,
                stopwatch.ElapsedMilliseconds,
                DateTime.UtcNow);
            job.MarkRun(run.FinishedTime ?? DateTime.UtcNow, "Success", draft.Summary);
            await _jobRunRepository.UpdateAsync(run, autoSave: true, cancellationToken: cancellationToken);
            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: cancellationToken);
            return new AiJobRunResultDto
            {
                Job = MapJob(job, await ResolveModelNamesAsync([job.ModelId])),
                Run = MapRun(run),
                Summary = draft.Summary
            };
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            run.Cancel(inputSnapshotJson, "AI 自动化任务已取消。", stopwatch.ElapsedMilliseconds, DateTime.UtcNow);
            job.MarkRun(run.FinishedTime ?? DateTime.UtcNow, "Cancelled", "AI 自动化任务已取消。");
            await _jobRunRepository.UpdateAsync(run, autoSave: true, cancellationToken: CancellationToken.None);
            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            var errorMessage = ex.GetBaseException().Message;
            run.Fail(inputSnapshotJson, errorMessage, stopwatch.ElapsedMilliseconds, DateTime.UtcNow);
            job.MarkRun(run.FinishedTime ?? DateTime.UtcNow, "Failed", errorMessage);
            await _jobRunRepository.UpdateAsync(run, autoSave: true, cancellationToken: CancellationToken.None);
            await _jobRepository.UpdateAsync(job, autoSave: true, cancellationToken: CancellationToken.None);
            return new AiJobRunResultDto
            {
                Job = MapJob(job, await ResolveModelNamesAsync([job.ModelId])),
                Run = MapRun(run),
                Summary = $"AI 自动化任务执行失败：{errorMessage}"
            };
        }
    }

    private async Task<BusinessOverviewDto> LoadPrivilegedBusinessOverviewAsync()
    {
        var context = await _businessContextLoader.LoadAsync();
        return _businessQueryBuilder.BuildOverview(
            context,
            new BusinessQueryAccess(CanViewProfitAnalysis: true, CanViewSensitiveAmounts: true));
    }

    private static AiAutomationDraft BuildDraft(
        AiJob job,
        ProjectPmoDashboardDto pmoDashboard,
        BusinessOverviewDto businessOverview)
    {
        return job.JobType switch
        {
            "ProjectWeeklySummary" => BuildProjectWeeklySummaryDraft(job, pmoDashboard, businessOverview),
            "DelayedTaskIdentification" => BuildDelayedTaskDraft(job, pmoDashboard),
            "PerformanceExceptionIdentification" => BuildPerformanceExceptionDraft(job, pmoDashboard, businessOverview),
            "HighRiskDueIdentification" => BuildHighRiskDueDraft(job, pmoDashboard),
            "OverdueReceivableIdentification" => BuildOverdueReceivableDraft(job, businessOverview),
            "BusinessMonthlySummary" => BuildBusinessMonthlySummaryDraft(job, businessOverview),
            _ => BuildProjectWeeklySummaryDraft(job, pmoDashboard, businessOverview)
        };
    }

    private static AiAutomationDraft BuildProjectWeeklySummaryDraft(
        AiJob job,
        ProjectPmoDashboardDto dashboard,
        BusinessOverviewDto businessOverview)
    {
        var maxItems = ResolveMaxItems(job, 12);
        var focusProjects = dashboard.Projects
            .OrderByDescending(x => x.HealthRiskScore)
            .ThenByDescending(x => x.IsKeyProject)
            .Take(maxItems)
            .Select(project => DraftItem(
                project.Id,
                project.Name,
                project.HealthStatus == "健康" ? "Info" : project.HealthStatus == "关注" ? "Warning" : "Critical",
                "项目状态摘要",
                $"进度 {project.Progress}% ，健康状态 {project.HealthStatus}，逾期任务 {project.OverdueTaskCount} 个，高风险 {project.HighRiskCount} 个，SPI {FormatRatio(project.SchedulePerformanceIndex)}，CPI {FormatRatio(project.CostPerformanceIndex)}。",
                project.HealthStatus == "健康" ? "保持当前节奏，持续跟踪下周里程碑和交付物。" : "请 PMO 与项目经理复核关键路径、阻塞任务、风险关闭计划和资源投入。"))
            .ToList();
        var metrics = dashboard.Metrics.Select(x => new { x.Label, x.Value, x.Unit }).ToList();
        var summary = $"生成 {focusProjects.Count} 条项目周度状态摘要，组合 SPI {dashboard.EarnedValueSummary.SchedulePerformanceIndex:0.00}，CPI {dashboard.EarnedValueSummary.CostPerformanceIndex:0.00}，经营预警 {businessOverview.WarningItems.Count} 条。";
        return Draft(job, "每周项目状态摘要", summary, focusProjects, new
        {
            metrics,
            earnedValue = dashboard.EarnedValueSummary,
            businessWarningCount = businessOverview.WarningItems.Count
        });
    }

    private static AiAutomationDraft BuildDelayedTaskDraft(AiJob job, ProjectPmoDashboardDto dashboard)
    {
        var maxItems = ResolveMaxItems(job, 20);
        var items = dashboard.ActionItems
            .Where(x => x.ActionKey == "task" && (x.Reason.Contains("逾期", StringComparison.OrdinalIgnoreCase) || x.Reason.Contains("到期", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(x => x.DueDate ?? DateTime.MaxValue)
            .Take(maxItems)
            .Select(action => DraftItem(
                action.ProjectId,
                action.ProjectName,
                action.Reason.Contains("逾期", StringComparison.OrdinalIgnoreCase) ? "Critical" : "Warning",
                action.Title,
                $"{action.Reason}，计划日期 {FormatDate(action.DueDate)}，当前状态 {action.Status}，优先级 {action.Level}。",
                "建议项目经理确认延期原因、更新恢复计划，并在需求与排期或任务明细中同步责任人与完成日期。"))
            .ToList();
        var summary = $"识别延期/临期任务 {items.Count} 条。";
        return Draft(job, "延期任务识别", summary, items, new
        {
            overdueTaskCount = dashboard.Projects.Sum(x => x.OverdueTaskCount),
            actionItemCount = dashboard.ActionItems.Count
        });
    }

    private static AiAutomationDraft BuildPerformanceExceptionDraft(
        AiJob job,
        ProjectPmoDashboardDto dashboard,
        BusinessOverviewDto businessOverview)
    {
        var maxItems = ResolveMaxItems(job, 20);
        var items = dashboard.Projects
            .Where(x =>
                (x.SchedulePerformanceIndex.HasValue && x.SchedulePerformanceIndex.Value > 0m && x.SchedulePerformanceIndex.Value < 0.9m) ||
                (x.CostPerformanceIndex.HasValue && x.CostPerformanceIndex.Value > 0m && x.CostPerformanceIndex.Value < 0.9m))
            .OrderBy(x => Math.Min(x.SchedulePerformanceIndex ?? 99m, x.CostPerformanceIndex ?? 99m))
            .Take(maxItems)
            .Select(project => DraftItem(
                project.Id,
                project.Name,
                (project.SchedulePerformanceIndex ?? 1m) < 0.75m || (project.CostPerformanceIndex ?? 1m) < 0.75m ? "Critical" : "Warning",
                "SPI/CPI 异常",
                $"SPI {FormatRatio(project.SchedulePerformanceIndex)}，CPI {FormatRatio(project.CostPerformanceIndex)}，PV {FormatAmount(project.PlannedValue)}，EV {FormatAmount(project.EarnedValue)}，成本 {FormatAmount(project.CostAmount)}。",
                "建议复核基线、挣值快照、实际成本和未完成任务，必要时发起变更或资源调配。"))
            .ToList();
        var businessWarnings = businessOverview.WarningItems
            .Where(x => x.Type.Contains("绩效", StringComparison.OrdinalIgnoreCase))
            .Select(x => new { x.ProjectId, x.ProjectName, x.Type, x.Level, x.Description })
            .ToList();
        var summary = $"识别 SPI/CPI 异常项目 {items.Count} 个，经营侧绩效预警 {businessWarnings.Count} 条。";
        return Draft(job, "SPI/CPI 异常识别", summary, items, new
        {
            earnedValue = dashboard.EarnedValueSummary,
            businessWarnings
        });
    }

    private static AiAutomationDraft BuildHighRiskDueDraft(AiJob job, ProjectPmoDashboardDto dashboard)
    {
        var maxItems = ResolveMaxItems(job, 20);
        var items = dashboard.ActionItems
            .Where(x =>
                x.ActionKey is "raid" or "issue" or "milestone" &&
                (x.Level is "P0" or "P1" || x.Reason.Contains("逾期", StringComparison.OrdinalIgnoreCase) || IsDueWithinDays(x.DueDate, 7)))
            .OrderByDescending(x => x.Level == "P0")
            .ThenBy(x => x.DueDate ?? DateTime.MaxValue)
            .Take(maxItems)
            .Select(action => DraftItem(
                action.ProjectId,
                action.ProjectName,
                action.Level == "P0" || action.Reason.Contains("逾期", StringComparison.OrdinalIgnoreCase) ? "Critical" : "Warning",
                action.Title,
                $"{action.SourceType}：{action.Reason}，截止 {FormatDate(action.DueDate)}，状态 {action.Status}，级别 {action.Level}。",
                "建议 PMO 确认关闭责任人、到期动作和升级路径，必要时进入治理行动队列。"))
            .ToList();
        var summary = $"识别高风险/到期治理项 {items.Count} 条。";
        return Draft(job, "高风险到期识别", summary, items, new
        {
            healthBuckets = dashboard.HealthBuckets,
            exceptionProjects = dashboard.ExceptionProjects.Select(x => new { x.Id, x.Name, x.HealthStatus, x.HighRiskCount, x.OverdueMilestoneCount })
        });
    }

    private static AiAutomationDraft BuildOverdueReceivableDraft(AiJob job, BusinessOverviewDto businessOverview)
    {
        var maxItems = ResolveMaxItems(job, 20);
        var riskWarnings = businessOverview.WarningItems
            .Where(x => x.Type.Contains("回款", StringComparison.OrdinalIgnoreCase) || x.ActionTab == "receivable")
            .OrderByDescending(x => x.Level == "P0")
            .ThenBy(x => x.ProjectCode)
            .Take(maxItems)
            .Select(warning => DraftItem(
                warning.ProjectId,
                warning.ProjectName,
                warning.Level == "P0" ? "Critical" : "Warning",
                warning.Type,
                warning.Description,
                "建议经营负责人复核回款计划、合同验收节点和客户沟通记录，并形成催收动作。"))
            .ToList();
        var summary = $"识别回款/经营收尾风险 {riskWarnings.Count} 条，经营总额口径：预计回款 {FormatAmount(businessOverview.YearEndReceivableForecast)}，已回款 {FormatAmount(businessOverview.ReceivedAmount)}。";
        return Draft(job, "逾期回款识别", summary, riskWarnings, new
        {
            businessOverview.ContractAmount,
            businessOverview.ReceivedAmount,
            businessOverview.YearEndReceivableForecast,
            businessOverview.ReceivableRiskProjectCount,
            businessOverview.TailClosingProjectCount
        });
    }

    private static AiAutomationDraft BuildBusinessMonthlySummaryDraft(AiJob job, BusinessOverviewDto businessOverview)
    {
        var maxItems = ResolveMaxItems(job, 12);
        var items = businessOverview.FocusProjects
            .Take(maxItems)
            .Select(item => DraftItem(
                item.ProjectId,
                item.ProjectName,
                item.Type.Contains("风险", StringComparison.OrdinalIgnoreCase) ? "Warning" : "Info",
                item.Type,
                item.Description,
                "建议经营例会确认负责人、目标日期和需要项目侧配合的输入材料。"))
            .ToList();
        var summary = $"生成经营月度摘要，合同 {FormatAmount(businessOverview.ContractAmount)}，已回款 {FormatAmount(businessOverview.ReceivedAmount)}，经营关注项目 {items.Count} 个。";
        return Draft(job, "经营管理月度摘要", summary, items, new
        {
            businessOverview.ContractAmount,
            businessOverview.CostAmount,
            businessOverview.GrossProfitRate,
            businessOverview.WarningItems
        });
    }

    private static DraftItemDto DraftItem(
        Guid projectId,
        string projectName,
        string level,
        string title,
        string reason,
        string suggestedAction)
    {
        return new DraftItemDto(projectId, projectName, level, title, reason, suggestedAction);
    }

    private static AiAutomationDraft Draft(
        AiJob job,
        string title,
        string summary,
        IReadOnlyList<DraftItemDto> items,
        object metrics)
    {
        var output = new
        {
            jobCode = job.JobCode,
            jobName = job.Name,
            jobType = job.JobType,
            generatedAt = DateTime.UtcNow,
            title,
            summary,
            outputTarget = TryDeserializeJson(job.OutputTargetJson),
            metrics,
            items,
            citations = items
                .Select(item => $"Project:{item.ProjectId}:{item.ProjectName}")
                .Distinct()
                .ToList(),
            handlingPolicy = "AI 自动化任务只生成草稿，不直接写回项目、经营或外部系统。"
        };
        return new AiAutomationDraft(Serialize(output), items.Count, summary);
    }

    private static string BuildInputSnapshotJson(
        AiJob job,
        string runMode,
        ProjectPmoDashboardDto pmoDashboard,
        BusinessOverviewDto businessOverview)
    {
        return Serialize(new
        {
            runMode,
            capturedAt = DateTime.UtcNow,
            job = new
            {
                job.Id,
                job.JobCode,
                job.Name,
                job.JobType,
                job.ContextScopeJson,
                job.OutputTargetJson,
                job.SchedulerJobKey
            },
            projectPortfolio = new
            {
                pmoDashboard.Metrics,
                pmoDashboard.EarnedValueSummary,
                projectCount = pmoDashboard.Projects.Count,
                actionItemCount = pmoDashboard.ActionItems.Count,
                exceptionProjectCount = pmoDashboard.ExceptionProjects.Count
            },
            business = new
            {
                businessOverview.ContractAmount,
                businessOverview.ReceivedAmount,
                businessOverview.YearEndReceivableForecast,
                businessOverview.ReceivableRiskProjectCount,
                warningCount = businessOverview.WarningItems.Count
            }
        });
    }

    private static bool ShouldRunForMode(AiJob job, string runMode, DateTime today)
    {
        if (!runMode.Equals("Scheduled", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var cadence = ResolveJsonString(job.ContextScopeJson, "scheduleCadence");
        if (string.IsNullOrWhiteSpace(cadence))
        {
            return true;
        }

        return cadence.Equals("Daily", StringComparison.OrdinalIgnoreCase) ||
            cadence.Equals("Weekday", StringComparison.OrdinalIgnoreCase) && today.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday ||
            cadence.Equals("WeeklyMonday", StringComparison.OrdinalIgnoreCase) && today.DayOfWeek == DayOfWeek.Monday ||
            cadence.Equals("MonthlyFirstDay", StringComparison.OrdinalIgnoreCase) && today.Day == 1 ||
            cadence.Equals("MonthlyFirstWorkday", StringComparison.OrdinalIgnoreCase) && IsFirstWorkday(today);
    }

    private static bool IsFirstWorkday(DateTime today)
    {
        if (today.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return false;
        }

        var firstDay = new DateTime(today.Year, today.Month, 1);
        while (firstDay.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            firstDay = firstDay.AddDays(1);
        }

        return today.Date == firstDay.Date;
    }

    private static int ResolveMaxItems(AiJob job, int defaultValue)
    {
        return Math.Clamp(ResolveJsonInt(job.ContextScopeJson, "maxItems") ?? defaultValue, 1, 100);
    }

    private static int? ResolveJsonInt(string json, string propertyName)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
                ? property.GetInt32()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string ResolveJsonString(string json, string propertyName)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            return document.RootElement.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
                ? property.GetString() ?? string.Empty
                : string.Empty;
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    private static object TryDeserializeJson(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<object>(string.IsNullOrWhiteSpace(json) ? "{}" : json, JsonOptions) ?? new { };
        }
        catch (JsonException)
        {
            return new { };
        }
    }

    private static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private static bool IsDueWithinDays(DateTime? dueDate, int days)
    {
        return dueDate.HasValue && dueDate.Value.Date <= DateTime.Today.AddDays(days);
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "未设置";
    }

    private static string FormatRatio(decimal? value)
    {
        return value.HasValue ? value.Value.ToString("0.00") : "-";
    }

    private static string FormatAmount(decimal? value)
    {
        return value.HasValue ? value.Value.ToString("0.##") : "-";
    }

    private static string NormalizeRunMode(string value)
    {
        return value.Equals("scheduled", StringComparison.OrdinalIgnoreCase) ? "Scheduled" : "Manual";
    }

    private static void ValidateJobInput(AiJobSaveInput input)
    {
        Check.NotNullOrWhiteSpace(input.JobCode, nameof(input.JobCode), 64);
        Check.NotNullOrWhiteSpace(input.Name, nameof(input.Name), 128);
        Check.NotNullOrWhiteSpace(input.JobType, nameof(input.JobType), 64);
        ValidateJsonObject(input.ContextScopeJson, "上下文范围配置");
        ValidateJsonObject(input.OutputTargetJson, "输出目标配置");
    }

    private static void ValidateJsonObject(string json, string label)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new UserFriendlyException($"{label} 必须是 JSON 对象。");
            }
        }
        catch (JsonException ex)
        {
            throw new UserFriendlyException($"{label} 不是合法 JSON：{ex.Message}");
        }
    }

    private async Task<Dictionary<Guid, string>> ResolveModelNamesAsync(IEnumerable<Guid?> modelIds)
    {
        var ids = modelIds
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToHashSet();
        if (ids.Count == 0)
        {
            return [];
        }

        return (await _modelRepository.GetListAsync(x => ids.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => x.DisplayName);
    }

    private static AiJobDto MapJob(AiJob item, IReadOnlyDictionary<Guid, string> modelNames)
    {
        return new AiJobDto
        {
            AssistantCode = item.AssistantCode,
            ContextScopeJson = item.ContextScopeJson,
            CreationTime = item.CreationTime,
            Description = item.Description,
            Id = item.Id,
            IsEnabled = item.IsEnabled,
            JobCode = item.JobCode,
            JobType = item.JobType,
            LastRunStatus = item.LastRunStatus,
            LastRunSummary = item.LastRunSummary,
            LastRunTime = item.LastRunTime,
            ModelId = item.ModelId,
            ModelName = item.ModelId.HasValue ? modelNames.GetValueOrDefault(item.ModelId.Value, string.Empty) : string.Empty,
            Name = item.Name,
            OutputTargetJson = item.OutputTargetJson,
            PromptTemplateCode = item.PromptTemplateCode,
            SchedulerJobKey = item.SchedulerJobKey
        };
    }

    private static AiJobRunDto MapRun(AiJobRun item)
    {
        return new AiJobRunDto
        {
            AiJobId = item.AiJobId,
            DraftItemCount = item.DraftItemCount,
            ElapsedMilliseconds = item.ElapsedMilliseconds,
            ErrorMessage = item.ErrorMessage,
            FinishedTime = item.FinishedTime,
            Id = item.Id,
            InputSnapshotJson = item.InputSnapshotJson,
            JobCode = item.JobCode,
            JobName = item.JobName,
            JobType = item.JobType,
            OutputDraftJson = item.OutputDraftJson,
            RunMode = item.RunMode,
            StartedTime = item.StartedTime,
            Status = item.Status
        };
    }

    private sealed record AiAutomationDraft(string OutputDraftJson, int ItemCount, string Summary);

    private sealed record DraftItemDto(
        Guid ProjectId,
        string ProjectName,
        string Level,
        string Title,
        string Reason,
        string SuggestedAction);
}
