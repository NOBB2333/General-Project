using System.Diagnostics;
using System.Text.Json;
using General.Admin.Permissions;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

public class AiToolRegistryService : ITransientDependency
{
    private const string BusinessContractBudgetReceivableToolCode = "business.contract.budget.receivable.read";
    private const string BusinessOverviewToolCode = "business.project.overview.read";
    private const string ProjectChangeToolCode = "project.change.read";
    private const string ProjectDocumentsToolCode = "project.documents.read";
    private const string ProjectEvmToolCode = "project.evm.read";
    private const string ProjectMeetingToolCode = "project.meeting.read";
    private const string ProjectOverviewToolCode = "project.overview.read";
    private const string ProjectQualityToolCode = "project.quality.read";
    private const string ProjectRaidToolCode = "project.raid.read";
    private const string ProjectScheduleToolCode = "project.schedule.milestones.read";
    private const string ProjectWbsTaskToolCode = "project.wbs.tasks.read";
    private const string WebSearchToolCode = "web.search";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly BusinessService _businessService;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiToolDefinition, Guid> _toolDefinitionRepository;
    private readonly IRepository<AiToolInvocation, Guid> _toolInvocationRepository;
    private readonly AiUsageService _usageService;
    private readonly AiWebSearchService _webSearchService;
    private readonly IPermissionChecker _permissionChecker;
    private readonly ProjectQueryService _projectQueryService;

    public AiToolRegistryService(
        BusinessService businessService,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IRepository<AiToolDefinition, Guid> toolDefinitionRepository,
        IRepository<AiToolInvocation, Guid> toolInvocationRepository,
        AiUsageService usageService,
        AiWebSearchService webSearchService,
        IPermissionChecker permissionChecker,
        ProjectQueryService projectQueryService)
    {
        _businessService = businessService;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _toolDefinitionRepository = toolDefinitionRepository;
        _toolInvocationRepository = toolInvocationRepository;
        _usageService = usageService;
        _webSearchService = webSearchService;
        _permissionChecker = permissionChecker;
        _projectQueryService = projectQueryService;
    }

    public async Task<List<AiToolDefinitionDto>> GetToolDefinitionsAsync()
    {
        await EnsureBuiltinDefinitionsAsync();
        return (await _toolDefinitionRepository.GetListAsync())
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(MapDefinition)
            .ToList();
    }

    public async Task<List<AiGatewayToolDefinition>> GetGatewayToolDefinitionsAsync()
    {
        return (await GetToolDefinitionsAsync())
            .Where(x => x.IsEnabled)
            .OrderBy(x => x.Sort)
            .Select(x => new AiGatewayToolDefinition(
                ToGatewayToolName(x.ToolCode),
                $"{x.Name}。{x.Description}".Trim('。'),
                x.InputSchemaJson))
            .ToList();
    }

    public async Task<AiToolInvokeResultDto> InvokeToolAsync(string toolCode, AiToolInvokeInput input)
    {
        var definition = await ResolveDefinitionAsync(toolCode);
        if (!definition.IsEnabled)
        {
            throw new UserFriendlyException($"AI 工具已停用：{definition.Name}");
        }

        await EnsureToolPermissionAsync(definition);

        var inputJson = NormalizeJson(input.InputJson, "{}");
        var invocation = new AiToolInvocation(
            _guidGenerator.Create(),
            definition.ToolCode,
            inputJson,
            definition.TimeoutSeconds,
            _currentTenant.Id,
            input.ConversationId,
            input.MessageId);
        await _toolInvocationRepository.InsertAsync(invocation, autoSave: true);

        var stopwatch = Stopwatch.StartNew();
        try
        {
            var handlerTask = InvokeBuiltinToolAsync(definition.ToolCode, inputJson);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(definition.TimeoutSeconds));
            if (await Task.WhenAny(handlerTask, timeoutTask) != handlerTask)
            {
                stopwatch.Stop();
                invocation.Fail("Timeout", $"工具调用超过 {definition.TimeoutSeconds} 秒未返回。", stopwatch.ElapsedMilliseconds);
                await _toolInvocationRepository.UpdateAsync(invocation, autoSave: true);
                await RecordToolUsageAsync(definition, invocation, "Timeout");
                return MapInvocationResult(invocation);
            }

            var result = await handlerTask;
            stopwatch.Stop();
            invocation.Complete(result.OutputJson, Serialize(result.SourceReferences), stopwatch.ElapsedMilliseconds);
            await _toolInvocationRepository.UpdateAsync(invocation, autoSave: true);
            await RecordToolUsageAsync(definition, invocation, "Completed");
            return MapInvocationResult(invocation);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            invocation.Fail("Failed", ex.Message, stopwatch.ElapsedMilliseconds);
            await _toolInvocationRepository.UpdateAsync(invocation, autoSave: true);
            await RecordToolUsageAsync(definition, invocation, "Failed");
            return MapInvocationResult(invocation);
        }
    }

    public async Task<List<AiToolInvocationDto>> GetToolInvocationsAsync(AiToolInvocationQueryInput input)
    {
        var invocations = await _toolInvocationRepository.GetListAsync();
        if (input.ConversationId.HasValue)
        {
            invocations = invocations.Where(x => x.ConversationId == input.ConversationId.Value).ToList();
        }

        if (input.MessageId.HasValue)
        {
            invocations = invocations.Where(x => x.MessageId == input.MessageId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.ToolCode))
        {
            var toolCode = input.ToolCode.Trim();
            invocations = invocations.Where(x => x.ToolCode.Equals(toolCode, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var status = input.Status.Trim();
            invocations = invocations.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 200);
        return invocations
            .OrderByDescending(x => x.CreationTime)
            .Take(maxResultCount)
            .Select(MapInvocation)
            .ToList();
    }

    public async Task<AiConversationToolContextDto> BuildConversationContextAsync(AiConversation conversation)
    {
        if (!Guid.TryParse(conversation.ContextId, out var projectId))
        {
            return new AiConversationToolContextDto();
        }

        var toolCode = ResolveContextToolCode(conversation.ContextType);
        if (string.IsNullOrWhiteSpace(toolCode))
        {
            return new AiConversationToolContextDto();
        }

        var result = await InvokeToolAsync(toolCode, new AiToolInvokeInput
        {
            ConversationId = conversation.Id,
            InputJson = Serialize(new { projectId })
        });
        if (!result.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            return new AiConversationToolContextDto();
        }

        var output = result.OutputJson.Length <= 5000 ? result.OutputJson : result.OutputJson[..5000];
        return new AiConversationToolContextDto
        {
            Citations = result.SourceReferences.Select(FormatCitation).Distinct().Take(12).ToList(),
            ContextPrompt = $"""
                             以下为系统通过只读工具读取的上下文。回答时必须基于这些来源，并在结论中保留可追溯引用。
                             工具：{result.ToolCode}
                             来源：{string.Join("；", result.SourceReferences.Take(12).Select(FormatCitation))}
                             数据：
                             {output}
                             """
        };
    }

    private async Task EnsureBuiltinDefinitionsAsync()
    {
        var existingDefinitions = await _toolDefinitionRepository.GetListAsync();
        foreach (var builtin in BuildBuiltinDefinitions())
        {
            var matched = existingDefinitions
                .FirstOrDefault(x => x.ToolCode.Equals(builtin.ToolCode, StringComparison.OrdinalIgnoreCase));
            if (matched == null)
            {
                await _toolDefinitionRepository.InsertAsync(new AiToolDefinition(
                    _guidGenerator.Create(),
                    builtin.ToolCode,
                    builtin.Name,
                    builtin.Description,
                    builtin.Category,
                    builtin.InputSchemaJson,
                    builtin.OutputSchemaJson,
                    builtin.PermissionCode,
                    _currentTenant.Id,
                    timeoutSeconds: builtin.TimeoutSeconds,
                    sort: builtin.Sort), autoSave: true);
                continue;
            }

            matched.UpdateMetadata(
                builtin.Name,
                builtin.Description,
                builtin.Category,
                builtin.InputSchemaJson,
                builtin.OutputSchemaJson,
                builtin.PermissionCode,
                builtin.TimeoutSeconds,
                builtin.Sort);
            await _toolDefinitionRepository.UpdateAsync(matched, autoSave: true);
        }
    }

    private async Task<AiToolDefinition> ResolveDefinitionAsync(string toolCode)
    {
        await EnsureBuiltinDefinitionsAsync();
        var normalizedToolCode = Check.NotNullOrWhiteSpace(toolCode, nameof(toolCode), 128).Trim();
        var definitions = await _toolDefinitionRepository.GetListAsync(x => x.ToolCode == normalizedToolCode);
        var definition = definitions
            .OrderByDescending(x => x.TenantId == _currentTenant.Id)
            .ThenByDescending(x => x.CreationTime)
            .FirstOrDefault();
        return definition ?? throw new UserFriendlyException($"AI 工具不存在：{normalizedToolCode}");
    }

    private async Task EnsureToolPermissionAsync(AiToolDefinition definition)
    {
        var permissionCode = string.IsNullOrWhiteSpace(definition.PermissionCode)
            ? AdminPermissions.Ai.ConversationUse
            : definition.PermissionCode;
        if (!await _permissionChecker.IsGrantedAsync(permissionCode))
        {
            throw new AbpAuthorizationException($"当前用户没有调用 AI 工具的权限：{permissionCode}");
        }
    }

    private async Task<ToolExecutionResult> InvokeBuiltinToolAsync(string toolCode, string inputJson)
    {
        return toolCode switch
        {
            ProjectOverviewToolCode => await BuildProjectOverviewAsync(inputJson),
            ProjectWbsTaskToolCode => await BuildProjectWbsTasksAsync(inputJson),
            ProjectScheduleToolCode => await BuildProjectScheduleAsync(inputJson),
            ProjectRaidToolCode => await BuildProjectRaidAsync(inputJson),
            ProjectQualityToolCode => await BuildProjectQualityAsync(inputJson),
            ProjectChangeToolCode => await BuildProjectChangeAsync(inputJson),
            ProjectDocumentsToolCode => await BuildProjectDocumentsAsync(inputJson),
            ProjectEvmToolCode => await BuildProjectEvmAsync(inputJson),
            ProjectMeetingToolCode => await BuildProjectMeetingsAsync(inputJson),
            BusinessOverviewToolCode => await BuildBusinessOverviewAsync(inputJson),
            BusinessContractBudgetReceivableToolCode => await BuildBusinessContractBudgetReceivableAsync(inputJson),
            WebSearchToolCode => await BuildWebSearchAsync(inputJson),
            _ => throw new UserFriendlyException($"AI 工具未实现：{toolCode}")
        };
    }

    private async Task RecordToolUsageAsync(AiToolDefinition definition, AiToolInvocation invocation, string status)
    {
        await _usageService.RecordUsageAsync(new AiUsageRecordInput
        {
            AuditSummary = $"内置 AI 工具调用：{definition.Name}",
            ConversationId = invocation.ConversationId,
            ElapsedMilliseconds = invocation.ElapsedMilliseconds,
            ErrorMessage = invocation.ErrorMessage,
            ProjectId = ReadProjectIdOrNull(invocation.InputJson),
            RequestPreview = invocation.InputJson,
            ResponsePreview = invocation.OutputJson,
            Scenario = definition.Category,
            Source = "BuiltinTool",
            Status = status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ? "Success" : status,
            ToolCode = invocation.ToolCode
        });
    }

    private async Task<ToolExecutionResult> BuildProjectOverviewAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            detail.Id,
            detail.ProjectCode,
            detail.Name,
            detail.Status,
            detail.Priority,
            detail.ProjectType,
            detail.ProjectSource,
            detail.ManagerName,
            detail.SponsorName,
            detail.OrganizationUnitName,
            detail.PlannedStartDate,
            detail.PlannedEndDate,
            detail.TaskCompletionRate,
            detail.HealthStatus,
            detail.HealthRiskScore,
            detail.HealthReasons,
            detail.HighRiskCount,
            detail.OpenIssueCount,
            detail.MemberCount,
            detail.TotalWorklogHours,
            detail.ContractTotalAmount,
            detail.BudgetTotalAmount,
            detail.ReceivedAmount
        };
        return Result(data, [ProjectReference(detail, "Project", "项目概览")]);
    }

    private async Task<ToolExecutionResult> BuildProjectWbsTasksAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            WbsNodes = detail.WbsNodes.Select(x => new
            {
                x.Id,
                x.WbsCode,
                x.Name,
                x.Type,
                x.Status,
                x.ParentId,
                x.OwnerName,
                x.EstimatedWorkHours,
                x.ActualWorkHours,
                x.CompletionRate,
                x.TaskCount,
                x.RequirementCount,
                x.ScopeDescription,
                x.DeliveryDescription
            }).Take(200),
            Tasks = detail.Tasks.Select(x => new
            {
                x.Id,
                x.TaskCode,
                x.Title,
                x.Status,
                x.Priority,
                x.WbsCode,
                x.WbsNodeName,
                x.RequirementTitle,
                x.OwnerName,
                x.DeveloperOwnerNames,
                x.ProductOwnerNames,
                x.TesterOwnerNames,
                x.PlannedStartTime,
                x.PlannedEndTime,
                x.EstimatedWorkHours,
                x.ActualWorkHours,
                x.IsBlocked,
                x.BlockReason
            }).Take(200),
            Dependencies = detail.TaskDependencies.Take(200)
        };
        return Result(data, BuildItemReferences(detail, "WBS/任务", detail.WbsNodes.Select(x => (x.Id, x.Name))).ToList());
    }

    private async Task<ToolExecutionResult> BuildProjectScheduleAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            Schedules = detail.Sprints.Select(x => new
            {
                x.Id,
                x.SprintCode,
                x.Name,
                x.Status,
                x.StartDate,
                x.EndDate,
                x.OwnerName,
                x.Progress,
                x.TaskCount,
                x.ClosedTaskCount,
                x.OverdueTaskCount,
                x.MilestoneCount,
                x.ClosedMilestoneCount,
                x.OverdueMilestoneCount,
                x.HighRiskCount,
                x.Summary
            }),
            Milestones = detail.Milestones.Select(x => new
            {
                x.Id,
                x.Name,
                x.Status,
                x.SprintName,
                x.OwnerName,
                x.PlannedCompletionDate,
                x.ActualCompletionDate
            })
        };
        var refs = BuildItemReferences(detail, "排期", detail.Sprints.Select(x => (x.Id, x.Name)))
            .Concat(BuildItemReferences(detail, "里程碑", detail.Milestones.Select(x => (x.Id, x.Name))))
            .ToList();
        return Result(data, refs);
    }

    private async Task<ToolExecutionResult> BuildProjectRaidAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            RaidItems = detail.RaidItems.Select(x => new
            {
                x.Id,
                x.Type,
                x.Title,
                x.Status,
                x.Level,
                x.OwnerName,
                x.Probability,
                x.Impact,
                x.RiskScore,
                x.PlannedResolveDate,
                x.IsOverdue,
                x.TriggerCondition,
                x.ResponseStrategy,
                x.ContingencyPlan
            }),
            Issues = detail.Issues.Select(x => new
            {
                x.Id,
                x.Title,
                x.Status,
                x.Type,
                x.Level,
                x.OwnerName,
                x.DueDate,
                x.IsOverdue,
                x.RequirementTitle,
                x.QualityChecklistTitle,
                x.AcceptanceCriteriaTitle
            }).Take(100)
        };
        return Result(data, BuildItemReferences(detail, "RAID", detail.RaidItems.Select(x => (x.Id, x.Title))).ToList());
    }

    private async Task<ToolExecutionResult> BuildProjectQualityAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            Checklists = detail.QualityChecklists,
            Inspections = detail.QualityInspections
        };
        var refs = BuildItemReferences(detail, "质量检查项", detail.QualityChecklists.Select(x => (x.Id, x.Title)))
            .Concat(BuildItemReferences(detail, "质量检查记录", detail.QualityInspections.Select(x => (x.Id, x.QualityChecklistTitle))))
            .ToList();
        return Result(data, refs);
    }

    private async Task<ToolExecutionResult> BuildProjectChangeAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            ChangeRequests = detail.ChangeRequests.Select(x => new
            {
                x.Id,
                x.RequestCode,
                x.Title,
                x.Status,
                x.ChangeType,
                x.ApplicantName,
                x.ImpactScope,
                x.Reason,
                x.ExpectedResult,
                x.Impacts,
                x.Approvals
            })
        };
        return Result(data, BuildItemReferences(detail, "变更请求", detail.ChangeRequests.Select(x => (x.Id, $"{x.RequestCode} {x.Title}"))).ToList());
    }

    private async Task<ToolExecutionResult> BuildProjectDocumentsAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            Documents = detail.Documents.Select(x => new
            {
                x.Id,
                x.Title,
                x.Category,
                x.Version,
                x.Status,
                x.OwnerName,
                x.Summary
            }).Take(200),
            Reports = detail.Reports.Select(x => new
            {
                x.Id,
                x.Title,
                x.ReportType,
                x.Status,
                x.AuthorName,
                x.SprintName,
                x.PeriodStart,
                x.PeriodEnd,
                x.CompletedSummary,
                x.NextPlan,
                x.RiskIssueSummary,
                x.MilestoneSummary,
                x.HoursSummary,
                x.BusinessSummary,
                x.AttachmentFileId,
                x.AttachmentFileName
            }).Take(120),
            Meetings = detail.MeetingRecords.Select(x => new
            {
                x.Id,
                x.Topic,
                x.Status,
                x.MeetingTime,
                x.FacilitatorName,
                x.ParticipantNames,
                x.Decisions,
                x.Minutes,
                x.AttachmentFileId,
                x.AttachmentFileName
            }).Take(120)
        };
        var refs = BuildItemReferences(detail, "项目文档", detail.Documents.Select(x => (x.Id, x.Title)))
            .Concat(BuildItemReferences(detail, "项目周报", detail.Reports.Select(x => (x.Id, x.Title))))
            .Concat(BuildItemReferences(detail, "会议纪要", detail.MeetingRecords.Select(x => (x.Id, x.Topic))))
            .ToList();
        return Result(data, refs);
    }

    private async Task<ToolExecutionResult> BuildProjectEvmAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            CurrentSnapshot = detail.PerformanceSnapshot,
            Snapshots = detail.PerformanceSnapshots.Select(x => new
            {
                x.Id,
                x.SnapshotDate,
                x.SnapshotPeriod,
                x.PlannedValue,
                x.EarnedValue,
                x.ActualCost,
                x.SchedulePerformanceIndex,
                x.CostPerformanceIndex
            }).Take(60)
        };
        return Result(data, BuildItemReferences(detail, "EVM 快照", detail.PerformanceSnapshots.Select(x => (x.Id, $"{x.SnapshotPeriod} {x.SnapshotDate:yyyy-MM-dd}"))).ToList());
    }

    private async Task<ToolExecutionResult> BuildProjectMeetingsAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var data = new
        {
            Project = ProjectSummary(detail),
            Meetings = detail.MeetingRecords.Select(x => new
            {
                x.Id,
                x.Topic,
                x.Status,
                x.MeetingTime,
                x.FacilitatorName,
                x.ParticipantNames,
                x.Decisions,
                x.Minutes,
                x.ActionItems
            }).Take(100)
        };
        return Result(data, BuildItemReferences(detail, "会议纪要", detail.MeetingRecords.Select(x => (x.Id, x.Topic))).ToList());
    }

    private async Task<ToolExecutionResult> BuildBusinessOverviewAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _businessService.GetProjectDetailAsync(projectId);
        var data = new
        {
            detail.CanViewProfitAnalysis,
            detail.CanViewSensitiveAmounts,
            detail.Summary
        };
        return Result(data, [BusinessReference(detail, "经营项目概览")]);
    }

    private async Task<ToolExecutionResult> BuildBusinessContractBudgetReceivableAsync(string inputJson)
    {
        var projectId = ReadProjectId(inputJson);
        var detail = await _businessService.GetProjectDetailAsync(projectId);
        var data = new
        {
            Project = detail.Summary,
            detail.CanViewProfitAnalysis,
            detail.CanViewSensitiveAmounts,
            Contracts = detail.Contracts,
            BudgetExecutions = detail.BudgetExecutions,
            Receivables = detail.Receivables,
            Invoices = detail.Invoices,
            Procurements = detail.Procurements,
            Acceptances = detail.Acceptances
        };
        var refs = new List<AiSourceReferenceDto> { BusinessReference(detail, "经营链摘要") };
        refs.AddRange(detail.Contracts.Select(x => BusinessItemReference(detail, "合同", x.Id, $"{x.ContractCode} {x.Title}")));
        refs.AddRange(detail.Receivables.Select(x => BusinessItemReference(detail, "回款", x.Id, $"{x.ReceivableCode} {x.Title}")));
        refs.AddRange(detail.BudgetExecutions.Select(x => BusinessItemReference(detail, "预算执行", x.Id, $"{x.BudgetCode} {x.Category}")));
        return Result(data, refs);
    }

    private async Task<ToolExecutionResult> BuildWebSearchAsync(string inputJson)
    {
        using var document = JsonDocument.Parse(NormalizeJson(inputJson, "{}"));
        var root = document.RootElement;
        var query = ReadString(root, "query");
        if (string.IsNullOrWhiteSpace(query))
        {
            throw new UserFriendlyException("联网搜索工具调用缺少 query。");
        }

        var result = await _webSearchService.SearchAsync(new AiWebSearchInput
        {
            ConfigId = TryReadGuid(root, "configId", out var configId) ? configId : null,
            ConversationId = TryReadGuid(root, "conversationId", out var conversationId) ? conversationId : null,
            MaxResults = ReadInt(root, "maxResults", 5),
            MessageId = TryReadGuid(root, "messageId", out var messageId) ? messageId : null,
            Query = query
        });

        return Result(result, [
            new AiSourceReferenceDto
            {
                SourceType = "WebSearch",
                SourceId = result.RecordId.ToString(),
                Title = $"联网搜索：{query}",
                Path = "#/ai/web-search"
            }
        ]);
    }

    private static Guid ReadProjectId(string inputJson)
    {
        using var document = JsonDocument.Parse(NormalizeJson(inputJson, "{}"));
        if (TryReadGuid(document.RootElement, "projectId", out var projectId) ||
            TryReadGuid(document.RootElement, "ProjectId", out projectId))
        {
            return projectId;
        }

        throw new UserFriendlyException("AI 工具调用缺少 projectId。");
    }

    private static Guid? ReadProjectIdOrNull(string inputJson)
    {
        try
        {
            return ReadProjectId(inputJson);
        }
        catch
        {
            return null;
        }
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return string.Empty;
        }

        return property.ValueKind == JsonValueKind.String ? property.GetString() ?? string.Empty : property.ToString();
    }

    private static int ReadInt(JsonElement element, string propertyName, int defaultValue)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return defaultValue;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
        {
            return value;
        }

        return int.TryParse(property.ToString(), out value) ? value : defaultValue;
    }

    private static bool TryReadGuid(JsonElement element, string propertyName, out Guid value)
    {
        value = Guid.Empty;
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.String && Guid.TryParse(property.GetString(), out value))
        {
            return true;
        }

        return property.ValueKind == JsonValueKind.Undefined && Guid.TryParse(property.ToString(), out value);
    }

    private static object ProjectSummary(ProjectDetailDto detail)
    {
        return new
        {
            detail.Id,
            detail.ProjectCode,
            detail.Name,
            detail.Status,
            detail.ManagerName,
            detail.TaskCompletionRate,
            detail.HealthStatus
        };
    }

    private static AiSourceReferenceDto ProjectReference(ProjectDetailDto detail, string sourceType, string title)
    {
        return new AiSourceReferenceDto
        {
            SourceType = sourceType,
            SourceId = detail.Id.ToString(),
            Title = $"{detail.ProjectCode} {detail.Name} · {title}",
            Path = $"#/project/detail/{detail.Id}"
        };
    }

    private static AiSourceReferenceDto BusinessReference(BusinessProjectDetailDto detail, string title)
    {
        return new AiSourceReferenceDto
        {
            SourceType = "BusinessProject",
            SourceId = detail.Summary.Id.ToString(),
            Title = $"{detail.Summary.ProjectCode} {detail.Summary.Name} · {title}",
            Path = $"#/business/projects/{detail.Summary.Id}"
        };
    }

    private static AiSourceReferenceDto BusinessItemReference(BusinessProjectDetailDto detail, string sourceType, Guid sourceId, string title)
    {
        return new AiSourceReferenceDto
        {
            SourceType = sourceType,
            SourceId = sourceId.ToString(),
            Title = $"{detail.Summary.ProjectCode} · {title}",
            Path = $"#/business/projects/{detail.Summary.Id}"
        };
    }

    private static IEnumerable<AiSourceReferenceDto> BuildItemReferences(
        ProjectDetailDto detail,
        string sourceType,
        IEnumerable<(Guid Id, string Title)> items)
    {
        yield return ProjectReference(detail, "Project", sourceType);
        foreach (var item in items.Take(50))
        {
            yield return new AiSourceReferenceDto
            {
                SourceType = sourceType,
                SourceId = item.Id.ToString(),
                Title = $"{detail.ProjectCode} · {item.Title}",
                Path = $"#/project/detail/{detail.Id}"
            };
        }
    }

    private static ToolExecutionResult Result(object data, List<AiSourceReferenceDto> references)
    {
        return new ToolExecutionResult(Serialize(data), references);
    }

    private static string ResolveContextToolCode(string contextType)
    {
        return contextType.Trim().ToLowerInvariant() switch
        {
            "project" => ProjectOverviewToolCode,
            "business" => BusinessOverviewToolCode,
            "risk" => ProjectRaidToolCode,
            _ => string.Empty
        };
    }

    private static string FormatCitation(AiSourceReferenceDto item)
    {
        return $"{item.SourceType}:{item.SourceId} {item.Title}".Trim();
    }

    private static AiToolDefinitionDto MapDefinition(AiToolDefinition item)
    {
        return new AiToolDefinitionDto
        {
            Category = item.Category,
            CreationTime = item.CreationTime,
            Description = item.Description,
            Id = item.Id,
            InputSchemaJson = item.InputSchemaJson,
            IsEnabled = item.IsEnabled,
            Name = item.Name,
            OutputSchemaJson = item.OutputSchemaJson,
            PermissionCode = item.PermissionCode,
            Sort = item.Sort,
            TimeoutSeconds = item.TimeoutSeconds,
            ToolCode = item.ToolCode
        };
    }

    private static AiToolInvokeResultDto MapInvocationResult(AiToolInvocation item)
    {
        return new AiToolInvokeResultDto
        {
            ElapsedMilliseconds = item.ElapsedMilliseconds,
            ErrorMessage = item.ErrorMessage,
            InvocationId = item.Id,
            OutputJson = item.OutputJson,
            SourceReferences = DeserializeReferences(item.SourceReferencesJson),
            Status = item.Status,
            ToolCode = item.ToolCode
        };
    }

    private static AiToolInvocationDto MapInvocation(AiToolInvocation item)
    {
        return new AiToolInvocationDto
        {
            ConversationId = item.ConversationId,
            CreationTime = item.CreationTime,
            ElapsedMilliseconds = item.ElapsedMilliseconds,
            ErrorMessage = item.ErrorMessage,
            InputJson = item.InputJson,
            InvocationId = item.Id,
            MessageId = item.MessageId,
            OutputJson = item.OutputJson,
            SourceReferences = DeserializeReferences(item.SourceReferencesJson),
            Status = item.Status,
            TimeoutSeconds = item.TimeoutSeconds,
            ToolCode = item.ToolCode
        };
    }

    private static List<AiSourceReferenceDto> DeserializeReferences(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<AiSourceReferenceDto>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, JsonOptions);
    }

    private static string NormalizeJson(string? json, string defaultValue)
    {
        var normalized = string.IsNullOrWhiteSpace(json) ? defaultValue : json.Trim();
        using var _ = JsonDocument.Parse(normalized);
        return normalized;
    }

    public static string FromGatewayToolName(string toolName)
    {
        return toolName.Replace("__dot__", ".", StringComparison.Ordinal);
    }

    private static string ToGatewayToolName(string toolCode)
    {
        return toolCode.Replace(".", "__dot__", StringComparison.Ordinal);
    }

    private static IReadOnlyList<BuiltinToolDefinition> BuildBuiltinDefinitions()
    {
        const string projectInputSchema = """
                                          {
                                            "type": "object",
                                            "required": ["projectId"],
                                            "properties": {
                                              "projectId": { "type": "string", "format": "uuid", "description": "项目ID" }
                                            }
                                          }
                                          """;
        const string outputSchema = """
                                    {
                                      "type": "object",
                                      "description": "工具返回精简业务数据和引用来源"
                                    }
                                    """;
        const string webSearchInputSchema = """
                                            {
                                              "type": "object",
                                              "required": ["query"],
                                              "properties": {
                                                "query": { "type": "string", "description": "联网搜索关键词或问题" },
                                                "maxResults": { "type": "integer", "description": "返回结果上限，1-20" },
                                                "configId": { "type": "string", "format": "uuid", "description": "指定联网搜索 Provider 配置，可选" }
                                              }
                                            }
                                            """;

        return
        [
            new BuiltinToolDefinition(ProjectOverviewToolCode, "读取项目概览", "读取项目基础信息、健康度、进度、工时和关键金额。", "Project", projectInputSchema, outputSchema, 10, 10),
            new BuiltinToolDefinition(ProjectWbsTaskToolCode, "读取 WBS 与任务", "读取 WBS、任务、负责人、计划日期、阻塞和依赖信息。", "Project", projectInputSchema, outputSchema, 15, 20),
            new BuiltinToolDefinition(ProjectScheduleToolCode, "读取排期与里程碑", "读取排期、里程碑、延期和风险统计。", "Project", projectInputSchema, outputSchema, 15, 30),
            new BuiltinToolDefinition(ProjectRaidToolCode, "读取 RAID", "读取风险、问题、行动项、决策和开放问题。", "Project", projectInputSchema, outputSchema, 15, 40),
            new BuiltinToolDefinition(ProjectQualityToolCode, "读取质量检查", "读取质量检查项和检查记录。", "Project", projectInputSchema, outputSchema, 15, 50),
            new BuiltinToolDefinition(ProjectChangeToolCode, "读取变更请求", "读取变更请求、影响和审批记录。", "Project", projectInputSchema, outputSchema, 15, 60),
            new BuiltinToolDefinition(ProjectDocumentsToolCode, "读取项目文档", "读取项目文档、项目周报、会议纪要和附件索引摘要。", "Project", projectInputSchema, outputSchema, 15, 70),
            new BuiltinToolDefinition(ProjectEvmToolCode, "读取 EVM 快照", "读取 PV、EV、AC、SPI、CPI 等绩效快照。", "Project", projectInputSchema, outputSchema, 15, 80),
            new BuiltinToolDefinition(ProjectMeetingToolCode, "读取会议纪要", "读取会议纪要、决策和行动项。", "Project", projectInputSchema, outputSchema, 15, 90),
            new BuiltinToolDefinition(BusinessOverviewToolCode, "读取经营项目概览", "读取项目经营汇总、合同额、回款、成本、利润和 EVM 指标。", "Business", projectInputSchema, outputSchema, 15, 100),
            new BuiltinToolDefinition(BusinessContractBudgetReceivableToolCode, "读取合同预算回款摘要", "读取合同、预算执行、回款、发票、采购和验收摘要。", "Business", projectInputSchema, outputSchema, 20, 110),
            new BuiltinToolDefinition(WebSearchToolCode, "联网搜索", "调用企业已配置的联网搜索 Provider，并保存搜索审计记录。不得发送客户、合同、人员和敏感项目原文。", "Knowledge", webSearchInputSchema, outputSchema, 30, 120)
        ];
    }

    private sealed record BuiltinToolDefinition(
        string ToolCode,
        string Name,
        string Description,
        string Category,
        string InputSchemaJson,
        string OutputSchemaJson,
        int TimeoutSeconds,
        int Sort,
        string PermissionCode = AdminPermissions.Ai.ConversationUse);

    private sealed record ToolExecutionResult(string OutputJson, List<AiSourceReferenceDto> SourceReferences);
}
