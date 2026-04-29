using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppOpenApiApplication : FullAuditedAggregateRoot<Guid>
{
    public string AppId { get; private set; }

    public string EncryptedSecret { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Name { get; private set; }

    public string Remark { get; private set; }

    public string Scopes { get; private set; }

    protected AppOpenApiApplication()
    {
        AppId = string.Empty;
        EncryptedSecret = string.Empty;
        Name = string.Empty;
        Remark = string.Empty;
        Scopes = "[]";
    }

    public AppOpenApiApplication(
        Guid id,
        string appId,
        string name,
        string encryptedSecret,
        string scopes,
        bool isEnabled = true,
        string? remark = null) : base(id)
    {
        AppId = Check.NotNullOrWhiteSpace(appId, nameof(appId), 64).Trim();
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        EncryptedSecret = Check.NotNullOrWhiteSpace(encryptedSecret, nameof(encryptedSecret), 2048);
        Scopes = Check.NotNullOrWhiteSpace(scopes, nameof(scopes), 1024);
        IsEnabled = isEnabled;
        Remark = Normalize(remark, 256);
    }

    public void Update(string name, string scopes, bool isEnabled, string? remark)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Scopes = Check.NotNullOrWhiteSpace(scopes, nameof(scopes), 1024);
        IsEnabled = isEnabled;
        Remark = Normalize(remark, 256);
    }

    public void ResetSecret(string encryptedSecret)
    {
        EncryptedSecret = Check.NotNullOrWhiteSpace(encryptedSecret, nameof(encryptedSecret), 2048);
    }

    public void Touch()
    {
        LastModificationTime = DateTime.UtcNow;
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
