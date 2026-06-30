using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Users;

namespace General.Admin.Ai;

public class AiConversationService : ITransientDependency
{
    private const int MaxAgentToolIterations = 3;
    private const int MaxCompressedContextMessages = 24;
    private const int MaxRecentContextMessages = 18;
    private const int DefaultContextBudgetTokens = 256_000;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly string[] RequiredBuiltinTemplateCodes =
    [
        "project-weekly-report",
        "meeting-minutes-cleanup",
        "risk-analysis",
        "issue-root-cause",
        "change-impact",
        "business-risk-explanation",
        "receivable-collection-suggestion",
        "project-retrospective"
    ];
    private static readonly AiScenarioAcceptanceCase[] DefaultAcceptanceCases =
    [
        new("clarify-mode", "澄清模式", "meeting-minutes-cleanup", AiConversationModeNames.Clarify),
        new("plan-mode", "规划模式", "project-weekly-report", AiConversationModeNames.Plan),
        new("project-weekly-report", "项目周报场景模板", "project-weekly-report", AiConversationModeNames.Execution),
        new("meeting-minutes-cleanup", "会议纪要场景模板", "meeting-minutes-cleanup", AiConversationModeNames.Execution),
        new("risk-analysis", "风险分析场景模板", "risk-analysis", AiConversationModeNames.Execution),
        new("business-risk-explanation", "经营解释场景模板", "business-risk-explanation", AiConversationModeNames.Execution)
    ];

    private readonly IRepository<AiConversation, Guid> _conversationRepository;
    private readonly ICurrentUser _currentUser;
    private readonly AiModelGatewayService _gatewayService;
    private readonly AiMemoryService _memoryService;
    private readonly AiUsageService _usageService;
    private readonly AiToolRegistryService _toolRegistryService;
    private readonly AiMcpService _mcpService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiMessage, Guid> _messageRepository;
    private readonly IRepository<AiModel, Guid> _modelRepository;
    private readonly IRepository<AiAssistant, Guid> _assistantRepository;
    private readonly IRepository<AiAgentDefinition, Guid> _agentRepository;
    private readonly IRepository<AiCommand, Guid> _commandRepository;
    private readonly IRepository<AiPersona, Guid> _personaRepository;
    private readonly IRepository<AiPromptTemplate, Guid> _promptTemplateRepository;
    private readonly IRepository<AiProvider, Guid> _providerRepository;
    private readonly IRepository<AiSkill, Guid> _skillRepository;

    public AiConversationService(
        ICurrentUser currentUser,
        AiModelGatewayService gatewayService,
        AiMemoryService memoryService,
        AiUsageService usageService,
        AiToolRegistryService toolRegistryService,
        AiMcpService mcpService,
        IGuidGenerator guidGenerator,
        IRepository<AiConversation, Guid> conversationRepository,
        IRepository<AiMessage, Guid> messageRepository,
        IRepository<AiModel, Guid> modelRepository,
        IRepository<AiAssistant, Guid> assistantRepository,
        IRepository<AiAgentDefinition, Guid> agentRepository,
        IRepository<AiCommand, Guid> commandRepository,
        IRepository<AiPersona, Guid> personaRepository,
        IRepository<AiPromptTemplate, Guid> promptTemplateRepository,
        IRepository<AiProvider, Guid> providerRepository,
        IRepository<AiSkill, Guid> skillRepository)
    {
        _currentUser = currentUser;
        _gatewayService = gatewayService;
        _memoryService = memoryService;
        _usageService = usageService;
        _toolRegistryService = toolRegistryService;
        _mcpService = mcpService;
        _guidGenerator = guidGenerator;
        _conversationRepository = conversationRepository;
        _messageRepository = messageRepository;
        _modelRepository = modelRepository;
        _assistantRepository = assistantRepository;
        _agentRepository = agentRepository;
        _commandRepository = commandRepository;
        _personaRepository = personaRepository;
        _promptTemplateRepository = promptTemplateRepository;
        _providerRepository = providerRepository;
        _skillRepository = skillRepository;
    }

    public async Task<List<AiConversationDto>> GetConversationsAsync(AiConversationQueryInput input)
    {
        var conversations = ApplyCurrentUserFilter(await _conversationRepository.GetListAsync());
        var status = string.IsNullOrWhiteSpace(input.Status) ? "Active" : input.Status.Trim();
        if (!status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            conversations = conversations
                .Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            conversations = conversations
                .Where(x =>
                    x.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.LastMessageSummary.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var modelNames = await ResolveModelNamesAsync();
        return conversations
            .OrderByDescending(x => x.LastMessageTime ?? x.CreationTime)
            .Select(item => MapConversation(item, modelNames))
            .ToList();
    }

    public async Task<AiConversationDetailDto> GetConversationAsync(Guid id)
    {
        var conversation = await GetOwnedConversationAsync(id);
        var messages = (await _messageRepository.GetListAsync(x => x.ConversationId == id))
            .OrderBy(x => x.SequenceNo)
            .ToList();
        var modelNames = await ResolveModelNamesAsync();
        var providerNames = await ResolveProviderNamesAsync();

        return MapConversationDetail(conversation, messages, modelNames, providerNames);
    }

    public async Task<AiConversationDetailDto> CreateConversationAsync(AiConversationCreateInput input)
    {
        var model = await ResolveModelAsync(input.ModelId);
        var title = string.IsNullOrWhiteSpace(input.Title) ? "新的 AI 会话" : input.Title.Trim();
        var conversation = new AiConversation(
            _guidGenerator.Create(),
            title,
            model?.Id,
            input.Mode,
            input.ContextType,
            input.ContextId,
            input.ContextSnapshotJson);

        await _conversationRepository.InsertAsync(conversation, autoSave: true);
        return await GetConversationAsync(conversation.Id);
    }

    public async Task<AiConversationDetailDto> CreateConversationFromTemplateAsync(Guid templateId, AiPromptTemplateConversationCreateInput input)
    {
        var template = await _promptTemplateRepository.GetAsync(templateId);
        if (!template.IsEnabled)
        {
            throw new UserFriendlyException("场景模板已停用，不能发起会话。");
        }

        var assistant = string.IsNullOrWhiteSpace(template.AssistantCode)
            ? null
            : (await _assistantRepository.GetListAsync(x => x.AssistantCode == template.AssistantCode && x.IsEnabled))
                .OrderBy(x => x.Sort)
                .FirstOrDefault();
        var modelId = input.ModelId ?? assistant?.DefaultModelId;
        var title = string.IsNullOrWhiteSpace(input.Title)
            ? $"{template.Name} · {DateTime.Now:yyyyMMdd HHmm}"
            : input.Title.Trim();
        var conversation = await CreateConversationAsync(new AiConversationCreateInput
        {
            ContextId = input.ContextId,
            ContextSnapshotJson = input.ContextSnapshotJson,
            ContextType = input.ContextType,
            Mode = template.Mode,
            ModelId = modelId,
            Title = title
        });

        var renderedPrompt = RenderTemplatePrompt(template, assistant, input.InputJson);
        await SendMessageAsync(conversation.Id, new AiConversationSendInput
        {
            Content = renderedPrompt,
            ModelId = modelId
        });
        return await GetConversationAsync(conversation.Id);
    }

    private async Task<AiConversationDetailDto> CreateConversationFromTemplateAsync(
        AiPromptTemplate template,
        AiPromptTemplateConversationCreateInput input,
        string mode)
    {
        if (!template.IsEnabled)
        {
            throw new UserFriendlyException("场景模板已停用，不能发起会话。");
        }

        var assistant = await ResolvePromptTemplateAssistantAsync(template, onlyEnabled: true);
        var modelId = input.ModelId ?? assistant?.DefaultModelId;
        var title = string.IsNullOrWhiteSpace(input.Title)
            ? $"{template.Name} · {DateTime.Now:yyyyMMdd HHmm}"
            : input.Title.Trim();
        var conversation = await CreateConversationAsync(new AiConversationCreateInput
        {
            ContextId = input.ContextId,
            ContextSnapshotJson = input.ContextSnapshotJson,
            ContextType = input.ContextType,
            Mode = mode,
            ModelId = modelId,
            Title = title
        });

        await SendMessageAsync(conversation.Id, new AiConversationSendInput
        {
            Content = RenderTemplatePrompt(template, assistant, input.InputJson, mode),
            ModelId = modelId
        });
        return await GetConversationAsync(conversation.Id);
    }

    public async Task<AiPromptTemplateDryRunResultDto> DryRunPromptTemplateAsync(Guid templateId, AiPromptTemplateDryRunInput input)
    {
        var template = await _promptTemplateRepository.GetAsync(templateId);
        var assistant = await ResolvePromptTemplateAssistantAsync(template, onlyEnabled: false);
        var toolDefinitions = await _toolRegistryService.GetToolDefinitionsAsync();
        var recommendedToolCodes = DeserializeList(template.ToolCodesJson);
        var missingToolCodes = recommendedToolCodes
            .Where(toolCode => toolDefinitions.All(item => !item.ToolCode.Equals(toolCode, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var renderedPrompt = RenderTemplatePrompt(template, assistant, input.InputJson);
        var structuredNode = BuildDryRunStructuredNode(template, assistant, input, recommendedToolCodes);
        var structuredOutput = ResolveStructuredOutput(
            AiGatewayChatResult.Ok(
                JsonSerializer.Serialize(structuredNode, JsonOptions),
                promptTokens: EstimateTokens(renderedPrompt),
                completionTokens: EstimateTokens(JsonSerializer.Serialize(structuredNode, JsonOptions)),
                model: assistant?.DefaultModelId?.ToString() ?? string.Empty,
                finishReason: "DryRun",
                rawResponseJson: JsonSerializer.Serialize(structuredNode, JsonOptions)),
            template.Mode);

        var errors = new List<string>();
        if (!template.IsEnabled)
        {
            errors.Add("模板已停用。");
        }

        if (assistant == null && !string.IsNullOrWhiteSpace(template.AssistantCode))
        {
            errors.Add($"未找到可用助手：{template.AssistantCode}");
        }

        if (!string.IsNullOrWhiteSpace(template.InputSchemaJson) && !IsValidJson(template.InputSchemaJson))
        {
            errors.Add("输入 Schema 不合法。");
        }

        if (!string.IsNullOrWhiteSpace(template.OutputSchemaJson) && !IsValidJson(template.OutputSchemaJson))
        {
            errors.Add("输出 Schema 不合法。");
        }

        return new AiPromptTemplateDryRunResultDto
        {
            AssistantCode = assistant?.AssistantCode ?? template.AssistantCode,
            AssistantName = assistant?.Name ?? string.Empty,
            ContextId = input.ContextId,
            ContextSnapshotJson = string.IsNullOrWhiteSpace(input.ContextSnapshotJson) ? "{}" : input.ContextSnapshotJson.Trim(),
            ContextType = string.IsNullOrWhiteSpace(input.ContextType) ? "None" : input.ContextType.Trim(),
            DisplayContent = structuredOutput.DisplayContent,
            Errors = errors,
            EstimatedCompletionTokens = EstimateTokens(structuredOutput.DisplayContent),
            EstimatedPromptTokens = EstimateTokens(renderedPrompt),
            InputJson = string.IsNullOrWhiteSpace(input.InputJson) ? "{}" : input.InputJson.Trim(),
            IsModelCallSkipped = true,
            MissingToolCodes = missingToolCodes,
            Mode = template.Mode,
            RenderedPrompt = renderedPrompt,
            RecommendedToolCodes = recommendedToolCodes,
            Scenario = template.Scenario,
            StructuredOutputJson = structuredOutput.StructuredNode?.ToJsonString(JsonOptions) ?? "{}",
            Success = errors.Count == 0,
            TemplateCode = template.TemplateCode,
            TemplateId = template.Id,
            TemplateName = template.Name,
            Warnings = new List<string>
            {
                assistant == null ? "模板未绑定助手，当前使用通用模板角色进行预览。" : string.Empty,
                missingToolCodes.Count > 0 ? $"有 {missingToolCodes.Count} 个推荐工具未注册。" : string.Empty
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToList()
        };
    }

    public async Task<AiPromptTemplateDryRunBatchResultDto> DryRunPromptTemplatesAsync(AiPromptTemplateDryRunBatchInput input)
    {
        var templates = await _promptTemplateRepository.GetListAsync();
        if (!input.IncludeDisabled)
        {
            templates = templates.Where(x => x.IsEnabled).ToList();
        }

        if (input.OnlyBuiltin)
        {
            templates = templates.Where(x => x.IsBuiltin).ToList();
        }

        if (input.TemplateCodes.Count > 0)
        {
            var selectedCodes = input.TemplateCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            templates = templates.Where(x => selectedCodes.Contains(x.TemplateCode)).ToList();
        }

        var orderedTemplates = templates
            .OrderBy(x => x.Category)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.TemplateCode)
            .ToList();
        var items = new List<AiPromptTemplateDryRunResultDto>();
        foreach (var template in orderedTemplates)
        {
            items.Add(await DryRunPromptTemplateAsync(template.Id, input));
        }

        var existingCodes = templates.Select(x => x.TemplateCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingRequiredCodes = input.OnlyBuiltin
            ? RequiredBuiltinTemplateCodes.Where(x => !existingCodes.Contains(x)).ToList()
            : [];
        var failedCount = items.Count(x => !x.Success) + missingRequiredCodes.Count;
        return new AiPromptTemplateDryRunBatchResultDto
        {
            FailedCount = failedCount,
            Items = items,
            MissingRequiredTemplateCodes = missingRequiredCodes,
            PassedCount = items.Count(x => x.Success),
            Success = failedCount == 0,
            TotalCount = items.Count
        };
    }

    public async Task<AiScenarioAcceptanceResultDto> RunScenarioAcceptanceAsync(AiScenarioAcceptanceInput input)
    {
        var templates = await _promptTemplateRepository.GetListAsync();
        var selectedCodes = input.TemplateCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var acceptanceCases = DefaultAcceptanceCases
            .Where(item => selectedCodes.Count == 0 || selectedCodes.Contains(item.TemplateCode) || selectedCodes.Contains(item.AcceptanceCode))
            .ToList();
        var templateMap = templates
            .GroupBy(x => x.TemplateCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(item => item.IsEnabled).First(), StringComparer.OrdinalIgnoreCase);
        var items = new List<AiScenarioAcceptanceItemDto>();

        foreach (var acceptanceCase in acceptanceCases)
        {
            if (!templateMap.TryGetValue(acceptanceCase.TemplateCode, out var template))
            {
                items.Add(new AiScenarioAcceptanceItemDto
                {
                    AcceptanceCode = acceptanceCase.AcceptanceCode,
                    AcceptanceName = acceptanceCase.AcceptanceName,
                    ErrorMessage = $"缺少内置场景模板：{acceptanceCase.TemplateCode}",
                    Success = false,
                    TemplateCode = acceptanceCase.TemplateCode
                });
                continue;
            }

            items.Add(await RunScenarioAcceptanceCaseAsync(acceptanceCase, template, input));
        }

        var expectedCodes = acceptanceCases.Select(x => x.TemplateCode).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existingCodes = templates.Select(x => x.TemplateCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missingRequiredCodes = expectedCodes.Where(x => !existingCodes.Contains(x)).ToList();
        var failedCount = items.Count(x => !x.Success) + missingRequiredCodes.Count;
        return new AiScenarioAcceptanceResultDto
        {
            FailedCount = failedCount,
            Items = items,
            MissingRequiredTemplateCodes = missingRequiredCodes,
            PassedCount = items.Count(x => x.Success),
            Success = failedCount == 0,
            TotalCount = items.Count
        };
    }

    public async Task<AiConversationDto> UpdateConversationAsync(Guid id, AiConversationUpdateInput input)
    {
        var conversation = await GetOwnedConversationAsync(id);
        var model = await ResolveModelAsync(input.ModelId);
        conversation.Rename(input.Title);
        conversation.UpdateModel(model?.Id);
        conversation.UpdateContext(input.ContextType, input.ContextId, input.ContextSnapshotJson);
        await _conversationRepository.UpdateAsync(conversation, autoSave: true);
        return MapConversation(conversation, await ResolveModelNamesAsync());
    }

    public async Task<AiConversationSendResultDto> SendMessageAsync(Guid id, AiConversationSendInput input)
    {
        var prepared = await PrepareSendAsync(id, input);
        var assistantMessageId = _guidGenerator.Create();
        var gatewayCall = await SendToGatewayAsync(
            prepared.Conversation,
            prepared.Provider,
            prepared.Model,
            prepared.GatewayMessages,
            prepared.Content,
            prepared.ConversationContext,
            quotaAlreadyChecked: false,
            assistantMessageId);
        var assistantMessage = CreateAssistantMessage(
            prepared.Conversation.Id,
            prepared.NextSequence + 1,
            gatewayCall.Provider,
            gatewayCall.Model,
            gatewayCall.Result,
            gatewayCall.FallbackFromModelId,
            prepared.Conversation.Mode,
            prepared.ConversationContext,
            prepared.ContextPlan,
            gatewayCall.ToolInvocations,
            gatewayCall.AvailableToolCodes,
            gatewayCall.ToolLoopIterations,
            assistantMessageId);
        prepared.Conversation.TouchMessage(prepared.Content, prepared.PromptTokens, 0);
        prepared.Conversation.TouchMessage(
            string.IsNullOrWhiteSpace(assistantMessage.ErrorMessage) ? assistantMessage.Content : assistantMessage.ErrorMessage,
            0,
            assistantMessage.CompletionTokens);

        if (prepared.Model != null && prepared.Conversation.ModelId != prepared.Model.Id)
        {
            prepared.Conversation.UpdateModel(prepared.Model.Id);
        }

        await _messageRepository.InsertAsync(prepared.UserMessage, autoSave: false);
        await _messageRepository.InsertAsync(assistantMessage, autoSave: false);
        await _conversationRepository.UpdateAsync(prepared.Conversation, autoSave: true);

        var modelNames = await ResolveModelNamesAsync();
        var providerNames = await ResolveProviderNamesAsync();
        return new AiConversationSendResultDto
        {
            Conversation = MapConversation(prepared.Conversation, modelNames),
            Messages =
            [
                MapMessage(prepared.UserMessage, modelNames, providerNames),
                MapMessage(assistantMessage, modelNames, providerNames)
            ]
        };
    }

    public async IAsyncEnumerable<AiConversationStreamEventDto> StreamMessageAsync(
        Guid id,
        AiConversationSendInput input,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var prepared = await PrepareSendAsync(id, input);
        yield return AiConversationStreamEventDto.UserMessageEvent(MapMessage(
            prepared.UserMessage,
            await ResolveModelNamesAsync(),
            await ResolveProviderNamesAsync()));

        if (prepared.Model == null || prepared.Provider == null)
        {
            var failed = AiGatewayChatResult.Fail("未配置可用模型，请先在 AI 智能能力区配置并启用模型供应商。");
            var assistantMessage = CreateAssistantMessage(
                prepared.Conversation.Id,
                prepared.NextSequence + 1,
                prepared.Provider,
                prepared.Model,
                failed,
                null,
                prepared.Conversation.Mode,
                prepared.ConversationContext,
                prepared.ContextPlan,
                availableToolCodes: [],
                messageId: null);
            await PersistPreparedMessagesAsync(prepared, assistantMessage);
            yield return AiConversationStreamEventDto.ErrorEvent(failed.ErrorMessage);
            yield return AiConversationStreamEventDto.CompletedEvent(await BuildSendResultAsync(prepared.Conversation, prepared.UserMessage, assistantMessage));
            yield break;
        }

        var promptTemplateCode = ResolvePromptTemplateCode(prepared.Content);
        var scenario = ResolveScenario(prepared.Conversation, promptTemplateCode);
        var requestPreview = string.Join(Environment.NewLine, prepared.GatewayMessages.Select(x => $"{x.Role}: {x.Content}"));
        var quota = await _usageService.CheckQuotaAsync(new AiUsageQuotaCheckInput
        {
            ModelId = prepared.Model.Id,
            ProjectId = ResolveProjectId(prepared.Conversation),
            PromptTemplateCode = promptTemplateCode,
            RequestPreview = requestPreview,
            Scenario = scenario,
            Source = "ModelChat",
            UserId = _currentUser.Id
        });
        if (!quota.Allowed)
        {
            var blockedResult = AiGatewayChatResult.Fail($"AI 调用被限额或安全策略拦截：{string.Join("；", quota.Warnings)}");
            await _usageService.RecordUsageAsync(new AiUsageRecordInput
            {
                ConversationId = prepared.Conversation.Id,
                ErrorMessage = blockedResult.ErrorMessage,
                ModelId = prepared.Model.Id,
                ProjectId = ResolveProjectId(prepared.Conversation),
                PromptTemplateCode = promptTemplateCode,
                RequestPreview = prepared.Content,
                Scenario = scenario,
                Source = "ModelChat",
                Status = "Blocked",
                UserId = _currentUser.Id
            });
            var assistantMessage = CreateAssistantMessage(
                prepared.Conversation.Id,
                prepared.NextSequence + 1,
                prepared.Provider,
                prepared.Model,
                blockedResult,
                null,
                prepared.Conversation.Mode,
                prepared.ConversationContext,
                prepared.ContextPlan,
                availableToolCodes: [],
                messageId: null);
            await PersistPreparedMessagesAsync(prepared, assistantMessage);
            yield return AiConversationStreamEventDto.ErrorEvent(blockedResult.ErrorMessage);
            yield return AiConversationStreamEventDto.CompletedEvent(await BuildSendResultAsync(prepared.Conversation, prepared.UserMessage, assistantMessage));
            yield break;
        }

        if ((await ResolveGatewayToolsAsync(prepared.Conversation, prepared.Model, prepared.ConversationContext, prepared.Content)).Count > 0)
        {
            var assistantMessageId = _guidGenerator.Create();
            var gatewayCall = await SendToGatewayAsync(
                prepared.Conversation,
                prepared.Provider,
                prepared.Model,
                prepared.GatewayMessages,
                prepared.Content,
                prepared.ConversationContext,
                quotaAlreadyChecked: true,
                assistantMessageId);
            var assistantMessage = CreateAssistantMessage(
                prepared.Conversation.Id,
                prepared.NextSequence + 1,
                gatewayCall.Provider,
                gatewayCall.Model,
                gatewayCall.Result,
                gatewayCall.FallbackFromModelId,
                prepared.Conversation.Mode,
                prepared.ConversationContext,
                prepared.ContextPlan,
                gatewayCall.ToolInvocations,
                gatewayCall.AvailableToolCodes,
                gatewayCall.ToolLoopIterations,
                assistantMessageId);
            await PersistPreparedMessagesAsync(prepared, assistantMessage);
            yield return AiConversationStreamEventDto.CompletedEvent(await BuildSendResultAsync(prepared.Conversation, prepared.UserMessage, assistantMessage));
            yield break;
        }

        var streamAssistantMessageId = _guidGenerator.Create();
        var contentBuilder = new StringBuilder();
        var rawResponseBuilder = new StringBuilder();
        var promptTokens = 0;
        var completionTokens = 0;
        var streamModel = prepared.Model.ModelCode;
        var finishReason = string.Empty;
        var errorMessage = string.Empty;
        var startedAt = DateTimeOffset.UtcNow;

        await foreach (var chunk in _gatewayService.StreamChatAsync(
            prepared.Provider,
            prepared.Model,
            prepared.GatewayMessages,
            cancellationToken: cancellationToken))
        {
            if (!string.IsNullOrWhiteSpace(chunk.RawResponseJson))
            {
                rawResponseBuilder.AppendLine(chunk.RawResponseJson);
            }

            if (!chunk.Success)
            {
                errorMessage = chunk.ErrorMessage;
                yield return AiConversationStreamEventDto.ErrorEvent(errorMessage);
                break;
            }

            if (!string.IsNullOrEmpty(chunk.ContentDelta))
            {
                contentBuilder.Append(chunk.ContentDelta);
                yield return AiConversationStreamEventDto.DeltaEvent(chunk.ContentDelta);
            }

            if (chunk.PromptTokens > 0)
            {
                promptTokens = chunk.PromptTokens;
            }

            if (chunk.CompletionTokens > 0)
            {
                completionTokens = chunk.CompletionTokens;
            }

            if (!string.IsNullOrWhiteSpace(chunk.Model))
            {
                streamModel = chunk.Model;
            }

            if (!string.IsNullOrWhiteSpace(chunk.FinishReason))
            {
                finishReason = chunk.FinishReason;
            }
        }

        var streamedContent = contentBuilder.ToString();
        var streamResult = string.IsNullOrWhiteSpace(errorMessage)
            ? string.IsNullOrWhiteSpace(streamedContent)
                ? AiGatewayChatResult.Fail("模型流式响应为空。", rawResponseBuilder.ToString())
                : AiGatewayChatResult.Ok(
                    streamedContent,
                    promptTokens > 0 ? promptTokens : EstimateTokens(string.Join(Environment.NewLine, prepared.GatewayMessages.Select(x => x.Content))),
                    completionTokens > 0 ? completionTokens : EstimateTokens(streamedContent),
                    streamModel,
                    finishReason,
                    rawResponseBuilder.ToString())
            : AiGatewayChatResult.Fail(errorMessage, rawResponseBuilder.ToString());

        await RecordModelUsageAsync(
            prepared.Conversation,
            prepared.Provider,
            prepared.Model,
            streamResult,
            prepared.GatewayMessages,
            promptTemplateCode,
            scenario,
            DateTimeOffset.UtcNow - startedAt);

        var finalMessage = CreateAssistantMessage(
            prepared.Conversation.Id,
            prepared.NextSequence + 1,
            prepared.Provider,
            prepared.Model,
            streamResult,
            null,
            prepared.Conversation.Mode,
            prepared.ConversationContext,
            prepared.ContextPlan,
            availableToolCodes: [],
            messageId: streamAssistantMessageId);
        await PersistPreparedMessagesAsync(prepared, finalMessage);
        yield return AiConversationStreamEventDto.CompletedEvent(await BuildSendResultAsync(prepared.Conversation, prepared.UserMessage, finalMessage));
    }

    public async Task<AiConversationDto> ArchiveConversationAsync(Guid id)
    {
        var conversation = await GetOwnedConversationAsync(id);
        conversation.Archive();
        await _conversationRepository.UpdateAsync(conversation, autoSave: true);
        return MapConversation(conversation, await ResolveModelNamesAsync());
    }

    public async Task<AiConversationDto> RestoreConversationAsync(Guid id)
    {
        var conversation = await GetOwnedConversationAsync(id);
        conversation.Restore();
        await _conversationRepository.UpdateAsync(conversation, autoSave: true);
        return MapConversation(conversation, await ResolveModelNamesAsync());
    }

    public async Task DeleteConversationAsync(Guid id)
    {
        var conversation = await GetOwnedConversationAsync(id);
        var messages = await _messageRepository.GetListAsync(x => x.ConversationId == conversation.Id);
        if (messages.Count > 0)
        {
            await _messageRepository.DeleteManyAsync(messages, autoSave: false);
        }

        await _conversationRepository.DeleteAsync(conversation, autoSave: true);
    }

    public async Task<AiConversationExportDto> ExportConversationAsync(Guid id)
    {
        var detail = await GetConversationAsync(id);
        var builder = new StringBuilder();
        builder.AppendLine($"# {detail.Title}");
        builder.AppendLine();
        builder.AppendLine($"- 模式：{detail.Mode}");
        builder.AppendLine($"- 上下文：{detail.ContextType} {detail.ContextId}".TrimEnd());
        builder.AppendLine($"- 模型：{detail.ModelName}");
        builder.AppendLine();

        foreach (var message in detail.Messages)
        {
            builder.AppendLine($"## {ResolveRoleTitle(message.Role)}");
            builder.AppendLine();
            if (!string.IsNullOrWhiteSpace(message.ErrorMessage))
            {
                builder.AppendLine($"> {message.ErrorMessage}");
                builder.AppendLine();
            }

            builder.AppendLine(message.Content);
            builder.AppendLine();
        }

        return new AiConversationExportDto
        {
            Content = builder.ToString().TrimEnd(),
            FileName = $"{NormalizeFileName(detail.Title)}.md"
        };
    }

    public async Task<AiMessageDto> ConfirmPlanAsync(Guid id, AiPlanConfirmInput input)
    {
        await GetOwnedConversationAsync(id);
        var message = await GetConversationMessageAsync(id, input.MessageId);
        var metadata = ParseMetadata(message.MetadataJson);
        var structured = GetStructuredObject(metadata);
        EnsurePlanStructured(structured);
        structured["confirmed"] = input.Confirmed;
        message.UpdateStructuredContent(RenderPlanContent(structured), metadata.ToJsonString());
        await _messageRepository.UpdateAsync(message, autoSave: true);

        var modelNames = await ResolveModelNamesAsync();
        var providerNames = await ResolveProviderNamesAsync();
        return MapMessage(message, modelNames, providerNames);
    }

    public async Task<AiMessageDto> TogglePlanStepAsync(Guid id, AiPlanStepToggleInput input)
    {
        await GetOwnedConversationAsync(id);
        var stepId = Check.NotNullOrWhiteSpace(input.StepId, nameof(input.StepId), 64).Trim();
        var message = await GetConversationMessageAsync(id, input.MessageId);
        var metadata = ParseMetadata(message.MetadataJson);
        var structured = GetStructuredObject(metadata);
        EnsurePlanStructured(structured);
        var steps = structured["steps"]?.AsArray() ?? throw new UserFriendlyException("计划步骤为空，无法更新。");
        var matched = false;
        foreach (var node in steps)
        {
            if (node is not JsonObject step)
            {
                continue;
            }

            if (string.Equals(step["id"]?.GetValue<string>(), stepId, StringComparison.OrdinalIgnoreCase))
            {
                step["isDone"] = input.IsDone;
                matched = true;
                break;
            }
        }

        if (!matched)
        {
            throw new UserFriendlyException("未找到指定计划步骤。");
        }

        message.UpdateStructuredContent(RenderPlanContent(structured), metadata.ToJsonString());
        await _messageRepository.UpdateAsync(message, autoSave: true);

        var modelNames = await ResolveModelNamesAsync();
        var providerNames = await ResolveProviderNamesAsync();
        return MapMessage(message, modelNames, providerNames);
    }

    public async Task<AiConversationDetailDto> CreatePlanDraftAsync(Guid id, AiPlanDraftInput input)
    {
        var conversation = await GetOwnedConversationAsync(id);
        var sourceMessage = await GetConversationMessageAsync(id, input.MessageId);
        var metadata = ParseMetadata(sourceMessage.MetadataJson);
        var structured = GetStructuredObject(metadata);
        EnsurePlanStructured(structured);
        structured["draftCreated"] = true;
        sourceMessage.UpdateStructuredContent(RenderPlanContent(structured), metadata.ToJsonString());

        var existingMessages = await _messageRepository.GetListAsync(x => x.ConversationId == id);
        var nextSequence = existingMessages.Count == 0 ? 1 : existingMessages.Max(x => x.SequenceNo) + 1;
        var title = string.IsNullOrWhiteSpace(input.Title) ? $"{conversation.Title} AI 任务草稿" : input.Title.Trim();
        var draftContent = RenderPlanDraftContent(title, structured);
        var draftMetadata = new JsonObject
        {
            ["mode"] = AiConversationModeNames.Plan,
            ["sourcePlanMessageId"] = sourceMessage.Id.ToString(),
            ["type"] = "aiTaskDraft",
            ["structured"] = structured.DeepClone()
        };
        var draftMessage = new AiMessage(
            _guidGenerator.Create(),
            conversation.Id,
            "assistant",
            draftContent,
            nextSequence,
            "Draft",
            modelId: conversation.ModelId,
            metadataJson: draftMetadata.ToJsonString());
        conversation.TouchMessage("已生成 AI 任务草稿", 0, 0);

        await _messageRepository.UpdateAsync(sourceMessage, autoSave: false);
        await _messageRepository.InsertAsync(draftMessage, autoSave: false);
        await _conversationRepository.UpdateAsync(conversation, autoSave: true);
        return await GetConversationAsync(id);
    }

    private async Task<AiConversation> GetOwnedConversationAsync(Guid id)
    {
        var conversation = await _conversationRepository.GetAsync(id);
        if (_currentUser.Id.HasValue && conversation.CreatorId != _currentUser.Id)
        {
            throw new UserFriendlyException("只能访问自己的 AI 会话。");
        }

        return conversation;
    }

    private async Task<AiConversationPreparedSend> PrepareSendAsync(Guid id, AiConversationSendInput input)
    {
        var conversation = await GetOwnedConversationAsync(id);
        if (conversation.Status.Equals("Archived", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("归档会话不能继续发送消息，请先恢复会话。");
        }

        var content = Check.NotNullOrWhiteSpace(input.Content, nameof(input.Content), 16_000).Trim();
        var existingMessages = (await _messageRepository.GetListAsync(x => x.ConversationId == id))
            .OrderBy(x => x.SequenceNo)
            .ToList();
        var nextSequence = existingMessages.Count == 0 ? 1 : existingMessages.Max(x => x.SequenceNo) + 1;
        var model = await ResolveModelAsync(input.ModelId ?? conversation.ModelId);
        var provider = model == null ? null : await _providerRepository.FindAsync(model.ProviderId);
        var promptTokens = EstimateTokens(content);
        var userMessage = new AiMessage(
            _guidGenerator.Create(),
            conversation.Id,
            "user",
            content,
            nextSequence,
            promptTokens: promptTokens,
            providerId: provider?.Id,
            modelId: model?.Id,
            metadataJson: SerializeUserMessageMetadata(conversation.Mode));
        var toolContext = await _toolRegistryService.BuildConversationContextAsync(conversation);
        var memoryContext = await _memoryService.BuildConversationMemoryContextAsync(conversation);
        var agentContext = await BuildAgentExecutionContextAsync(input);
        var conversationContext = CombineConversationContexts(toolContext, memoryContext, agentContext);
        var contextPlan = BuildContextPlan(conversation, existingMessages, content, conversationContext, model);
        return new AiConversationPreparedSend(
            conversation,
            content,
            existingMessages,
            nextSequence,
            model,
            provider,
            promptTokens,
            userMessage,
            conversationContext,
            contextPlan,
            contextPlan.Messages);
    }

    private async Task PersistPreparedMessagesAsync(AiConversationPreparedSend prepared, AiMessage assistantMessage)
    {
        prepared.Conversation.TouchMessage(prepared.Content, prepared.PromptTokens, 0);
        prepared.Conversation.TouchMessage(
            string.IsNullOrWhiteSpace(assistantMessage.ErrorMessage) ? assistantMessage.Content : assistantMessage.ErrorMessage,
            0,
            assistantMessage.CompletionTokens);

        if (prepared.Model != null && prepared.Conversation.ModelId != prepared.Model.Id)
        {
            prepared.Conversation.UpdateModel(prepared.Model.Id);
        }

        await _messageRepository.InsertAsync(prepared.UserMessage, autoSave: false);
        await _messageRepository.InsertAsync(assistantMessage, autoSave: false);
        await _conversationRepository.UpdateAsync(prepared.Conversation, autoSave: true);
    }

    private async Task<AiConversationSendResultDto> BuildSendResultAsync(
        AiConversation conversation,
        AiMessage userMessage,
        AiMessage assistantMessage)
    {
        var modelNames = await ResolveModelNamesAsync();
        var providerNames = await ResolveProviderNamesAsync();
        return new AiConversationSendResultDto
        {
            Conversation = MapConversation(conversation, modelNames),
            Messages =
            [
                MapMessage(userMessage, modelNames, providerNames),
                MapMessage(assistantMessage, modelNames, providerNames)
            ]
        };
    }


    private async Task<AiScenarioAcceptanceItemDto> RunScenarioAcceptanceCaseAsync(
        AiScenarioAcceptanceCase acceptanceCase,
        AiPromptTemplate template,
        AiScenarioAcceptanceInput input)
    {
        try
        {
            var conversation = await CreateConversationFromTemplateAsync(template, new AiPromptTemplateConversationCreateInput
            {
                ContextId = input.ContextId,
                ContextSnapshotJson = string.IsNullOrWhiteSpace(input.ContextSnapshotJson) ? "{}" : input.ContextSnapshotJson.Trim(),
                ContextType = string.IsNullOrWhiteSpace(input.ContextType)
                    ? ResolveAcceptanceContextType(template)
                    : input.ContextType.Trim(),
                InputJson = string.IsNullOrWhiteSpace(input.InputJson)
                    ? BuildAcceptanceInputJson(template)
                    : input.InputJson.Trim(),
                ModelId = input.ModelId,
                Title = string.IsNullOrWhiteSpace(input.Title)
                    ? $"AI 验收 · {acceptanceCase.AcceptanceName} · {DateTime.Now:yyyyMMdd HHmm}"
                    : $"{input.Title.Trim()} · {acceptanceCase.AcceptanceName}"
            }, acceptanceCase.ModeOverride);
            var assistantMessage = conversation.Messages
                .Where(x => x.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.SequenceNo)
                .FirstOrDefault();
            var errorMessage = assistantMessage?.ErrorMessage ?? string.Empty;
            var contentPreview = NormalizePreview(assistantMessage?.Content ?? string.Empty, 600);
            var validationMessages = assistantMessage == null
                ? ["模型未返回助手消息。"]
                : ValidateAcceptanceOutput(acceptanceCase, assistantMessage);

            return new AiScenarioAcceptanceItemDto
            {
                AcceptanceCode = acceptanceCase.AcceptanceCode,
                AcceptanceName = acceptanceCase.AcceptanceName,
                AssistantMessagePreview = contentPreview,
                ConversationId = conversation.Id,
                ErrorMessage = errorMessage,
                Mode = acceptanceCase.ModeOverride,
                Scenario = template.Scenario,
                Success = assistantMessage != null &&
                          assistantMessage.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase) &&
                          string.IsNullOrWhiteSpace(errorMessage) &&
                          validationMessages.Count == 0,
                TemplateCode = template.TemplateCode,
                TemplateId = template.Id,
                TemplateName = template.Name,
                ValidationMessages = validationMessages
            };
        }
        catch (Exception ex)
        {
            return new AiScenarioAcceptanceItemDto
            {
                AcceptanceCode = acceptanceCase.AcceptanceCode,
                AcceptanceName = acceptanceCase.AcceptanceName,
                ErrorMessage = ex.Message,
                Mode = acceptanceCase.ModeOverride,
                Scenario = template.Scenario,
                Success = false,
                TemplateCode = template.TemplateCode,
                TemplateId = template.Id,
                TemplateName = template.Name,
                ValidationMessages = [ex.Message]
            };
        }
    }

    private List<AiConversation> ApplyCurrentUserFilter(List<AiConversation> conversations)
    {
        return _currentUser.Id.HasValue
            ? conversations.Where(x => x.CreatorId == _currentUser.Id).ToList()
            : conversations;
    }

    private async Task<AiModel?> ResolveModelAsync(Guid? modelId)
    {
        if (modelId.HasValue)
        {
            var requested = await _modelRepository.GetAsync(modelId.Value);
            if (!requested.IsEnabled)
            {
                throw new UserFriendlyException("选择的模型已停用。");
            }

            return requested;
        }

        var models = (await _modelRepository.GetListAsync(x => x.IsEnabled))
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Sort)
            .ThenBy(x => x.DisplayName)
            .ToList();
        return models.FirstOrDefault();
    }

    private async Task<AiAssistant?> ResolvePromptTemplateAssistantAsync(AiPromptTemplate template, bool onlyEnabled)
    {
        if (string.IsNullOrWhiteSpace(template.AssistantCode))
        {
            return null;
        }

        var assistants = await _assistantRepository.GetListAsync(x => x.AssistantCode == template.AssistantCode);
        if (onlyEnabled)
        {
            assistants = assistants.Where(x => x.IsEnabled).ToList();
        }

        return assistants
            .OrderByDescending(x => x.IsEnabled)
            .ThenBy(x => x.Sort)
            .FirstOrDefault();
    }

    private async Task<AiConversationGatewayCallResult> SendToGatewayAsync(
        AiConversation conversation,
        AiProvider? provider,
        AiModel? model,
        IReadOnlyCollection<AiGatewayMessage> messages,
        string content,
        AiConversationToolContextDto toolContext,
        bool quotaAlreadyChecked,
        Guid? assistantMessageId)
    {
        if (model == null || provider == null)
        {
            return new AiConversationGatewayCallResult(
                AiGatewayChatResult.Fail("未配置可用模型，请先在 AI 智能能力区配置并启用模型供应商。"),
                provider,
                model,
                null);
        }

        var promptTemplateCode = ResolvePromptTemplateCode(content);
        var scenario = ResolveScenario(conversation, promptTemplateCode);
        if (!quotaAlreadyChecked)
        {
            var quota = await _usageService.CheckQuotaAsync(new AiUsageQuotaCheckInput
            {
                ModelId = model.Id,
                ProjectId = ResolveProjectId(conversation),
                PromptTemplateCode = promptTemplateCode,
                RequestPreview = string.Join(Environment.NewLine, messages.Select(x => $"{x.Role}: {x.Content}")),
                Scenario = scenario,
                Source = "ModelChat",
                UserId = _currentUser.Id
            });
            if (!quota.Allowed)
            {
                var blockedResult = AiGatewayChatResult.Fail($"AI 调用被限额或安全策略拦截：{string.Join("；", quota.Warnings)}");
                await _usageService.RecordUsageAsync(new AiUsageRecordInput
                {
                    ConversationId = conversation.Id,
                    ErrorMessage = blockedResult.ErrorMessage,
                    ModelId = model.Id,
                    ProjectId = ResolveProjectId(conversation),
                    PromptTemplateCode = promptTemplateCode,
                    RequestPreview = content,
                    Scenario = scenario,
                    Source = "ModelChat",
                    Status = "Blocked",
                    UserId = _currentUser.Id
                });
                return new AiConversationGatewayCallResult(blockedResult, provider, model, null);
            }
        }

        var gatewayTools = await ResolveGatewayToolsAsync(conversation, model, toolContext, content);
        var availableToolCodes = gatewayTools
            .Select(x => ResolveGatewayToolCode(x.Name))
            .ToList();
        var primaryCall = await SendModelWithToolLoopAsync(
            conversation,
            provider,
            model,
            messages,
            gatewayTools,
            promptTemplateCode,
            scenario,
            assistantMessageId);
        var result = primaryCall.Result;
        var toolInvocations = primaryCall.ToolInvocations;

        if (result.Success || !model.FallbackModelId.HasValue)
        {
            return new AiConversationGatewayCallResult(
                result,
                provider,
                model,
                null,
                toolInvocations,
                availableToolCodes,
                primaryCall.ToolIterations);
        }

        var fallbackModel = await _modelRepository.FindAsync(model.FallbackModelId.Value);
        if (fallbackModel == null || !fallbackModel.IsEnabled)
        {
            return new AiConversationGatewayCallResult(
                AiGatewayChatResult.Fail($"主模型调用失败，且降级模型不可用。原始错误：{result.ErrorMessage}"),
                provider,
                model,
                null);
        }

        var fallbackProvider = await _providerRepository.FindAsync(fallbackModel.ProviderId);
        if (fallbackProvider == null || !fallbackProvider.IsEnabled)
        {
            return new AiConversationGatewayCallResult(
                AiGatewayChatResult.Fail($"主模型调用失败，且降级模型供应商不可用。原始错误：{result.ErrorMessage}"),
                provider,
                model,
                null);
        }

        var fallbackGatewayTools = await ResolveGatewayToolsAsync(conversation, fallbackModel, toolContext, content);
        var fallbackAvailableToolCodes = fallbackGatewayTools
            .Select(x => ResolveGatewayToolCode(x.Name))
            .ToList();
        var fallbackCall = await SendModelWithToolLoopAsync(
            conversation,
            fallbackProvider,
            fallbackModel,
            messages,
            fallbackGatewayTools,
            promptTemplateCode,
            scenario,
            assistantMessageId);
        var fallbackResult = fallbackCall.Result;
        var fallbackToolInvocations = fallbackCall.ToolInvocations;

        if (!fallbackResult.Success)
        {
            return new AiConversationGatewayCallResult(
                AiGatewayChatResult.Fail($"主模型和降级模型均调用失败。主模型错误：{result.ErrorMessage}；降级模型错误：{fallbackResult.ErrorMessage}"),
                fallbackProvider,
                fallbackModel,
                model.Id,
                toolInvocations.Concat(fallbackToolInvocations).ToList(),
                availableToolCodes.Concat(fallbackAvailableToolCodes).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                primaryCall.ToolIterations + fallbackCall.ToolIterations);
        }

        return new AiConversationGatewayCallResult(
            fallbackResult,
            fallbackProvider,
            fallbackModel,
            model.Id,
            toolInvocations.Concat(fallbackToolInvocations).ToList(),
            availableToolCodes.Concat(fallbackAvailableToolCodes).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
            primaryCall.ToolIterations + fallbackCall.ToolIterations);
    }

    private async Task<AiModelToolLoopResult> SendModelWithToolLoopAsync(
        AiConversation conversation,
        AiProvider provider,
        AiModel model,
        IReadOnlyCollection<AiGatewayMessage> messages,
        IReadOnlyCollection<AiGatewayToolDefinition> gatewayTools,
        string promptTemplateCode,
        string scenario,
        Guid? assistantMessageId)
    {
        var allToolInvocations = new List<AiToolInvokeResultDto>();
        var currentMessages = messages.ToList();
        var toolIterations = 0;

        for (var iteration = 0; iteration <= MaxAgentToolIterations; iteration++)
        {
            var startedAt = DateTimeOffset.UtcNow;
            var result = await _gatewayService.SendChatAsync(provider, model, currentMessages, tools: gatewayTools);
            await RecordModelUsageAsync(
                conversation,
                provider,
                model,
                result,
                currentMessages,
                promptTemplateCode,
                scenario,
                DateTimeOffset.UtcNow - startedAt);

            if (!result.Success || result.ToolCalls.Count == 0)
            {
                return new AiModelToolLoopResult(result, allToolInvocations, toolIterations);
            }

            if (iteration >= MaxAgentToolIterations)
            {
                return new AiModelToolLoopResult(
                    AiGatewayChatResult.Fail($"Agent 工具循环超过 {MaxAgentToolIterations} 轮，已停止继续执行。"),
                    allToolInvocations,
                    toolIterations);
            }

            var currentToolInvocations = await ExecuteGatewayToolCallsAsync(conversation, result.ToolCalls, assistantMessageId);
            toolIterations++;
            allToolInvocations.AddRange(currentToolInvocations);
            currentMessages = currentMessages
                .Concat([
                    new AiGatewayMessage("assistant", BuildToolIntentSummary(result.ToolCalls)),
                    new AiGatewayMessage("user", BuildToolResultPrompt(currentToolInvocations))
                ])
                .ToList();
        }

        return new AiModelToolLoopResult(
            AiGatewayChatResult.Fail($"Agent 工具循环超过 {MaxAgentToolIterations} 轮，已停止继续执行。"),
            allToolInvocations,
            toolIterations);
    }

    private async Task<List<AiGatewayToolDefinition>> ResolveGatewayToolsAsync(
        AiConversation conversation,
        AiModel model,
        AiConversationToolContextDto toolContext,
        string content)
    {
        if (!HasModelCapability(model, "toolCalling") || !ShouldEnableToolLoop(conversation, toolContext, content))
        {
            return [];
        }

        var builtinTools = await _toolRegistryService.GetGatewayToolDefinitionsAsync();
        var mcpTools = await _mcpService.GetGatewayToolDefinitionsAsync();
        return builtinTools
            .Concat(mcpTools)
            .Take(20)
            .ToList();
    }

    private static bool ShouldEnableToolLoop(
        AiConversation conversation,
        AiConversationToolContextDto toolContext,
        string content)
    {
        if (Guid.TryParse(conversation.ContextId, out _))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(toolContext.ContextPrompt) &&
            (toolContext.ContextPrompt.Contains("Agent 定义：", StringComparison.OrdinalIgnoreCase) ||
             toolContext.ContextPrompt.Contains("固定技能：", StringComparison.OrdinalIgnoreCase) ||
             toolContext.ContextPrompt.Contains("快捷命令：", StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return content.Contains("联网搜索", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("Web Search", StringComparison.OrdinalIgnoreCase) ||
               content.Contains("/web_search", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<List<AiToolInvokeResultDto>> ExecuteGatewayToolCallsAsync(
        AiConversation conversation,
        IReadOnlyCollection<AiGatewayToolCall> toolCalls,
        Guid? assistantMessageId)
    {
        var results = new List<AiToolInvokeResultDto>();
        var hasContextProject = Guid.TryParse(conversation.ContextId, out var projectId);

        foreach (var toolCall in toolCalls.Take(5))
        {
            var result = AiMcpService.IsGatewayToolName(toolCall.Name)
                ? await _mcpService.InvokeGatewayToolAsync(toolCall.Name, NormalizeToolArguments(toolCall.ArgumentsJson))
                : await _toolRegistryService.InvokeToolAsync(
                    AiToolRegistryService.FromGatewayToolName(toolCall.Name),
                    new AiToolInvokeInput
                    {
                        ConversationId = conversation.Id,
                        InputJson = hasContextProject
                            ? MergeToolArguments(toolCall.ArgumentsJson, projectId)
                            : NormalizeToolArguments(toolCall.ArgumentsJson),
                        MessageId = assistantMessageId
                    });
            results.Add(result);
        }

        return results;
    }

    private static string BuildToolIntentSummary(IReadOnlyCollection<AiGatewayToolCall> toolCalls)
    {
        return $"""
                模型请求调用以下受控工具，系统已在权限、超时和审计约束下执行：
                {string.Join(Environment.NewLine, toolCalls.Select(x => $"- {ResolveGatewayToolCode(x.Name)}：{x.ArgumentsJson}"))}
                """;
    }

    private static string ResolveGatewayToolCode(string gatewayToolName)
    {
        return AiMcpService.IsGatewayToolName(gatewayToolName)
            ? gatewayToolName
            : AiToolRegistryService.FromGatewayToolName(gatewayToolName);
    }

    private static string BuildToolResultPrompt(IReadOnlyCollection<AiToolInvokeResultDto> toolInvocations)
    {
        var payload = toolInvocations.Select(x => new
        {
            x.ToolCode,
            x.Status,
            x.ErrorMessage,
            OutputJson = x.OutputJson.Length <= 5000 ? x.OutputJson : x.OutputJson[..5000],
            SourceReferences = x.SourceReferences
        });
        return $"""
                以下是受控工具调用结果。请基于这些结果生成最终答复；如果工具失败，明确说明缺失的数据和下一步人工处理建议。
                {JsonSerializer.Serialize(payload, JsonOptions)}
                """;
    }

    private static string MergeToolArguments(string argumentsJson, Guid projectId)
    {
        JsonObject arguments;
        try
        {
            arguments = JsonNode.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson) as JsonObject ?? [];
        }
        catch
        {
            arguments = [];
        }

        arguments["projectId"] = projectId.ToString();
        return arguments.ToJsonString(JsonOptions);
    }

    private static string NormalizeToolArguments(string argumentsJson)
    {
        try
        {
            return (JsonNode.Parse(string.IsNullOrWhiteSpace(argumentsJson) ? "{}" : argumentsJson) as JsonObject ?? [])
                .ToJsonString(JsonOptions);
        }
        catch
        {
            return "{}";
        }
    }

    private static List<string> BuildAssistantCitations(
        AiConversationToolContextDto toolContext,
        IReadOnlyCollection<AiToolInvokeResultDto>? toolInvocations)
    {
        return toolContext.Citations
            .Concat((toolInvocations ?? [])
                .SelectMany(x => x.SourceReferences)
                .Select(x => $"{x.SourceType}:{x.SourceId} {x.Title}".Trim()))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .Take(30)
            .ToList();
    }

    private async Task RecordModelUsageAsync(
        AiConversation conversation,
        AiProvider provider,
        AiModel model,
        AiGatewayChatResult result,
        IReadOnlyCollection<AiGatewayMessage> messages,
        string promptTemplateCode,
        string scenario,
        TimeSpan elapsed)
    {
        await _usageService.RecordUsageAsync(new AiUsageRecordInput
        {
            AuditSummary = result.Success ? "AI 会话模型调用成功" : "AI 会话模型调用失败",
            CompletionTokens = result.CompletionTokens > 0 ? result.CompletionTokens : result.Success ? EstimateTokens(result.Content) : 0,
            ConversationId = conversation.Id,
            ElapsedMilliseconds = (long)elapsed.TotalMilliseconds,
            ErrorMessage = result.ErrorMessage,
            ModelId = model.Id,
            ProjectId = ResolveProjectId(conversation),
            PromptTemplateCode = promptTemplateCode,
            PromptTokens = result.PromptTokens > 0 ? result.PromptTokens : EstimateTokens(string.Join(Environment.NewLine, messages.Select(x => x.Content))),
            ProviderId = provider.Id,
            RequestPreview = string.Join(Environment.NewLine, messages.Select(x => $"{x.Role}: {x.Content}")),
            ResponsePreview = result.Success ? result.Content : result.RawResponseJson,
            Scenario = scenario,
            Source = "ModelChat",
            Status = result.Success ? "Success" : "Failed",
            UserId = _currentUser.Id
        });
    }

    private AiMessage CreateAssistantMessage(
        Guid conversationId,
        int sequenceNo,
        AiProvider? provider,
        AiModel? model,
        AiGatewayChatResult gatewayResult,
        Guid? fallbackFromModelId,
        string mode,
        AiConversationToolContextDto toolContext,
        AiConversationContextPlan contextPlan,
        IReadOnlyCollection<AiToolInvokeResultDto>? toolInvocations = null,
        IReadOnlyCollection<string>? availableToolCodes = null,
        int toolLoopIterations = 0,
        Guid? messageId = null)
    {
        var structuredOutput = ResolveStructuredOutput(gatewayResult, mode);
        var citations = BuildAssistantCitations(toolContext, toolInvocations);
        if (!gatewayResult.Success)
        {
            return new AiMessage(
                messageId ?? _guidGenerator.Create(),
                conversationId,
                "assistant",
                string.Empty,
                sequenceNo,
                "Failed",
                providerId: provider?.Id,
                modelId: model?.Id,
                citationsJson: SerializeList(citations),
                metadataJson: SerializeGatewayMetadata(
                    gatewayResult,
                    fallbackFromModelId,
                    structuredOutput,
                    toolContext,
                    contextPlan,
                    toolInvocations,
                    availableToolCodes,
                    toolLoopIterations),
                errorMessage: gatewayResult.ErrorMessage);
        }

        return new AiMessage(
            messageId ?? _guidGenerator.Create(),
            conversationId,
            "assistant",
            structuredOutput.DisplayContent,
            sequenceNo,
            "Completed",
            provider?.Id,
            model?.Id,
            completionTokens: gatewayResult.CompletionTokens > 0 ? gatewayResult.CompletionTokens : EstimateTokens(structuredOutput.DisplayContent),
            citationsJson: SerializeList(citations),
            metadataJson: SerializeGatewayMetadata(
                gatewayResult,
                fallbackFromModelId,
                structuredOutput,
                toolContext,
                contextPlan,
                toolInvocations,
                availableToolCodes,
                toolLoopIterations));
    }

    private static string SerializeGatewayMetadata(
        AiGatewayChatResult gatewayResult,
        Guid? fallbackFromModelId,
        AiStructuredOutputResult structuredOutput,
        AiConversationToolContextDto toolContext,
        AiConversationContextPlan contextPlan,
        IReadOnlyCollection<AiToolInvokeResultDto>? toolInvocations = null,
        IReadOnlyCollection<string>? availableToolCodes = null,
        int toolLoopIterations = 0)
    {
        var invocationNodes = new JsonArray();
        foreach (var invocation in toolInvocations ?? [])
        {
            invocationNodes.Add(new JsonObject
            {
                ["elapsedMilliseconds"] = invocation.ElapsedMilliseconds,
                ["errorMessage"] = invocation.ErrorMessage,
                ["invocationId"] = invocation.InvocationId.ToString(),
                ["status"] = invocation.Status,
                ["toolCode"] = invocation.ToolCode
            });
        }

        var availableToolNodes = new JsonArray();
        foreach (var toolCode in availableToolCodes ?? [])
        {
            availableToolNodes.Add(toolCode);
        }

        var estimatedPromptTokens = gatewayResult.PromptTokens > 0
            ? gatewayResult.PromptTokens
            : contextPlan.EstimatedPromptTokens;
        var runStepNodes = BuildRunStepNodes(
            gatewayResult,
            toolContext,
            contextPlan,
            toolInvocations,
            availableToolCodes,
            toolLoopIterations);

        var metadata = new JsonObject
        {
            ["fallbackFromModelId"] = fallbackFromModelId?.ToString(),
            ["finishReason"] = gatewayResult.FinishReason,
            ["gateway"] = "AiModelGateway",
            ["model"] = gatewayResult.Model,
            ["promptTokens"] = gatewayResult.PromptTokens,
            ["rawResponse"] = gatewayResult.RawResponseJson.Length <= 4000
                ? gatewayResult.RawResponseJson
                : gatewayResult.RawResponseJson[..4000],
            ["structured"] = structuredOutput.StructuredNode?.DeepClone(),
            ["toolContext"] = new JsonObject
            {
                ["citationCount"] = toolContext.Citations.Count,
                ["hasContextPrompt"] = !string.IsNullOrWhiteSpace(toolContext.ContextPrompt)
            },
            ["contextEngineering"] = new JsonObject
            {
                ["strategy"] = contextPlan.Strategy,
                ["automaticCompression"] = contextPlan.AutomaticCompression,
                ["selectionPolicy"] = contextPlan.SelectionPolicy,
                ["estimatedPromptTokens"] = estimatedPromptTokens,
                ["contextBudgetTokens"] = contextPlan.ContextBudgetTokens,
                ["recentMessageCount"] = contextPlan.RecentMessageCount,
                ["compressedMessageCount"] = contextPlan.CompressedMessageCount,
                ["citationCount"] = contextPlan.InjectedCitationCount,
                ["hasInjectedContext"] = contextPlan.HasInjectedContext
            },
            ["availableTools"] = availableToolNodes,
            ["toolLoop"] = new JsonObject
            {
                ["enabled"] = availableToolNodes.Count > 0,
                ["iterations"] = toolLoopIterations,
                ["maxIterations"] = MaxAgentToolIterations,
                ["invocationCount"] = toolInvocations?.Count ?? 0
            },
            ["runSteps"] = runStepNodes,
            ["toolInvocations"] = invocationNodes
        };
        return metadata.ToJsonString();
    }

    private static JsonArray BuildRunStepNodes(
        AiGatewayChatResult gatewayResult,
        AiConversationToolContextDto toolContext,
        AiConversationContextPlan contextPlan,
        IReadOnlyCollection<AiToolInvokeResultDto>? toolInvocations,
        IReadOnlyCollection<string>? availableToolCodes,
        int toolLoopIterations)
    {
        var toolCount = availableToolCodes?.Count ?? 0;
        var invocationCount = toolInvocations?.Count ?? 0;
        var hasFailedTool = toolInvocations?.Any(x => !x.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)) == true;
        return
        [
            BuildRunStepNode("context", "构建上下文", "Completed",
                contextPlan.HasInjectedContext
                    ? $"已注入 {toolContext.Citations.Count} 条业务/记忆/Agent 引用，保留最近 {contextPlan.RecentMessageCount} 条，压缩 {contextPlan.CompressedMessageCount} 条。"
                    : $"未命中额外业务上下文，保留最近 {contextPlan.RecentMessageCount} 条，压缩 {contextPlan.CompressedMessageCount} 条。"),
            BuildRunStepNode("tool-schema", "准备工具", toolCount > 0 ? "Completed" : "Skipped",
                toolCount > 0 ? $"本轮开放 {toolCount} 个受控工具给模型自主选择。" : "本轮未开放工具，普通对话保持流式输出。"),
            BuildRunStepNode("model", "模型推理", gatewayResult.Success ? "Completed" : "Failed",
                gatewayResult.Success
                    ? $"模型完成输出，结束原因：{(string.IsNullOrWhiteSpace(gatewayResult.FinishReason) ? "-" : gatewayResult.FinishReason)}。"
                    : gatewayResult.ErrorMessage),
            BuildRunStepNode("tool-loop", "工具循环", hasFailedTool ? "Failed" : invocationCount > 0 ? "Completed" : "Skipped",
                invocationCount > 0 ? $"执行 {toolLoopIterations} 轮，共 {invocationCount} 次工具调用。" : "模型未请求工具调用。"),
            BuildRunStepNode("final", "生成答复", gatewayResult.Success ? "Completed" : "Failed",
                gatewayResult.Success ? "已保存 AI 回复、引用和运行审计。" : "已保存失败记录，等待人工处理。")
        ];
    }

    private static JsonObject BuildRunStepNode(string code, string title, string status, string summary)
    {
        return new JsonObject
        {
            ["code"] = code,
            ["title"] = title,
            ["status"] = status,
            ["summary"] = summary
        };
    }

    private static AiConversationToolContextDto CombineConversationContexts(
        AiConversationToolContextDto toolContext,
        AiConversationMemoryContextDto memoryContext,
        AiConversationToolContextDto agentContext)
    {
        return new AiConversationToolContextDto
        {
            Citations = toolContext.Citations
                .Concat(memoryContext.Citations)
                .Concat(agentContext.Citations)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Take(20)
                .ToList(),
            ContextPrompt = string.Join(
                Environment.NewLine + Environment.NewLine,
                new[] { toolContext.ContextPrompt, memoryContext.ContextPrompt, agentContext.ContextPrompt }.Where(x => !string.IsNullOrWhiteSpace(x)))
        };
    }

    private async Task<AiConversationToolContextDto> BuildAgentExecutionContextAsync(AiConversationSendInput input)
    {
        var sections = new List<string>();
        var citations = new List<string>();
        AiCommand? command = null;

        if (!string.IsNullOrWhiteSpace(input.CommandCode))
        {
            var commandCode = input.CommandCode.Trim();
            command = (await _commandRepository.GetListAsync(x => x.IsEnabled))
                .FirstOrDefault(x =>
                    x.CommandCode.Equals(commandCode, StringComparison.OrdinalIgnoreCase) ||
                    x.SlashName.Equals(commandCode, StringComparison.OrdinalIgnoreCase));
            if (command != null)
            {
                sections.Add($"""
                              快捷命令：
                              - 命令：{command.SlashName} {command.Name}
                              - 描述：{command.Description}
                              - 模式：{command.Mode}
                              - 示例：{command.ExampleInput}
                              """);
                citations.Add($"AiCommand:{command.CommandCode} {command.Name}");
            }
        }

        var agentCode = FirstNotEmpty(input.AgentCode, command?.AgentCode);
        AiAgentDefinition? agent = null;
        if (!string.IsNullOrWhiteSpace(agentCode))
        {
            agent = (await _agentRepository.GetListAsync(x => x.IsEnabled))
                .FirstOrDefault(x => x.AgentCode.Equals(agentCode, StringComparison.OrdinalIgnoreCase));
            if (agent != null)
            {
                sections.Add($"""
                              Agent 定义：
                              - Agent：{agent.Name}（{agent.AgentCode}）
                              - 角色：{agent.RoleType}
                              - 说明：{agent.Description}
                              - 系统提示词：
                              {agent.SystemPrompt}
                              """);
                citations.Add($"AiAgent:{agent.AgentCode} {agent.Name}");
            }
        }

        var personaCode = agent?.PersonaCode ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(personaCode))
        {
            var persona = (await _personaRepository.GetListAsync(x => x.IsEnabled))
                .FirstOrDefault(x => x.PersonaCode.Equals(personaCode, StringComparison.OrdinalIgnoreCase));
            if (persona != null)
            {
                sections.Add($"""
                              角色画像：
                              - 画像：{persona.Name}（{persona.PersonaCode}）
                              - 角色：{persona.RoleType}
                              - 人格设定：{persona.PersonalityPrompt}
                              - 边界：{persona.BoundaryPrompt}
                              - 表达风格：{persona.StylePrompt}
                              """);
                citations.Add($"AiPersona:{persona.PersonaCode} {persona.Name}");
            }
        }

        var skillCodes = DeserializeList(agent?.SkillCodesJson ?? "[]")
            .Concat([FirstNotEmpty(input.SkillCode, command?.SkillCode)])
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (skillCodes.Count > 0)
        {
            var skills = (await _skillRepository.GetListAsync(x => x.IsEnabled))
                .Where(x => skillCodes.Contains(x.SkillCode, StringComparer.OrdinalIgnoreCase))
                .OrderBy(x => x.Sort)
                .ToList();
            foreach (var skill in skills)
            {
                sections.Add($"""
                              固定技能：
                              - 技能：{skill.Name}（{skill.SkillCode}）
                              - 分类：{skill.Category}
                              - 默认模式：{skill.DefaultMode}
                              - 技能要求：
                              {skill.Instruction}
                              - 推荐工具：{string.Join("、", DeserializeList(skill.ToolCodesJson))}
                              """);
                citations.Add($"AiSkill:{skill.SkillCode} {skill.Name}");
            }
        }

        if (command != null && !string.IsNullOrWhiteSpace(command.PromptTemplateCode))
        {
            var template = (await _promptTemplateRepository.GetListAsync(x => x.IsEnabled))
                .FirstOrDefault(x => x.TemplateCode.Equals(command.PromptTemplateCode, StringComparison.OrdinalIgnoreCase));
            if (template != null)
            {
                sections.Add($"""
                              快捷命令绑定模板：
                              - 模板：{template.Name}（{template.TemplateCode}）
                              - 场景：{template.Scenario}
                              - 模板要求：
                              {template.PromptContent}
                              """);
                citations.Add($"AiPromptTemplate:{template.TemplateCode} {template.Name}");
            }
        }

        return new AiConversationToolContextDto
        {
            Citations = citations.Distinct(StringComparer.OrdinalIgnoreCase).Take(12).ToList(),
            ContextPrompt = sections.Count == 0
                ? string.Empty
                : "以下为本次用户选择的 AI Agent/Skill/Command 运行约束。必须优先遵守，且不得绕过系统权限、工具审计和写回审批。" +
                  Environment.NewLine +
                  Environment.NewLine +
                  string.Join(Environment.NewLine + Environment.NewLine, sections)
        };
    }

    private static string FirstNotEmpty(params string?[] values)
    {
        return values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim() ?? string.Empty;
    }

    private static string SerializeUserMessageMetadata(string mode)
    {
        return new JsonObject
        {
            ["mode"] = AiConversationModeNames.Normalize(mode),
            ["type"] = mode.Equals(AiConversationModeNames.Clarify, StringComparison.OrdinalIgnoreCase)
                ? "clarificationAnswer"
                : "userInput"
        }.ToJsonString();
    }

    private static string RenderTemplatePrompt(AiPromptTemplate template, AiAssistant? assistant, string inputJson, string? modeOverride = null)
    {
        var normalizedInputJson = string.IsNullOrWhiteSpace(inputJson) ? "{}" : inputJson.Trim();
        var mode = string.IsNullOrWhiteSpace(modeOverride) ? template.Mode : AiConversationModeNames.Normalize(modeOverride);
        return $"""
               请按以下 AI 场景模板完成输出。

               助手角色：
               {assistant?.SystemPrompt ?? "使用通用企业项目管理助手角色。"}

               模板名称：{template.Name}
               模板编码：{template.TemplateCode}
               模板分类：{template.Category}
               场景：{template.Scenario}
               模式：{mode}

               模板要求：
               {template.PromptContent}

               输入参数：
               {normalizedInputJson}

               输出约束：
               1. 必须基于系统上下文、工具引用和用户输入输出。
               2. 不编造系统记录中不存在的项目、金额、日期、负责人和状态。
               3. 如果信息不足，先列出需要补充的信息。
               4. 需要写回业务对象时，只输出草稿和待确认项，不直接声明已写回。
               """;
    }

    private static Guid? ResolveProjectId(AiConversation conversation)
    {
        return Guid.TryParse(conversation.ContextId, out var projectId) ? projectId : null;
    }

    private static string ResolvePromptTemplateCode(string content)
    {
        const string marker = "模板编码：";
        var index = content.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return string.Empty;
        }

        var start = index + marker.Length;
        var end = content.IndexOf('\n', start);
        var value = end > start ? content[start..end] : content[start..];
        return value.Trim().Length <= 128 ? value.Trim() : value.Trim()[..128];
    }

    private static string ResolveScenario(AiConversation conversation, string promptTemplateCode)
    {
        if (!string.IsNullOrWhiteSpace(promptTemplateCode))
        {
            return promptTemplateCode;
        }

        if (!string.IsNullOrWhiteSpace(conversation.ContextType))
        {
            return conversation.ContextType;
        }

        return AiConversationModeNames.Normalize(conversation.Mode);
    }

    private static bool HasModelCapability(AiModel model, string capability)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(model.CapabilitiesJson, JsonOptions)?
                .Contains(capability, StringComparer.OrdinalIgnoreCase) == true;
        }
        catch
        {
            return false;
        }
    }

    private static AiConversationContextPlan BuildContextPlan(
        AiConversation conversation,
        IReadOnlyCollection<AiMessage> existingMessages,
        string content,
        AiConversationToolContextDto toolContext,
        AiModel? model)
    {
        var messages = new List<AiGatewayMessage>();
        var systemPrompt = BuildModePrompt(conversation);
        var orderedMessages = existingMessages
            .OrderBy(x => x.SequenceNo)
            .Where(x => x.Role is "assistant" or "system" or "user")
            .Where(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.ErrorMessage))
            .ToList();
        var recentMessages = orderedMessages
            .TakeLast(MaxRecentContextMessages)
            .ToList();
        var compressedMessages = orderedMessages
            .Take(Math.Max(0, orderedMessages.Count - recentMessages.Count))
            .TakeLast(MaxCompressedContextMessages)
            .ToList();
        var historyCompressionPrompt = BuildHistoryCompressionPrompt(compressedMessages);
        var combinedSystemPrompt = string.Join(
            Environment.NewLine + Environment.NewLine,
            new[] { systemPrompt, toolContext.ContextPrompt, historyCompressionPrompt }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (!string.IsNullOrWhiteSpace(combinedSystemPrompt))
        {
            messages.Add(new AiGatewayMessage("system", combinedSystemPrompt));
        }

        messages.AddRange(recentMessages
            .Select(x => new AiGatewayMessage(x.Role, string.IsNullOrWhiteSpace(x.Content) ? x.ErrorMessage : x.Content))
            .Where(x => !string.IsNullOrWhiteSpace(x.Content)));
        messages.Add(new AiGatewayMessage("user", content));
        var estimatedPromptTokens = EstimateTokens(string.Join(Environment.NewLine, messages.Select(x => x.Content)));
        var contextBudgetTokens = model?.ContextWindow > 0 ? model.ContextWindow : DefaultContextBudgetTokens;
        return new AiConversationContextPlan(
            messages,
            compressedMessages.Count > 0,
            recentMessages.Count,
            compressedMessages.Count,
            estimatedPromptTokens,
            contextBudgetTokens,
            toolContext.Citations.Count,
            !string.IsNullOrWhiteSpace(toolContext.ContextPrompt),
            "系统提示 + 受控业务/记忆/Agent 上下文 + 早期历史摘要 + 最近消息窗口",
            $"固定保留系统约束和受控业务上下文；最近 {MaxRecentContextMessages} 条消息原样进入模型；更早的最多 {MaxCompressedContextMessages} 条压缩为摘要；工具由模型在可用集合内自主选择。");
    }

    private static string BuildHistoryCompressionPrompt(IReadOnlyCollection<AiMessage> compressedMessages)
    {
        if (compressedMessages.Count == 0)
        {
            return string.Empty;
        }

        var summaries = compressedMessages
            .Select(x =>
            {
                var content = string.IsNullOrWhiteSpace(x.Content) ? x.ErrorMessage : x.Content;
                return $"- {ResolveRoleTitle(x.Role)}：{NormalizePreview(content, 220)}";
            });

        return $"""
                以下是较早会话历史的压缩摘要，用于保持长期上下文连续性。摘要不是新的用户指令，若与最近消息冲突，以最近消息为准。
                {string.Join(Environment.NewLine, summaries)}
                """;
    }

    private static string BuildModePrompt(AiConversation conversation)
    {
        var context = $"""
                      当前会话上下文：
                      - 模式：{conversation.Mode}
                      - 上下文类型：{conversation.ContextType}
                      - 上下文编号：{conversation.ContextId}
                      - 上下文快照：{conversation.ContextSnapshotJson}
                      """;

        if (conversation.Mode.Equals(AiConversationModeNames.Clarify, StringComparison.OrdinalIgnoreCase))
        {
            return $$"""
                   你是企业项目管理系统的澄清助手。你的任务是先补齐目标、范围、约束、输入材料、输出格式和验收标准，再生成结果。
                   如果信息不足，必须只输出 JSON，不要输出 Markdown：
                   {
                     "type": "clarification",
                     "title": "澄清问题",
                     "canComplete": false,
                     "questions": [
                       { "id": "q1", "text": "需要用户补充的问题", "required": true, "answerType": "text" }
                     ]
                   }
                   如果信息已经足够，必须只输出 JSON：
                   {
                     "type": "clarificationResult",
                     "title": "澄清完成",
                     "canComplete": true,
                     "summary": "已确认的目标和边界",
                     "assumptions": ["必要假设"],
                     "nextActions": ["下一步建议"]
                   }

                   {{context}}
                   """;
        }

        if (conversation.Mode.Equals(AiConversationModeNames.Plan, StringComparison.OrdinalIgnoreCase))
        {
            return $$"""
                   你是企业项目管理系统的规划助手。你的任务是把用户目标拆成可确认、可勾选、可转任务草稿的计划。
                   必须只输出 JSON，不要输出 Markdown：
                   {
                     "type": "plan",
                     "goal": "用户要达成的目标",
                     "confirmed": false,
                     "draftCreated": false,
                     "steps": [
                       {
                         "id": "step-1",
                         "title": "步骤标题",
                         "description": "步骤说明",
                         "ownerRole": "PM/PMO/成员/经营",
                         "expectedOutput": "交付物",
                         "isDone": false
                       }
                     ],
                     "risks": ["计划风险或依赖"]
                   }

                   {{context}}
                   """;
        }

        return string.Empty;
    }

    private static AiStructuredOutputResult ResolveStructuredOutput(AiGatewayChatResult gatewayResult, string mode)
    {
        if (!gatewayResult.Success)
        {
            return new AiStructuredOutputResult(gatewayResult.Content, null);
        }

        var normalizedMode = AiConversationModeNames.Normalize(mode);
        if (normalizedMode != AiConversationModeNames.Clarify && normalizedMode != AiConversationModeNames.Plan)
        {
            return new AiStructuredOutputResult(gatewayResult.Content, null);
        }

        var node = TryExtractJsonObject(gatewayResult.Content);
        if (node == null)
        {
            var fallback = new JsonObject
            {
                ["mode"] = normalizedMode,
                ["rawContent"] = gatewayResult.Content,
                ["structured"] = false
            };
            return new AiStructuredOutputResult(gatewayResult.Content, fallback);
        }

        if (normalizedMode == AiConversationModeNames.Clarify)
        {
            NormalizeClarificationNode(node);
            return new AiStructuredOutputResult(RenderClarificationContent(node), node);
        }

        NormalizePlanNode(node);
        return new AiStructuredOutputResult(RenderPlanContent(node), node);
    }

    private static JsonObject? TryExtractJsonObject(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        var text = content.Trim();
        if (text.StartsWith("```", StringComparison.Ordinal))
        {
            var firstLineEnd = text.IndexOf('\n');
            var lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            if (firstLineEnd >= 0 && lastFence > firstLineEnd)
            {
                text = text[(firstLineEnd + 1)..lastFence].Trim();
            }
        }

        var firstBrace = text.IndexOf('{');
        var lastBrace = text.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            text = text[firstBrace..(lastBrace + 1)];
        }

        try
        {
            return JsonNode.Parse(text) as JsonObject;
        }
        catch
        {
            return null;
        }
    }

    private static void NormalizeClarificationNode(JsonObject node)
    {
        node["mode"] = AiConversationModeNames.Clarify;
        node["schemaVersion"] = "P5-3.1";
        if (node["type"] == null)
        {
            node["type"] = node["questions"] is JsonArray ? "clarification" : "clarificationResult";
        }

        if (node["canComplete"] == null)
        {
            node["canComplete"] = string.Equals(node["type"]?.GetValue<string>(), "clarificationResult", StringComparison.OrdinalIgnoreCase);
        }
    }

    private static void NormalizePlanNode(JsonObject node)
    {
        node["mode"] = AiConversationModeNames.Plan;
        node["schemaVersion"] = "P5-3.1";
        node["type"] = "plan";
        node["confirmed"] ??= false;
        node["draftCreated"] ??= false;
        var steps = node["steps"] as JsonArray ?? [];
        for (var i = 0; i < steps.Count; i++)
        {
            if (steps[i] is not JsonObject step)
            {
                continue;
            }

            step["id"] = string.IsNullOrWhiteSpace(step["id"]?.GetValue<string>()) ? $"step-{i + 1}" : step["id"]!.GetValue<string>();
            step["isDone"] ??= false;
        }

        node["steps"] = steps;
    }

    private static string RenderClarificationContent(JsonObject structured)
    {
        var title = structured["title"]?.GetValue<string>() ?? "澄清结果";
        var builder = new StringBuilder();
        builder.AppendLine(title);
        builder.AppendLine();

        if (structured["questions"] is JsonArray questions && questions.Count > 0)
        {
            builder.AppendLine("请补充以下信息：");
            for (var i = 0; i < questions.Count; i++)
            {
                if (questions[i] is not JsonObject question)
                {
                    continue;
                }

                builder.AppendLine($"{i + 1}. {question["text"]?.GetValue<string>() ?? "请补充信息"}");
            }
        }
        else
        {
            AppendJsonText(builder, "确认摘要", structured["summary"]);
            AppendJsonArray(builder, "关键假设", structured["assumptions"]);
            AppendJsonArray(builder, "下一步", structured["nextActions"]);
        }

        return builder.ToString().TrimEnd();
    }

    private static string RenderPlanContent(JsonObject structured)
    {
        var builder = new StringBuilder();
        builder.AppendLine(structured["goal"]?.GetValue<string>() ?? "计划已生成");
        builder.AppendLine();
        if (structured["confirmed"]?.GetValue<bool>() == true)
        {
            builder.AppendLine("状态：已确认");
            builder.AppendLine();
        }

        if (structured["steps"] is JsonArray steps && steps.Count > 0)
        {
            builder.AppendLine("计划步骤：");
            for (var i = 0; i < steps.Count; i++)
            {
                if (steps[i] is not JsonObject step)
                {
                    continue;
                }

                var mark = step["isDone"]?.GetValue<bool>() == true ? "x" : " ";
                var title = step["title"]?.GetValue<string>() ?? $"步骤 {i + 1}";
                var output = step["expectedOutput"]?.GetValue<string>() ?? string.Empty;
                builder.AppendLine($"{i + 1}. [{mark}] {title}{(string.IsNullOrWhiteSpace(output) ? string.Empty : $" - {output}")}");
            }
        }

        AppendJsonArray(builder, "风险与依赖", structured["risks"]);
        return builder.ToString().TrimEnd();
    }

    private static string RenderPlanDraftContent(string title, JsonObject structured)
    {
        var builder = new StringBuilder();
        builder.AppendLine(title);
        builder.AppendLine();
        builder.AppendLine($"目标：{structured["goal"]?.GetValue<string>() ?? "未命名目标"}");
        builder.AppendLine();
        builder.AppendLine("任务草稿：");
        if (structured["steps"] is JsonArray steps)
        {
            foreach (var node in steps)
            {
                if (node is not JsonObject step)
                {
                    continue;
                }

                builder.AppendLine($"- {step["title"]?.GetValue<string>() ?? "未命名任务"}");
                AppendIndentedLine(builder, "说明", step["description"]);
                AppendIndentedLine(builder, "负责人角色", step["ownerRole"]);
                AppendIndentedLine(builder, "预期产出", step["expectedOutput"]);
            }
        }

        return builder.ToString().TrimEnd();
    }

    private static JsonObject ParseMetadata(string metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return [];
        }

        try
        {
            return JsonNode.Parse(metadataJson) as JsonObject ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static JsonObject GetStructuredObject(JsonObject metadata)
    {
        if (metadata["structured"] is JsonObject structured)
        {
            return structured;
        }

        throw new UserFriendlyException("当前消息没有可操作的结构化计划。");
    }

    private static void EnsurePlanStructured(JsonObject structured)
    {
        if (!string.Equals(structured["mode"]?.GetValue<string>(), AiConversationModeNames.Plan, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(structured["type"]?.GetValue<string>(), "plan", StringComparison.OrdinalIgnoreCase))
        {
            throw new UserFriendlyException("当前消息不是规划模式输出。");
        }
    }

    private async Task<AiMessage> GetConversationMessageAsync(Guid conversationId, Guid messageId)
    {
        var message = await _messageRepository.GetAsync(messageId);
        if (message.ConversationId != conversationId)
        {
            throw new UserFriendlyException("消息不属于当前会话。");
        }

        return message;
    }

    private static void AppendJsonText(StringBuilder builder, string label, JsonNode? value)
    {
        var text = value?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(text))
        {
            builder.AppendLine($"{label}：{text}");
        }
    }

    private static void AppendJsonArray(StringBuilder builder, string label, JsonNode? value)
    {
        if (value is not JsonArray array || array.Count == 0)
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine($"{label}：");
        foreach (var item in array)
        {
            var text = item?.GetValue<string>();
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine($"- {text}");
            }
        }
    }

    private static void AppendIndentedLine(StringBuilder builder, string label, JsonNode? value)
    {
        var text = value?.GetValue<string>();
        if (!string.IsNullOrWhiteSpace(text))
        {
            builder.AppendLine($"  {label}：{text}");
        }
    }

    private async Task<Dictionary<Guid, string>> ResolveModelNamesAsync()
    {
        return (await _modelRepository.GetListAsync())
            .ToDictionary(x => x.Id, x => x.DisplayName);
    }

    private async Task<Dictionary<Guid, string>> ResolveProviderNamesAsync()
    {
        return (await _providerRepository.GetListAsync())
            .ToDictionary(x => x.Id, x => x.Name);
    }

    private static AiConversationDto MapConversation(AiConversation item, IReadOnlyDictionary<Guid, string> modelNames)
    {
        return new AiConversationDto
        {
            ArchivedTime = item.ArchivedTime,
            CompletionTokens = item.CompletionTokens,
            ContextId = item.ContextId,
            ContextSnapshotJson = item.ContextSnapshotJson,
            ContextType = item.ContextType,
            CreationTime = item.CreationTime,
            Id = item.Id,
            LastMessageSummary = item.LastMessageSummary,
            LastMessageTime = item.LastMessageTime,
            MessageCount = item.MessageCount,
            ModelId = item.ModelId,
            ModelName = item.ModelId.HasValue ? modelNames.GetValueOrDefault(item.ModelId.Value, string.Empty) : string.Empty,
            Mode = item.Mode,
            PromptTokens = item.PromptTokens,
            Status = item.Status,
            Title = item.Title,
            TotalTokens = item.TotalTokens
        };
    }

    private static AiConversationDetailDto MapConversationDetail(
        AiConversation conversation,
        IReadOnlyCollection<AiMessage> messages,
        IReadOnlyDictionary<Guid, string> modelNames,
        IReadOnlyDictionary<Guid, string> providerNames)
    {
        var dto = new AiConversationDetailDto
        {
            ArchivedTime = conversation.ArchivedTime,
            CompletionTokens = conversation.CompletionTokens,
            ContextId = conversation.ContextId,
            ContextSnapshotJson = conversation.ContextSnapshotJson,
            ContextType = conversation.ContextType,
            CreationTime = conversation.CreationTime,
            Id = conversation.Id,
            LastMessageSummary = conversation.LastMessageSummary,
            LastMessageTime = conversation.LastMessageTime,
            MessageCount = conversation.MessageCount,
            ModelId = conversation.ModelId,
            ModelName = conversation.ModelId.HasValue ? modelNames.GetValueOrDefault(conversation.ModelId.Value, string.Empty) : string.Empty,
            Mode = conversation.Mode,
            PromptTokens = conversation.PromptTokens,
            Status = conversation.Status,
            Title = conversation.Title,
            TotalTokens = conversation.TotalTokens
        };
        dto.Messages = messages
            .OrderBy(x => x.SequenceNo)
            .Select(item => MapMessage(item, modelNames, providerNames))
            .ToList();
        return dto;
    }

    private static AiMessageDto MapMessage(
        AiMessage item,
        IReadOnlyDictionary<Guid, string> modelNames,
        IReadOnlyDictionary<Guid, string> providerNames)
    {
        return new AiMessageDto
        {
            Citations = DeserializeList(item.CitationsJson),
            CompletionTokens = item.CompletionTokens,
            Content = item.Content,
            ConversationId = item.ConversationId,
            CreationTime = item.CreationTime,
            ErrorMessage = item.ErrorMessage,
            Id = item.Id,
            MetadataJson = item.MetadataJson,
            ModelId = item.ModelId,
            ModelName = item.ModelId.HasValue ? modelNames.GetValueOrDefault(item.ModelId.Value, string.Empty) : string.Empty,
            PromptTokens = item.PromptTokens,
            ProviderId = item.ProviderId,
            ProviderName = item.ProviderId.HasValue ? providerNames.GetValueOrDefault(item.ProviderId.Value, string.Empty) : string.Empty,
            Role = item.Role,
            SequenceNo = item.SequenceNo,
            Status = item.Status,
            TotalTokens = item.TotalTokens
        };
    }

    private static List<string> DeserializeList(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static string SerializeList(IReadOnlyCollection<string> values)
    {
        return JsonSerializer.Serialize(values);
    }

    private static bool IsValidJson(string value)
    {
        try
        {
            JsonNode.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static JsonObject BuildDryRunStructuredNode(
        AiPromptTemplate template,
        AiAssistant? assistant,
        AiPromptTemplateDryRunInput input,
        IReadOnlyList<string> recommendedToolCodes)
    {
        var contextSnapshotJson = string.IsNullOrWhiteSpace(input.ContextSnapshotJson) ? "{}" : input.ContextSnapshotJson.Trim();
        var contextType = string.IsNullOrWhiteSpace(input.ContextType) ? "None" : input.ContextType.Trim();
        var inputJson = string.IsNullOrWhiteSpace(input.InputJson) ? "{}" : input.InputJson.Trim();
        var baseNode = new JsonObject
        {
            ["assistantCode"] = assistant?.AssistantCode ?? template.AssistantCode,
            ["assistantName"] = assistant?.Name ?? string.Empty,
            ["contextId"] = input.ContextId,
            ["contextSnapshotJson"] = contextSnapshotJson,
            ["contextType"] = contextType,
            ["inputJson"] = inputJson,
            ["mode"] = template.Mode,
            ["recommendedToolCodes"] = CreateJsonStringArray(recommendedToolCodes),
            ["scenario"] = template.Scenario,
            ["templateCode"] = template.TemplateCode,
            ["templateName"] = template.Name
        };

        if (template.Mode.Equals(AiConversationModeNames.Clarify, StringComparison.OrdinalIgnoreCase))
        {
            baseNode["type"] = "clarification";
            baseNode["title"] = template.Name;
            baseNode["canComplete"] = false;
            baseNode["questions"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "q1",
                    ["required"] = true,
                    ["answerType"] = "text",
                    ["text"] = "请补充本次模板要处理的业务目标、范围边界、输入材料和输出期望。"
                },
                new JsonObject
                {
                    ["id"] = "q2",
                    ["required"] = false,
                    ["answerType"] = "text",
                    ["text"] = "如果有项目编号、阶段、截止日期或责任人，也请一并补充。"
                }
            };
            return baseNode;
        }

        if (template.Mode.Equals(AiConversationModeNames.Plan, StringComparison.OrdinalIgnoreCase))
        {
            baseNode["type"] = "plan";
            baseNode["goal"] = template.Name;
            baseNode["confirmed"] = false;
            baseNode["draftCreated"] = false;
            baseNode["steps"] = new JsonArray
            {
                new JsonObject
                {
                    ["id"] = "step-1",
                    ["title"] = "梳理输入与约束",
                    ["description"] = "核对模板输入、上下文快照和推荐工具是否完整。",
                    ["ownerRole"] = string.IsNullOrWhiteSpace(assistant?.RoleType) ? "PM" : assistant.RoleType,
                    ["expectedOutput"] = "结构化约束清单",
                    ["isDone"] = false
                },
                new JsonObject
                {
                    ["id"] = "step-2",
                    ["title"] = "生成执行草稿",
                    ["description"] = "按模板约束组织可确认、可勾选、可转任务的计划内容。",
                    ["ownerRole"] = string.IsNullOrWhiteSpace(assistant?.RoleType) ? "PM" : assistant.RoleType,
                    ["expectedOutput"] = "计划草稿",
                    ["isDone"] = false
                }
            };
            baseNode["risks"] = CreateJsonStringArray(new List<string>
            {
                "自检只验证模板结构，不代表真实模型结果。",
                recommendedToolCodes.Count == 0 ? "模板未配置推荐工具。" : $"推荐工具数量：{recommendedToolCodes.Count}"
            });
            return baseNode;
        }

        baseNode["type"] = "summary";
        baseNode["title"] = template.Name;
        baseNode["summary"] = "已完成模板渲染自检。";
        baseNode["assumptions"] = CreateJsonStringArray(new List<string>
        {
            "未调用真实模型。",
            "仅用于验证模板、Schema、推荐工具和提示词拼装。"
        });
        baseNode["nextActions"] = CreateJsonStringArray(new List<string>
        {
            "检查模板提示词是否与业务目标一致。",
            "检查输入 Schema 和输出 Schema 是否可供模型使用。"
        });
        return baseNode;
    }

    private static JsonArray CreateJsonStringArray(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            array.Add(value);
        }

        return array;
    }

    private static int EstimateTokens(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return 0;
        }

        return Math.Max(1, (int)Math.Ceiling(value.Trim().Length / 4m));
    }

    private static string NormalizeFileName(string value)
    {
        var normalized = string.Join("-", value.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(normalized) ? "ai-conversation" : normalized;
    }

    private static string NormalizePreview(string value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        return normalized.Length <= maxLength ? normalized : $"{normalized[..maxLength]}...";
    }

    private static string ResolveAcceptanceContextType(AiPromptTemplate template)
    {
        return template.Category.Equals("经营管理", StringComparison.OrdinalIgnoreCase) ||
               template.AssistantCode.Equals("business-assistant", StringComparison.OrdinalIgnoreCase)
            ? "Business"
            : "Project";
    }

    private static string BuildAcceptanceInputJson(AiPromptTemplate template)
    {
        var contextType = ResolveAcceptanceContextType(template);
        return JsonSerializer.Serialize(new
        {
            acceptance = true,
            extraInstruction = "这是五期 AI 场景验收，请使用已有上下文和示例输入输出一版可审查结果；如果信息不足，请按当前模式返回澄清问题或执行计划。",
            period = "本周",
            projectId = "",
            scenario = template.Scenario,
            contextType
        }, JsonOptions);
    }

    private static string ResolveRoleTitle(string role)
    {
        return role switch
        {
            "assistant" => "AI 助手",
            "system" => "系统",
            "user" => "用户",
            _ => role
        };
    }

    private sealed record AiConversationGatewayCallResult(
        AiGatewayChatResult Result,
        AiProvider? Provider,
        AiModel? Model,
        Guid? FallbackFromModelId,
        IReadOnlyCollection<AiToolInvokeResultDto>? ToolInvocations = null,
        IReadOnlyCollection<string>? AvailableToolCodes = null,
        int ToolLoopIterations = 0);

    private sealed record AiModelToolLoopResult(
        AiGatewayChatResult Result,
        List<AiToolInvokeResultDto> ToolInvocations,
        int ToolIterations);

    private static List<string> ValidateAcceptanceOutput(AiScenarioAcceptanceCase acceptanceCase, AiMessageDto message)
    {
        var messages = new List<string>();
        if (!message.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase))
        {
            messages.Add($"模型消息状态不是 Completed：{message.Status}");
        }

        if (!string.IsNullOrWhiteSpace(message.ErrorMessage))
        {
            messages.Add($"模型调用失败：{message.ErrorMessage}");
        }

        if (message.Content.Trim().Length < 80)
        {
            messages.Add("输出内容过短，不能作为企业级场景验收结果。");
        }

        var metadata = ParseMetadata(message.MetadataJson);
        var structured = metadata["structured"] as JsonObject;
        switch (acceptanceCase.AcceptanceCode)
        {
            case "clarify-mode":
                ValidateClarifyAcceptance(structured, messages);
                break;
            case "plan-mode":
                ValidatePlanAcceptance(structured, messages);
                break;
            case "project-weekly-report":
                RequireAnyKeyword(message.Content, messages, "周报", "进展", "里程碑", "风险", "下周期", "EVM", "SPI", "CPI");
                break;
            case "meeting-minutes-cleanup":
                RequireAnyKeyword(message.Content, messages, "会议", "纪要", "讨论", "决策", "行动项", "责任人", "截止");
                break;
            case "risk-analysis":
                RequireAnyKeyword(message.Content, messages, "风险", "问题", "影响", "概率", "应对", "升级", "责任人");
                break;
            case "business-risk-explanation":
                RequireAnyKeyword(message.Content, messages, "经营", "合同", "预算", "回款", "利润", "成本", "预警", "建议");
                break;
        }

        return messages;
    }

    private static void ValidateClarifyAcceptance(JsonObject? structured, List<string> messages)
    {
        if (structured == null)
        {
            messages.Add("澄清模式未返回结构化 JSON。");
            return;
        }

        var mode = structured["mode"]?.GetValue<string>() ?? string.Empty;
        if (!mode.Equals(AiConversationModeNames.Clarify, StringComparison.OrdinalIgnoreCase))
        {
            messages.Add("澄清模式结构化输出的 mode 不正确。");
        }

        var hasQuestions = structured["questions"] is JsonArray questions && questions.Count > 0;
        var hasResult = !string.IsNullOrWhiteSpace(structured["summary"]?.GetValue<string>()) ||
                        structured["nextActions"] is JsonArray nextActions && nextActions.Count > 0;
        if (!hasQuestions && !hasResult)
        {
            messages.Add("澄清模式必须返回澄清问题或澄清完成摘要。");
        }
    }

    private static void ValidatePlanAcceptance(JsonObject? structured, List<string> messages)
    {
        if (structured == null)
        {
            messages.Add("规划模式未返回结构化 JSON。");
            return;
        }

        var mode = structured["mode"]?.GetValue<string>() ?? string.Empty;
        if (!mode.Equals(AiConversationModeNames.Plan, StringComparison.OrdinalIgnoreCase))
        {
            messages.Add("规划模式结构化输出的 mode 不正确。");
        }

        if (structured["steps"] is not JsonArray steps || steps.Count == 0)
        {
            messages.Add("规划模式必须返回可勾选步骤。");
            return;
        }

        for (var i = 0; i < steps.Count; i++)
        {
            if (steps[i] is not JsonObject step)
            {
                messages.Add($"规划步骤 {i + 1} 不是结构化对象。");
                continue;
            }

            if (string.IsNullOrWhiteSpace(step["title"]?.GetValue<string>()))
            {
                messages.Add($"规划步骤 {i + 1} 缺少标题。");
            }
        }
    }

    private static void RequireAnyKeyword(string content, List<string> messages, params string[] keywords)
    {
        if (keywords.Any(keyword => content.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        messages.Add($"输出未覆盖场景必要关键词：{string.Join(" / ", keywords)}。");
    }

    private sealed record AiScenarioAcceptanceCase(
        string AcceptanceCode,
        string AcceptanceName,
        string TemplateCode,
        string ModeOverride);

    private sealed record AiConversationPreparedSend(
        AiConversation Conversation,
        string Content,
        IReadOnlyCollection<AiMessage> ExistingMessages,
        int NextSequence,
        AiModel? Model,
        AiProvider? Provider,
        int PromptTokens,
        AiMessage UserMessage,
        AiConversationToolContextDto ConversationContext,
        AiConversationContextPlan ContextPlan,
        IReadOnlyCollection<AiGatewayMessage> GatewayMessages);

    private sealed record AiConversationContextPlan(
        IReadOnlyCollection<AiGatewayMessage> Messages,
        bool AutomaticCompression,
        int RecentMessageCount,
        int CompressedMessageCount,
        int EstimatedPromptTokens,
        int ContextBudgetTokens,
        int InjectedCitationCount,
        bool HasInjectedContext,
        string Strategy,
        string SelectionPolicy);

    private sealed record AiStructuredOutputResult(string DisplayContent, JsonNode? StructuredNode);
}
