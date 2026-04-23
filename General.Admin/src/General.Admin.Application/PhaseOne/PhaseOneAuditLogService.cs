using Volo.Abp.AuditLogging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.PhaseOne;

public class PhaseOneAuditLogService : ITransientDependency
{
    private readonly IRepository<AuditLog, Guid> _auditLogRepository;
    private readonly PhaseOneRequestAuditStore _requestAuditStore;

    public PhaseOneAuditLogService(
        IRepository<AuditLog, Guid> auditLogRepository,
        PhaseOneRequestAuditStore requestAuditStore)
    {
        _auditLogRepository = auditLogRepository;
        _requestAuditStore = requestAuditStore;
    }

    public async Task<List<PhaseOneAuditLogItemDto>> GetListAsync(PhaseOneAuditLogQueryInput input)
    {
        var keyword = input.Keyword?.Trim();
        var category = input.Category?.Trim().ToLowerInvariant();
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 500);
        var requestLogs = await GetRequestLogsAsync(input);

        if (requestLogs.Count > 0)
        {
            return requestLogs
                .OrderByDescending(x => x.ExecutionTime)
                .Take(maxResultCount)
                .ToList();
        }

        var logs = await _auditLogRepository.GetListAsync(includeDetails: true);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            logs = logs
                .Where(x =>
                    ContainsIgnoreCase(x.UserName, keyword) ||
                    ContainsIgnoreCase(x.TenantName, keyword) ||
                    ContainsIgnoreCase(x.Url, keyword) ||
                    ContainsIgnoreCase(x.ClientIpAddress, keyword) ||
                    x.Actions.Any(action =>
                        ContainsIgnoreCase(action.ServiceName, keyword) ||
                        ContainsIgnoreCase(action.MethodName, keyword)))
                .ToList();
        }

        logs = FilterByCategory(logs, category);

        return logs
            .OrderByDescending(x => x.ExecutionTime)
            .Take(maxResultCount)
            .Select(x => new PhaseOneAuditLogItemDto
            {
                Id = x.Id,
                ActionSummary = x.Actions
                    .OrderByDescending(action => action.ExecutionTime)
                    .Select(action => $"{action.ServiceName}.{action.MethodName}")
                    .FirstOrDefault(),
                BrowserInfo = x.BrowserInfo,
                ClientIpAddress = x.ClientIpAddress,
                ExecutionDuration = x.ExecutionDuration,
                ExecutionTime = x.ExecutionTime,
                ExceptionMessage = NormalizeException(x.Exceptions),
                HasException = !string.IsNullOrWhiteSpace(x.Exceptions),
                HttpMethod = x.HttpMethod,
                HttpStatusCode = x.HttpStatusCode,
                TenantName = x.TenantName,
                Url = x.Url,
                UserName = x.UserName
            })
            .ToList();
    }

    public async Task<PhaseOneLogDashboardDto> GetDashboardAsync(PhaseOneAuditLogQueryInput input)
    {
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 500);
        var requestLogs = await GetRequestLogsAsync(input);
        if (requestLogs.Count > 0)
        {
            var orderedRequestLogs = requestLogs
                .OrderByDescending(x => x.ExecutionTime)
                .Take(maxResultCount)
                .ToList();

            return new PhaseOneLogDashboardDto
            {
                AccessLogs = FilterByCategory(orderedRequestLogs, "access"),
                AuditLogs = FilterByCategory(orderedRequestLogs, "audit"),
                ExceptionLogs = FilterByCategory(orderedRequestLogs, "exception"),
                OperationLogs = FilterByCategory(orderedRequestLogs, "operation"),
                TopApis = orderedRequestLogs
                    .GroupBy(x => x.Url ?? "-")
                    .OrderByDescending(x => x.Count())
                    .Take(8)
                    .Select(x => new PhaseOneLogStatItemDto { Count = x.Count(), Key = x.Key, Label = x.Key })
                    .ToList(),
                TopMenus = orderedRequestLogs
                    .GroupBy(x => x.ActionSummary ?? "-")
                    .OrderByDescending(x => x.Count())
                    .Take(8)
                    .Select(x => new PhaseOneLogStatItemDto { Count = x.Count(), Key = x.Key, Label = x.Key })
                    .ToList(),
                TopUsers = orderedRequestLogs
                    .GroupBy(x => x.UserName ?? "-")
                    .OrderByDescending(x => x.Count())
                    .Take(8)
                    .Select(x => new PhaseOneLogStatItemDto { Count = x.Count(), Key = x.Key, Label = x.Key })
                    .ToList()
            };
        }

        var allLogs = await _auditLogRepository.GetListAsync(includeDetails: true);
        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            allLogs = (await GetListAsync(input))
                .Select(item => allLogs.First(x => x.Id == item.Id))
                .ToList();
        }

        var ordered = allLogs.OrderByDescending(x => x.ExecutionTime).Take(maxResultCount).ToList();
        var dtoMap = ordered.ToDictionary(x => x.Id, MapItem);

        return new PhaseOneLogDashboardDto
        {
            AccessLogs = FilterByCategory(ordered, "access").Select(x => dtoMap[x.Id]).ToList(),
            AuditLogs = FilterByCategory(ordered, "audit").Select(x => dtoMap[x.Id]).ToList(),
            ExceptionLogs = FilterByCategory(ordered, "exception").Select(x => dtoMap[x.Id]).ToList(),
            OperationLogs = FilterByCategory(ordered, "operation").Select(x => dtoMap[x.Id]).ToList(),
            TopApis = ordered
                .GroupBy(x => x.Url ?? "-")
                .OrderByDescending(x => x.Count())
                .Take(8)
                .Select(x => new PhaseOneLogStatItemDto { Count = x.Count(), Key = x.Key, Label = x.Key })
                .ToList(),
            TopMenus = ordered
                .GroupBy(x => x.Actions.FirstOrDefault()?.MethodName ?? "-")
                .OrderByDescending(x => x.Count())
                .Take(8)
                .Select(x => new PhaseOneLogStatItemDto { Count = x.Count(), Key = x.Key, Label = x.Key })
                .ToList(),
            TopUsers = ordered
                .GroupBy(x => x.UserName ?? "-")
                .OrderByDescending(x => x.Count())
                .Take(8)
                .Select(x => new PhaseOneLogStatItemDto { Count = x.Count(), Key = x.Key, Label = x.Key })
                .ToList()
        };
    }

    private static List<AuditLog> FilterByCategory(List<AuditLog> logs, string? category)
    {
        return category switch
        {
            "access" => logs.Where(x => !x.Actions.Any() && string.IsNullOrWhiteSpace(x.Exceptions)).ToList(),
            "audit" => logs.Where(x => x.EntityChanges.Any()).ToList(),
            "exception" => logs.Where(x => !string.IsNullOrWhiteSpace(x.Exceptions)).ToList(),
            "operation" => logs.Where(x => x.Actions.Any()).ToList(),
            _ => logs
        };
    }

    private static List<PhaseOneAuditLogItemDto> FilterByCategory(List<PhaseOneAuditLogItemDto> logs, string? category)
    {
        return category switch
        {
            "access" => logs.Where(x => !x.HasException && string.Equals(x.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)).ToList(),
            "audit" => logs.Where(x => !string.IsNullOrWhiteSpace(x.ActionSummary)).ToList(),
            "exception" => logs.Where(x => x.HasException || (x.HttpStatusCode ?? 200) >= 500).ToList(),
            "operation" => logs.Where(x => !string.Equals(x.HttpMethod, "GET", StringComparison.OrdinalIgnoreCase)).ToList(),
            _ => logs
        };
    }

    private static PhaseOneAuditLogItemDto MapItem(AuditLog x)
    {
        return new PhaseOneAuditLogItemDto
        {
            Id = x.Id,
            ActionSummary = x.Actions
                .OrderByDescending(action => action.ExecutionTime)
                .Select(action => $"{action.ServiceName}.{action.MethodName}")
                .FirstOrDefault(),
            BrowserInfo = x.BrowserInfo,
            ClientIpAddress = x.ClientIpAddress,
            ExecutionDuration = x.ExecutionDuration,
            ExecutionTime = x.ExecutionTime,
            ExceptionMessage = NormalizeException(x.Exceptions),
            HasException = !string.IsNullOrWhiteSpace(x.Exceptions),
            HttpMethod = x.HttpMethod,
            HttpStatusCode = x.HttpStatusCode,
            TenantName = x.TenantName,
            Url = x.Url,
            UserName = x.UserName
        };
    }

    private static bool ContainsIgnoreCase(string? value, string keyword)
    {
        return !string.IsNullOrWhiteSpace(value) &&
               value.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeException(string? exceptionText)
    {
        if (string.IsNullOrWhiteSpace(exceptionText))
        {
            return null;
        }

        var firstLine = exceptionText
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstLine)
            ? exceptionText.Trim()
            : firstLine.Trim();
    }

    private async Task<List<PhaseOneAuditLogItemDto>> GetRequestLogsAsync(PhaseOneAuditLogQueryInput input)
    {
        var logs = (await _requestAuditStore.ReadAsync())
            .Select(item => new PhaseOneAuditLogItemDto
            {
                ActionSummary = item.ActionSummary,
                BrowserInfo = item.BrowserInfo,
                ClientIpAddress = item.ClientIpAddress,
                ExecutionDuration = item.ExecutionDuration,
                ExecutionTime = item.ExecutionTime,
                ExceptionMessage = item.ExceptionMessage,
                HasException = item.HasException,
                HttpMethod = item.HttpMethod,
                HttpStatusCode = item.HttpStatusCode,
                Id = item.Id,
                TenantName = item.TenantName,
                Url = item.Url,
                UserName = item.UserName
            })
            .ToList();

        if (input.StartTime.HasValue)
        {
            logs = logs.Where(x => x.ExecutionTime >= input.StartTime.Value).ToList();
        }

        if (input.EndTime.HasValue)
        {
            logs = logs.Where(x => x.ExecutionTime <= input.EndTime.Value).ToList();
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            logs = logs.Where(x =>
                ContainsIgnoreCase(x.UserName, keyword) ||
                ContainsIgnoreCase(x.TenantName, keyword) ||
                ContainsIgnoreCase(x.Url, keyword) ||
                ContainsIgnoreCase(x.ClientIpAddress, keyword) ||
                ContainsIgnoreCase(x.ActionSummary, keyword)).ToList();
        }

        return FilterByCategory(logs, input.Category?.Trim().ToLowerInvariant());
    }
}
