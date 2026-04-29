using Volo.Abp.Domain.Entities.Auditing;

namespace General.Admin.Platform;

public class AppDictData : FullAuditedAggregateRoot<Guid>
{
    public Guid DictTypeId { get; private set; }

    public bool IsEnabled { get; private set; }

    public string Label { get; private set; }

    public string Remark { get; private set; }

    public int Sort { get; private set; }

    public string TagColor { get; private set; }

    public string Value { get; private set; }

    protected AppDictData()
    {
        Label = string.Empty;
        Remark = string.Empty;
        TagColor = string.Empty;
        Value = string.Empty;
    }

    public AppDictData(
        Guid id,
        Guid dictTypeId,
        string label,
        string value,
        int sort = 0,
        bool isEnabled = true,
        string? tagColor = null,
        string? remark = null) : base(id)
    {
        DictTypeId = dictTypeId;
        Label = Check.NotNullOrWhiteSpace(label, nameof(label), 128).Trim();
        Value = Check.NotNullOrWhiteSpace(value, nameof(value), 128).Trim();
        Sort = sort;
        IsEnabled = isEnabled;
        TagColor = Normalize(tagColor, 32);
        Remark = Normalize(remark, 256);
    }

    public void Update(string label, string value, int sort, bool isEnabled, string? tagColor, string? remark)
    {
        Label = Check.NotNullOrWhiteSpace(label, nameof(label), 128).Trim();
        Value = Check.NotNullOrWhiteSpace(value, nameof(value), 128).Trim();
        Sort = sort;
        IsEnabled = isEnabled;
        TagColor = Normalize(tagColor, 32);
        Remark = Normalize(remark, 256);
    }

    private static string Normalize(string? value, int maxLength)
    {
        var normalized = value?.Trim() ?? string.Empty;
        return normalized.Length <= maxLength ? normalized : normalized[..maxLength];
    }
}
