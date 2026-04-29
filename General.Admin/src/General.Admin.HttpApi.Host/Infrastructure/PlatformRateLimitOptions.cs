namespace General.Admin.Infrastructure;

public class PlatformRateLimitOptions
{
    public const string SectionName = "RateLimiting";

    public string ForwardedForHeader { get; set; } = "X-Forwarded-For";

    public RateLimitRule General { get; set; } = new(600, 1);

    public RateLimitRule Login { get; set; } = new(20, 1);

    public RateLimitRule FileUpload { get; set; } = new(60, 1);

    public RateLimitRule OpenApi { get; set; } = new(120, 1);

    public List<string> TrustedProxies { get; set; } = [];
}

public class RateLimitRule
{
    public RateLimitRule()
    {
    }

    public RateLimitRule(int permitLimit, int windowMinutes)
    {
        PermitLimit = permitLimit;
        WindowMinutes = windowMinutes;
    }

    public int PermitLimit { get; set; }

    public int WindowMinutes { get; set; }
}
