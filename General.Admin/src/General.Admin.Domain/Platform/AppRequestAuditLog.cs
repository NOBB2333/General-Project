using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppRequestAuditLog : CreationAuditedAggregateRoot<Guid>
{
    public string? ActionSummary { get; private set; }

    public string? BrowserInfo { get; private set; }

    public string Category { get; private set; }

    public string? ClientIpAddress { get; private set; }

    public int ExecutionDuration { get; private set; }

    public DateTime ExecutionTime { get; private set; }

    public string? ExceptionMessage { get; private set; }

    public bool HasException { get; private set; }

    public string? HttpMethod { get; private set; }

    public int? HttpStatusCode { get; private set; }

    public string? MenuTitle { get; private set; }

    public string? TenantName { get; private set; }

    public string? Url { get; private set; }

    public string? UserName { get; private set; }

    protected AppRequestAuditLog()
    {
        Category = "api";
    }

    public AppRequestAuditLog(
        Guid id,
        DateTime executionTime,
        int executionDuration,
        bool hasException,
        string? httpMethod,
        int? httpStatusCode,
        string? url,
        string? userName,
        string? tenantName,
        string? clientIpAddress,
        string? browserInfo,
        string? actionSummary,
        string? menuTitle,
        string? category,
        string? exceptionMessage) : base(id)
    {
        ExecutionTime = executionTime;
        ExecutionDuration = executionDuration;
        HasException = hasException;
        HttpMethod = Normalize(httpMethod);
        HttpStatusCode = httpStatusCode;
        Url = Normalize(url);
        UserName = Normalize(userName);
        TenantName = Normalize(tenantName);
        ClientIpAddress = Normalize(clientIpAddress);
        BrowserInfo = Normalize(browserInfo);
        ActionSummary = Normalize(actionSummary);
        MenuTitle = Normalize(menuTitle);
        Category = string.IsNullOrWhiteSpace(category) ? "api" : category.Trim();
        ExceptionMessage = Normalize(exceptionMessage);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
