using System.Text.Json;

namespace General.Admin.PhaseOne;

internal static class PhaseOneSerializationHelper
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static List<Guid> DeserializeGuidList(string json)
    {
        return DeserializeList<Guid>(json);
    }

    public static List<string> DeserializeStringList(string json)
    {
        return DeserializeList<string>(json);
    }

    public static string SerializeGuids(IEnumerable<Guid> values)
    {
        return JsonSerializer.Serialize(
            values.Distinct().OrderBy(x => x),
            SerializerOptions);
    }

    public static string SerializeStrings(IEnumerable<string> values)
    {
        return JsonSerializer.Serialize(
            values.Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase),
            SerializerOptions);
    }

    private static List<T> DeserializeList<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
