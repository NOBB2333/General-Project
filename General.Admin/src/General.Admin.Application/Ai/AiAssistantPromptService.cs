using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

public class AiAssistantPromptService : ITransientDependency
{
    private const string BusinessContractBudgetReceivableToolCode = "business.contract.budget.receivable.read";
    private const string BusinessOverviewToolCode = "business.project.overview.read";
    private const string ProjectChangeToolCode = "project.change.read";
    private const string ProjectEvmToolCode = "project.evm.read";
    private const string ProjectMeetingToolCode = "project.meeting.read";
    private const string ProjectOverviewToolCode = "project.overview.read";
    private const string ProjectQualityToolCode = "project.quality.read";
    private const string ProjectRaidToolCode = "project.raid.read";
    private const string ProjectScheduleToolCode = "project.schedule.milestones.read";
    private const string ProjectWbsTaskToolCode = "project.wbs.tasks.read";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IRepository<AiAssistant, Guid> _assistantRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiModel, Guid> _modelRepository;
    private readonly IRepository<AiPromptTemplate, Guid> _promptTemplateRepository;

    public AiAssistantPromptService(
        IRepository<AiAssistant, Guid> assistantRepository,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IRepository<AiModel, Guid> modelRepository,
        IRepository<AiPromptTemplate, Guid> promptTemplateRepository)
    {
        _assistantRepository = assistantRepository;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _modelRepository = modelRepository;
        _promptTemplateRepository = promptTemplateRepository;
    }

    public async Task<List<AiAssistantDto>> GetAssistantsAsync(bool includeDisabled = true)
    {
        await EnsureBuiltinAssistantsAsync();
        var models = await ResolveModelNamesAsync();
        return (await _assistantRepository.GetListAsync())
            .Where(x => includeDisabled || x.IsEnabled)
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(item => MapAssistant(item, models))
            .ToList();
    }

    public async Task<AiAssistantDto> CreateAssistantAsync(AiAssistantSaveInput input)
    {
        await EnsureAssistantCodeAvailableAsync(input.AssistantCode);
        var entity = new AiAssistant(
            _guidGenerator.Create(),
            input.AssistantCode,
            input.Name,
            input.RoleType,
            input.SystemPrompt,
            _currentTenant.Id,
            input.DefaultMode,
            input.DefaultModelId,
            input.Description,
            Serialize(input.ToolCodes),
            input.ConfigJson,
            input.IsEnabled,
            sort: input.Sort);
        await _assistantRepository.InsertAsync(entity, autoSave: true);
        return MapAssistant(entity, await ResolveModelNamesAsync());
    }

    public async Task<AiAssistantDto> UpdateAssistantAsync(Guid id, AiAssistantSaveInput input)
    {
        var entity = await _assistantRepository.GetAsync(id);
        if (!entity.AssistantCode.Equals(input.AssistantCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("助手编码创建后不能修改。");
        }

        entity.Update(
            input.Name,
            input.RoleType,
            input.SystemPrompt,
            input.DefaultMode,
            input.DefaultModelId,
            input.Description,
            Serialize(input.ToolCodes),
            input.ConfigJson,
            input.IsEnabled,
            input.Sort);
        await _assistantRepository.UpdateAsync(entity, autoSave: true);
        return MapAssistant(entity, await ResolveModelNamesAsync());
    }

    public async Task DeleteAssistantAsync(Guid id)
    {
        var entity = await _assistantRepository.GetAsync(id);
        if (entity.IsBuiltin)
        {
            throw new UserFriendlyException("内置助手不能删除，可停用或调整提示词。");
        }

        await _assistantRepository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<List<AiPromptTemplateDto>> GetPromptTemplatesAsync(AiPromptTemplateQueryInput input)
    {
        await EnsureBuiltinAssistantsAsync();
        await EnsureBuiltinPromptTemplatesAsync();
        var templates = await _promptTemplateRepository.GetListAsync();
        if (!input.IncludeDisabled)
        {
            templates = templates.Where(x => x.IsEnabled).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            templates = templates.Where(x =>
                x.TemplateCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                x.Description.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            var category = input.Category.Trim();
            templates = templates.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Scenario))
        {
            var scenario = input.Scenario.Trim();
            templates = templates.Where(x => x.Scenario.Equals(scenario, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.AssistantCode))
        {
            var assistantCode = input.AssistantCode.Trim();
            templates = templates.Where(x => x.AssistantCode.Equals(assistantCode, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var assistants = (await _assistantRepository.GetListAsync()).ToDictionary(x => x.AssistantCode, x => x.Name, StringComparer.OrdinalIgnoreCase);
        return templates
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(item => MapPromptTemplate(item, assistants))
            .ToList();
    }

    public async Task<AiPromptTemplateDto> CreatePromptTemplateAsync(AiPromptTemplateSaveInput input)
    {
        await EnsurePromptTemplateCodeAvailableAsync(input.TemplateCode);
        var entity = new AiPromptTemplate(
            _guidGenerator.Create(),
            input.TemplateCode,
            input.Name,
            input.Category,
            input.Scenario,
            input.Mode,
            input.AssistantCode,
            input.PromptContent,
            _currentTenant.Id,
            input.Description,
            input.InputSchemaJson,
            input.OutputSchemaJson,
            Serialize(input.ToolCodes),
            input.Version,
            input.IsEnabled,
            sort: input.Sort);
        await _promptTemplateRepository.InsertAsync(entity, autoSave: true);
        return await MapPromptTemplateAsync(entity);
    }

    public async Task<AiPromptTemplateDto> UpdatePromptTemplateAsync(Guid id, AiPromptTemplateSaveInput input)
    {
        var entity = await _promptTemplateRepository.GetAsync(id);
        if (!entity.TemplateCode.Equals(input.TemplateCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("模板编码创建后不能修改。");
        }

        entity.Update(
            input.Name,
            input.Category,
            input.Scenario,
            input.Mode,
            input.AssistantCode,
            input.PromptContent,
            input.Description,
            input.InputSchemaJson,
            input.OutputSchemaJson,
            Serialize(input.ToolCodes),
            input.Version,
            input.IsEnabled,
            input.Sort);
        await _promptTemplateRepository.UpdateAsync(entity, autoSave: true);
        return await MapPromptTemplateAsync(entity);
    }

    public async Task DeletePromptTemplateAsync(Guid id)
    {
        var entity = await _promptTemplateRepository.GetAsync(id);
        if (entity.IsBuiltin)
        {
            throw new UserFriendlyException("内置场景模板不能删除，可停用或调整提示词。");
        }

        await _promptTemplateRepository.DeleteAsync(entity, autoSave: true);
    }

    private async Task EnsureBuiltinAssistantsAsync()
    {
        var existingCodes = (await _assistantRepository.GetListAsync())
            .Select(x => x.AssistantCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in BuildBuiltinAssistants())
        {
            if (existingCodes.Contains(item.AssistantCode))
            {
                continue;
            }

            await _assistantRepository.InsertAsync(new AiAssistant(
                _guidGenerator.Create(),
                item.AssistantCode,
                item.Name,
                item.RoleType,
                item.SystemPrompt,
                _currentTenant.Id,
                item.DefaultMode,
                description: item.Description,
                toolCodesJson: Serialize(item.ToolCodes),
                isBuiltin: true,
                sort: item.Sort), autoSave: true);
        }
    }

    private async Task EnsureBuiltinPromptTemplatesAsync()
    {
        var existingCodes = (await _promptTemplateRepository.GetListAsync())
            .Select(x => x.TemplateCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in BuildBuiltinPromptTemplates())
        {
            if (existingCodes.Contains(item.TemplateCode))
            {
                continue;
            }

            await _promptTemplateRepository.InsertAsync(new AiPromptTemplate(
                _guidGenerator.Create(),
                item.TemplateCode,
                item.Name,
                item.Category,
                item.Scenario,
                item.Mode,
                item.AssistantCode,
                item.PromptContent,
                _currentTenant.Id,
                item.Description,
                item.InputSchemaJson,
                item.OutputSchemaJson,
                Serialize(item.ToolCodes),
                isBuiltin: true,
                sort: item.Sort), autoSave: true);
        }
    }

    private async Task EnsureAssistantCodeAvailableAsync(string assistantCode)
    {
        var normalizedCode = Check.NotNullOrWhiteSpace(assistantCode, nameof(assistantCode), 64).Trim();
        if (await _assistantRepository.AnyAsync(x => x.AssistantCode == normalizedCode))
        {
            throw new UserFriendlyException($"助手编码已存在：{normalizedCode}");
        }
    }

    private async Task EnsurePromptTemplateCodeAvailableAsync(string templateCode)
    {
        var normalizedCode = Check.NotNullOrWhiteSpace(templateCode, nameof(templateCode), 128).Trim();
        if (await _promptTemplateRepository.AnyAsync(x => x.TemplateCode == normalizedCode))
        {
            throw new UserFriendlyException($"模板编码已存在：{normalizedCode}");
        }
    }

    private async Task<AiPromptTemplateDto> MapPromptTemplateAsync(AiPromptTemplate entity)
    {
        var assistants = (await _assistantRepository.GetListAsync()).ToDictionary(x => x.AssistantCode, x => x.Name, StringComparer.OrdinalIgnoreCase);
        return MapPromptTemplate(entity, assistants);
    }

    private async Task<Dictionary<Guid, string>> ResolveModelNamesAsync()
    {
        return (await _modelRepository.GetListAsync()).ToDictionary(x => x.Id, x => x.DisplayName);
    }

    private static AiAssistantDto MapAssistant(AiAssistant item, IReadOnlyDictionary<Guid, string> modelNames)
    {
        return new AiAssistantDto
        {
            AssistantCode = item.AssistantCode,
            ConfigJson = item.ConfigJson,
            CreationTime = item.CreationTime,
            DefaultMode = item.DefaultMode,
            DefaultModelId = item.DefaultModelId,
            DefaultModelName = item.DefaultModelId.HasValue ? modelNames.GetValueOrDefault(item.DefaultModelId.Value, string.Empty) : string.Empty,
            Description = item.Description,
            Id = item.Id,
            IsBuiltin = item.IsBuiltin,
            IsEnabled = item.IsEnabled,
            Name = item.Name,
            RoleType = item.RoleType,
            Sort = item.Sort,
            SystemPrompt = item.SystemPrompt,
            ToolCodes = DeserializeList(item.ToolCodesJson)
        };
    }

    private static AiPromptTemplateDto MapPromptTemplate(AiPromptTemplate item, IReadOnlyDictionary<string, string> assistantNames)
    {
        return new AiPromptTemplateDto
        {
            AssistantCode = item.AssistantCode,
            AssistantName = assistantNames.GetValueOrDefault(item.AssistantCode, string.Empty),
            Category = item.Category,
            CreationTime = item.CreationTime,
            Description = item.Description,
            Id = item.Id,
            InputSchemaJson = item.InputSchemaJson,
            IsBuiltin = item.IsBuiltin,
            IsEnabled = item.IsEnabled,
            Mode = item.Mode,
            Name = item.Name,
            OutputSchemaJson = item.OutputSchemaJson,
            PromptContent = item.PromptContent,
            Scenario = item.Scenario,
            Sort = item.Sort,
            TemplateCode = item.TemplateCode,
            ToolCodes = DeserializeList(item.ToolCodesJson),
            Version = item.Version
        };
    }

    private static string Serialize(IReadOnlyCollection<string> values)
    {
        return JsonSerializer.Serialize(values, JsonOptions);
    }

    private static List<string> DeserializeList(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static IReadOnlyList<BuiltinAssistant> BuildBuiltinAssistants()
    {
        return
        [
            new BuiltinAssistant(
                "pm-assistant",
                "PM 项目助手",
                "PM",
                "面向项目经理，关注范围、计划、任务、排期、风险、问题、会议、质量、工时和交付闭环。",
                "Plan",
                [
                    ProjectOverviewToolCode,
                    ProjectWbsTaskToolCode,
                    ProjectScheduleToolCode,
                    ProjectRaidToolCode,
                    ProjectQualityToolCode,
                    ProjectChangeToolCode,
                    ProjectEvmToolCode,
                    ProjectMeetingToolCode
                ],
                """
                你是企业项目管理系统中的 PM 项目助手。你必须以项目执行闭环为中心，优先读取系统中的项目概览、WBS、任务、排期、RAID、质量、变更、EVM 和会议纪要。
                输出要求：
                1. 先给结论，再列证据。
                2. 风险、问题、延期、质量缺陷必须给出责任人、截止日期和下一步动作。
                3. 不编造系统中不存在的字段和记录。
                4. 涉及写回任务、风险、变更或纪要时，只生成草稿，等待人工确认。
                """,
                10),
            new BuiltinAssistant(
                "pmo-assistant",
                "PMO 管控助手",
                "PMO",
                "面向 PMO，关注组合健康度、进度偏差、EVM、重大风险、治理动作和跨项目共性问题。",
                "Plan",
                [ProjectOverviewToolCode, ProjectScheduleToolCode, ProjectRaidToolCode, ProjectEvmToolCode, ProjectChangeToolCode],
                """
                你是企业项目管理系统中的 PMO 管控助手。你必须从治理视角分析项目进展、SPI/CPI、重大风险、变更影响和需要升级的管理动作。
                输出要求：
                1. 用项目组合治理语言表达，不写泛泛建议。
                2. 明确区分“需要 PM 处理”“需要 PMO 协调”“需要管理层决策”。
                3. 对 SPI/CPI、延期和高风险必须解释可能原因和推荐行动。
                4. 所有治理动作都要能追溯到系统记录。
                """,
                20),
            new BuiltinAssistant(
                "business-assistant",
                "经营分析助手",
                "Business",
                "面向经营管理，关注合同、预算、成本、开票、回款、利润和经营风险。",
                "Clarify",
                [BusinessOverviewToolCode, BusinessContractBudgetReceivableToolCode, ProjectEvmToolCode],
                """
                你是企业项目管理系统中的经营分析助手。你必须结合项目执行数据和经营数据，解释合同、预算、成本、开票、回款、利润和经营风险。
                输出要求：
                1. 金额、利润、回款等敏感数据只基于系统工具返回结果。
                2. 先说明当前经营状态，再说明风险来源和建议动作。
                3. 对逾期回款、低毛利、预算超支、验收滞后必须给出可执行建议。
                4. 不替代财务系统，不输出法律或财务定论。
                """,
                30)
        ];
    }

    private static IReadOnlyList<BuiltinPromptTemplate> BuildBuiltinPromptTemplates()
    {
        const string projectInputSchema = """
                                          {
                                            "type": "object",
                                            "properties": {
                                              "projectId": { "type": "string", "format": "uuid" },
                                              "period": { "type": "string", "description": "统计周期" },
                                              "extraInstruction": { "type": "string", "description": "额外要求" }
                                            }
                                          }
                                          """;
        const string markdownOutputSchema = """
                                            {
                                              "type": "object",
                                              "properties": {
                                                "markdown": { "type": "string" },
                                                "actionItems": { "type": "array" },
                                                "risks": { "type": "array" }
                                              }
                                            }
                                            """;

        return
        [
            new BuiltinPromptTemplate(
                "project-weekly-report",
                "项目周报生成",
                "项目管理",
                "WeeklyReport",
                "Plan",
                "pm-assistant",
                "生成一份项目周报，覆盖本周期进展、里程碑、任务完成、风险问题、质量与变更、EVM 指标、下周期计划和需要协调事项。必须引用项目概览、排期、任务、RAID、EVM 和会议纪要数据。",
                [ProjectOverviewToolCode, ProjectWbsTaskToolCode, ProjectScheduleToolCode, ProjectRaidToolCode, ProjectEvmToolCode, ProjectMeetingToolCode],
                10,
                projectInputSchema,
                markdownOutputSchema),
            new BuiltinPromptTemplate(
                "meeting-minutes-cleanup",
                "会议纪要整理",
                "项目管理",
                "MeetingMinutes",
                "Clarify",
                "pm-assistant",
                "根据会议材料整理会议纪要。输出会议主题、背景、讨论要点、决策事项、行动项、责任人、截止日期和关联风险/需求/任务。信息不足时先提出澄清问题。",
                [ProjectOverviewToolCode, ProjectMeetingToolCode, ProjectRaidToolCode],
                20,
                projectInputSchema,
                markdownOutputSchema),
            new BuiltinPromptTemplate(
                "risk-analysis",
                "风险分析",
                "项目治理",
                "RiskAnalysis",
                "Plan",
                "pm-assistant",
                "分析项目风险和问题，按概率、影响、紧急程度和处理路径排序。输出高优先级风险、触发条件、应对策略、责任人、计划解决日期和需要升级的事项。",
                [ProjectOverviewToolCode, ProjectRaidToolCode, ProjectScheduleToolCode, ProjectEvmToolCode],
                30,
                projectInputSchema,
                markdownOutputSchema),
            new BuiltinPromptTemplate(
                "issue-root-cause",
                "问题根因分析",
                "项目治理",
                "IssueRootCause",
                "Clarify",
                "pm-assistant",
                "围绕指定问题做根因分析。先确认问题现象、影响范围、时间线、责任边界和已有处理动作，再输出可能根因、验证方法、纠正措施和预防措施。",
                [ProjectOverviewToolCode, ProjectRaidToolCode, ProjectQualityToolCode, ProjectMeetingToolCode],
                40,
                projectInputSchema,
                markdownOutputSchema),
            new BuiltinPromptTemplate(
                "change-impact",
                "变更影响说明",
                "项目治理",
                "ChangeImpact",
                "Plan",
                "pm-assistant",
                "生成变更影响说明，覆盖范围、进度、成本、质量、合同、资源、风险和验收影响。必须列出可执行的评审动作和待确认项。",
                [ProjectOverviewToolCode, ProjectChangeToolCode, ProjectWbsTaskToolCode, ProjectScheduleToolCode, BusinessContractBudgetReceivableToolCode],
                50,
                projectInputSchema,
                markdownOutputSchema),
            new BuiltinPromptTemplate(
                "business-risk-explanation",
                "经营风险解释",
                "经营管理",
                "BusinessRisk",
                "Clarify",
                "business-assistant",
                "解释项目经营风险，覆盖合同额、预算执行、成本、开票、回款、毛利、验收和项目执行偏差。输出风险来源、影响金额、责任动作和建议优先级。",
                [BusinessOverviewToolCode, BusinessContractBudgetReceivableToolCode, ProjectEvmToolCode],
                60,
                projectInputSchema,
                markdownOutputSchema),
            new BuiltinPromptTemplate(
                "receivable-collection-suggestion",
                "回款催收建议",
                "经营管理",
                "ReceivableCollection",
                "Plan",
                "business-assistant",
                "根据合同、发票、回款计划和验收状态生成回款催收建议。输出催收对象、依据、当前阻塞、推荐话术、内部协同动作和升级条件。",
                [BusinessOverviewToolCode, BusinessContractBudgetReceivableToolCode, ProjectScheduleToolCode, ProjectMeetingToolCode],
                70,
                projectInputSchema,
                markdownOutputSchema),
            new BuiltinPromptTemplate(
                "project-retrospective",
                "项目复盘报告",
                "项目收尾",
                "Retrospective",
                "Plan",
                "pmo-assistant",
                "生成项目复盘报告，覆盖目标达成、范围变化、进度偏差、成本偏差、质量表现、风险闭环、经营结果、经验教训和组织过程资产沉淀。",
                [ProjectOverviewToolCode, ProjectWbsTaskToolCode, ProjectScheduleToolCode, ProjectRaidToolCode, ProjectQualityToolCode, ProjectChangeToolCode, ProjectEvmToolCode, BusinessContractBudgetReceivableToolCode],
                80,
                projectInputSchema,
                markdownOutputSchema)
        ];
    }

    private sealed record BuiltinAssistant(
        string AssistantCode,
        string Name,
        string RoleType,
        string Description,
        string DefaultMode,
        IReadOnlyList<string> ToolCodes,
        string SystemPrompt,
        int Sort);

    private sealed record BuiltinPromptTemplate(
        string TemplateCode,
        string Name,
        string Category,
        string Scenario,
        string Mode,
        string AssistantCode,
        string PromptContent,
        IReadOnlyList<string> ToolCodes,
        int Sort,
        string InputSchemaJson,
        string OutputSchemaJson,
        string Description = "");
}
