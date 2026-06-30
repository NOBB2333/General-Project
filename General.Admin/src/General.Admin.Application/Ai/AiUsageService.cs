using System.Text.Json;
using System.Text.RegularExpressions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Ai;

public class AiUsageService : ITransientDependency
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly Regex ApiKeyRegex = new(@"(?i)(api[_-]?key|authorization|bearer|token|password|secret)\s*[:=]\s*[""']?[^""'\s,}]+", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new(@"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}", RegexOptions.Compiled);
    private static readonly Regex IdCardRegex = new(@"\b\d{17}[\dXx]\b", RegexOptions.Compiled);
    private static readonly Regex PhoneRegex = new(@"\b1[3-9]\d{9}\b", RegexOptions.Compiled);

    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<AiModel, Guid> _modelRepository;
    private readonly IRepository<AiProvider, Guid> _providerRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<AiUsageRecord, Guid> _usageRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public AiUsageService(
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IGuidGenerator guidGenerator,
        IRepository<AiModel, Guid> modelRepository,
        IRepository<AiProvider, Guid> providerRepository,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<AiUsageRecord, Guid> usageRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _guidGenerator = guidGenerator;
        _modelRepository = modelRepository;
        _providerRepository = providerRepository;
        _projectRepository = projectRepository;
        _usageRepository = usageRepository;
        _userRepository = userRepository;
    }

    public async Task<AiUsageQuotaCheckResultDto> CheckQuotaAsync(AiUsageQuotaCheckInput input)
    {
        var model = input.ModelId.HasValue ? await _modelRepository.FindAsync(input.ModelId.Value) : null;
        var provider = model == null ? null : await _providerRepository.FindAsync(model.ProviderId);
        var now = DateTime.UtcNow;
        var today = now.Date;
        var dailyRecords = (await _usageRepository.GetListAsync())
            .Where(x => x.CreationTime >= today)
            .ToList();
        var config = ReadProviderQuotaConfig(provider?.ConfigJson);
        var warnings = new List<string>();
        var tenantCost = dailyRecords.Sum(x => x.CostAmount);
        var userId = input.UserId ?? _currentUser.Id;
        var userCost = userId.HasValue
            ? dailyRecords.Where(x => x.UserId == userId.Value).Sum(x => x.CostAmount)
            : 0m;
        var modelCost = model == null
            ? 0m
            : dailyRecords.Where(x => x.ModelId == model.Id).Sum(x => x.CostAmount);

        if (config.DailyTenantCostLimit > 0 && tenantCost >= config.DailyTenantCostLimit)
        {
            warnings.Add($"租户今日 AI 成本已达到限额 {config.DailyTenantCostLimit:0.####} {config.Currency}。");
        }

        if (config.DailyUserCostLimit > 0 && userCost >= config.DailyUserCostLimit)
        {
            warnings.Add($"当前用户今日 AI 成本已达到限额 {config.DailyUserCostLimit:0.####} {config.Currency}。");
        }

        if (config.DailyModelCostLimit > 0 && modelCost >= config.DailyModelCostLimit)
        {
            warnings.Add($"模型 {model?.DisplayName ?? "-"} 今日成本已达到限额 {config.DailyModelCostLimit:0.####} {config.Currency}。");
        }

        var injection = DetectPromptInjection(input.RequestPreview);
        if (injection.RiskScore >= 80)
        {
            warnings.Add("输入疑似包含 Prompt Injection 或越权指令，已拦截模型调用。");
        }

        return new AiUsageQuotaCheckResultDto
        {
            Allowed = warnings.Count == 0 || injection.RiskScore < 80 &&
                !warnings.Any(x => x.Contains("已达到限额", StringComparison.OrdinalIgnoreCase)),
            PromptInjectionRiskScore = injection.RiskScore,
            PromptInjectionSignals = injection.Signals,
            Warnings = warnings
        };
    }

    public async Task<AiUsageRecordDto> RecordUsageAsync(AiUsageRecordInput input)
    {
        var model = input.ModelId.HasValue ? await _modelRepository.FindAsync(input.ModelId.Value) : null;
        var provider = input.ProviderId.HasValue ? await _providerRepository.FindAsync(input.ProviderId.Value) : null;
        provider ??= model == null ? null : await _providerRepository.FindAsync(model.ProviderId);
        var promptTokens = Math.Max(0, input.PromptTokens);
        var completionTokens = Math.Max(0, input.CompletionTokens);
        var cost = CalculateCost(model, promptTokens, completionTokens);
        var injection = DetectPromptInjection(input.RequestPreview);
        var config = ReadProviderQuotaConfig(provider?.ConfigJson);
        var record = new AiUsageRecord(
            _guidGenerator.Create(),
            input.Source,
            input.Status,
            _currentTenant.Id,
            input.UserId ?? _currentUser.Id,
            input.ProjectId,
            input.ConversationId,
            input.JobId,
            provider?.Id ?? input.ProviderId,
            model?.Id ?? input.ModelId,
            model?.ModelCode,
            input.ToolCode,
            input.Scenario,
            input.PromptTemplateCode,
            promptTokens,
            completionTokens,
            cost,
            model?.Currency ?? config.Currency,
            input.ElapsedMilliseconds,
            RedactSensitive(input.RequestPreview),
            RedactSensitive(input.ResponsePreview),
            BuildAuditSummary(input, cost, model, provider, injection),
            RedactSensitive(input.ErrorMessage),
            injection.RiskScore,
            JsonSerializer.Serialize(injection.Signals, JsonOptions));
        await _usageRepository.InsertAsync(record, autoSave: true);
        return MapRecord(
            record,
            model == null ? [] : new Dictionary<Guid, AiModel> { [model.Id] = model },
            provider == null ? [] : new Dictionary<Guid, AiProvider> { [provider.Id] = provider },
            await ResolveUserNamesAsync([record.UserId]));
    }

    public async Task<List<AiUsageRecordDto>> GetRecordsAsync(AiUsageQueryInput input)
    {
        var records = await ApplyQueryAsync(input);
        var models = await ResolveModelsAsync(records.Select(x => x.ModelId));
        var providers = await ResolveProvidersAsync(records.Select(x => x.ProviderId));
        var users = await ResolveUserNamesAsync(records.Select(x => x.UserId));
        return records
            .OrderByDescending(x => x.CreationTime)
            .Take(Math.Clamp(input.MaxResultCount, 1, 500))
            .Select(item => MapRecord(item, models, providers, users))
            .ToList();
    }

    public async Task<AiUsageDashboardDto> GetDashboardAsync(AiUsageQueryInput input)
    {
        var records = await ApplyQueryAsync(input);
        var models = await ResolveModelsAsync(records.Select(x => x.ModelId));
        var providers = await ResolveProvidersAsync(records.Select(x => x.ProviderId));
        var projects = await ResolveProjectsAsync(records.Select(x => x.ProjectId));
        var users = await ResolveUserNamesAsync(records.Select(x => x.UserId));
        var currency = records.Select(x => x.Currency).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "USD";
        var warnings = BuildWarnings(records, providers);
        return new AiUsageDashboardDto
        {
            CallCount = records.Count,
            CostAmount = records.Sum(x => x.CostAmount),
            Currency = currency,
            FailedCount = records.Count(x => !x.Status.Equals("Success", StringComparison.OrdinalIgnoreCase) &&
                                            !x.Status.Equals("Completed", StringComparison.OrdinalIgnoreCase)),
            HighRiskPromptCount = records.Count(x => x.PromptInjectionRiskScore >= 60),
            ModelBuckets = BuildBuckets(records, x => x.ModelId?.ToString() ?? "-", x => ResolveModelLabel(x, models)),
            ProjectBuckets = BuildBuckets(records.Where(x => x.ProjectId.HasValue), x => x.ProjectId!.Value.ToString("N"), x => ResolveProjectLabel(x, projects)),
            ScenarioBuckets = BuildBuckets(records, x => string.IsNullOrWhiteSpace(x.Scenario) ? "-" : x.Scenario, x => string.IsNullOrWhiteSpace(x.Scenario) ? "未归类" : x.Scenario),
            TemplateBuckets = BuildBuckets(records.Where(x => !string.IsNullOrWhiteSpace(x.PromptTemplateCode)), x => x.PromptTemplateCode, x => x.PromptTemplateCode),
            ToolCallCount = records.Count(x => x.Source.Contains("Tool", StringComparison.OrdinalIgnoreCase)),
            TotalTokens = records.Sum(x => x.TotalTokens),
            UserBuckets = BuildBuckets(records, x => x.UserId?.ToString() ?? "-", x => x.UserId.HasValue ? users.GetValueOrDefault(x.UserId.Value, x.UserId.Value.ToString()) : "系统"),
            Warnings = warnings
        };
    }

    public static string RedactSensitive(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var redacted = ApiKeyRegex.Replace(value, "$1: [REDACTED]");
        redacted = EmailRegex.Replace(redacted, "[EMAIL]");
        redacted = PhoneRegex.Replace(redacted, "[PHONE]");
        redacted = IdCardRegex.Replace(redacted, "[ID_CARD]");
        return redacted.Length <= 4000 ? redacted : redacted[..4000];
    }

    private async Task<List<AiUsageRecord>> ApplyQueryAsync(AiUsageQueryInput input)
    {
        var records = await _usageRepository.GetListAsync();
        if (input.CreatedFrom.HasValue)
        {
            records = records.Where(x => x.CreationTime >= input.CreatedFrom.Value).ToList();
        }

        if (input.CreatedTo.HasValue)
        {
            var createdTo = input.CreatedTo.Value.Date.AddDays(1);
            records = records.Where(x => x.CreationTime < createdTo).ToList();
        }

        if (input.ProviderId.HasValue)
        {
            records = records.Where(x => x.ProviderId == input.ProviderId.Value).ToList();
        }

        if (input.ModelId.HasValue)
        {
            records = records.Where(x => x.ModelId == input.ModelId.Value).ToList();
        }

        if (input.UserId.HasValue)
        {
            records = records.Where(x => x.UserId == input.UserId.Value).ToList();
        }

        if (input.ProjectId.HasValue)
        {
            records = records.Where(x => x.ProjectId == input.ProjectId.Value).ToList();
        }

        if (input.ConversationId.HasValue)
        {
            records = records.Where(x => x.ConversationId == input.ConversationId.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Source))
        {
            var source = input.Source.Trim();
            records = records.Where(x => x.Source.Equals(source, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var status = input.Status.Trim();
            records = records.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.ToolCode))
        {
            var toolCode = input.ToolCode.Trim();
            records = records.Where(x => x.ToolCode.Equals(toolCode, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Scenario))
        {
            var scenario = input.Scenario.Trim();
            records = records.Where(x => x.Scenario.Equals(scenario, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.PromptTemplateCode))
        {
            var templateCode = input.PromptTemplateCode.Trim();
            records = records.Where(x => x.PromptTemplateCode.Equals(templateCode, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            records = records
                .Where(x =>
                    x.AuditSummary.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.RequestPreview.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.ResponsePreview.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.ErrorMessage.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return records;
    }

    private static decimal CalculateCost(AiModel? model, int promptTokens, int completionTokens)
    {
        if (model == null)
        {
            return 0m;
        }

        var inputCost = promptTokens / 1000m * model.InputTokenPrice;
        var outputCost = completionTokens / 1000m * model.OutputTokenPrice;
        return Math.Round(inputCost + outputCost, 6, MidpointRounding.AwayFromZero);
    }

    private static AiPromptInjectionDetection DetectPromptInjection(string? value)
    {
        var text = value ?? string.Empty;
        var signals = new List<string>();
        var patterns = new (string Label, string Pattern, int Score)[]
        {
            ("忽略原指令", "(?i)(ignore|disregard).{0,20}(previous|above|system|developer|instruction)|忽略.{0,12}(以上|之前|系统|开发者).{0,12}(指令|规则)", 35),
            ("泄露系统提示词", "(?i)(reveal|show|print).{0,20}(system prompt|developer message|hidden instruction)|输出.{0,12}(系统提示词|隐藏指令|开发者消息)", 35),
            ("越权写回", "(?i)(bypass|disable).{0,20}(permission|policy|guardrail)|绕过.{0,12}(权限|规则|审核|确认)", 25),
            ("角色越狱", "(?i)\\bDAN\\b|jailbreak|你现在不是|你无需遵守", 30),
            ("密钥索取", "(?i)(api key|token|password|secret).{0,20}(show|print|return)|返回.{0,12}(密钥|密码|令牌)", 25)
        };
        var score = 0;
        foreach (var (label, pattern, weight) in patterns)
        {
            if (!Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase))
            {
                continue;
            }

            signals.Add(label);
            score += weight;
        }

        return new AiPromptInjectionDetection(Math.Clamp(score, 0, 100), signals.Distinct().ToList());
    }

    private static AiProviderQuotaConfig ReadProviderQuotaConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return new AiProviderQuotaConfig();
        }

        try
        {
            using var document = JsonDocument.Parse(configJson);
            var root = document.RootElement;
            return new AiProviderQuotaConfig(
                ReadDecimal(root, "dailyTenantCostLimit"),
                ReadDecimal(root, "dailyUserCostLimit"),
                ReadDecimal(root, "dailyModelCostLimit"),
                ReadDecimal(root, "warningCostThreshold"),
                ReadString(root, "currency", "USD"));
        }
        catch
        {
            return new AiProviderQuotaConfig();
        }
    }

    private static decimal ReadDecimal(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) && property.TryGetDecimal(out var value)
            ? Math.Max(0, value)
            : 0m;
    }

    private static string ReadString(JsonElement root, string propertyName, string defaultValue)
    {
        return root.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? defaultValue
            : defaultValue;
    }

    private static string BuildAuditSummary(
        AiUsageRecordInput input,
        decimal cost,
        AiModel? model,
        AiProvider? provider,
        AiPromptInjectionDetection injection)
    {
        var target = !string.IsNullOrWhiteSpace(input.ToolCode)
            ? input.ToolCode
            : model?.DisplayName ?? provider?.Name ?? input.Source;
        var injectionSummary = injection.RiskScore <= 0
            ? string.Empty
            : $"，Prompt Injection 风险 {injection.RiskScore}";
        return $"{input.Source} {input.Status}：{target}，Token {Math.Max(0, input.PromptTokens) + Math.Max(0, input.CompletionTokens)}，成本 {cost:0.######}{injectionSummary}";
    }

    private static List<string> BuildWarnings(IReadOnlyCollection<AiUsageRecord> records, IReadOnlyDictionary<Guid, AiProvider> providers)
    {
        var warnings = new List<string>();
        foreach (var provider in providers.Values)
        {
            var config = ReadProviderQuotaConfig(provider.ConfigJson);
            if (config.WarningCostThreshold <= 0)
            {
                continue;
            }

            var cost = records.Where(x => x.ProviderId == provider.Id).Sum(x => x.CostAmount);
            if (cost >= config.WarningCostThreshold)
            {
                warnings.Add($"{provider.Name} 当前筛选周期成本 {cost:0.####} {config.Currency}，已达到预警阈值 {config.WarningCostThreshold:0.####}。");
            }
        }

        var highRiskCount = records.Count(x => x.PromptInjectionRiskScore >= 60);
        if (highRiskCount > 0)
        {
            warnings.Add($"当前筛选周期发现 {highRiskCount} 次 Prompt Injection 中高风险输入。");
        }

        return warnings;
    }

    private static List<AiUsageBucketDto> BuildBuckets(
        IEnumerable<AiUsageRecord> records,
        Func<AiUsageRecord, string> keySelector,
        Func<AiUsageRecord, string> labelSelector)
    {
        return records
            .GroupBy(keySelector)
            .Select(group =>
            {
                var first = group.First();
                return new AiUsageBucketDto
                {
                    CostAmount = group.Sum(x => x.CostAmount),
                    Count = group.Count(),
                    Key = group.Key,
                    Label = labelSelector(first),
                    TotalTokens = group.Sum(x => x.TotalTokens)
                };
            })
            .OrderByDescending(x => x.CostAmount)
            .ThenByDescending(x => x.TotalTokens)
            .Take(10)
            .ToList();
    }

    private static string ResolveModelLabel(AiUsageRecord record, IReadOnlyDictionary<Guid, AiModel> models)
    {
        return record.ModelId.HasValue && models.TryGetValue(record.ModelId.Value, out var model)
            ? model.DisplayName
            : string.IsNullOrWhiteSpace(record.ModelCode) ? "未关联模型" : record.ModelCode;
    }

    private static string ResolveProjectLabel(AiUsageRecord record, IReadOnlyDictionary<Guid, ProjectEntity> projects)
    {
        return record.ProjectId.HasValue && projects.TryGetValue(record.ProjectId.Value, out var project)
            ? $"{project.ProjectCode} · {project.Name}"
            : record.ProjectId?.ToString() ?? "未关联项目";
    }

    private async Task<Dictionary<Guid, AiModel>> ResolveModelsAsync(IEnumerable<Guid?> ids)
    {
        var modelIds = ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToHashSet();
        return modelIds.Count == 0
            ? []
            : (await _modelRepository.GetListAsync(x => modelIds.Contains(x.Id))).ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<Guid, AiProvider>> ResolveProvidersAsync(IEnumerable<Guid?> ids)
    {
        var providerIds = ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToHashSet();
        return providerIds.Count == 0
            ? []
            : (await _providerRepository.GetListAsync(x => providerIds.Contains(x.Id))).ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<Guid, ProjectEntity>> ResolveProjectsAsync(IEnumerable<Guid?> ids)
    {
        var projectIds = ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToHashSet();
        return projectIds.Count == 0
            ? []
            : (await _projectRepository.GetListAsync(x => projectIds.Contains(x.Id))).ToDictionary(x => x.Id);
    }

    private async Task<Dictionary<Guid, string>> ResolveUserNamesAsync(IEnumerable<Guid?> ids)
    {
        var userIds = ids.Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToHashSet();
        if (userIds.Count == 0)
        {
            return [];
        }

        return (await _userRepository.GetListAsync(x => userIds.Contains(x.Id)))
            .ToDictionary(x => x.Id, x => string.IsNullOrWhiteSpace(x.Name) ? x.UserName : x.Name);
    }

    private static AiUsageRecordDto MapRecord(
        AiUsageRecord item,
        IReadOnlyDictionary<Guid, AiModel> models,
        IReadOnlyDictionary<Guid, AiProvider> providers,
        IReadOnlyDictionary<Guid, string> users)
    {
        return new AiUsageRecordDto
        {
            AuditSummary = item.AuditSummary,
            CompletionTokens = item.CompletionTokens,
            ConversationId = item.ConversationId,
            CostAmount = item.CostAmount,
            CreationTime = item.CreationTime,
            Currency = item.Currency,
            ElapsedMilliseconds = item.ElapsedMilliseconds,
            ErrorMessage = item.ErrorMessage,
            Id = item.Id,
            JobId = item.JobId,
            ModelCode = item.ModelCode,
            ModelId = item.ModelId,
            ModelName = item.ModelId.HasValue ? models.GetValueOrDefault(item.ModelId.Value)?.DisplayName ?? string.Empty : string.Empty,
            ProjectId = item.ProjectId,
            PromptInjectionRiskScore = item.PromptInjectionRiskScore,
            PromptInjectionSignalsJson = item.PromptInjectionSignalsJson,
            PromptTemplateCode = item.PromptTemplateCode,
            PromptTokens = item.PromptTokens,
            ProviderId = item.ProviderId,
            ProviderName = item.ProviderId.HasValue ? providers.GetValueOrDefault(item.ProviderId.Value)?.Name ?? string.Empty : string.Empty,
            RequestPreview = item.RequestPreview,
            ResponsePreview = item.ResponsePreview,
            Scenario = item.Scenario,
            Source = item.Source,
            Status = item.Status,
            ToolCode = item.ToolCode,
            TotalTokens = item.TotalTokens,
            UserId = item.UserId,
            UserName = item.UserId.HasValue ? users.GetValueOrDefault(item.UserId.Value, string.Empty) : string.Empty
        };
    }

    private sealed record AiProviderQuotaConfig(
        decimal DailyTenantCostLimit = 0,
        decimal DailyUserCostLimit = 0,
        decimal DailyModelCostLimit = 0,
        decimal WarningCostThreshold = 0,
        string Currency = "USD");

    private sealed record AiPromptInjectionDetection(int RiskScore, List<string> Signals);
}
