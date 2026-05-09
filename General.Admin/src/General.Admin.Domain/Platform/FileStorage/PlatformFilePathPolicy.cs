namespace General.Admin.Platform;

public static class PlatformFilePathPolicy
{
    private const int MaxCategoryLength = 64;
    private const int MaxParentPathLength = 256;

    public static string NormalizeCategory(string? category)
    {
        var value = NormalizeSegment(category, "default", MaxCategoryLength, nameof(category));
        if (value.Contains('/'))
        {
            throw new BusinessException("Platform:InvalidFilePath")
                .WithData("Path", category ?? string.Empty)
                .WithData("Reason", "文件分类不能包含路径分隔符。");
        }

        return value;
    }

    public static string? NormalizeParentPath(string? parentPath)
    {
        if (string.IsNullOrWhiteSpace(parentPath))
        {
            return null;
        }

        var value = parentPath.Trim().Replace('\\', '/');
        EnsureRelativePath(value, MaxParentPathLength, nameof(parentPath));

        var segments = value
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(segment => NormalizeSegment(segment, null, MaxParentPathLength, nameof(parentPath)))
            .ToArray();
        if (segments.Length == 0)
        {
            return null;
        }

        var normalized = string.Join('/', segments);
        if (normalized.Length > MaxParentPathLength)
        {
            throw new BusinessException("Platform:InvalidFilePath")
                .WithData("Path", parentPath)
                .WithData("Reason", "父级路径长度超过限制。");
        }

        return normalized;
    }

    public static string BuildRelativePath(string category, string? parentPath, string fileName)
    {
        return string.Join(
            '/',
            new[] { NormalizeCategory(category), NormalizeParentPath(parentPath), fileName.Trim() }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static string NormalizeSegment(string? value, string? defaultValue, int maxLength, string parameterName)
    {
        var normalized = value?.Trim().Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            if (defaultValue is not null)
            {
                return defaultValue;
            }

            throw new BusinessException("Platform:InvalidFilePath")
                .WithData("Path", value ?? string.Empty)
                .WithData("Reason", $"{parameterName} 不能为空。");
        }

        EnsureRelativePath(normalized, maxLength, parameterName);
        if (normalized.Contains('/'))
        {
            foreach (var segment in normalized.Split('/'))
            {
                EnsureSafeSegment(segment, value);
            }
        }
        else
        {
            EnsureSafeSegment(normalized, value);
        }

        return normalized;
    }

    private static void EnsureRelativePath(string value, int maxLength, string parameterName)
    {
        if (value.Length > maxLength)
        {
            throw new BusinessException("Platform:InvalidFilePath")
                .WithData("Path", value)
                .WithData("Reason", $"{parameterName} 长度超过限制。");
        }

        if (value.StartsWith('/') ||
            value.Contains("//", StringComparison.Ordinal) ||
            value.Contains(':', StringComparison.Ordinal) ||
            value.Any(char.IsControl))
        {
            throw new BusinessException("Platform:InvalidFilePath")
                .WithData("Path", value)
                .WithData("Reason", "文件路径必须是安全的相对路径。");
        }
    }

    private static void EnsureSafeSegment(string segment, string? originalPath)
    {
        if (string.IsNullOrWhiteSpace(segment) ||
            segment is "." or ".." ||
            segment.Contains(':', StringComparison.Ordinal) ||
            segment.Any(char.IsControl))
        {
            throw new BusinessException("Platform:InvalidFilePath")
                .WithData("Path", originalPath ?? string.Empty)
                .WithData("Reason", "文件路径包含非法目录片段。");
        }
    }
}
