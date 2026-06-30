using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Integration;

public class IntegrationNotificationService : ITransientDependency
{
    private const string SensitiveExternalValue = "[敏感字段已隐藏]";

    private static readonly Regex PlaceholderRegex = new(@"\{\{\s*(?<name>[a-zA-Z0-9_.-]+)\s*\}\}", RegexOptions.Compiled);
    private static readonly string[] SensitiveKeyFragments =
    [
        "amount",
        "budgetamount",
        "collectionamount",
        "contractamount",
        "costamount",
        "fee",
        "grossprofit",
        "invoiceamount",
        "paymentamount",
        "price",
        "procurementamount",
        "profit",
        "receivableamount",
        "receivableforecast",
        "receivedamount",
        "revenueamount",
        "金额",
        "成本额",
        "发票金额",
        "合同额",
        "利润",
        "回款金额",
        "收入金额",
        "应收金额",
        "预算额",
        "采购金额"
    ];

    private sealed class NotificationDispatchTarget
    {
        public string TargetId { get; init; } = string.Empty;

        public string TargetType { get; init; } = string.Empty;
    }

    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IntegrationDispatchService _dispatchService;
    private readonly IRepository<IntegrationGroupBinding, Guid> _groupBindingRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<IntegrationNotificationRule, Guid> _ruleRepository;
    private readonly IRepository<IntegrationMessageTemplate, Guid> _templateRepository;
    private readonly IRepository<IntegrationUserBinding, Guid> _userBindingRepository;

    public IntegrationNotificationService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IRepository<IntegrationConnector, Guid> connectorRepository,
        ICurrentTenant currentTenant,
        IntegrationDispatchService dispatchService,
        IRepository<IntegrationGroupBinding, Guid> groupBindingRepository,
        IGuidGenerator guidGenerator,
        IRepository<IntegrationNotificationRule, Guid> ruleRepository,
        IRepository<IntegrationMessageTemplate, Guid> templateRepository,
        IRepository<IntegrationUserBinding, Guid> userBindingRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _connectorRepository = connectorRepository;
        _currentTenant = currentTenant;
        _dispatchService = dispatchService;
        _groupBindingRepository = groupBindingRepository;
        _guidGenerator = guidGenerator;
        _ruleRepository = ruleRepository;
        _templateRepository = templateRepository;
        _userBindingRepository = userBindingRepository;
    }

    public async Task<PlatformPagedResultDto<IntegrationMessageTemplateDto>> GetTemplatesAsync(IntegrationMessageTemplateQueryInput input)
    {
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 100);
        var skipCount = Math.Max(input.SkipCount, 0);
        var queryable = await _templateRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.TemplateCode.Contains(keyword) ||
                x.Name.Contains(keyword) ||
                x.TitleTemplate.Contains(keyword) ||
                x.BodyTemplate.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(input.MessageType))
        {
            var messageType = input.MessageType.Trim();
            queryable = queryable.Where(x => x.MessageType == messageType);
        }

        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var templates = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount));

        return new PlatformPagedResultDto<IntegrationMessageTemplateDto>
        {
            Items = templates.Select(MapTemplate).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationMessageTemplateDto> SaveTemplateAsync(IntegrationMessageTemplateSaveInput input)
    {
        ValidateJson(input.CardJson, "卡片 JSON 格式不正确。", "{}");
        ValidateJson(input.VariableSchemaJson, "变量 Schema JSON 格式不正确。", "{}");
        ValidateJson(input.PreviewVariablesJson, "预览变量 JSON 格式不正确。", "{}");
        await EnsureTemplateCodeUniqueAsync(input.TemplateCode, input.Id);

        IntegrationMessageTemplate template;
        if (input.Id.HasValue)
        {
            template = await _templateRepository.GetAsync(input.Id.Value);
            template.Update(
                input.TemplateCode,
                input.Name,
                input.MessageType,
                input.TitleTemplate,
                input.BodyTemplate,
                input.CardJson,
                input.VariableSchemaJson,
                input.PreviewVariablesJson,
                input.Status,
                input.Remark);
            await _templateRepository.UpdateAsync(template, autoSave: true);
        }
        else
        {
            template = new IntegrationMessageTemplate(
                _guidGenerator.Create(),
                input.TemplateCode,
                input.Name,
                input.MessageType,
                input.TitleTemplate,
                input.BodyTemplate,
                input.CardJson,
                input.VariableSchemaJson,
                input.PreviewVariablesJson,
                input.Status,
                _currentTenant.Id,
                input.Remark);
            await _templateRepository.InsertAsync(template, autoSave: true);
        }

        return MapTemplate(template);
    }

    public async Task<IntegrationTemplateRenderResultDto> RenderTemplateAsync(IntegrationTemplateRenderInput input)
    {
        var titleTemplate = input.TitleTemplate;
        var bodyTemplate = input.BodyTemplate;
        var cardJson = input.CardJson;
        if (input.TemplateId.HasValue)
        {
            var template = await _templateRepository.GetAsync(input.TemplateId.Value);
            titleTemplate = template.TitleTemplate;
            bodyTemplate = template.BodyTemplate;
            cardJson = template.CardJson;
        }

        using var variablesDocument = ParseJson(input.VariablesJson, "变量 JSON 格式不正确。", "{}");
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        FlattenVariables(variablesDocument.RootElement, string.Empty, variables);
        var missingVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        return new IntegrationTemplateRenderResultDto
        {
            RenderedTitle = RenderText(titleTemplate, variables, missingVariables),
            RenderedBody = RenderText(bodyTemplate, variables, missingVariables),
            RenderedCardJson = RenderText(cardJson, variables, missingVariables),
            MissingVariables = missingVariables.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    public async Task<PlatformPagedResultDto<IntegrationNotificationRuleDto>> GetRulesAsync(IntegrationNotificationRuleQueryInput input)
    {
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 100);
        var skipCount = Math.Max(input.SkipCount, 0);
        var queryable = await _ruleRepository.GetQueryableAsync();

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.RuleCode.Contains(keyword) ||
                x.Name.Contains(keyword) ||
                x.EventCode.Contains(keyword) ||
                x.EventName.Contains(keyword) ||
                x.ConnectorCode.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(input.EventCode))
        {
            var eventCode = input.EventCode.Trim();
            queryable = queryable.Where(x => x.EventCode == eventCode);
        }

        if (input.ConnectorId.HasValue)
        {
            queryable = queryable.Where(x => x.ConnectorId == input.ConnectorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var rules = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(skipCount)
            .Take(maxResultCount));
        var templateIds = rules.Select(x => x.TemplateId).Distinct().ToList();
        var templates = (await _templateRepository.GetListAsync(x => templateIds.Contains(x.Id)))
            .ToDictionary(x => x.Id);

        return new PlatformPagedResultDto<IntegrationNotificationRuleDto>
        {
            Items = rules.Select(x => MapRule(x, templates.GetValueOrDefault(x.TemplateId))).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationNotificationRuleDto> SaveRuleAsync(IntegrationNotificationRuleSaveInput input)
    {
        ValidateJson(input.RecipientRuleJson, "接收人规则 JSON 格式不正确。", "{}");
        ValidateJson(input.ConditionJson, "触发条件 JSON 格式不正确。", "{}");
        await EnsureRuleCodeUniqueAsync(input.RuleCode, input.Id);

        var template = await _templateRepository.GetAsync(input.TemplateId);
        var connector = input.ConnectorId.HasValue
            ? await _connectorRepository.GetAsync(input.ConnectorId.Value)
            : null;

        IntegrationNotificationRule rule;
        if (input.Id.HasValue)
        {
            rule = await _ruleRepository.GetAsync(input.Id.Value);
            rule.Update(
                input.RuleCode,
                input.Name,
                input.EventCode,
                input.EventName,
                input.TemplateId,
                connector?.Id,
                connector?.ConnectorCode ?? string.Empty,
                connector?.ProviderType ?? string.Empty,
                input.ChannelType,
                input.TargetType,
                input.RecipientRuleJson,
                input.ConditionJson,
                input.QuietStartTime,
                input.QuietEndTime,
                input.RateLimitWindowMinutes,
                input.RateLimitMaxCount,
                input.Status,
                input.Remark);
            await _ruleRepository.UpdateAsync(rule, autoSave: true);
        }
        else
        {
            rule = new IntegrationNotificationRule(
                _guidGenerator.Create(),
                input.RuleCode,
                input.Name,
                input.EventCode,
                input.EventName,
                input.TemplateId,
                connector?.Id,
                connector?.ConnectorCode ?? string.Empty,
                connector?.ProviderType ?? string.Empty,
                input.ChannelType,
                input.TargetType,
                input.RecipientRuleJson,
                input.ConditionJson,
                input.QuietStartTime,
                input.QuietEndTime,
                input.RateLimitWindowMinutes,
                input.RateLimitMaxCount,
                input.Status,
                _currentTenant.Id,
                input.Remark);
            await _ruleRepository.InsertAsync(rule, autoSave: true);
        }

        return MapRule(rule, template);
    }

    public async Task<IntegrationNotificationRuleDto> ToggleRuleAsync(Guid id, IntegrationNotificationRuleToggleInput input)
    {
        var rule = await _ruleRepository.GetAsync(id);
        rule.Toggle(input.Status);
        await _ruleRepository.UpdateAsync(rule, autoSave: true);
        var template = await _templateRepository.FindAsync(rule.TemplateId);
        return MapRule(rule, template);
    }

    public async Task<IntegrationNotificationEvaluationResultDto> EvaluateEventAsync(IntegrationNotificationEventInput input)
    {
        var eventCode = Check.NotNullOrWhiteSpace(input.EventCode, nameof(input.EventCode), 128).Trim();
        var occurredAt = input.OccurredAt ?? DateTime.UtcNow;
        using var variablesDocument = ParseJson(input.VariablesJson, "事件变量 JSON 格式不正确。", "{}");
        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        FlattenVariables(variablesDocument.RootElement, string.Empty, variables);

        var queryable = await _ruleRepository.GetQueryableAsync();
        var rules = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(x => x.EventCode == eventCode && x.Status == IntegrationNotificationRuleStatus.Active)
            .OrderBy(x => x.CreationTime));
        var templateIds = rules.Select(x => x.TemplateId).Distinct().ToList();
        var templates = (await _templateRepository.GetListAsync(x => templateIds.Contains(x.Id)))
            .ToDictionary(x => x.Id);
        var items = new List<IntegrationNotificationRuleEvaluationDto>();

        foreach (var rule in rules)
        {
            var template = templates.GetValueOrDefault(rule.TemplateId);
            var blockReason = ResolveBlockReason(rule, template, variablesDocument.RootElement, occurredAt);
            var missingVariables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var canSend = string.IsNullOrWhiteSpace(blockReason);
            var renderedBody = template == null ? string.Empty : RenderText(template.BodyTemplate, variables, missingVariables);
            var renderedCardJson = template == null ? "{}" : RenderText(template.CardJson, variables, missingVariables);
            var renderedTitle = template == null ? string.Empty : RenderText(template.TitleTemplate, variables, missingVariables);
            items.Add(new IntegrationNotificationRuleEvaluationDto
            {
                BlockReason = blockReason,
                CanSend = canSend,
                ChannelType = rule.ChannelType,
                ConnectorCode = rule.ConnectorCode,
                ConnectorId = rule.ConnectorId,
                MissingVariables = missingVariables.OrderBy(x => x, StringComparer.OrdinalIgnoreCase).ToList(),
                RenderedBody = renderedBody,
                RenderedCardJson = renderedCardJson,
                RenderedTitle = renderedTitle,
                RuleCode = rule.RuleCode,
                RuleId = rule.Id,
                RuleName = rule.Name,
                TargetType = rule.TargetType,
                TemplateId = rule.TemplateId,
                TemplateName = template?.Name ?? string.Empty
            });

            if (input.CommitTriggered && canSend)
            {
                rule.MarkTriggered(occurredAt);
                await _ruleRepository.UpdateAsync(rule, autoSave: true);
            }
        }

        return new IntegrationNotificationEvaluationResultDto
        {
            BlockedCount = items.Count(x => !x.CanSend),
            EventCode = eventCode,
            Items = items,
            ReadyCount = items.Count(x => x.CanSend),
            TotalRuleCount = items.Count
        };
    }

    public async Task<IntegrationNotificationDispatchResultDto> DispatchEventAsync(IntegrationNotificationEventInput input)
    {
        var occurredAt = input.OccurredAt ?? DateTime.UtcNow;
        var evaluation = await EvaluateEventAsync(new IntegrationNotificationEventInput
        {
            CommitTriggered = false,
            EventCode = input.EventCode,
            OccurredAt = occurredAt,
            VariablesJson = input.VariablesJson
        });
        using var variablesDocument = ParseJson(input.VariablesJson, "事件变量 JSON 格式不正确。", "{}");
        var items = new List<IntegrationNotificationDispatchItemDto>();

        foreach (var evaluationItem in evaluation.Items)
        {
            if (!evaluationItem.CanSend)
            {
                items.Add(new IntegrationNotificationDispatchItemDto
                {
                    BlockReason = evaluationItem.BlockReason,
                    Message = evaluationItem.BlockReason,
                    RuleCode = evaluationItem.RuleCode,
                    RuleName = evaluationItem.RuleName,
                    Status = "Blocked"
                });
                continue;
            }

            if (evaluationItem.ConnectorId == null)
            {
                items.Add(new IntegrationNotificationDispatchItemDto
                {
                    BlockReason = "通知规则未配置连接器。",
                    Message = "通知规则未配置连接器。",
                    RuleCode = evaluationItem.RuleCode,
                    RuleName = evaluationItem.RuleName,
                    Status = "Skipped"
                });
                continue;
            }

            if (evaluationItem.MissingVariables.Count > 0)
            {
                var message = $"模板缺少变量：{string.Join("、", evaluationItem.MissingVariables)}。";
                items.Add(new IntegrationNotificationDispatchItemDto
                {
                    BlockReason = message,
                    Message = message,
                    RuleCode = evaluationItem.RuleCode,
                    RuleName = evaluationItem.RuleName,
                    Status = "Skipped"
                });
                continue;
            }

            var rule = await _ruleRepository.GetAsync(evaluationItem.RuleId);
            var template = await _templateRepository.GetAsync(evaluationItem.TemplateId);
            var targets = await ResolveDispatchTargetsAsync(rule, variablesDocument.RootElement);
            if (targets.Count == 0)
            {
                items.Add(new IntegrationNotificationDispatchItemDto
                {
                    BlockReason = "未解析到有效接收目标。",
                    Message = "未解析到有效接收目标。",
                    RuleCode = evaluationItem.RuleCode,
                    RuleName = evaluationItem.RuleName,
                    Status = "NoTarget"
                });
                continue;
            }

            var body = BuildDispatchBody(
                template.MessageType,
                evaluationItem.RenderedTitle,
                evaluationItem.RenderedBody,
                evaluationItem.RenderedCardJson);
            var attempted = false;
            foreach (var target in targets)
            {
                try
                {
                    var sendResult = await _dispatchService.SendMessageAsync(new IntegrationMessageSendInput
                    {
                        Body = body,
                        BusinessKey = BuildDispatchBusinessKey(input.EventCode, rule, variablesDocument.RootElement),
                        BusinessModule = "Integration",
                        ConnectorId = evaluationItem.ConnectorId.Value,
                        MessageType = template.MessageType,
                        SourceType = "NotificationRule",
                        TargetId = target.TargetId,
                        TargetType = target.TargetType
                    });
                    attempted = true;
                    items.Add(new IntegrationNotificationDispatchItemDto
                    {
                        DeliveryRecordId = sendResult.DeliveryRecordId,
                        Message = sendResult.Message,
                        RuleCode = evaluationItem.RuleCode,
                        RuleName = evaluationItem.RuleName,
                        Status = sendResult.Success ? "Sent" : "Failed",
                        TargetId = target.TargetId,
                        TargetType = target.TargetType
                    });
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    attempted = true;
                    items.Add(new IntegrationNotificationDispatchItemDto
                    {
                        Message = ex is UserFriendlyException ? ex.Message : $"通知投递失败：{ex.Message}",
                        RuleCode = evaluationItem.RuleCode,
                        RuleName = evaluationItem.RuleName,
                        Status = "Failed",
                        TargetId = target.TargetId,
                        TargetType = target.TargetType
                    });
                }
            }

            if (attempted)
            {
                rule.MarkTriggered(occurredAt);
                await _ruleRepository.UpdateAsync(rule, autoSave: true);
            }
        }

        return new IntegrationNotificationDispatchResultDto
        {
            BlockedCount = items.Count(x => x.Status == "Blocked"),
            FailedCount = items.Count(x => x.Status == "Failed"),
            Items = items,
            NoTargetCount = items.Count(x => x.Status == "NoTarget"),
            SentCount = items.Count(x => x.Status == "Sent"),
            SkippedCount = items.Count(x => x.Status == "Skipped")
        };
    }

    private async Task<IReadOnlyList<NotificationDispatchTarget>> ResolveDispatchTargetsAsync(
        IntegrationNotificationRule rule,
        JsonElement variablesRoot)
    {
        using var recipientRuleDocument = ParseJson(rule.RecipientRuleJson, "接收人规则 JSON 格式不正确。", "{}");
        var root = recipientRuleDocument.RootElement;
        var mode = ReadJsonString(root, "mode");
        if (string.IsNullOrWhiteSpace(mode))
        {
            mode = rule.ChannelType.Equals(IntegrationNotificationChannelTypes.Group, StringComparison.OrdinalIgnoreCase)
                ? "ProjectGroup"
                : "Direct";
        }

        var targets = new List<NotificationDispatchTarget>();
        if (mode.Equals("Mixed", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(ReadDirectTargets(rule, root, variablesRoot));
            targets.AddRange(await ReadInternalUserTargetsAsync(rule, root, variablesRoot));
            targets.AddRange(await ReadProjectGroupTargetsAsync(rule, root, variablesRoot));
        }
        else if (mode.Equals("InternalUsers", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(await ReadInternalUserTargetsAsync(rule, root, variablesRoot));
        }
        else if (mode.Equals("ProjectGroup", StringComparison.OrdinalIgnoreCase) ||
                 mode.Equals("GroupBinding", StringComparison.OrdinalIgnoreCase))
        {
            targets.AddRange(await ReadProjectGroupTargetsAsync(rule, root, variablesRoot));
        }
        else
        {
            targets.AddRange(ReadDirectTargets(rule, root, variablesRoot));
        }

        return targets
            .Where(x => !string.IsNullOrWhiteSpace(x.TargetId))
            .GroupBy(x => $"{x.TargetType}:{x.TargetId}", StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToList();
    }

    private static List<NotificationDispatchTarget> ReadDirectTargets(
        IntegrationNotificationRule rule,
        JsonElement recipientRuleRoot,
        JsonElement variablesRoot)
    {
        var targetType = ResolveRuleTargetType(rule, ReadJsonString(recipientRuleRoot, "targetType"));
        var targetIds = new List<string>();
        AddJsonValues(targetIds, recipientRuleRoot, "targetId");
        AddJsonValues(targetIds, recipientRuleRoot, "targetIds");

        var targetPath = ReadJsonString(recipientRuleRoot, "targetPath");
        if (!string.IsNullOrWhiteSpace(targetPath) && TryReadPath(variablesRoot, targetPath, out var targetElement))
        {
            AddJsonValues(targetIds, targetElement);
        }

        return targetIds
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(targetId => new NotificationDispatchTarget
            {
                TargetId = targetId,
                TargetType = targetType
            })
            .ToList();
    }

    private async Task<List<NotificationDispatchTarget>> ReadInternalUserTargetsAsync(
        IntegrationNotificationRule rule,
        JsonElement recipientRuleRoot,
        JsonElement variablesRoot)
    {
        if (rule.ConnectorId == null)
        {
            return [];
        }

        var userIds = ReadUserIds(recipientRuleRoot, variablesRoot);
        if (userIds.Count == 0)
        {
            return [];
        }

        var bindings = await _userBindingRepository.GetListAsync(x =>
            x.ConnectorId == rule.ConnectorId.Value &&
            x.Status == IntegrationBindingStatus.Bound);
        var targetType = ResolveRuleTargetType(rule, ReadJsonString(recipientRuleRoot, "targetType"));
        return bindings
            .Where(x => x.InternalUserId.HasValue && userIds.Contains(x.InternalUserId.Value))
            .Where(x => !string.IsNullOrWhiteSpace(x.ExternalUserId))
            .Select(x => new NotificationDispatchTarget
            {
                TargetId = x.ExternalUserId,
                TargetType = targetType
            })
            .ToList();
    }

    private async Task<List<NotificationDispatchTarget>> ReadProjectGroupTargetsAsync(
        IntegrationNotificationRule rule,
        JsonElement recipientRuleRoot,
        JsonElement variablesRoot)
    {
        if (rule.ConnectorId == null)
        {
            return [];
        }

        var objectType = ReadJsonString(recipientRuleRoot, "objectType");
        objectType = string.IsNullOrWhiteSpace(objectType) ? "Project" : objectType.Trim();
        var projectId = ReadJsonString(recipientRuleRoot, "projectId");
        if (string.IsNullOrWhiteSpace(projectId))
        {
            var projectIdPath = ReadJsonString(recipientRuleRoot, "projectIdPath");
            projectIdPath = string.IsNullOrWhiteSpace(projectIdPath) ? "project.id" : projectIdPath.Trim();
            if (TryReadPath(variablesRoot, projectIdPath, out var projectIdElement))
            {
                projectId = ReadComparableValue(projectIdElement);
            }
        }

        if (string.IsNullOrWhiteSpace(projectId))
        {
            return [];
        }

        var useRobotCode = ReadJsonString(recipientRuleRoot, "useRobotCode")
            .Equals("true", StringComparison.OrdinalIgnoreCase);
        var targetTypeOverride = ReadJsonString(recipientRuleRoot, "targetType");
        var bindings = await _groupBindingRepository.GetListAsync(x =>
            x.ConnectorId == rule.ConnectorId.Value &&
            x.InternalObjectType == objectType &&
            x.InternalObjectId == projectId &&
            x.Status == IntegrationBindingStatus.Bound);

        return bindings
            .Select(x =>
            {
                var targetId = useRobotCode && !string.IsNullOrWhiteSpace(x.RobotCode)
                    ? x.RobotCode
                    : x.ExternalGroupId;
                var targetType = ResolveRuleTargetType(rule, targetTypeOverride);
                if (string.IsNullOrWhiteSpace(targetTypeOverride))
                {
                    targetType = string.IsNullOrWhiteSpace(x.GroupType) ? "Group" : x.GroupType;
                }

                return new NotificationDispatchTarget
                {
                    TargetId = targetId,
                    TargetType = targetType
                };
            })
            .ToList();
    }

    private static HashSet<Guid> ReadUserIds(JsonElement recipientRuleRoot, JsonElement variablesRoot)
    {
        var userIds = new HashSet<Guid>();
        var rawValues = new List<string>();
        AddJsonValues(rawValues, recipientRuleRoot, "userId");
        AddJsonValues(rawValues, recipientRuleRoot, "userIds");

        foreach (var pathProperty in new[] { "userIdPath", "userIdsPath" })
        {
            var path = ReadJsonString(recipientRuleRoot, pathProperty);
            if (!string.IsNullOrWhiteSpace(path) && TryReadPath(variablesRoot, path, out var element))
            {
                AddJsonValues(rawValues, element);
            }
        }

        if (rawValues.Count == 0)
        {
            foreach (var fallbackPath in new[] { "recipient.userIds", "recipient.userId", "task.ownerUserId", "risk.ownerUserId", "ownerUserId" })
            {
                if (TryReadPath(variablesRoot, fallbackPath, out var element))
                {
                    AddJsonValues(rawValues, element);
                }
            }
        }

        foreach (var rawValue in rawValues)
        {
            if (Guid.TryParse(rawValue, out var userId))
            {
                userIds.Add(userId);
            }
        }

        return userIds;
    }

    private static void AddJsonValues(List<string> values, JsonElement root, string propertyName)
    {
        if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(propertyName, out var element))
        {
            AddJsonValues(values, element);
        }
    }

    private static void AddJsonValues(List<string> values, JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                {
                    AddJsonValues(values, item);
                }
                break;
            case JsonValueKind.String:
                var stringValue = element.GetString();
                if (!string.IsNullOrWhiteSpace(stringValue))
                {
                    values.Add(stringValue.Trim());
                }
                break;
            case JsonValueKind.Number:
            case JsonValueKind.True:
            case JsonValueKind.False:
                values.Add(element.GetRawText());
                break;
        }
    }

    private static string ReadJsonString(JsonElement root, string propertyName)
    {
        if (root.ValueKind != JsonValueKind.Object || !root.TryGetProperty(propertyName, out var value))
        {
            return string.Empty;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString()?.Trim() ?? string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Number => value.GetRawText(),
            _ => string.Empty
        };
    }

    private static string ResolveRuleTargetType(IntegrationNotificationRule rule, string targetTypeOverride)
    {
        if (!string.IsNullOrWhiteSpace(targetTypeOverride))
        {
            return targetTypeOverride.Trim();
        }

        return string.IsNullOrWhiteSpace(rule.TargetType) ? "OpenId" : rule.TargetType;
    }

    private static string BuildDispatchBusinessKey(
        string eventCode,
        IntegrationNotificationRule rule,
        JsonElement variablesRoot)
    {
        foreach (var path in new[] { "business.key", "businessKey", "project.id", "task.id", "risk.id", "meeting.id" })
        {
            if (TryReadPath(variablesRoot, path, out var value))
            {
                var key = ReadComparableValue(value);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    return key.Length <= 128 ? key : key[..128];
                }
            }
        }

        var fallback = $"{eventCode}:{rule.RuleCode}";
        return fallback.Length <= 128 ? fallback : fallback[..128];
    }

    private static string BuildDispatchBody(
        string messageType,
        string renderedTitle,
        string renderedBody,
        string renderedCardJson)
    {
        if (messageType.Equals(IntegrationMessageTemplateTypes.Card, StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(renderedCardJson) ? "{}" : renderedCardJson;
        }

        if (string.IsNullOrWhiteSpace(renderedTitle))
        {
            return renderedBody;
        }

        if (string.IsNullOrWhiteSpace(renderedBody))
        {
            return renderedTitle;
        }

        return $"{renderedTitle}{Environment.NewLine}{Environment.NewLine}{renderedBody}";
    }

    private async Task EnsureTemplateCodeUniqueAsync(string templateCode, Guid? id)
    {
        var normalizedCode = Check.NotNullOrWhiteSpace(templateCode, nameof(templateCode), 64).Trim();
        var duplicate = await _templateRepository.FirstOrDefaultAsync(x => x.TemplateCode == normalizedCode);
        if (duplicate != null && (!id.HasValue || duplicate.Id != id.Value))
        {
            throw new UserFriendlyException("消息模板编码已存在。");
        }
    }

    private async Task EnsureRuleCodeUniqueAsync(string ruleCode, Guid? id)
    {
        var normalizedCode = Check.NotNullOrWhiteSpace(ruleCode, nameof(ruleCode), 64).Trim();
        var duplicate = await _ruleRepository.FirstOrDefaultAsync(x => x.RuleCode == normalizedCode);
        if (duplicate != null && (!id.HasValue || duplicate.Id != id.Value))
        {
            throw new UserFriendlyException("通知规则编码已存在。");
        }
    }

    private static void ValidateJson(string value, string errorMessage, string defaultValue)
    {
        using var _ = ParseJson(value, errorMessage, defaultValue);
    }

    private static string ResolveBlockReason(
        IntegrationNotificationRule rule,
        IntegrationMessageTemplate? template,
        JsonElement variablesRoot,
        DateTime occurredAt)
    {
        if (template == null)
        {
            return "消息模板不存在。";
        }

        if (template.Status != IntegrationMessageTemplateStatus.Active)
        {
            return "消息模板未启用。";
        }

        if (!IsConditionMatched(rule.ConditionJson, variablesRoot))
        {
            return "触发条件不满足。";
        }

        if (IsInQuietPeriod(rule.QuietStartTime, rule.QuietEndTime, occurredAt.TimeOfDay))
        {
            return "当前处于规则静默期。";
        }

        if (IsRateLimited(rule, occurredAt))
        {
            return $"规则在 {rule.RateLimitWindowMinutes} 分钟频控窗口内已触发。";
        }

        return string.Empty;
    }

    private static bool IsConditionMatched(string conditionJson, JsonElement variablesRoot)
    {
        using var conditionDocument = ParseJson(conditionJson, "触发条件 JSON 格式不正确。", "{}");
        var root = conditionDocument.RootElement;
        if (root.ValueKind != JsonValueKind.Object || !root.EnumerateObject().Any())
        {
            return true;
        }

        return MatchEquals(root, variablesRoot, "equals", expectedEqual: true) &&
               MatchEquals(root, variablesRoot, "notEquals", expectedEqual: false) &&
               MatchContains(root, variablesRoot) &&
               MatchDecimalBoundary(root, variablesRoot, "min", isMin: true) &&
               MatchDecimalBoundary(root, variablesRoot, "max", isMin: false);
    }

    private static bool MatchEquals(JsonElement conditionRoot, JsonElement variablesRoot, string conditionName, bool expectedEqual)
    {
        if (!conditionRoot.TryGetProperty(conditionName, out var condition) || condition.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        foreach (var property in condition.EnumerateObject())
        {
            if (!TryReadPath(variablesRoot, property.Name, out var actual))
            {
                return false;
            }

            var actualText = ReadComparableValue(actual);
            var expectedText = ReadComparableValue(property.Value);
            var isEqual = actualText.Equals(expectedText, StringComparison.OrdinalIgnoreCase);
            if (isEqual != expectedEqual)
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchContains(JsonElement conditionRoot, JsonElement variablesRoot)
    {
        if (!conditionRoot.TryGetProperty("contains", out var condition) || condition.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        foreach (var property in condition.EnumerateObject())
        {
            if (!TryReadPath(variablesRoot, property.Name, out var actual))
            {
                return false;
            }

            var actualText = ReadComparableValue(actual);
            var expectedText = ReadComparableValue(property.Value);
            if (!actualText.Contains(expectedText, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static bool MatchDecimalBoundary(JsonElement conditionRoot, JsonElement variablesRoot, string conditionName, bool isMin)
    {
        if (!conditionRoot.TryGetProperty(conditionName, out var condition) || condition.ValueKind != JsonValueKind.Object)
        {
            return true;
        }

        foreach (var property in condition.EnumerateObject())
        {
            if (!TryReadPath(variablesRoot, property.Name, out var actual) ||
                !TryReadDecimal(actual, out var actualNumber) ||
                !TryReadDecimal(property.Value, out var expectedNumber))
            {
                return false;
            }

            if (isMin && actualNumber < expectedNumber)
            {
                return false;
            }

            if (!isMin && actualNumber > expectedNumber)
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryReadPath(JsonElement root, string path, out JsonElement value)
    {
        value = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (value.ValueKind != JsonValueKind.Object || !value.TryGetProperty(segment, out value))
            {
                return false;
            }
        }

        return true;
    }

    private static string ReadComparableValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Undefined => string.Empty,
            _ => element.GetRawText()
        };
    }

    private static bool TryReadDecimal(JsonElement element, out decimal value)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetDecimal(out value);
        }

        if (element.ValueKind == JsonValueKind.String)
        {
            return decimal.TryParse(element.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        value = 0;
        return false;
    }

    private static bool IsInQuietPeriod(string quietStartTime, string quietEndTime, TimeSpan currentTime)
    {
        if (!TryParseClockTime(quietStartTime, out var start) || !TryParseClockTime(quietEndTime, out var end) || start == end)
        {
            return false;
        }

        return start < end
            ? currentTime >= start && currentTime < end
            : currentTime >= start || currentTime < end;
    }

    private static bool IsRateLimited(IntegrationNotificationRule rule, DateTime occurredAt)
    {
        if (rule.RateLimitWindowMinutes <= 0 ||
            rule.RateLimitMaxCount <= 0 ||
            rule.RateLimitMaxCount > 1 ||
            !rule.LastTriggeredTime.HasValue)
        {
            return false;
        }

        return rule.LastTriggeredTime.Value >= occurredAt.AddMinutes(-rule.RateLimitWindowMinutes);
    }

    private static bool TryParseClockTime(string value, out TimeSpan time)
    {
        return TimeSpan.TryParseExact(value?.Trim(), ["h\\:mm", "hh\\:mm", "h\\:mm\\:ss", "hh\\:mm\\:ss"], CultureInfo.InvariantCulture, out time);
    }

    private static JsonDocument ParseJson(string? value, string errorMessage, string defaultValue)
    {
        try
        {
            return JsonDocument.Parse(string.IsNullOrWhiteSpace(value) ? defaultValue : value);
        }
        catch (JsonException)
        {
            throw new UserFriendlyException(errorMessage);
        }
    }

    private static void FlattenVariables(JsonElement element, string prefix, IDictionary<string, string> variables)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var key = string.IsNullOrWhiteSpace(prefix) ? property.Name : $"{prefix}.{property.Name}";
                FlattenVariables(property.Value, key, variables);
            }

            if (!string.IsNullOrWhiteSpace(prefix) && !variables.ContainsKey(prefix))
            {
                variables[prefix] = SanitizeJsonVariable(prefix, element);
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            return;
        }

        var value = element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Undefined => string.Empty,
            _ => element.GetRawText()
        };
        variables[prefix] = SanitizeExternalVariable(prefix, value);
    }

    internal static string SanitizeExternalVariable(string key, string value)
    {
        return IsSensitiveExternalKey(key) ? SensitiveExternalValue : value;
    }

    internal static string SanitizeJsonVariable(string key, JsonElement element)
    {
        if (IsSensitiveExternalKey(key))
        {
            return SensitiveExternalValue;
        }

        var node = JsonNode.Parse(element.GetRawText());
        SanitizeJsonNode(node);
        return node?.ToJsonString() ?? "{}";
    }

    private static void SanitizeJsonNode(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject jsonObject:
                foreach (var property in jsonObject.ToList())
                {
                    if (IsSensitiveExternalKey(property.Key))
                    {
                        jsonObject[property.Key] = SensitiveExternalValue;
                        continue;
                    }

                    SanitizeJsonNode(property.Value);
                }
                break;
            case JsonArray jsonArray:
                foreach (var item in jsonArray)
                {
                    SanitizeJsonNode(item);
                }
                break;
        }
    }

    private static bool IsSensitiveExternalKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        var normalized = key.Replace("_", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
        return SensitiveKeyFragments.Any(fragment => normalized.Contains(fragment, StringComparison.OrdinalIgnoreCase));
    }

    private static string RenderText(
        string template,
        IReadOnlyDictionary<string, string> variables,
        ISet<string> missingVariables)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        return PlaceholderRegex.Replace(template, match =>
        {
            var name = match.Groups["name"].Value;
            if (variables.TryGetValue(name, out var value))
            {
                return value;
            }

            missingVariables.Add(name);
            return match.Value;
        });
    }

    private static IntegrationMessageTemplateDto MapTemplate(IntegrationMessageTemplate template)
    {
        return new IntegrationMessageTemplateDto
        {
            BodyTemplate = template.BodyTemplate,
            CardJson = template.CardJson,
            CreationTime = template.CreationTime,
            Id = template.Id,
            MessageType = template.MessageType,
            Name = template.Name,
            PreviewVariablesJson = template.PreviewVariablesJson,
            Remark = template.Remark,
            Status = template.Status,
            TemplateCode = template.TemplateCode,
            TitleTemplate = template.TitleTemplate,
            VariableSchemaJson = template.VariableSchemaJson
        };
    }

    private static IntegrationNotificationRuleDto MapRule(
        IntegrationNotificationRule rule,
        IntegrationMessageTemplate? template)
    {
        return new IntegrationNotificationRuleDto
        {
            ChannelType = rule.ChannelType,
            ConditionJson = rule.ConditionJson,
            ConnectorCode = rule.ConnectorCode,
            ConnectorId = rule.ConnectorId,
            CreationTime = rule.CreationTime,
            EventCode = rule.EventCode,
            EventName = rule.EventName,
            Id = rule.Id,
            LastTriggeredTime = rule.LastTriggeredTime,
            Name = rule.Name,
            ProviderType = rule.ProviderType,
            QuietEndTime = rule.QuietEndTime,
            QuietStartTime = rule.QuietStartTime,
            RateLimitMaxCount = rule.RateLimitMaxCount,
            RateLimitWindowMinutes = rule.RateLimitWindowMinutes,
            RecipientRuleJson = rule.RecipientRuleJson,
            Remark = rule.Remark,
            RuleCode = rule.RuleCode,
            Status = rule.Status,
            TargetType = rule.TargetType,
            TemplateCode = template?.TemplateCode ?? string.Empty,
            TemplateId = rule.TemplateId,
            TemplateName = template?.Name ?? string.Empty
        };
    }
}
