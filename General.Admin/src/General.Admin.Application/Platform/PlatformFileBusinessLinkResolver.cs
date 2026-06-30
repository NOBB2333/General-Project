namespace General.Admin.Platform;

public static class PlatformFileBusinessLinkResolver
{
    private static readonly Dictionary<string, ProjectFileCategoryDescriptor> ProjectFileCategoryDescriptors = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Project/AcceptanceCriteria"] = new("验收标准文件", "验收标准", "delivery"),
        ["Project/AcceptanceRecord"] = new("验收记录文件", "验收记录", "delivery"),
        ["Project/Charter"] = new("项目章程", "项目章程", "overview"),
        ["Project/Delivery"] = new("交付物文件", "交付物", "delivery"),
        ["Project/Meeting"] = new("会议附件", "会议记录", "docs"),
        ["Project/Report"] = new("项目报告附件", "项目报告", "docs"),
        ["Project/Requirement"] = new("需求附件", "项目需求", "tasks"),
        ["Project/RequirementPrd"] = new("PRD 文档", "需求 PRD", "tasks")
    };

    public static string BuildBusinessKey(string? businessType, string? businessId)
    {
        if (string.IsNullOrWhiteSpace(businessType) || string.IsNullOrWhiteSpace(businessId))
        {
            return string.Empty;
        }

        return $"{businessType.Trim()}:{businessId.Trim()}";
    }

    public static IEnumerable<string> ResolveBusinessKeyAliases(string? businessType, string? businessId)
    {
        var key = BuildBusinessKey(businessType, businessId);
        if (key != string.Empty)
        {
            yield return key;
        }

        string[] aliases = businessType?.Trim() switch
        {
            "BusinessContract" => ["Contract"],
            "BusinessProcurement" => ["Procurement"],
            "BusinessBudget" => ["Budget"],
            "BusinessReceivable" => ["Receivable"],
            "BusinessInvoice" => ["Invoice"],
            "BusinessAcceptance" => ["Acceptance"],
            "BusinessChain" => ["Chain"],
            _ => []
        };

        foreach (var alias in aliases)
        {
            var aliasKey = BuildBusinessKey(alias, businessId);
            if (aliasKey != string.Empty)
            {
                yield return aliasKey;
            }
        }
    }

    public static string ResolveProjectTabByCategory(string? category)
    {
        var descriptor = ResolveProjectFileCategoryDescriptor(category);
        return descriptor?.Tab ?? "docs";
    }

    public static string? ResolveProjectObjectTypeNameByCategory(string? category)
    {
        return ResolveProjectFileCategoryDescriptor(category)?.TypeName;
    }

    public static string? ResolveProjectObjectNameByCategory(string? category, string? projectName)
    {
        var descriptor = ResolveProjectFileCategoryDescriptor(category);
        if (descriptor == null)
        {
            return null;
        }

        return string.IsNullOrWhiteSpace(projectName)
            ? descriptor.DefaultObjectName
            : $"{projectName.Trim()} / {descriptor.DefaultObjectName}";
    }

    private static ProjectFileCategoryDescriptor? ResolveProjectFileCategoryDescriptor(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return null;
        }

        return ProjectFileCategoryDescriptors.TryGetValue(category.Trim(), out var descriptor)
            ? descriptor
            : null;
    }
}

internal sealed record ProjectFileCategoryDescriptor(string TypeName, string DefaultObjectName, string Tab);
