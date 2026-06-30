using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Threading;

namespace General.Admin.Controllers;

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Ai)]
[Route("api/app/ai")]
public class AiController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly AiConversationService _aiConversationService;
    private readonly AiAssistantPromptService _aiAssistantPromptService;
    private readonly AiMcpService _aiMcpService;
    private readonly AiMemoryService _aiMemoryService;
    private readonly AiJobService _aiJobService;
    private readonly AiUsageService _aiUsageService;
    private readonly AiAgentDesignService _aiAgentDesignService;
    private readonly IAiAgentRuntime _aiAgentRuntime;
    private readonly AiProviderService _aiProviderService;
    private readonly AiToolRegistryService _aiToolRegistryService;
    private readonly AiWebSearchService _aiWebSearchService;
    private readonly AiWritebackService _aiWritebackService;

    public AiController(
        AiConversationService aiConversationService,
        AiAssistantPromptService aiAssistantPromptService,
        AiMcpService aiMcpService,
        AiMemoryService aiMemoryService,
        AiJobService aiJobService,
        AiUsageService aiUsageService,
        AiAgentDesignService aiAgentDesignService,
        IAiAgentRuntime aiAgentRuntime,
        AiProviderService aiProviderService,
        AiToolRegistryService aiToolRegistryService,
        AiWebSearchService aiWebSearchService,
        AiWritebackService aiWritebackService)
    {
        _aiConversationService = aiConversationService;
        _aiAssistantPromptService = aiAssistantPromptService;
        _aiMcpService = aiMcpService;
        _aiMemoryService = aiMemoryService;
        _aiJobService = aiJobService;
        _aiUsageService = aiUsageService;
        _aiAgentDesignService = aiAgentDesignService;
        _aiAgentRuntime = aiAgentRuntime;
        _aiProviderService = aiProviderService;
        _aiToolRegistryService = aiToolRegistryService;
        _aiWebSearchService = aiWebSearchService;
        _aiWritebackService = aiWritebackService;
    }

    [HttpGet("providers")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    public async Task<ActionResult<ApiResponse<List<AiProviderDto>>>> GetProvidersAsync()
    {
        return ApiResponse<List<AiProviderDto>>.Ok(await _aiProviderService.GetProvidersAsync());
    }

    [HttpPost("providers")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<AiProviderDto>>> CreateProviderAsync([FromBody] AiProviderSaveInput input)
    {
        return ApiResponse<AiProviderDto>.Ok(await _aiProviderService.CreateProviderAsync(input));
    }

    [HttpPut("providers/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<AiProviderDto>>> UpdateProviderAsync(Guid id, [FromBody] AiProviderSaveInput input)
    {
        return ApiResponse<AiProviderDto>.Ok(await _aiProviderService.UpdateProviderAsync(id, input));
    }

    [HttpPost("providers/{id:guid}/test")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<AiProviderTestResultDto>>> TestProviderAsync(Guid id)
    {
        return ApiResponse<AiProviderTestResultDto>.Ok(await _aiProviderService.TestProviderAsync(id));
    }

    [HttpPost("providers/{id:guid}/models/discover")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<AiProviderModelDiscoveryResultDto>>> DiscoverProviderModelsAsync(Guid id)
    {
        return ApiResponse<AiProviderModelDiscoveryResultDto>.Ok(await _aiProviderService.DiscoverModelsAsync(id));
    }

    [HttpPost("providers/{id:guid}/models/sync")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<AiProviderModelDiscoveryResultDto>>> SyncProviderModelsAsync(Guid id)
    {
        return ApiResponse<AiProviderModelDiscoveryResultDto>.Ok(await _aiProviderService.SyncModelsAsync(id));
    }

    [HttpDelete("providers/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProviderAsync(Guid id)
    {
        await _aiProviderService.DeleteProviderAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("models")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    public async Task<ActionResult<ApiResponse<List<AiModelDto>>>> GetModelsAsync([FromQuery] Guid? providerId = null)
    {
        return ApiResponse<List<AiModelDto>>.Ok(await _aiProviderService.GetModelsAsync(providerId));
    }

    [HttpPost("models")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<AiModelDto>>> CreateModelAsync([FromBody] AiModelSaveInput input)
    {
        return ApiResponse<AiModelDto>.Ok(await _aiProviderService.CreateModelAsync(input));
    }

    [HttpPut("models/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<AiModelDto>>> UpdateModelAsync(Guid id, [FromBody] AiModelSaveInput input)
    {
        return ApiResponse<AiModelDto>.Ok(await _aiProviderService.UpdateModelAsync(id, input));
    }

    [HttpDelete("models/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ProviderManage)]
    [PlatformEndpoint("Ai.Provider.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteModelAsync(Guid id)
    {
        await _aiProviderService.DeleteModelAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("conversations")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    public async Task<ActionResult<ApiResponse<List<AiConversationDto>>>> GetConversationsAsync([FromQuery] AiConversationQueryInput input)
    {
        return ApiResponse<List<AiConversationDto>>.Ok(await _aiConversationService.GetConversationsAsync(input));
    }

    [HttpGet("conversations/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    public async Task<ActionResult<ApiResponse<AiConversationDetailDto>>> GetConversationAsync(Guid id)
    {
        return ApiResponse<AiConversationDetailDto>.Ok(await _aiConversationService.GetConversationAsync(id));
    }

    [HttpPost("conversations")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiConversationDetailDto>>> CreateConversationAsync([FromBody] AiConversationCreateInput input)
    {
        return ApiResponse<AiConversationDetailDto>.Ok(await _aiConversationService.CreateConversationAsync(input));
    }

    [HttpPut("conversations/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiConversationDto>>> UpdateConversationAsync(Guid id, [FromBody] AiConversationUpdateInput input)
    {
        return ApiResponse<AiConversationDto>.Ok(await _aiConversationService.UpdateConversationAsync(id, input));
    }

    [HttpPost("conversations/{id:guid}/messages")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiConversationSendResultDto>>> SendMessageAsync(Guid id, [FromBody] AiConversationSendInput input)
    {
        return ApiResponse<AiConversationSendResultDto>.Ok(await _aiConversationService.SendMessageAsync(id, input));
    }

    [HttpPost("conversations/{id:guid}/messages/stream")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task StreamMessageAsync(Guid id, [FromBody] AiConversationSendInput input, CancellationToken cancellationToken)
    {
        Response.ContentType = "application/x-ndjson; charset=utf-8";
        await foreach (var streamEvent in _aiConversationService.StreamMessageAsync(id, input, cancellationToken))
        {
            await Response.WriteAsync(JsonSerializer.Serialize(streamEvent, JsonOptions), cancellationToken);
            await Response.WriteAsync("\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    [HttpPost("conversations/{id:guid}/plan/confirm")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiMessageDto>>> ConfirmPlanAsync(Guid id, [FromBody] AiPlanConfirmInput input)
    {
        return ApiResponse<AiMessageDto>.Ok(await _aiConversationService.ConfirmPlanAsync(id, input));
    }

    [HttpPost("conversations/{id:guid}/plan/steps/toggle")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiMessageDto>>> TogglePlanStepAsync(Guid id, [FromBody] AiPlanStepToggleInput input)
    {
        return ApiResponse<AiMessageDto>.Ok(await _aiConversationService.TogglePlanStepAsync(id, input));
    }

    [HttpPost("conversations/{id:guid}/plan/draft")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiConversationDetailDto>>> CreatePlanDraftAsync(Guid id, [FromBody] AiPlanDraftInput input)
    {
        return ApiResponse<AiConversationDetailDto>.Ok(await _aiConversationService.CreatePlanDraftAsync(id, input));
    }

    [HttpPost("conversations/{id:guid}/archive")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiConversationDto>>> ArchiveConversationAsync(Guid id)
    {
        return ApiResponse<AiConversationDto>.Ok(await _aiConversationService.ArchiveConversationAsync(id));
    }

    [HttpPost("conversations/{id:guid}/restore")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiConversationDto>>> RestoreConversationAsync(Guid id)
    {
        return ApiResponse<AiConversationDto>.Ok(await _aiConversationService.RestoreConversationAsync(id));
    }

    [HttpGet("conversations/{id:guid}/export")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    public async Task<ActionResult<ApiResponse<AiConversationExportDto>>> ExportConversationAsync(Guid id)
    {
        return ApiResponse<AiConversationExportDto>.Ok(await _aiConversationService.ExportConversationAsync(id));
    }

    [HttpDelete("conversations/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteConversationAsync(Guid id)
    {
        await _aiConversationService.DeleteConversationAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("tools")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    public async Task<ActionResult<ApiResponse<List<AiToolDefinitionDto>>>> GetToolsAsync()
    {
        return ApiResponse<List<AiToolDefinitionDto>>.Ok(await _aiToolRegistryService.GetToolDefinitionsAsync());
    }

    [HttpPost("tools/{toolCode}/invoke")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiToolInvokeResultDto>>> InvokeToolAsync(string toolCode, [FromBody] AiToolInvokeInput input)
    {
        return ApiResponse<AiToolInvokeResultDto>.Ok(await _aiToolRegistryService.InvokeToolAsync(toolCode, input));
    }

    [HttpGet("tool-invocations")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    public async Task<ActionResult<ApiResponse<List<AiToolInvocationDto>>>> GetToolInvocationsAsync([FromQuery] AiToolInvocationQueryInput input)
    {
        return ApiResponse<List<AiToolInvocationDto>>.Ok(await _aiToolRegistryService.GetToolInvocationsAsync(input));
    }

    [HttpGet("assistants")]
    [Authorize(AdminPermissions.Ai.AssistantManage)]
    public async Task<ActionResult<ApiResponse<List<AiAssistantDto>>>> GetAssistantsAsync([FromQuery] bool includeDisabled = true)
    {
        return ApiResponse<List<AiAssistantDto>>.Ok(await _aiAssistantPromptService.GetAssistantsAsync(includeDisabled));
    }

    [HttpPost("assistants")]
    [Authorize(AdminPermissions.Ai.AssistantManage)]
    [PlatformEndpoint("Ai.Assistant.Manage")]
    public async Task<ActionResult<ApiResponse<AiAssistantDto>>> CreateAssistantAsync([FromBody] AiAssistantSaveInput input)
    {
        return ApiResponse<AiAssistantDto>.Ok(await _aiAssistantPromptService.CreateAssistantAsync(input));
    }

    [HttpPut("assistants/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AssistantManage)]
    [PlatformEndpoint("Ai.Assistant.Manage")]
    public async Task<ActionResult<ApiResponse<AiAssistantDto>>> UpdateAssistantAsync(Guid id, [FromBody] AiAssistantSaveInput input)
    {
        return ApiResponse<AiAssistantDto>.Ok(await _aiAssistantPromptService.UpdateAssistantAsync(id, input));
    }

    [HttpDelete("assistants/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AssistantManage)]
    [PlatformEndpoint("Ai.Assistant.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAssistantAsync(Guid id)
    {
        await _aiAssistantPromptService.DeleteAssistantAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("prompt-templates")]
    [Authorize(AdminPermissions.Ai.PromptManage)]
    public async Task<ActionResult<ApiResponse<List<AiPromptTemplateDto>>>> GetPromptTemplatesAsync([FromQuery] AiPromptTemplateQueryInput input)
    {
        return ApiResponse<List<AiPromptTemplateDto>>.Ok(await _aiAssistantPromptService.GetPromptTemplatesAsync(input));
    }

    [HttpPost("prompt-templates")]
    [Authorize(AdminPermissions.Ai.PromptManage)]
    [PlatformEndpoint("Ai.Prompt.Manage")]
    public async Task<ActionResult<ApiResponse<AiPromptTemplateDto>>> CreatePromptTemplateAsync([FromBody] AiPromptTemplateSaveInput input)
    {
        return ApiResponse<AiPromptTemplateDto>.Ok(await _aiAssistantPromptService.CreatePromptTemplateAsync(input));
    }

    [HttpPut("prompt-templates/{id:guid}")]
    [Authorize(AdminPermissions.Ai.PromptManage)]
    [PlatformEndpoint("Ai.Prompt.Manage")]
    public async Task<ActionResult<ApiResponse<AiPromptTemplateDto>>> UpdatePromptTemplateAsync(Guid id, [FromBody] AiPromptTemplateSaveInput input)
    {
        return ApiResponse<AiPromptTemplateDto>.Ok(await _aiAssistantPromptService.UpdatePromptTemplateAsync(id, input));
    }

    [HttpPost("prompt-templates/dry-run")]
    [Authorize(AdminPermissions.Ai.PromptManage)]
    [PlatformEndpoint("Ai.Prompt.Manage")]
    public async Task<ActionResult<ApiResponse<AiPromptTemplateDryRunBatchResultDto>>> DryRunPromptTemplatesAsync([FromBody] AiPromptTemplateDryRunBatchInput input)
    {
        return ApiResponse<AiPromptTemplateDryRunBatchResultDto>.Ok(await _aiConversationService.DryRunPromptTemplatesAsync(input));
    }

    [HttpPost("prompt-templates/acceptance-run")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiScenarioAcceptanceResultDto>>> RunScenarioAcceptanceAsync([FromBody] AiScenarioAcceptanceInput input)
    {
        return ApiResponse<AiScenarioAcceptanceResultDto>.Ok(await _aiConversationService.RunScenarioAcceptanceAsync(input));
    }

    [HttpPost("prompt-templates/{id:guid}/conversations")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiConversationDetailDto>>> CreateConversationFromPromptTemplateAsync(Guid id, [FromBody] AiPromptTemplateConversationCreateInput input)
    {
        return ApiResponse<AiConversationDetailDto>.Ok(await _aiConversationService.CreateConversationFromTemplateAsync(id, input));
    }

    [HttpPost("prompt-templates/{id:guid}/dry-run")]
    [Authorize(AdminPermissions.Ai.PromptManage)]
    [PlatformEndpoint("Ai.Prompt.Manage")]
    public async Task<ActionResult<ApiResponse<AiPromptTemplateDryRunResultDto>>> DryRunPromptTemplateAsync(Guid id, [FromBody] AiPromptTemplateDryRunInput input)
    {
        return ApiResponse<AiPromptTemplateDryRunResultDto>.Ok(await _aiConversationService.DryRunPromptTemplateAsync(id, input));
    }

    [HttpDelete("prompt-templates/{id:guid}")]
    [Authorize(AdminPermissions.Ai.PromptManage)]
    [PlatformEndpoint("Ai.Prompt.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePromptTemplateAsync(Guid id)
    {
        await _aiAssistantPromptService.DeletePromptTemplateAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("skills")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    public async Task<ActionResult<ApiResponse<List<AiSkillDto>>>> GetSkillsAsync([FromQuery] AiAgentDesignQueryInput input)
    {
        return ApiResponse<List<AiSkillDto>>.Ok(await _aiAgentDesignService.GetSkillsAsync(input));
    }

    [HttpPost("skills")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiSkillDto>>> CreateSkillAsync([FromBody] AiSkillSaveInput input)
    {
        return ApiResponse<AiSkillDto>.Ok(await _aiAgentDesignService.CreateSkillAsync(input));
    }

    [HttpPut("skills/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiSkillDto>>> UpdateSkillAsync(Guid id, [FromBody] AiSkillSaveInput input)
    {
        return ApiResponse<AiSkillDto>.Ok(await _aiAgentDesignService.UpdateSkillAsync(id, input));
    }

    [HttpDelete("skills/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSkillAsync(Guid id)
    {
        await _aiAgentDesignService.DeleteSkillAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("personas")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    public async Task<ActionResult<ApiResponse<List<AiPersonaDto>>>> GetPersonasAsync([FromQuery] AiAgentDesignQueryInput input)
    {
        return ApiResponse<List<AiPersonaDto>>.Ok(await _aiAgentDesignService.GetPersonasAsync(input));
    }

    [HttpPost("personas")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiPersonaDto>>> CreatePersonaAsync([FromBody] AiPersonaSaveInput input)
    {
        return ApiResponse<AiPersonaDto>.Ok(await _aiAgentDesignService.CreatePersonaAsync(input));
    }

    [HttpPut("personas/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiPersonaDto>>> UpdatePersonaAsync(Guid id, [FromBody] AiPersonaSaveInput input)
    {
        return ApiResponse<AiPersonaDto>.Ok(await _aiAgentDesignService.UpdatePersonaAsync(id, input));
    }

    [HttpDelete("personas/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePersonaAsync(Guid id)
    {
        await _aiAgentDesignService.DeletePersonaAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("commands")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    public async Task<ActionResult<ApiResponse<List<AiCommandDto>>>> GetCommandsAsync([FromQuery] AiAgentDesignQueryInput input)
    {
        return ApiResponse<List<AiCommandDto>>.Ok(await _aiAgentDesignService.GetCommandsAsync(input));
    }

    [HttpPost("commands")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiCommandDto>>> CreateCommandAsync([FromBody] AiCommandSaveInput input)
    {
        return ApiResponse<AiCommandDto>.Ok(await _aiAgentDesignService.CreateCommandAsync(input));
    }

    [HttpPut("commands/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiCommandDto>>> UpdateCommandAsync(Guid id, [FromBody] AiCommandSaveInput input)
    {
        return ApiResponse<AiCommandDto>.Ok(await _aiAgentDesignService.UpdateCommandAsync(id, input));
    }

    [HttpDelete("commands/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCommandAsync(Guid id)
    {
        await _aiAgentDesignService.DeleteCommandAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("agents")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    public async Task<ActionResult<ApiResponse<List<AiAgentDefinitionDto>>>> GetAgentsAsync([FromQuery] AiAgentDesignQueryInput input)
    {
        return ApiResponse<List<AiAgentDefinitionDto>>.Ok(await _aiAgentDesignService.GetAgentsAsync(input));
    }

    [HttpPost("agents")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiAgentDefinitionDto>>> CreateAgentAsync([FromBody] AiAgentDefinitionSaveInput input)
    {
        return ApiResponse<AiAgentDefinitionDto>.Ok(await _aiAgentDesignService.CreateAgentAsync(input));
    }

    [HttpPut("agents/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiAgentDefinitionDto>>> UpdateAgentAsync(Guid id, [FromBody] AiAgentDefinitionSaveInput input)
    {
        return ApiResponse<AiAgentDefinitionDto>.Ok(await _aiAgentDesignService.UpdateAgentAsync(id, input));
    }

    [HttpPost("agents/{id:guid}/run-plan")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<AiAgentRunPlanDto>>> BuildAgentRunPlanAsync(Guid id, [FromBody] AiAgentRunPlanInput input)
    {
        return ApiResponse<AiAgentRunPlanDto>.Ok(await _aiAgentRuntime.BuildRunPlanAsync(id, input));
    }

    [HttpDelete("agents/{id:guid}")]
    [Authorize(AdminPermissions.Ai.AgentManage)]
    [PlatformEndpoint("Ai.Agent.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAgentAsync(Guid id)
    {
        await _aiAgentDesignService.DeleteAgentAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("artifacts")]
    [Authorize(AdminPermissions.Ai.ArtifactManage)]
    public async Task<ActionResult<ApiResponse<List<AiArtifactDto>>>> GetArtifactsAsync([FromQuery] AiArtifactQueryInput input)
    {
        return ApiResponse<List<AiArtifactDto>>.Ok(await _aiAgentDesignService.GetArtifactsAsync(input));
    }

    [HttpPost("artifacts")]
    [Authorize(AdminPermissions.Ai.ArtifactManage)]
    [PlatformEndpoint("Ai.Artifact.Manage")]
    public async Task<ActionResult<ApiResponse<AiArtifactDto>>> CreateArtifactAsync([FromBody] AiArtifactSaveInput input)
    {
        return ApiResponse<AiArtifactDto>.Ok(await _aiAgentDesignService.CreateArtifactAsync(input));
    }

    [HttpPut("artifacts/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ArtifactManage)]
    [PlatformEndpoint("Ai.Artifact.Manage")]
    public async Task<ActionResult<ApiResponse<AiArtifactDto>>> UpdateArtifactAsync(Guid id, [FromBody] AiArtifactSaveInput input)
    {
        return ApiResponse<AiArtifactDto>.Ok(await _aiAgentDesignService.UpdateArtifactAsync(id, input));
    }

    [HttpDelete("artifacts/{id:guid}")]
    [Authorize(AdminPermissions.Ai.ArtifactManage)]
    [PlatformEndpoint("Ai.Artifact.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteArtifactAsync(Guid id)
    {
        await _aiAgentDesignService.DeleteArtifactAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("web-search/configs")]
    [Authorize(AdminPermissions.Ai.WebSearchManage)]
    public async Task<ActionResult<ApiResponse<List<AiWebSearchConfigDto>>>> GetWebSearchConfigsAsync([FromQuery] bool includeDisabled = true)
    {
        return ApiResponse<List<AiWebSearchConfigDto>>.Ok(await _aiWebSearchService.GetConfigsAsync(includeDisabled));
    }

    [HttpPost("web-search/configs")]
    [Authorize(AdminPermissions.Ai.WebSearchManage)]
    [PlatformEndpoint("Ai.WebSearch.Manage")]
    public async Task<ActionResult<ApiResponse<AiWebSearchConfigDto>>> CreateWebSearchConfigAsync([FromBody] AiWebSearchConfigSaveInput input)
    {
        return ApiResponse<AiWebSearchConfigDto>.Ok(await _aiWebSearchService.CreateConfigAsync(input));
    }

    [HttpPut("web-search/configs/{id:guid}")]
    [Authorize(AdminPermissions.Ai.WebSearchManage)]
    [PlatformEndpoint("Ai.WebSearch.Manage")]
    public async Task<ActionResult<ApiResponse<AiWebSearchConfigDto>>> UpdateWebSearchConfigAsync(Guid id, [FromBody] AiWebSearchConfigSaveInput input)
    {
        return ApiResponse<AiWebSearchConfigDto>.Ok(await _aiWebSearchService.UpdateConfigAsync(id, input));
    }

    [HttpDelete("web-search/configs/{id:guid}")]
    [Authorize(AdminPermissions.Ai.WebSearchManage)]
    [PlatformEndpoint("Ai.WebSearch.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWebSearchConfigAsync(Guid id)
    {
        await _aiWebSearchService.DeleteConfigAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("web-search/records")]
    [Authorize(AdminPermissions.Ai.WebSearchManage)]
    public async Task<ActionResult<ApiResponse<List<AiWebSearchRecordDto>>>> GetWebSearchRecordsAsync([FromQuery] AiWebSearchRecordQueryInput input)
    {
        return ApiResponse<List<AiWebSearchRecordDto>>.Ok(await _aiWebSearchService.GetRecordsAsync(input));
    }

    [HttpPost("web-search/search")]
    [Authorize(AdminPermissions.Ai.WebSearchManage)]
    [PlatformEndpoint("Ai.WebSearch.Manage")]
    public async Task<ActionResult<ApiResponse<AiWebSearchResultDto>>> RunWebSearchAsync([FromBody] AiWebSearchInput input)
    {
        return ApiResponse<AiWebSearchResultDto>.Ok(await _aiWebSearchService.SearchAsync(input));
    }

    [HttpGet("mcp/servers")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    public async Task<ActionResult<ApiResponse<List<AiMcpServerDto>>>> GetMcpServersAsync()
    {
        return ApiResponse<List<AiMcpServerDto>>.Ok(await _aiMcpService.GetServersAsync());
    }

    [HttpPost("mcp/servers")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    [PlatformEndpoint("Ai.Mcp.Manage")]
    public async Task<ActionResult<ApiResponse<AiMcpServerDto>>> CreateMcpServerAsync([FromBody] AiMcpServerSaveInput input)
    {
        return ApiResponse<AiMcpServerDto>.Ok(await _aiMcpService.CreateServerAsync(input));
    }

    [HttpPut("mcp/servers/{id:guid}")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    [PlatformEndpoint("Ai.Mcp.Manage")]
    public async Task<ActionResult<ApiResponse<AiMcpServerDto>>> UpdateMcpServerAsync(Guid id, [FromBody] AiMcpServerSaveInput input)
    {
        return ApiResponse<AiMcpServerDto>.Ok(await _aiMcpService.UpdateServerAsync(id, input));
    }

    [HttpDelete("mcp/servers/{id:guid}")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    [PlatformEndpoint("Ai.Mcp.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMcpServerAsync(Guid id)
    {
        await _aiMcpService.DeleteServerAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("mcp/servers/{id:guid}/test")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    [PlatformEndpoint("Ai.Mcp.Manage")]
    public async Task<ActionResult<ApiResponse<AiMcpConnectionTestResultDto>>> TestMcpServerAsync(Guid id)
    {
        return ApiResponse<AiMcpConnectionTestResultDto>.Ok(await _aiMcpService.TestConnectionAsync(id));
    }

    [HttpPost("mcp/servers/{id:guid}/discover-tools")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    [PlatformEndpoint("Ai.Mcp.Manage")]
    public async Task<ActionResult<ApiResponse<AiMcpDiscoveryResultDto>>> DiscoverMcpToolsAsync(Guid id)
    {
        return ApiResponse<AiMcpDiscoveryResultDto>.Ok(await _aiMcpService.DiscoverToolsAsync(id));
    }

    [HttpGet("mcp/tools")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    public async Task<ActionResult<ApiResponse<List<AiMcpToolDto>>>> GetMcpToolsAsync([FromQuery] Guid? serverId = null)
    {
        return ApiResponse<List<AiMcpToolDto>>.Ok(await _aiMcpService.GetToolsAsync(serverId));
    }

    [HttpPut("mcp/tools/{id:guid}/policy")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    [PlatformEndpoint("Ai.Mcp.Manage")]
    public async Task<ActionResult<ApiResponse<AiMcpToolDto>>> UpdateMcpToolPolicyAsync(Guid id, [FromBody] AiMcpToolPolicyInput input)
    {
        return ApiResponse<AiMcpToolDto>.Ok(await _aiMcpService.UpdateToolPolicyAsync(id, input));
    }

    [HttpPost("mcp/tools/{id:guid}/invoke")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    [PlatformEndpoint("Ai.Mcp.Manage")]
    public async Task<ActionResult<ApiResponse<AiMcpInvokeResultDto>>> InvokeMcpToolAsync(Guid id, [FromBody] AiMcpInvokeInput input)
    {
        return ApiResponse<AiMcpInvokeResultDto>.Ok(await _aiMcpService.InvokeToolAsync(id, input));
    }

    [HttpGet("mcp/invocations")]
    [Authorize(AdminPermissions.Ai.McpManage)]
    public async Task<ActionResult<ApiResponse<List<AiMcpInvocationDto>>>> GetMcpInvocationsAsync([FromQuery] AiMcpInvocationQueryInput input)
    {
        return ApiResponse<List<AiMcpInvocationDto>>.Ok(await _aiMcpService.GetInvocationsAsync(input));
    }

    [HttpGet("memories")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    public async Task<ActionResult<ApiResponse<List<AiMemoryDto>>>> GetMemoriesAsync([FromQuery] AiMemoryQueryInput input)
    {
        return ApiResponse<List<AiMemoryDto>>.Ok(await _aiMemoryService.GetMemoriesAsync(input));
    }

    [HttpPost("memories")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    [PlatformEndpoint("Ai.Memory.Manage")]
    public async Task<ActionResult<ApiResponse<AiMemoryDto>>> CreateMemoryAsync([FromBody] AiMemorySaveInput input)
    {
        return ApiResponse<AiMemoryDto>.Ok(await _aiMemoryService.CreateMemoryAsync(input));
    }

    [HttpPut("memories/{id:guid}")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    [PlatformEndpoint("Ai.Memory.Manage")]
    public async Task<ActionResult<ApiResponse<AiMemoryDto>>> UpdateMemoryAsync(Guid id, [FromBody] AiMemorySaveInput input)
    {
        return ApiResponse<AiMemoryDto>.Ok(await _aiMemoryService.UpdateMemoryAsync(id, input));
    }

    [HttpPost("memories/{id:guid}/toggle")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    [PlatformEndpoint("Ai.Memory.Manage")]
    public async Task<ActionResult<ApiResponse<AiMemoryDto>>> ToggleMemoryAsync(Guid id, [FromBody] AiMemoryToggleInput input)
    {
        return ApiResponse<AiMemoryDto>.Ok(await _aiMemoryService.ToggleMemoryAsync(id, input.IsEnabled));
    }

    [HttpDelete("memories/{id:guid}")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    [PlatformEndpoint("Ai.Memory.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteMemoryAsync(Guid id)
    {
        await _aiMemoryService.DeleteMemoryAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpGet("knowledge-sources")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    public async Task<ActionResult<ApiResponse<List<AiKnowledgeSourceDto>>>> GetKnowledgeSourcesAsync([FromQuery] AiKnowledgeSourceQueryInput input)
    {
        return ApiResponse<List<AiKnowledgeSourceDto>>.Ok(await _aiMemoryService.GetKnowledgeSourcesAsync(input));
    }

    [HttpPost("knowledge-sources/project/{projectId:guid}/documents/import")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    [PlatformEndpoint("Ai.Memory.Manage")]
    public async Task<ActionResult<ApiResponse<AiMemoryImportResultDto>>> ImportProjectDocumentsAsync(Guid projectId)
    {
        return ApiResponse<AiMemoryImportResultDto>.Ok(await _aiMemoryService.ImportProjectDocumentsAsync(projectId));
    }

    [HttpPost("knowledge-sources/project/{projectId:guid}/meetings/import")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    [PlatformEndpoint("Ai.Memory.Manage")]
    public async Task<ActionResult<ApiResponse<AiMemoryImportResultDto>>> ImportProjectMeetingsAsync(Guid projectId)
    {
        return ApiResponse<AiMemoryImportResultDto>.Ok(await _aiMemoryService.ImportProjectMeetingsAsync(projectId));
    }

    [HttpPost("knowledge-sources/project/{projectId:guid}/risks/import")]
    [Authorize(AdminPermissions.Ai.MemoryManage)]
    [PlatformEndpoint("Ai.Memory.Manage")]
    public async Task<ActionResult<ApiResponse<AiMemoryImportResultDto>>> ImportProjectRisksAsync(Guid projectId)
    {
        return ApiResponse<AiMemoryImportResultDto>.Ok(await _aiMemoryService.ImportProjectRisksAsync(projectId));
    }

    [HttpGet("jobs")]
    [Authorize(AdminPermissions.Ai.JobManage)]
    public async Task<ActionResult<ApiResponse<List<AiJobDto>>>> GetJobsAsync([FromQuery] AiJobQueryInput input)
    {
        return ApiResponse<List<AiJobDto>>.Ok(await _aiJobService.GetJobsAsync(input));
    }

    [HttpPost("jobs")]
    [Authorize(AdminPermissions.Ai.JobManage)]
    [PlatformEndpoint("Ai.Job.Manage")]
    public async Task<ActionResult<ApiResponse<AiJobDto>>> CreateJobAsync([FromBody] AiJobSaveInput input)
    {
        return ApiResponse<AiJobDto>.Ok(await _aiJobService.CreateJobAsync(input));
    }

    [HttpPut("jobs/{id:guid}")]
    [Authorize(AdminPermissions.Ai.JobManage)]
    [PlatformEndpoint("Ai.Job.Manage")]
    public async Task<ActionResult<ApiResponse<AiJobDto>>> UpdateJobAsync(Guid id, [FromBody] AiJobSaveInput input)
    {
        return ApiResponse<AiJobDto>.Ok(await _aiJobService.UpdateJobAsync(id, input));
    }

    [HttpPost("jobs/{id:guid}/toggle")]
    [Authorize(AdminPermissions.Ai.JobManage)]
    [PlatformEndpoint("Ai.Job.Manage")]
    public async Task<ActionResult<ApiResponse<AiJobDto>>> ToggleJobAsync(Guid id, [FromBody] AiJobToggleInput input)
    {
        return ApiResponse<AiJobDto>.Ok(await _aiJobService.ToggleJobAsync(id, input.IsEnabled));
    }

    [HttpDelete("jobs/{id:guid}")]
    [Authorize(AdminPermissions.Ai.JobManage)]
    [PlatformEndpoint("Ai.Job.Manage")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteJobAsync(Guid id)
    {
        await _aiJobService.DeleteJobAsync(id);
        return ApiResponse<bool>.Ok(true);
    }

    [HttpPost("jobs/{id:guid}/run")]
    [Authorize(AdminPermissions.Ai.JobRun)]
    [PlatformEndpoint("Ai.Job.Run")]
    public async Task<ActionResult<ApiResponse<AiJobRunResultDto>>> RunJobAsync(Guid id)
    {
        return ApiResponse<AiJobRunResultDto>.Ok(await _aiJobService.RunJobAsync(id));
    }

    [HttpGet("job-runs")]
    [Authorize(AdminPermissions.Ai.JobManage)]
    public async Task<ActionResult<ApiResponse<List<AiJobRunDto>>>> GetJobRunsAsync([FromQuery] AiJobRunQueryInput input)
    {
        return ApiResponse<List<AiJobRunDto>>.Ok(await _aiJobService.GetJobRunsAsync(input));
    }

    [HttpGet("usage/dashboard")]
    [Authorize(AdminPermissions.Ai.UsageView)]
    public async Task<ActionResult<ApiResponse<AiUsageDashboardDto>>> GetUsageDashboardAsync([FromQuery] AiUsageQueryInput input)
    {
        return ApiResponse<AiUsageDashboardDto>.Ok(await _aiUsageService.GetDashboardAsync(input));
    }

    [HttpGet("usage/records")]
    [Authorize(AdminPermissions.Ai.UsageView)]
    public async Task<ActionResult<ApiResponse<List<AiUsageRecordDto>>>> GetUsageRecordsAsync([FromQuery] AiUsageQueryInput input)
    {
        return ApiResponse<List<AiUsageRecordDto>>.Ok(await _aiUsageService.GetRecordsAsync(input));
    }

    [HttpGet("writebacks")]
    [Authorize(AdminPermissions.Ai.WritebackView)]
    public async Task<ActionResult<ApiResponse<AiPagedResultDto<AiWritebackRequestDto>>>> GetWritebacksAsync([FromQuery] AiWritebackRequestQueryInput input)
    {
        return ApiResponse<AiPagedResultDto<AiWritebackRequestDto>>.Ok(await _aiWritebackService.GetRequestsAsync(input));
    }

    [HttpPost("writebacks")]
    [Authorize(AdminPermissions.Ai.ConversationUse)]
    [PlatformEndpoint("Ai.Conversation.Use")]
    public async Task<ActionResult<ApiResponse<AiWritebackRequestDto>>> CreateWritebackAsync([FromBody] AiWritebackRequestCreateInput input)
    {
        return ApiResponse<AiWritebackRequestDto>.Ok(await _aiWritebackService.CreateRequestAsync(input));
    }

    [HttpPost("writebacks/{id:guid}/approve")]
    [Authorize(AdminPermissions.Ai.WritebackApprove)]
    [PlatformEndpoint("Ai.Writeback.Approve")]
    public async Task<ActionResult<ApiResponse<AiWritebackRequestDto>>> ApproveWritebackAsync(Guid id)
    {
        return ApiResponse<AiWritebackRequestDto>.Ok(await _aiWritebackService.ApproveAsync(id));
    }

    [HttpPost("writebacks/{id:guid}/reject")]
    [Authorize(AdminPermissions.Ai.WritebackApprove)]
    [PlatformEndpoint("Ai.Writeback.Approve")]
    public async Task<ActionResult<ApiResponse<AiWritebackRequestDto>>> RejectWritebackAsync(Guid id, [FromBody] AiWritebackDecisionInput input)
    {
        return ApiResponse<AiWritebackRequestDto>.Ok(await _aiWritebackService.RejectAsync(id, input));
    }

    [HttpPost("writebacks/{id:guid}/cancel")]
    [Authorize(AdminPermissions.Ai.WritebackApprove)]
    [PlatformEndpoint("Ai.Writeback.Approve")]
    public async Task<ActionResult<ApiResponse<AiWritebackRequestDto>>> CancelWritebackAsync(Guid id)
    {
        return ApiResponse<AiWritebackRequestDto>.Ok(await _aiWritebackService.CancelAsync(id));
    }
}
