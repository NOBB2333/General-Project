using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Ai;

public class AiAgentDesignService : ITransientDependency
{
    private const string BusinessOverviewToolCode = "business.project.overview.read";
    private const string ProjectEvmToolCode = "project.evm.read";
    private const string ProjectMeetingToolCode = "project.meeting.read";
    private const string ProjectOverviewToolCode = "project.overview.read";
    private const string ProjectRaidToolCode = "project.raid.read";
    private const string ProjectScheduleToolCode = "project.schedule.milestones.read";
    private const string ProjectWbsTaskToolCode = "project.wbs.tasks.read";
    private const string WebSearchToolCode = "web.search";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IRepository<AiAgentDefinition, Guid> _agentRepository;
    private readonly IRepository<AiArtifact, Guid> _artifactRepository;
    private readonly IRepository<AiCommand, Guid> _commandRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiModel, Guid> _modelRepository;
    private readonly IRepository<AiPersona, Guid> _personaRepository;
    private readonly IRepository<AiSkill, Guid> _skillRepository;

    public AiAgentDesignService(
        IRepository<AiAgentDefinition, Guid> agentRepository,
        IRepository<AiArtifact, Guid> artifactRepository,
        IRepository<AiCommand, Guid> commandRepository,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator,
        IRepository<AiModel, Guid> modelRepository,
        IRepository<AiPersona, Guid> personaRepository,
        IRepository<AiSkill, Guid> skillRepository)
    {
        _agentRepository = agentRepository;
        _artifactRepository = artifactRepository;
        _commandRepository = commandRepository;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        _modelRepository = modelRepository;
        _personaRepository = personaRepository;
        _skillRepository = skillRepository;
    }

    public async Task<List<AiSkillDto>> GetSkillsAsync(AiAgentDesignQueryInput input)
    {
        await EnsureBuiltinAsync();
        var items = await _skillRepository.GetListAsync();
        return items
            .Where(x => input.IncludeDisabled || x.IsEnabled)
            .Where(x => string.IsNullOrWhiteSpace(input.Category) || x.Category.Equals(input.Category.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(x => IsKeywordMatch(input.Keyword, x.SkillCode, x.Name, x.Description))
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(MapSkill)
            .ToList();
    }

    public async Task<AiSkillDto> CreateSkillAsync(AiSkillSaveInput input)
    {
        await EnsureCodeAvailableAsync(_skillRepository, input.SkillCode, x => x.SkillCode, "技能编码");
        var entity = new AiSkill(
            _guidGenerator.Create(),
            input.SkillCode,
            input.Name,
            input.Category,
            input.Instruction,
            _currentTenant.Id,
            input.DefaultMode,
            input.Description,
            input.InputSchemaJson,
            input.OutputSchemaJson,
            Serialize(input.ToolCodes),
            input.IsEnabled,
            sort: input.Sort);
        await _skillRepository.InsertAsync(entity, autoSave: true);
        return MapSkill(entity);
    }

    public async Task<AiSkillDto> UpdateSkillAsync(Guid id, AiSkillSaveInput input)
    {
        var entity = await _skillRepository.GetAsync(id);
        EnsureSameCode(entity.SkillCode, input.SkillCode, "技能编码");
        entity.Update(
            input.Name,
            input.Category,
            input.Instruction,
            input.DefaultMode,
            input.Description,
            input.InputSchemaJson,
            input.OutputSchemaJson,
            Serialize(input.ToolCodes),
            input.IsEnabled,
            input.Sort);
        await _skillRepository.UpdateAsync(entity, autoSave: true);
        return MapSkill(entity);
    }

    public async Task DeleteSkillAsync(Guid id)
    {
        var entity = await _skillRepository.GetAsync(id);
        EnsureNotBuiltin(entity.IsBuiltin, "内置技能不能删除，可停用或调整配置。");
        await _skillRepository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<List<AiPersonaDto>> GetPersonasAsync(AiAgentDesignQueryInput input)
    {
        await EnsureBuiltinAsync();
        var items = await _personaRepository.GetListAsync();
        return items
            .Where(x => input.IncludeDisabled || x.IsEnabled)
            .Where(x => string.IsNullOrWhiteSpace(input.RoleType) || x.RoleType.Equals(input.RoleType.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(x => IsKeywordMatch(input.Keyword, x.PersonaCode, x.Name, x.Description))
            .OrderBy(x => x.RoleType)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(MapPersona)
            .ToList();
    }

    public async Task<AiPersonaDto> CreatePersonaAsync(AiPersonaSaveInput input)
    {
        await EnsureCodeAvailableAsync(_personaRepository, input.PersonaCode, x => x.PersonaCode, "角色画像编码");
        var entity = new AiPersona(
            _guidGenerator.Create(),
            input.PersonaCode,
            input.Name,
            input.RoleType,
            input.PersonalityPrompt,
            input.BoundaryPrompt,
            input.StylePrompt,
            _currentTenant.Id,
            input.Description,
            input.ConfigJson,
            input.IsEnabled,
            sort: input.Sort);
        await _personaRepository.InsertAsync(entity, autoSave: true);
        return MapPersona(entity);
    }

    public async Task<AiPersonaDto> UpdatePersonaAsync(Guid id, AiPersonaSaveInput input)
    {
        var entity = await _personaRepository.GetAsync(id);
        EnsureSameCode(entity.PersonaCode, input.PersonaCode, "角色画像编码");
        entity.Update(
            input.Name,
            input.RoleType,
            input.PersonalityPrompt,
            input.BoundaryPrompt,
            input.StylePrompt,
            input.Description,
            input.ConfigJson,
            input.IsEnabled,
            input.Sort);
        await _personaRepository.UpdateAsync(entity, autoSave: true);
        return MapPersona(entity);
    }

    public async Task DeletePersonaAsync(Guid id)
    {
        var entity = await _personaRepository.GetAsync(id);
        EnsureNotBuiltin(entity.IsBuiltin, "内置角色画像不能删除，可停用或调整配置。");
        await _personaRepository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<List<AiCommandDto>> GetCommandsAsync(AiAgentDesignQueryInput input)
    {
        await EnsureBuiltinAsync();
        var items = await _commandRepository.GetListAsync();
        return items
            .Where(x => input.IncludeDisabled || x.IsEnabled)
            .Where(x => IsKeywordMatch(input.Keyword, x.CommandCode, x.SlashName, x.Name, x.Description))
            .OrderBy(x => x.Sort)
            .ThenBy(x => x.SlashName)
            .Select(MapCommand)
            .ToList();
    }

    public async Task<AiCommandDto> CreateCommandAsync(AiCommandSaveInput input)
    {
        await EnsureCodeAvailableAsync(_commandRepository, input.CommandCode, x => x.CommandCode, "命令编码");
        var entity = new AiCommand(
            _guidGenerator.Create(),
            input.CommandCode,
            input.SlashName,
            input.Name,
            _currentTenant.Id,
            input.Mode,
            input.SkillCode,
            input.PromptTemplateCode,
            input.AgentCode,
            input.InputPlaceholder,
            input.ExampleInput,
            input.Description,
            input.ConfigJson,
            input.IsEnabled,
            sort: input.Sort);
        await _commandRepository.InsertAsync(entity, autoSave: true);
        return MapCommand(entity);
    }

    public async Task<AiCommandDto> UpdateCommandAsync(Guid id, AiCommandSaveInput input)
    {
        var entity = await _commandRepository.GetAsync(id);
        EnsureSameCode(entity.CommandCode, input.CommandCode, "命令编码");
        entity.Update(
            input.SlashName,
            input.Name,
            input.Mode,
            input.SkillCode,
            input.PromptTemplateCode,
            input.AgentCode,
            input.InputPlaceholder,
            input.ExampleInput,
            input.Description,
            input.ConfigJson,
            input.IsEnabled,
            input.Sort);
        await _commandRepository.UpdateAsync(entity, autoSave: true);
        return MapCommand(entity);
    }

    public async Task DeleteCommandAsync(Guid id)
    {
        var entity = await _commandRepository.GetAsync(id);
        EnsureNotBuiltin(entity.IsBuiltin, "内置快捷命令不能删除，可停用或调整配置。");
        await _commandRepository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<List<AiAgentDefinitionDto>> GetAgentsAsync(AiAgentDesignQueryInput input)
    {
        await EnsureBuiltinAsync();
        var models = await ResolveModelNamesAsync();
        var items = await _agentRepository.GetListAsync();
        return items
            .Where(x => input.IncludeDisabled || x.IsEnabled)
            .Where(x => string.IsNullOrWhiteSpace(input.RoleType) || x.RoleType.Equals(input.RoleType.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(x => IsKeywordMatch(input.Keyword, x.AgentCode, x.Name, x.Description))
            .OrderBy(x => x.RoleType)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(item => MapAgent(item, models))
            .ToList();
    }

    public async Task<AiAgentDefinitionDto> CreateAgentAsync(AiAgentDefinitionSaveInput input)
    {
        await EnsureCodeAvailableAsync(_agentRepository, input.AgentCode, x => x.AgentCode, "Agent 编码");
        var entity = new AiAgentDefinition(
            _guidGenerator.Create(),
            input.AgentCode,
            input.Name,
            input.RoleType,
            input.SystemPrompt,
            _currentTenant.Id,
            input.PersonaCode,
            input.DefaultModelId,
            input.Description,
            Serialize(input.ToolCodes),
            Serialize(input.SkillCodes),
            input.RuntimeConfigJson,
            input.IsEnabled,
            sort: input.Sort);
        await _agentRepository.InsertAsync(entity, autoSave: true);
        return MapAgent(entity, await ResolveModelNamesAsync());
    }

    public async Task<AiAgentDefinitionDto> UpdateAgentAsync(Guid id, AiAgentDefinitionSaveInput input)
    {
        var entity = await _agentRepository.GetAsync(id);
        EnsureSameCode(entity.AgentCode, input.AgentCode, "Agent 编码");
        entity.Update(
            input.Name,
            input.RoleType,
            input.SystemPrompt,
            input.PersonaCode,
            input.DefaultModelId,
            input.Description,
            Serialize(input.ToolCodes),
            Serialize(input.SkillCodes),
            input.RuntimeConfigJson,
            input.IsEnabled,
            input.Sort);
        await _agentRepository.UpdateAsync(entity, autoSave: true);
        return MapAgent(entity, await ResolveModelNamesAsync());
    }

    public async Task DeleteAgentAsync(Guid id)
    {
        var entity = await _agentRepository.GetAsync(id);
        EnsureNotBuiltin(entity.IsBuiltin, "内置 Agent 不能删除，可停用或调整配置。");
        await _agentRepository.DeleteAsync(entity, autoSave: true);
    }

    public async Task<List<AiArtifactDto>> GetArtifactsAsync(AiArtifactQueryInput input)
    {
        await EnsureBuiltinAsync();
        var items = await _artifactRepository.GetListAsync();
        return items
            .Where(x => input.IncludeDisabled || x.IsEnabled)
            .Where(x => string.IsNullOrWhiteSpace(input.Category) || x.Category.Equals(input.Category.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(x => string.IsNullOrWhiteSpace(input.RendererType) || x.RendererType.Equals(input.RendererType.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(x => IsKeywordMatch(input.Keyword, x.ArtifactCode, x.Name, x.Description))
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.Name)
            .Select(MapArtifact)
            .ToList();
    }

    public async Task<AiArtifactDto> CreateArtifactAsync(AiArtifactSaveInput input)
    {
        await EnsureCodeAvailableAsync(_artifactRepository, input.ArtifactCode, x => x.ArtifactCode, "Artifact 编码");
        var entity = new AiArtifact(
            _guidGenerator.Create(),
            input.ArtifactCode,
            input.Name,
            input.Category,
            input.RendererType,
            input.JsonSchema,
            _currentTenant.Id,
            input.ExamplePayloadJson,
            input.SystemHint,
            input.Description,
            input.IsEnabled,
            sort: input.Sort);
        await _artifactRepository.InsertAsync(entity, autoSave: true);
        return MapArtifact(entity);
    }

    public async Task<AiArtifactDto> UpdateArtifactAsync(Guid id, AiArtifactSaveInput input)
    {
        var entity = await _artifactRepository.GetAsync(id);
        EnsureSameCode(entity.ArtifactCode, input.ArtifactCode, "Artifact 编码");
        entity.Update(
            input.Name,
            input.Category,
            input.RendererType,
            input.JsonSchema,
            input.ExamplePayloadJson,
            input.SystemHint,
            input.Description,
            input.IsEnabled,
            input.Sort);
        await _artifactRepository.UpdateAsync(entity, autoSave: true);
        return MapArtifact(entity);
    }

    public async Task DeleteArtifactAsync(Guid id)
    {
        var entity = await _artifactRepository.GetAsync(id);
        EnsureNotBuiltin(entity.IsBuiltin, "内置 Artifact 不能删除，可停用或调整配置。");
        await _artifactRepository.DeleteAsync(entity, autoSave: true);
    }

    private async Task EnsureBuiltinAsync()
    {
        await EnsureBuiltinSkillsAsync();
        await EnsureBuiltinPersonasAsync();
        await EnsureBuiltinCommandsAsync();
        await EnsureBuiltinAgentsAsync();
        await EnsureBuiltinArtifactsAsync();
    }

    private async Task EnsureBuiltinSkillsAsync()
    {
        var existingCodes = (await _skillRepository.GetListAsync()).Select(x => x.SkillCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in BuildBuiltinSkills())
        {
            if (existingCodes.Contains(item.Code))
            {
                continue;
            }

            await _skillRepository.InsertAsync(new AiSkill(
                _guidGenerator.Create(),
                item.Code,
                item.Name,
                item.Category,
                item.Instruction,
                _currentTenant.Id,
                item.Mode,
                item.Description,
                "{}",
                "{}",
                Serialize(item.ToolCodes),
                true,
                true,
                item.Sort), autoSave: true);
        }
    }

    private async Task EnsureBuiltinPersonasAsync()
    {
        var existingCodes = (await _personaRepository.GetListAsync()).Select(x => x.PersonaCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in BuildBuiltinPersonas())
        {
            if (existingCodes.Contains(item.Code))
            {
                continue;
            }

            await _personaRepository.InsertAsync(new AiPersona(
                _guidGenerator.Create(),
                item.Code,
                item.Name,
                item.RoleType,
                item.PersonalityPrompt,
                item.BoundaryPrompt,
                item.StylePrompt,
                _currentTenant.Id,
                item.Description,
                "{}",
                true,
                true,
                item.Sort), autoSave: true);
        }
    }

    private async Task EnsureBuiltinCommandsAsync()
    {
        var existingCodes = (await _commandRepository.GetListAsync()).Select(x => x.CommandCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in BuildBuiltinCommands())
        {
            if (existingCodes.Contains(item.Code))
            {
                continue;
            }

            await _commandRepository.InsertAsync(new AiCommand(
                _guidGenerator.Create(),
                item.Code,
                item.SlashName,
                item.Name,
                _currentTenant.Id,
                item.Mode,
                item.SkillCode,
                item.PromptTemplateCode,
                item.AgentCode,
                item.InputPlaceholder,
                item.ExampleInput,
                item.Description,
                "{}",
                true,
                true,
                item.Sort), autoSave: true);
        }
    }

    private async Task EnsureBuiltinAgentsAsync()
    {
        var existingCodes = (await _agentRepository.GetListAsync()).Select(x => x.AgentCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in BuildBuiltinAgents())
        {
            if (existingCodes.Contains(item.Code))
            {
                continue;
            }

            await _agentRepository.InsertAsync(new AiAgentDefinition(
                _guidGenerator.Create(),
                item.Code,
                item.Name,
                item.RoleType,
                item.SystemPrompt,
                _currentTenant.Id,
                item.PersonaCode,
                description: item.Description,
                toolCodesJson: Serialize(item.ToolCodes),
                skillCodesJson: Serialize(item.SkillCodes),
                runtimeConfigJson: "{\"maxToolRounds\":4,\"requiresWritebackApproval\":true}",
                isBuiltin: true,
                sort: item.Sort), autoSave: true);
        }
    }

    private async Task EnsureBuiltinArtifactsAsync()
    {
        var existingCodes = (await _artifactRepository.GetListAsync()).Select(x => x.ArtifactCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var item in BuildBuiltinArtifacts())
        {
            if (existingCodes.Contains(item.Code))
            {
                continue;
            }

            await _artifactRepository.InsertAsync(new AiArtifact(
                _guidGenerator.Create(),
                item.Code,
                item.Name,
                item.Category,
                item.RendererType,
                item.JsonSchema,
                _currentTenant.Id,
                item.ExamplePayloadJson,
                item.SystemHint,
                item.Description,
                true,
                true,
                item.Sort), autoSave: true);
        }
    }

    private static IReadOnlyList<BuiltinSkill> BuildBuiltinSkills()
    {
        return
        [
            new("project-weekly-report", "项目周报生成", "项目管理", "Plan", "读取项目进度、任务、风险、质量和 EVM 数据，生成面向 PM/PMO 的周报草稿。", [ProjectOverviewToolCode, ProjectScheduleToolCode, ProjectRaidToolCode, ProjectEvmToolCode], "先列本周结论，再列进展、风险、下周计划和需要协调事项。不得编造系统不存在的数据。", 10),
            new("risk-analysis", "项目风险分析", "项目治理", "Clarify", "分析 RAID、延期、质量缺陷和变更影响，输出优先行动建议。", [ProjectOverviewToolCode, ProjectRaidToolCode, ProjectEvmToolCode], "优先识别高概率高影响风险，必须给出责任人、截止时间、触发条件和升级路径。", 20),
            new("meeting-minutes", "会议纪要整理", "项目执行", "Execution", "把会议记录整理为结论、决议、行动项和写回草稿。", [ProjectMeetingToolCode, ProjectWbsTaskToolCode], "输出会议结论、决议、行动项、风险和待确认问题。写回动作只生成草稿。", 30),
            new("business-explanation", "经营指标解释", "经营管理", "Clarify", "解释合同、预算、回款、利润和项目执行数据之间的偏差。", [BusinessOverviewToolCode, ProjectEvmToolCode], "用经营语言解释偏差，区分项目执行原因、合同商务原因和回款风险原因。", 40),
            new("web-research-brief", "联网资料简报", "知识检索", "Knowledge", "在启用联网搜索配置后，检索外部资料并生成来源可追溯的摘要。", [WebSearchToolCode], "只引用搜索结果中的来源，不输出无法追溯的事实。涉及企业内部数据时不得外发敏感内容。", 50)
        ];
    }

    private static IReadOnlyList<BuiltinPersona> BuildBuiltinPersonas()
    {
        return
        [
            new("pm-pragmatic", "务实型 PM", "PM", "关注项目目标、范围、计划、任务、风险和资源约束。", "不能替 PM 直接承诺交付日期；所有建议必须落到责任人和下一步动作。", "先结论后证据，语言简洁，避免管理学空话。", "适合日常项目推进、周报和风险处置。", 10),
            new("pmo-governance", "治理型 PMO", "PMO", "关注组合健康度、SPI/CPI、重大风险、跨部门阻塞和治理动作。", "不能绕开权限查看敏感经营数据；治理建议必须可追溯。", "使用治理语言，明确 PM、PMO、管理层各自要做什么。", "适合 PMO 执行总览、项目复盘和风险升级。", 20),
            new("business-analyst", "经营分析师", "Business", "关注合同、预算、成本、回款、利润和经营预警。", "不能给出未经审批的财务口径；敏感金额必须服从权限。", "偏经营分析表达，解释原因、影响和建议动作。", "适合经营总览、项目经营解释和回款风险分析。", 30)
        ];
    }

    private static IReadOnlyList<BuiltinCommand> BuildBuiltinCommands()
    {
        return
        [
            new("cmd-weekly-report", "/周报", "生成项目周报", "Plan", "project-weekly-report", "project-weekly-report", "pm-agent", "输入项目名称或项目编号", "为当前项目生成本周周报", "按周报技能生成草稿。", 10),
            new("cmd-risk-analysis", "/风险分析", "分析项目风险", "Clarify", "risk-analysis", "risk-analysis", "pmo-agent", "输入项目名称、风险主题或留空使用当前上下文", "分析当前项目高优先级风险", "按风险分析技能生成行动建议。", 20),
            new("cmd-meeting-minutes", "/会议纪要", "整理会议纪要", "Execution", "meeting-minutes", "meeting-minutes-cleanup", "pm-agent", "粘贴会议原文或选择会议记录", "整理今天评审会纪要", "整理为决议和行动项。", 30),
            new("cmd-business-brief", "/经营解释", "解释经营偏差", "Clarify", "business-explanation", "business-risk-explanation", "business-agent", "输入项目或经营问题", "解释这个项目回款风险", "解释经营数据和项目执行的关系。", 40)
        ];
    }

    private static IReadOnlyList<BuiltinAgent> BuildBuiltinAgents()
    {
        return
        [
            new("pm-agent", "PM 项目 Agent", "PM", "pm-pragmatic", "面向 PM，围绕计划、任务、排期、风险、会议和交付闭环进行辅助。", [ProjectOverviewToolCode, ProjectScheduleToolCode, ProjectWbsTaskToolCode, ProjectRaidToolCode, ProjectEvmToolCode, ProjectMeetingToolCode], ["project-weekly-report", "meeting-minutes", "risk-analysis"], "你是企业项目管理系统中的 PM 项目 Agent。所有输出必须基于系统数据和用户输入，不确定时先澄清。涉及写回时只生成草稿并等待审批。", 10),
            new("pmo-agent", "PMO 治理 Agent", "PMO", "pmo-governance", "面向 PMO，关注项目组合健康度、偏差、风险升级和治理行动。", [ProjectOverviewToolCode, ProjectScheduleToolCode, ProjectRaidToolCode, ProjectEvmToolCode], ["risk-analysis", "project-weekly-report"], "你是企业项目管理系统中的 PMO 治理 Agent。需要区分 PM 处理、PMO 协调和管理层决策。", 20),
            new("business-agent", "经营分析 Agent", "Business", "business-analyst", "面向经营管理，解释合同、预算、回款、成本、利润和项目执行之间的关系。", [BusinessOverviewToolCode, ProjectEvmToolCode], ["business-explanation"], "你是企业项目管理系统中的经营分析 Agent。所有金额和经营指标必须服从后端口径和权限。", 30)
        ];
    }

    private static IReadOnlyList<BuiltinArtifact> BuildBuiltinArtifacts()
    {
        return
        [
            new("metric-card", "指标卡", "Dashboard", "metric-card", "{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"},\"value\":{\"type\":\"string\"},\"trend\":{\"type\":\"string\"},\"status\":{\"type\":\"string\"}}}", "{\"title\":\"SPI\",\"value\":\"0.92\",\"trend\":\"较上周下降 0.04\",\"status\":\"warning\"}", "适合输出单个项目指标、EVM 指标或经营指标。", "模型只能输出 JSON，不允许输出 HTML。", 10),
            new("action-list", "行动队列", "Dashboard", "action-list", "{\"type\":\"object\",\"properties\":{\"items\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"title\":{\"type\":\"string\"},\"owner\":{\"type\":\"string\"},\"dueDate\":{\"type\":\"string\"},\"priority\":{\"type\":\"string\"}}}}}", "{\"items\":[{\"title\":\"确认延期风险处置人\",\"owner\":\"PMO\",\"dueDate\":\"2026-07-01\",\"priority\":\"高\"}]}", "适合输出 PM/PMO 下一步动作。", "每个行动项必须包含责任人和截止时间。", 20),
            new("risk-matrix", "风险矩阵", "Chart", "risk-matrix", "{\"type\":\"object\",\"properties\":{\"risks\":{\"type\":\"array\",\"items\":{\"type\":\"object\",\"properties\":{\"name\":{\"type\":\"string\"},\"probability\":{\"type\":\"number\"},\"impact\":{\"type\":\"number\"},\"level\":{\"type\":\"string\"}}}}}", "{\"risks\":[{\"name\":\"需求冻结延迟\",\"probability\":0.7,\"impact\":0.8,\"level\":\"高\"}]}", "适合输出 RAID 风险矩阵。", "概率和影响必须在 0 到 1 之间。", 30)
        ];
    }

    private async Task<Dictionary<Guid, string>> ResolveModelNamesAsync()
    {
        return (await _modelRepository.GetListAsync()).ToDictionary(x => x.Id, x => x.DisplayName);
    }

    private static AiSkillDto MapSkill(AiSkill item)
    {
        return new AiSkillDto
        {
            Category = item.Category,
            CreationTime = item.CreationTime,
            DefaultMode = item.DefaultMode,
            Description = item.Description,
            Id = item.Id,
            InputSchemaJson = item.InputSchemaJson,
            Instruction = item.Instruction,
            IsBuiltin = item.IsBuiltin,
            IsEnabled = item.IsEnabled,
            Name = item.Name,
            OutputSchemaJson = item.OutputSchemaJson,
            SkillCode = item.SkillCode,
            Sort = item.Sort,
            ToolCodes = DeserializeList(item.ToolCodesJson)
        };
    }

    private static AiPersonaDto MapPersona(AiPersona item)
    {
        return new AiPersonaDto
        {
            BoundaryPrompt = item.BoundaryPrompt,
            ConfigJson = item.ConfigJson,
            CreationTime = item.CreationTime,
            Description = item.Description,
            Id = item.Id,
            IsBuiltin = item.IsBuiltin,
            IsEnabled = item.IsEnabled,
            Name = item.Name,
            PersonaCode = item.PersonaCode,
            PersonalityPrompt = item.PersonalityPrompt,
            RoleType = item.RoleType,
            Sort = item.Sort,
            StylePrompt = item.StylePrompt
        };
    }

    private static AiCommandDto MapCommand(AiCommand item)
    {
        return new AiCommandDto
        {
            AgentCode = item.AgentCode,
            CommandCode = item.CommandCode,
            ConfigJson = item.ConfigJson,
            CreationTime = item.CreationTime,
            Description = item.Description,
            ExampleInput = item.ExampleInput,
            Id = item.Id,
            InputPlaceholder = item.InputPlaceholder,
            IsBuiltin = item.IsBuiltin,
            IsEnabled = item.IsEnabled,
            Mode = item.Mode,
            Name = item.Name,
            PromptTemplateCode = item.PromptTemplateCode,
            SkillCode = item.SkillCode,
            Sort = item.Sort,
            SlashName = item.SlashName
        };
    }

    private static AiAgentDefinitionDto MapAgent(AiAgentDefinition item, IReadOnlyDictionary<Guid, string> modelNames)
    {
        return new AiAgentDefinitionDto
        {
            AgentCode = item.AgentCode,
            CreationTime = item.CreationTime,
            DefaultModelId = item.DefaultModelId,
            DefaultModelName = item.DefaultModelId.HasValue ? modelNames.GetValueOrDefault(item.DefaultModelId.Value, string.Empty) : string.Empty,
            Description = item.Description,
            Id = item.Id,
            IsBuiltin = item.IsBuiltin,
            IsEnabled = item.IsEnabled,
            Name = item.Name,
            PersonaCode = item.PersonaCode,
            RoleType = item.RoleType,
            RuntimeConfigJson = item.RuntimeConfigJson,
            SkillCodes = DeserializeList(item.SkillCodesJson),
            Sort = item.Sort,
            SystemPrompt = item.SystemPrompt,
            ToolCodes = DeserializeList(item.ToolCodesJson)
        };
    }

    private static AiArtifactDto MapArtifact(AiArtifact item)
    {
        return new AiArtifactDto
        {
            ArtifactCode = item.ArtifactCode,
            Category = item.Category,
            CreationTime = item.CreationTime,
            Description = item.Description,
            ExamplePayloadJson = item.ExamplePayloadJson,
            Id = item.Id,
            IsBuiltin = item.IsBuiltin,
            IsEnabled = item.IsEnabled,
            JsonSchema = item.JsonSchema,
            Name = item.Name,
            RendererType = item.RendererType,
            Sort = item.Sort,
            SystemHint = item.SystemHint
        };
    }

    private async Task EnsureCodeAvailableAsync<TEntity>(
        IRepository<TEntity, Guid> repository,
        string code,
        Func<TEntity, string> codeSelector,
        string label)
        where TEntity : class, Volo.Abp.Domain.Entities.IEntity<Guid>
    {
        var normalizedCode = Check.NotNullOrWhiteSpace(code, nameof(code), 128).Trim();
        var exists = (await repository.GetListAsync()).Any(x => codeSelector(x).Equals(normalizedCode, StringComparison.OrdinalIgnoreCase));
        if (exists)
        {
            throw new UserFriendlyException($"{label}已存在：{normalizedCode}");
        }
    }

    private static void EnsureSameCode(string currentCode, string inputCode, string label)
    {
        if (!currentCode.Equals(inputCode.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException($"{label}创建后不能修改。");
        }
    }

    private static void EnsureNotBuiltin(bool isBuiltin, string message)
    {
        if (isBuiltin)
        {
            throw new UserFriendlyException(message);
        }
    }

    private static bool IsKeywordMatch(string? keyword, params string[] values)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return true;
        }

        var normalized = keyword.Trim();
        return values.Any(x => x.Contains(normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static string Serialize(IReadOnlyCollection<string> values)
    {
        var normalized = values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        return JsonSerializer.Serialize(normalized, JsonOptions);
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

    private sealed record BuiltinSkill(string Code, string Name, string Category, string Mode, string Description, IReadOnlyList<string> ToolCodes, string Instruction, int Sort);

    private sealed record BuiltinPersona(string Code, string Name, string RoleType, string PersonalityPrompt, string BoundaryPrompt, string StylePrompt, string Description, int Sort);

    private sealed record BuiltinCommand(string Code, string SlashName, string Name, string Mode, string SkillCode, string PromptTemplateCode, string AgentCode, string InputPlaceholder, string ExampleInput, string Description, int Sort);

    private sealed record BuiltinAgent(string Code, string Name, string RoleType, string PersonaCode, string Description, IReadOnlyList<string> ToolCodes, IReadOnlyList<string> SkillCodes, string SystemPrompt, int Sort);

    private sealed record BuiltinArtifact(string Code, string Name, string Category, string RendererType, string JsonSchema, string ExamplePayloadJson, string Description, string SystemHint, int Sort);
}
