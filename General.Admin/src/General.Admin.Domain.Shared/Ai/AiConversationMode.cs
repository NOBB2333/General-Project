namespace General.Admin.Ai;

public enum AiConversationMode
{
    General = 0,
    Clarify = 1,
    Plan = 2,
    Knowledge = 3,
    Execution = 4
}

public static class AiConversationModeNames
{
    public const string Clarify = nameof(AiConversationMode.Clarify);
    public const string Execution = nameof(AiConversationMode.Execution);
    public const string General = nameof(AiConversationMode.General);
    public const string Knowledge = nameof(AiConversationMode.Knowledge);
    public const string Plan = nameof(AiConversationMode.Plan);

    public static string Normalize(string? mode)
    {
        if (Enum.TryParse<AiConversationMode>(mode, ignoreCase: true, out var parsed))
        {
            return parsed.ToString();
        }

        return General;
    }
}
