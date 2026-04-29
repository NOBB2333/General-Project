using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppDictType : FullAuditedAggregateRoot<Guid>
{
    public string Code { get; private set; }

    public string Name { get; private set; }

    public string Remark { get; private set; }

    public int Sort { get; private set; }

    protected AppDictType()
    {
        Code = string.Empty;
        Name = string.Empty;
        Remark = string.Empty;
    }

    public AppDictType(Guid id, string code, string name, int sort = 0, string? remark = null) : base(id)
    {
        Code = NormalizeCode(code);
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Sort = sort;
        Remark = Normalize(remark, 256);
    }

    public void Update(string name, int sort, string? remark)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), 128).Trim();
        Sort = sort;
        Remark = Normalize(remark, 256);
    }

    public static string NormalizeCode(string code)
    {
        return Check.NotNullOrWhiteSpace(code, nameof(code), 64).Trim().ToLowerInvariant();
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
