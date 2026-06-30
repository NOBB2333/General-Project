using System.Text;
using System.Text.Json;
using General.Admin.Project;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace General.Admin.Ai;

public class AiMemoryService : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiKnowledgeSource, Guid> _knowledgeSourceRepository;
    private readonly IRepository<AiMemory, Guid> _memoryRepository;
    private readonly ProjectQueryService _projectQueryService;

    public AiMemoryService(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IGuidGenerator guidGenerator,
        IRepository<AiKnowledgeSource, Guid> knowledgeSourceRepository,
        IRepository<AiMemory, Guid> memoryRepository,
        ProjectQueryService projectQueryService)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _guidGenerator = guidGenerator;
        _knowledgeSourceRepository = knowledgeSourceRepository;
        _memoryRepository = memoryRepository;
        _projectQueryService = projectQueryService;
    }

    public async Task<List<AiMemoryDto>> GetMemoriesAsync(AiMemoryQueryInput input)
    {
        var memories = await _memoryRepository.GetListAsync();
        if (!input.IncludeDisabled)
        {
            memories = memories.Where(x => x.IsEnabled).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.MemoryType))
        {
            var memoryType = input.MemoryType.Trim();
            memories = memories.Where(x => x.MemoryType.Equals(memoryType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.ContextType))
        {
            var contextType = input.ContextType.Trim();
            memories = memories.Where(x => x.ContextType.Equals(contextType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.ContextId))
        {
            var contextId = input.ContextId.Trim();
            memories = memories.Where(x => x.ContextId.Equals(contextId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (input.OwnerUserId.HasValue)
        {
            memories = memories.Where(x => x.OwnerUserId == input.OwnerUserId.Value).ToList();
        }

        if (input.KnowledgeSourceId.HasValue)
        {
            memories = memories.Where(x => x.KnowledgeSourceId == input.KnowledgeSourceId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            memories = memories
                .Where(x =>
                    x.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Summary.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        var sourceTitles = await ResolveSourceTitlesAsync(memories.Select(x => x.KnowledgeSourceId));
        return memories
            .OrderByDescending(x => x.IsEnabled)
            .ThenByDescending(x => x.Importance)
            .ThenByDescending(x => x.CreationTime)
            .Take(Math.Clamp(input.MaxResultCount, 1, 500))
            .Select(item => MapMemory(item, sourceTitles))
            .ToList();
    }

    public async Task<AiMemoryDto> CreateMemoryAsync(AiMemorySaveInput input)
    {
        ValidateMemoryInput(input);
        var ownerUserId = ResolveOwnerUserId(input.MemoryType, input.OwnerUserId);
        var entity = new AiMemory(
            _guidGenerator.Create(),
            input.MemoryType,
            input.Title,
            input.Content,
            _currentTenant.Id,
            input.Summary,
            input.ContextType,
            input.ContextId,
            ownerUserId,
            input.KnowledgeSourceId,
            input.TagsJson,
            input.SourceReferencesJson,
            input.Importance,
            input.IsEnabled);
        await _memoryRepository.InsertAsync(entity, autoSave: true);
        return MapMemory(entity, await ResolveSourceTitlesAsync([entity.KnowledgeSourceId]));
    }

    public async Task<AiMemoryDto> UpdateMemoryAsync(Guid id, AiMemorySaveInput input)
    {
        ValidateMemoryInput(input);
        var entity = await _memoryRepository.GetAsync(id);
        entity.Update(
            input.MemoryType,
            input.Title,
            input.Content,
            input.Summary,
            input.ContextType,
            input.ContextId,
            ResolveOwnerUserId(input.MemoryType, input.OwnerUserId),
            input.KnowledgeSourceId,
            input.TagsJson,
            input.SourceReferencesJson,
            input.Importance,
            input.IsEnabled);
        await _memoryRepository.UpdateAsync(entity, autoSave: true);
        return MapMemory(entity, await ResolveSourceTitlesAsync([entity.KnowledgeSourceId]));
    }

    public async Task<AiMemoryDto> ToggleMemoryAsync(Guid id, bool isEnabled)
    {
        var entity = await _memoryRepository.GetAsync(id);
        entity.Toggle(isEnabled);
        await _memoryRepository.UpdateAsync(entity, autoSave: true);
        return MapMemory(entity, await ResolveSourceTitlesAsync([entity.KnowledgeSourceId]));
    }

    public async Task DeleteMemoryAsync(Guid id)
    {
        await _memoryRepository.DeleteAsync(id, autoSave: true);
    }

    public async Task<List<AiKnowledgeSourceDto>> GetKnowledgeSourcesAsync(AiKnowledgeSourceQueryInput input)
    {
        var sources = await _knowledgeSourceRepository.GetListAsync();
        if (!string.IsNullOrWhiteSpace(input.SourceType))
        {
            var sourceType = input.SourceType.Trim();
            sources = sources.Where(x => x.SourceType.Equals(sourceType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.ContextType))
        {
            var contextType = input.ContextType.Trim();
            sources = sources.Where(x => x.ContextType.Equals(contextType, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.ContextId))
        {
            var contextId = input.ContextId.Trim();
            sources = sources.Where(x => x.ContextId.Equals(contextId, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (input.ProjectId.HasValue)
        {
            sources = sources.Where(x => x.ProjectId == input.ProjectId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            sources = sources
                .Where(x =>
                    x.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.Summary.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.ContentText.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return sources
            .OrderByDescending(x => x.LastIndexedTime)
            .Take(Math.Clamp(input.MaxResultCount, 1, 500))
            .Select(MapSource)
            .ToList();
    }

    public async Task<AiMemoryImportResultDto> ImportProjectDocumentsAsync(Guid projectId)
    {
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var imported = 0;
        var skipped = 0;
        foreach (var document in detail.Documents)
        {
            var content = string.IsNullOrWhiteSpace(document.Summary)
                ? $"{document.Category} {document.Title} {document.Status}".Trim()
                : document.Summary.Trim();
            if (string.IsNullOrWhiteSpace(content))
            {
                skipped++;
                continue;
            }

            await UpsertSourceAndMemoryAsync(
                "ProjectDocument",
                $"project:{projectId}:document:{document.Id}",
                $"项目文档：{document.Title}",
                content,
                content,
                detail,
                "Project",
                ["项目文档", document.Category],
                new
                {
                    documentId = document.Id,
                    document.Category,
                    document.OwnerName,
                    document.Status,
                    document.Version
                });
            imported++;
        }

        return new AiMemoryImportResultDto
        {
            ImportedCount = imported,
            ProjectId = projectId,
            SkippedCount = skipped,
            SourceType = "ProjectDocument"
        };
    }

    public async Task<AiMemoryImportResultDto> ImportProjectMeetingsAsync(Guid projectId)
    {
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var imported = 0;
        var skipped = 0;
        foreach (var meeting in detail.MeetingRecords)
        {
            var content = BuildMeetingContent(meeting);
            if (string.IsNullOrWhiteSpace(content))
            {
                skipped++;
                continue;
            }

            await UpsertSourceAndMemoryAsync(
                "ProjectMeeting",
                $"project:{projectId}:meeting:{meeting.Id}",
                $"会议纪要：{meeting.Topic}",
                BuildSummary(content, 1000),
                content,
                detail,
                "Scenario",
                ["会议纪要", meeting.Status],
                new
                {
                    meetingId = meeting.Id,
                    meeting.MeetingTime,
                    meeting.FacilitatorName,
                    meeting.ParticipantNames,
                    actionItemCount = meeting.ActionItems.Count
                });
            imported++;
        }

        return new AiMemoryImportResultDto
        {
            ImportedCount = imported,
            ProjectId = projectId,
            SkippedCount = skipped,
            SourceType = "ProjectMeeting"
        };
    }

    public async Task<AiMemoryImportResultDto> ImportProjectRisksAsync(Guid projectId)
    {
        var detail = await _projectQueryService.GetDetailAsync(projectId);
        var imported = 0;
        var skipped = 0;
        foreach (var risk in detail.RaidItems.Where(x => x.Type == ProjectRaidTypes.Risk))
        {
            var content = BuildRiskContent(risk);
            if (string.IsNullOrWhiteSpace(content))
            {
                skipped++;
                continue;
            }

            await UpsertSourceAndMemoryAsync(
                "ProjectRiskReview",
                $"project:{projectId}:risk:{risk.Id}",
                $"风险复盘：{risk.Title}",
                BuildSummary(content, 1000),
                content,
                detail,
                "Scenario",
                ["风险复盘", risk.Level, risk.Status],
                new
                {
                    riskId = risk.Id,
                    risk.Level,
                    risk.Status,
                    risk.RiskScore,
                    risk.OwnerName,
                    risk.PlannedResolveDate
                });
            imported++;
        }

        return new AiMemoryImportResultDto
        {
            ImportedCount = imported,
            ProjectId = projectId,
            SkippedCount = skipped,
            SourceType = "ProjectRiskReview"
        };
    }

    public async Task<AiConversationMemoryContextDto> BuildConversationMemoryContextAsync(AiConversation conversation)
    {
        var memories = (await _memoryRepository.GetListAsync(x => x.IsEnabled))
            .Where(memory =>
                memory.MemoryType == "Organization" ||
                IsCurrentUserMemory(memory) ||
                IsConversationContextMemory(memory, conversation) ||
                IsConversationScenarioMemory(memory, conversation))
            .OrderByDescending(x => x.Importance)
            .ThenByDescending(x => x.CreationTime)
            .Take(8)
            .ToList();

        if (memories.Count == 0)
        {
            return new AiConversationMemoryContextDto();
        }

        var sourceTitles = await ResolveSourceTitlesAsync(memories.Select(x => x.KnowledgeSourceId));
        var prompt = new StringBuilder();
        prompt.AppendLine("以下是可用于本次回答的 AI 记忆。必须结合上下文判断是否采用，并保留来源引用：");
        foreach (var memory in memories)
        {
            prompt.AppendLine($"- [{memory.MemoryType}] {memory.Title}：{BuildSummary(memory.Content, 600)}");
        }

        return new AiConversationMemoryContextDto
        {
            Citations = memories.Select(memory => FormatMemoryCitation(memory, sourceTitles)).Distinct().ToList(),
            ContextPrompt = prompt.ToString().Trim()
        };
    }

    private async Task UpsertSourceAndMemoryAsync(
        string sourceType,
        string sourceId,
        string title,
        string summary,
        string content,
        ProjectDetailDto detail,
        string memoryType,
        IReadOnlyCollection<string> tags,
        object metadata)
    {
        var contextId = detail.Id.ToString();
        var metadataJson = JsonSerializer.Serialize(metadata, JsonOptions);
        var source = (await _knowledgeSourceRepository.GetListAsync(x => x.SourceType == sourceType && x.SourceId == sourceId)).FirstOrDefault();
        if (source == null)
        {
            source = new AiKnowledgeSource(
                _guidGenerator.Create(),
                sourceType,
                sourceId,
                title,
                summary,
                content,
                _currentTenant.Id,
                "Project",
                contextId,
                detail.Id,
                metadataJson);
            await _knowledgeSourceRepository.InsertAsync(source, autoSave: false);
        }
        else
        {
            source.Refresh(title, summary, content, "Project", contextId, detail.Id, metadataJson);
            await _knowledgeSourceRepository.UpdateAsync(source, autoSave: false);
        }

        var sourceReferencesJson = SerializeSourceReferences(sourceType, sourceId, title, detail.Id);
        var tagsJson = JsonSerializer.Serialize(tags.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList(), JsonOptions);
        var memory = (await _memoryRepository.GetListAsync(x => x.KnowledgeSourceId == source.Id)).FirstOrDefault();
        if (memory == null)
        {
            await _memoryRepository.InsertAsync(new AiMemory(
                _guidGenerator.Create(),
                memoryType,
                title,
                content,
                _currentTenant.Id,
                summary,
                "Project",
                contextId,
                null,
                source.Id,
                tagsJson,
                sourceReferencesJson,
                importance: ResolveImportImportance(sourceType),
                isEnabled: true), autoSave: true);
            return;
        }

        memory.Update(
            memoryType,
            title,
            content,
            summary,
            "Project",
            contextId,
            null,
            source.Id,
            tagsJson,
            sourceReferencesJson,
            ResolveImportImportance(sourceType),
            memory.IsEnabled);
        await _memoryRepository.UpdateAsync(memory, autoSave: true);
    }

    private Guid? ResolveOwnerUserId(string memoryType, Guid? ownerUserId)
    {
        return memoryType.Equals("User", StringComparison.OrdinalIgnoreCase)
            ? ownerUserId ?? _currentUser.Id
            : ownerUserId;
    }

    private bool IsCurrentUserMemory(AiMemory memory)
    {
        return memory.MemoryType == "User" && _currentUser.Id.HasValue && memory.OwnerUserId == _currentUser.Id.Value;
    }

    private static bool IsConversationContextMemory(AiMemory memory, AiConversation conversation)
    {
        return !string.IsNullOrWhiteSpace(memory.ContextType) &&
               !string.IsNullOrWhiteSpace(memory.ContextId) &&
               memory.ContextType.Equals(conversation.ContextType, StringComparison.OrdinalIgnoreCase) &&
               memory.ContextId.Equals(conversation.ContextId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsConversationScenarioMemory(AiMemory memory, AiConversation conversation)
    {
        return memory.MemoryType == "Scenario" &&
               !string.IsNullOrWhiteSpace(memory.ContextType) &&
               memory.ContextType.Equals(conversation.ContextType, StringComparison.OrdinalIgnoreCase);
    }

    private async Task<IReadOnlyDictionary<Guid, string>> ResolveSourceTitlesAsync(IEnumerable<Guid?> sourceIds)
    {
        var ids = sourceIds.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToHashSet();
        if (ids.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        return (await _knowledgeSourceRepository.GetListAsync(x => ids.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => x.Title);
    }

    private static void ValidateMemoryInput(AiMemorySaveInput input)
    {
        Check.NotNullOrWhiteSpace(input.Title, nameof(input.Title), 256);
        Check.NotNullOrWhiteSpace(input.Content, nameof(input.Content), 8000);
        ValidateJsonArray(input.TagsJson, "记忆标签");
        ValidateJsonArray(input.SourceReferencesJson, "来源引用");
    }

    private static void ValidateJsonArray(string? json, string label)
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "[]" : json.Trim());
        if (document.RootElement.ValueKind != JsonValueKind.Array)
        {
            throw new UserFriendlyException($"{label} 必须是 JSON 数组。");
        }
    }

    private static string BuildMeetingContent(ProjectMeetingRecordDto meeting)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"主题：{meeting.Topic}");
        builder.AppendLine($"时间：{meeting.MeetingTime:yyyy-MM-dd HH:mm}");
        builder.AppendLine($"主持人：{meeting.FacilitatorName}");
        if (!string.IsNullOrWhiteSpace(meeting.Minutes))
        {
            builder.AppendLine($"纪要：{meeting.Minutes}");
        }

        if (!string.IsNullOrWhiteSpace(meeting.Decisions))
        {
            builder.AppendLine($"决议：{meeting.Decisions}");
        }

        if (meeting.ActionItems.Count > 0)
        {
            builder.AppendLine("行动项：");
            foreach (var action in meeting.ActionItems)
            {
                builder.AppendLine($"- {action.Title}；负责人：{action.AssigneeName}；状态：{action.Status}；到期：{action.DueDate:yyyy-MM-dd}");
            }
        }

        return builder.ToString().Trim();
    }

    private static string BuildRiskContent(ProjectRaidItemDto risk)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"风险：{risk.Title}");
        builder.AppendLine($"等级：{risk.Level}；状态：{risk.Status}；风险分：{risk.RiskScore}；概率：{risk.Probability}；影响：{risk.Impact}");
        builder.AppendLine($"负责人：{risk.OwnerName}；计划解决日期：{risk.PlannedResolveDate:yyyy-MM-dd}");
        if (!string.IsNullOrWhiteSpace(risk.TriggerCondition))
        {
            builder.AppendLine($"触发条件：{risk.TriggerCondition}");
        }

        if (!string.IsNullOrWhiteSpace(risk.ResponseStrategy))
        {
            builder.AppendLine($"应对策略：{risk.ResponseStrategy}");
        }

        if (!string.IsNullOrWhiteSpace(risk.ContingencyPlan))
        {
            builder.AppendLine($"应急预案：{risk.ContingencyPlan}");
        }

        return builder.ToString().Trim();
    }

    private static string BuildSummary(string value, int maxLength)
    {
        var normalized = string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }

    private static int ResolveImportImportance(string sourceType)
    {
        return sourceType switch
        {
            "ProjectRiskReview" => 80,
            "ProjectMeeting" => 70,
            "ProjectDocument" => 60,
            _ => 50
        };
    }

    private static string SerializeSourceReferences(string sourceType, string sourceId, string title, Guid projectId)
    {
        var references = new[]
        {
            new
            {
                path = $"/project/detail?id={projectId}",
                sourceId,
                sourceType,
                title
            }
        };
        return JsonSerializer.Serialize(references, JsonOptions);
    }

    private static string FormatMemoryCitation(AiMemory memory, IReadOnlyDictionary<Guid, string> sourceTitles)
    {
        var sourceTitle = memory.KnowledgeSourceId.HasValue
            ? sourceTitles.GetValueOrDefault(memory.KnowledgeSourceId.Value, string.Empty)
            : string.Empty;
        return string.IsNullOrWhiteSpace(sourceTitle)
            ? $"AI记忆/{memory.MemoryType}/{memory.Title}"
            : $"AI记忆/{memory.MemoryType}/{memory.Title}（来源：{sourceTitle}）";
    }

    private static AiMemoryDto MapMemory(AiMemory item, IReadOnlyDictionary<Guid, string> sourceTitles)
    {
        return new AiMemoryDto
        {
            Content = item.Content,
            ContextId = item.ContextId,
            ContextType = item.ContextType,
            CreationTime = item.CreationTime,
            Id = item.Id,
            Importance = item.Importance,
            IsEnabled = item.IsEnabled,
            KnowledgeSourceId = item.KnowledgeSourceId,
            KnowledgeSourceTitle = item.KnowledgeSourceId.HasValue ? sourceTitles.GetValueOrDefault(item.KnowledgeSourceId.Value, string.Empty) : string.Empty,
            LastUsedTime = item.LastUsedTime,
            MemoryType = item.MemoryType,
            OwnerUserId = item.OwnerUserId,
            SourceReferencesJson = item.SourceReferencesJson,
            Summary = item.Summary,
            TagsJson = item.TagsJson,
            Title = item.Title,
            UseCount = item.UseCount
        };
    }

    private static AiKnowledgeSourceDto MapSource(AiKnowledgeSource item)
    {
        return new AiKnowledgeSourceDto
        {
            ContextId = item.ContextId,
            ContextType = item.ContextType,
            ContentText = item.ContentText,
            CreationTime = item.CreationTime,
            Id = item.Id,
            LastIndexedTime = item.LastIndexedTime,
            MetadataJson = item.MetadataJson,
            ProjectId = item.ProjectId,
            SourceId = item.SourceId,
            SourceType = item.SourceType,
            Status = item.Status,
            Summary = item.Summary,
            Title = item.Title
        };
    }
}
