namespace General.Admin.Infrastructure;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public int ExpireMinutes { get; set; } = 480;

    public string Issuer { get; set; } = "General.Admin";

    public string Audience { get; set; } = "General.Web";

    public string SecurityKey { get; set; } = "General.Admin.Local.Development.Secret.Key.2026";
}
