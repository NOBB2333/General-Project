namespace General.Admin.Ai;

public static class AiProviderProtocolNames
{
    public const string AnthropicMessages = nameof(AnthropicMessages);
    public const string OpenAIResponses = nameof(OpenAIResponses);

    public static bool IsSupported(string? providerType)
    {
        return string.Equals(providerType, OpenAIResponses, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(providerType, AnthropicMessages, StringComparison.OrdinalIgnoreCase);
    }
}
