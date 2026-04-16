using Volo.Abp.AuditLogging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace General.Admin.PhaseOne;

public class PhaseOneAuditLogService : ITransientDependency
{
    private readonly IRepository<AuditLog, Guid> _auditLogRepository;

    public PhaseOneAuditLogService(IRepository<AuditLog, Guid> auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<List<PhaseOneAuditLogItemDto>> GetListAsync(PhaseOneAuditLogQueryInput input)
    {
        var keyword = input.Keyword?.Trim();
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 500);
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
}
