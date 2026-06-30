using System.Collections.Generic;

namespace General.Admin.Integration;

public static class IntegrationMessageTemplateTypes
{
    public const string Text = "Text";
    public const string Markdown = "Markdown";
    public const string Card = "Card";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Text,
        Markdown,
        Card
    };
}

public static class IntegrationMessageTemplateStatus
{
    public const string Draft = "Draft";
    public const string Active = "Active";
    public const string Disabled = "Disabled";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Draft,
        Active,
        Disabled
    };
}

public static class IntegrationNotificationRuleStatus
{
    public const string Active = "Active";
    public const string Disabled = "Disabled";
    public const string Paused = "Paused";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Active,
        Disabled,
        Paused
    };
}

public static class IntegrationNotificationChannelTypes
{
    public const string Personal = "Personal";
    public const string Group = "Group";
    public const string Mixed = "Mixed";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        Personal,
        Group,
        Mixed
    };
}
