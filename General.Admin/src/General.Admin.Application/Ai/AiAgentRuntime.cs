using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.Ai;

public interface IAiAgentRuntime
{
    Task<AiAgentRunPlanDto> BuildRunPlanAsync(Guid agentId, AiAgentRunPlanInput input);
}

/// <summary>
/// 企业 AI Agent 运行时入口。当前阶段只生成受控运行计划，真实执行仍走会话服务和工具审计链路。
/// </summary>
public class AiAgentRuntime : IAiAgentRuntime, ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IRepository<AiAgentDefinition, Guid> _agentRepository;
    private readonly IRepository<AiModel, Guid> _modelRepository;

    public AiAgentRuntime(
        IRepository<AiAgentDefinition, Guid> agentRepository,
        IRepository<AiModel, Guid> modelRepository)
    {
        _agentRepository = agentRepository;
        _modelRepository = modelRepository;
    }

    public async Task<AiAgentRunPlanDto> BuildRunPlanAsync(Guid agentId, AiAgentRunPlanInput input)
    {
        var agent = await _agentRepository.GetAsync(agentId);
        var modelName = string.Empty;
        if (agent.DefaultModelId.HasValue)
        {
            var model = await _modelRepository.FindAsync(agent.DefaultModelId.Value);
            modelName = model?.DisplayName ?? string.Empty;
        }

        return new AiAgentRunPlanDto
        {
            AgentCode = agent.AgentCode,
            AgentName = agent.Name,
            ModelName = modelName,
            PersonaCode = agent.PersonaCode,
            SkillCodes = DeserializeList(agent.SkillCodesJson),
            SystemPrompt = agent.SystemPrompt,
            ToolCodes = DeserializeList(agent.ToolCodesJson)
        };
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
}
