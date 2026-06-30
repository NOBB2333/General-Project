using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace General.Admin.Integration;

internal static class IntegrationWebhookParserUtilities
{
    internal static string BuildDedupeKey(string connectorCode, string eventId, string rawBody)
    {
        if (!string.IsNullOrWhiteSpace(eventId))
        {
            return $"{connectorCode}:{eventId.Trim()}";
        }

        return $"{connectorCode}:{ComputeSha256(rawBody)}";
    }

    internal static string BuildNormalizedPayload(
        string connectorCode,
        string eventType,
        string actionCode,
        string businessKey,
        JsonElement raw)
    {
        return JsonSerializer.Serialize(new
        {
            action = actionCode,
            business_key = businessKey,
            connector_code = connectorCode,
            event_type = eventType,
            raw
        });
    }

    internal static string ComputeSortedSha1(params string[] values)
    {
        var sorted = values
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .OrderBy(item => item, StringComparer.Ordinal)
            .ToArray();
        return Convert.ToHexString(SHA1.HashData(Encoding.UTF8.GetBytes(string.Concat(sorted)))).ToLowerInvariant();
    }

    internal static string ComputeSha256(string value)
    {
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    }

    internal static string ReadConfig(string configJson, params string[] keys)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return string.Empty;
        }

        using var document = JsonDocument.Parse(configJson);
        return ReadString(document.RootElement, keys);
    }

    internal static string ReadHeader(string headersJson, params string[] names)
    {
        if (string.IsNullOrWhiteSpace(headersJson))
        {
            return string.Empty;
        }

        try
        {
            using var document = JsonDocument.Parse(headersJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return string.Empty;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (names.Any(name => property.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    return property.Value.ValueKind == JsonValueKind.String
                        ? property.Value.GetString()?.Trim() ?? string.Empty
                        : property.Value.ToString().Trim();
                }
            }
        }
        catch (JsonException)
        {
            return string.Empty;
        }

        return string.Empty;
    }

    internal static string ReadString(JsonElement element, params string[] paths)
    {
        foreach (var path in paths)
        {
            if (TryReadPath(element, path, out var value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    internal static string ReadStringFromObject(JsonElement element, params string[] paths)
    {
        var raw = ReadString(element, paths);
        if (raw.TrimStart().StartsWith('{'))
        {
            try
            {
                using var document = JsonDocument.Parse(raw);
                return ReadString(document.RootElement, "action", "actionCode", "action_code", "businessKey", "business_key");
            }
            catch (JsonException)
            {
                return raw;
            }
        }

        return raw;
    }

    internal static JsonElement ParseRawBody(string rawBody, string fallbackJson = "{}")
    {
        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(rawBody) ? fallbackJson : rawBody);
        return document.RootElement.Clone();
    }

    private static bool TryReadPath(JsonElement root, string path, out string value)
    {
        value = string.Empty;
        var current = root;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out current))
            {
                return false;
            }
        }

        if (current.ValueKind == JsonValueKind.String)
        {
            value = current.GetString()?.Trim() ?? string.Empty;
        }
        else if (current.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
        {
            value = current.ToString().Trim();
        }
        else if (current.ValueKind == JsonValueKind.Object)
        {
            value = current.GetRawText();
        }

        return !string.IsNullOrWhiteSpace(value);
    }
}
