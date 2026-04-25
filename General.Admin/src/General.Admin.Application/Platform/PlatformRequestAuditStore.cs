using Volo.Abp.Domain.Repositories;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Linq;

namespace General.Admin.Platform;

public class PlatformRequestAuditStore : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly PlatformRequestAuditQueue _queue;
    private readonly IRepository<AppRequestAuditLog, Guid> _requestAuditLogRepository;

    public PlatformRequestAuditStore(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        PlatformRequestAuditQueue queue,
        IRepository<AppRequestAuditLog, Guid> requestAuditLogRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _queue = queue;
        _requestAuditLogRepository = requestAuditLogRepository;
    }

    public Task AppendAsync(PlatformRequestAuditEntry entry)
    {
        if (!_queue.TryEnqueue(entry))
        {
            throw new InvalidOperationException("Failed to enqueue request audit log.");
        }

        return Task.CompletedTask;
    }

    public async Task<List<PlatformRequestAuditEntry>> QueryAsync(PlatformAuditLogQueryInput input)
    {
        try
        {
            var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 500);
            var category = input.Category?.Trim().ToLowerInvariant();
            var keyword = input.Keyword?.Trim();

            var queryable = await _requestAuditLogRepository.GetQueryableAsync();

            if (input.StartTime.HasValue)
            {
                queryable = queryable.Where(x => x.ExecutionTime >= input.StartTime.Value);
            }

            if (input.EndTime.HasValue)
            {
                queryable = queryable.Where(x => x.ExecutionTime <= input.EndTime.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                queryable = queryable.Where(x =>
                    (x.UserName != null && x.UserName.Contains(keyword)) ||
                    (x.TenantName != null && x.TenantName.Contains(keyword)) ||
                    (x.Url != null && x.Url.Contains(keyword)) ||
                    (x.ClientIpAddress != null && x.ClientIpAddress.Contains(keyword)) ||
                    (x.ActionSummary != null && x.ActionSummary.Contains(keyword)) ||
                    (x.MenuTitle != null && x.MenuTitle.Contains(keyword)));
            }

            queryable = ApplyCategoryFilter(queryable, category)
                .OrderByDescending(x => x.ExecutionTime)
                .Take(maxResultCount);

            var logs = await _asyncQueryableExecuter.ToListAsync(queryable);
            return logs.Select(MapEntry).ToList();
        }
        catch (Exception ex) when (ex.ToString().Contains("no such table: AppRequestAuditLogs", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }
    }

    private static IQueryable<AppRequestAuditLog> ApplyCategoryFilter(
        IQueryable<AppRequestAuditLog> queryable,
        string? category)
    {
        return category switch
        {
            "access" => queryable.Where(x => !x.HasException && x.HttpMethod == "GET" && x.Category != "pagevisit"),
            "audit" => queryable.Where(x => x.ActionSummary != null && x.ActionSummary != string.Empty),
            "exception" => queryable.Where(x => x.HasException || (x.HttpStatusCode ?? 200) >= 500),
            "operation" => queryable.Where(x => x.HttpMethod != "GET" && x.Category != "pagevisit"),
            "pagevisit" => queryable.Where(x => x.Category == "pagevisit"),
            _ => queryable.Where(x => x.Category != "pagevisit")
        };
    }

    private static PlatformRequestAuditEntry MapEntry(AppRequestAuditLog log)
    {
        return new PlatformRequestAuditEntry
        {
            ActionSummary = log.ActionSummary,
            BrowserInfo = log.BrowserInfo,
            Category = log.Category,
            ClientIpAddress = log.ClientIpAddress,
            ExecutionDuration = log.ExecutionDuration,
            ExecutionTime = log.ExecutionTime,
            ExceptionMessage = log.ExceptionMessage,
            HasException = log.HasException,
            HttpMethod = log.HttpMethod,
            HttpStatusCode = log.HttpStatusCode,
            Id = log.Id,
            MenuTitle = log.MenuTitle,
            TenantName = log.TenantName,
            Url = log.Url,
            UserName = log.UserName
        };
    }
}

public class PlatformRequestAuditEntry
{
    public string? ActionSummary { get; set; }

    public string? BrowserInfo { get; set; }

    public string? Category { get; set; } // "api" | "pagevisit"

    public string? ClientIpAddress { get; set; }

    public int ExecutionDuration { get; set; }

    public DateTime ExecutionTime { get; set; }

    public string? ExceptionMessage { get; set; }

    public bool HasException { get; set; }

    public string? HttpMethod { get; set; }

    public int? HttpStatusCode { get; set; }

    public Guid Id { get; set; }

    public string? MenuTitle { get; set; } // populated for pagevisit category

    public string? TenantName { get; set; }

    public string? Url { get; set; }

    public string? UserName { get; set; }
}
