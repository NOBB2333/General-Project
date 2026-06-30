using System.Text.Json;
using System.Text.Json.Serialization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Integration;

public class IntegrationSyncImportService : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly string[] GroupArrayNames =
    [
        "groups",
        "groupItems",
        "groupBindings",
        "externalGroups",
        "chats",
        "chatList"
    ];

    private static readonly string[] MeetingArrayNames =
    [
        "meetings",
        "meetingItems",
        "meetingRecords",
        "meetingList",
        "calendarEvents"
    ];

    private readonly IntegrationBindingService _bindingService;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<IntegrationIdMapping, Guid> _idMappingRepository;
    private readonly IRepository<ProjectMeetingRecord, Guid> _meetingRecordRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;

    public IntegrationSyncImportService(
        IntegrationBindingService bindingService,
        IGuidGenerator guidGenerator,
        IRepository<IntegrationIdMapping, Guid> idMappingRepository,
        IRepository<ProjectMeetingRecord, Guid> meetingRecordRepository,
        IRepository<ProjectEntity, Guid> projectRepository)
    {
        _bindingService = bindingService;
        _guidGenerator = guidGenerator;
        _idMappingRepository = idMappingRepository;
        _meetingRecordRepository = meetingRecordRepository;
        _projectRepository = projectRepository;
    }

    public async Task<IntegrationSyncResult> SyncGroupsAsync(
        IntegrationConnector connector,
        string configJson,
        string requestJson,
        string providerName)
    {
        var rows = ExtractRows(configJson, requestJson, GroupArrayNames);
        var conflictItems = new List<object>();
        var successCount = 0;
        var skippedCount = 0;

        foreach (var row in rows)
        {
            var externalGroupId = ReadString(
                row,
                "externalGroupId",
                "external_group_id",
                "openChatId",
                "open_chat_id",
                "chatId",
                "chat_id",
                "conversationId",
                "conversation_id",
                "groupId",
                "group_id",
                "id");
            if (string.IsNullOrWhiteSpace(externalGroupId))
            {
                skippedCount++;
                conflictItems.Add(new { reason = "外部群 ID 为空。", raw = row.GetRawText() });
                continue;
            }

            var internalObjectId = ReadString(row, "internalObjectId", "internal_object_id", "projectId", "project.id");
            var internalObjectType = ReadString(row, "internalObjectType", "internal_object_type");
            if (string.IsNullOrWhiteSpace(internalObjectType) && !string.IsNullOrWhiteSpace(internalObjectId))
            {
                internalObjectType = "Project";
            }

            var status = ReadString(row, "status");
            if (string.IsNullOrWhiteSpace(status))
            {
                status = string.IsNullOrWhiteSpace(internalObjectId)
                    ? IntegrationBindingStatus.Pending
                    : IntegrationBindingStatus.Bound;
            }

            await _bindingService.SaveGroupBindingAsync(new IntegrationGroupBindingSaveInput
            {
                ConnectorId = connector.Id,
                ExternalGroupId = externalGroupId,
                ExternalGroupName = ReadString(row, "externalGroupName", "external_group_name", "chatName", "chat_name", "name", "title"),
                GroupType = ReadString(row, "groupType", "group_type", "chatType", "chat_type") is { Length: > 0 } groupType
                    ? groupType
                    : "Chat",
                InternalObjectId = internalObjectId,
                InternalObjectName = ReadString(row, "internalObjectName", "internal_object_name", "projectName", "project.name"),
                InternalObjectType = internalObjectType,
                Remark = ReadString(row, "remark") is { Length: > 0 } remark
                    ? remark
                    : $"{providerName} 群同步导入",
                RobotCode = ReadString(row, "robotCode", "robot_code", "botId", "bot_id"),
                Status = status
            });
            successCount++;
        }

        return BuildResult(
            providerName,
            "群同步",
            rows.Count,
            successCount,
            skippedCount,
            conflictItems,
            new
            {
                GroupCount = rows.Count,
                ImportedCount = successCount
            });
    }

    public async Task<IntegrationSyncResult> SyncMeetingsAsync(
        IntegrationConnector connector,
        string configJson,
        string requestJson,
        string providerName)
    {
        var rows = ExtractRows(configJson, requestJson, MeetingArrayNames);
        var conflictItems = new List<object>();
        var createdCount = 0;
        var skippedCount = 0;

        foreach (var row in rows)
        {
            var externalMeetingId = ReadString(
                row,
                "externalMeetingId",
                "external_meeting_id",
                "meetingId",
                "meeting_id",
                "eventId",
                "event_id",
                "calendarEventId",
                "calendar_event_id",
                "id");
            if (string.IsNullOrWhiteSpace(externalMeetingId))
            {
                skippedCount++;
                conflictItems.Add(new { reason = "外部会议 ID 为空。", raw = row.GetRawText() });
                continue;
            }

            var existingMapping = await _idMappingRepository.FindAsync(item =>
                item.ConnectorId == connector.Id &&
                item.ExternalObjectType == "ExternalMeeting" &&
                item.ExternalObjectId == externalMeetingId);
            if (existingMapping != null)
            {
                skippedCount++;
                continue;
            }

            var project = await ResolveProjectAsync(row);
            if (project == null)
            {
                skippedCount++;
                conflictItems.Add(new { reason = "会议记录未匹配到内部项目。", externalMeetingId, raw = row.GetRawText() });
                continue;
            }

            var facilitatorUserId = ReadGuid(row, "facilitatorUserId", "facilitator_user_id", "hostUserId", "host_user_id", "ownerUserId", "owner_user_id")
                ?? project.ManagerUserId;
            var topic = ReadString(row, "topic", "title", "summary", "name");
            var meetingRecord = new ProjectMeetingRecord(
                _guidGenerator.Create(),
                project.Id,
                string.IsNullOrWhiteSpace(topic) ? $"{providerName} 外部会议" : topic,
                ReadDateTime(row, "meetingTime", "meeting_time", "startTime", "start_time", "start") ?? DateTime.Now,
                facilitatorUserId,
                ReadGuidList(row, "participantUserIds", "participant_user_ids", "attendeeUserIds", "attendee_user_ids"),
                ReadString(row, "status") is { Length: > 0 } status ? status : "已确认",
                ReadString(row, "minutes", "content", "description"),
                ReadString(row, "decisions", "decision"),
                ReadGuid(row, "attachmentFileId", "attachment_file_id"));
            await _meetingRecordRepository.InsertAsync(meetingRecord, autoSave: true);

            await _bindingService.SaveIdMappingAsync(new IntegrationIdMappingSaveInput
            {
                ConnectorId = connector.Id,
                ExternalObjectId = externalMeetingId,
                ExternalObjectType = "ExternalMeeting",
                InternalObjectId = meetingRecord.Id.ToString(),
                InternalObjectType = "ProjectMeetingRecord",
                Remark = $"{providerName} 会议记录同步导入",
                Source = IntegrationBindingMatchTypes.Sync,
                Status = IntegrationBindingStatus.Bound
            });
            createdCount++;
        }

        return BuildResult(
            providerName,
            "会议记录同步",
            rows.Count,
            createdCount,
            skippedCount,
            conflictItems,
            new
            {
                MeetingCount = rows.Count,
                CreatedCount = createdCount
            });
    }

    private async Task<ProjectEntity?> ResolveProjectAsync(JsonElement row)
    {
        var projectId = ReadGuid(row, "projectId", "project_id", "project.id", "internalProjectId", "internal_project_id");
        if (projectId.HasValue)
        {
            return await _projectRepository.FindAsync(projectId.Value);
        }

        var projectCode = ReadString(row, "projectCode", "project_code", "project.code");
        if (string.IsNullOrWhiteSpace(projectCode))
        {
            return null;
        }

        var projects = await _projectRepository.GetListAsync(item => item.ProjectCode == projectCode);
        return projects.FirstOrDefault();
    }

    private static IntegrationSyncResult BuildResult(
        string providerName,
        string actionName,
        int totalCount,
        int successCount,
        int skippedCount,
        IReadOnlyCollection<object> conflictItems,
        object payload)
    {
        var failedCount = conflictItems.Count;
        return new IntegrationSyncResult
        {
            ConflictCount = failedCount,
            ConflictJson = failedCount == 0 ? "{}" : JsonSerializer.Serialize(conflictItems, JsonOptions),
            CreatedCount = successCount,
            Message = $"{providerName}{actionName}完成：处理 {totalCount} 条，成功 {successCount} 条，跳过 {skippedCount} 条，冲突 {failedCount} 条。",
            ResultJson = JsonSerializer.Serialize(payload, JsonOptions),
            SkippedCount = skippedCount,
            SuccessCount = successCount,
            TotalCount = totalCount
        };
    }

    private static List<JsonElement> ExtractRows(string configJson, string requestJson, IReadOnlyCollection<string> arrayNames)
    {
        var rows = new List<JsonElement>();
        ExtractRows(configJson, rows, arrayNames, depth: 0);
        ExtractRows(requestJson, rows, arrayNames, depth: 0);
        return rows;
    }

    private static void ExtractRows(string json, List<JsonElement> rows, IReadOnlyCollection<string> arrayNames, int depth)
    {
        if (string.IsNullOrWhiteSpace(json) || depth > 6)
        {
            return;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            Visit(document.RootElement, rows, arrayNames, depth);
        }
        catch (JsonException)
        {
            throw new UserFriendlyException("外部系统同步导入配置 JSON 格式不正确。");
        }
    }

    private static void Visit(JsonElement element, List<JsonElement> rows, IReadOnlyCollection<string> arrayNames, int depth)
    {
        if (depth > 6)
        {
            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (arrayNames.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) &&
                property.Value.ValueKind == JsonValueKind.Array)
            {
                rows.AddRange(property.Value.EnumerateArray()
                    .Where(item => item.ValueKind == JsonValueKind.Object)
                    .Select(item => item.Clone()));
                continue;
            }

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                Visit(property.Value, rows, arrayNames, depth + 1);
            }
            else if (property.Value.ValueKind == JsonValueKind.String &&
                     IsNestedJsonProperty(property.Name))
            {
                ExtractRows(property.Value.GetString() ?? string.Empty, rows, arrayNames, depth + 1);
            }
        }
    }

    private static bool IsNestedJsonProperty(string propertyName)
    {
        return propertyName.Equals("requestJson", StringComparison.OrdinalIgnoreCase) ||
               propertyName.Equals("configJson", StringComparison.OrdinalIgnoreCase) ||
               propertyName.Equals("rawPayloadJson", StringComparison.OrdinalIgnoreCase);
    }

    private static string ReadString(JsonElement element, params string[] paths)
    {
        foreach (var path in paths)
        {
            if (TryReadPath(element, path, out var value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static Guid? ReadGuid(JsonElement element, params string[] paths)
    {
        var value = ReadString(element, paths);
        return Guid.TryParse(value, out var guid) ? guid : null;
    }

    private static List<Guid> ReadGuidList(JsonElement element, params string[] paths)
    {
        foreach (var path in paths)
        {
            if (!TryResolvePath(element, path, out var current))
            {
                continue;
            }

            if (current.ValueKind == JsonValueKind.Array)
            {
                var parsedItems = new List<Guid>();
                foreach (var item in current.EnumerateArray())
                {
                    var value = item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString();
                    if (Guid.TryParse(value, out var guid))
                    {
                        parsedItems.Add(guid);
                    }
                }

                return parsedItems.Distinct().ToList();
            }

            var raw = current.ValueKind == JsonValueKind.String ? current.GetString() : current.ToString();
            var values = raw?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];
            var parsed = new List<Guid>();
            foreach (var item in values)
            {
                if (Guid.TryParse(item, out var guid))
                {
                    parsed.Add(guid);
                }
            }

            parsed = parsed.Distinct().ToList();
            if (parsed.Count > 0)
            {
                return parsed;
            }
        }

        return [];
    }

    private static DateTime? ReadDateTime(JsonElement element, params string[] paths)
    {
        var value = ReadString(element, paths);
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTime.TryParse(value, out var dateTime))
        {
            return dateTime;
        }

        if (long.TryParse(value, out var timestamp))
        {
            return timestamp > 10_000_000_000
                ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime
                : DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        }

        return null;
    }

    private static bool TryReadPath(JsonElement root, string path, out string value)
    {
        value = string.Empty;
        if (!TryResolvePath(root, path, out var current))
        {
            return false;
        }

        value = current.ValueKind switch
        {
            JsonValueKind.String => current.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => current.ToString().Trim(),
            _ => string.Empty
        };
        return !string.IsNullOrWhiteSpace(value);
    }

    private static bool TryResolvePath(JsonElement root, string path, out JsonElement value)
    {
        value = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (value.ValueKind != JsonValueKind.Object || !TryGetProperty(value, segment, out value))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryGetProperty(JsonElement element, string name, out JsonElement value)
    {
        if (element.TryGetProperty(name, out value))
        {
            return true;
        }

        foreach (var property in element.EnumerateObject())
        {
            if (property.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
