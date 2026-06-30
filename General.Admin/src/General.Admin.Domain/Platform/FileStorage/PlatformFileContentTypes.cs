using System.Net.Mime;

namespace General.Admin.Platform;

public static class PlatformFileContentTypes
{
    public static IReadOnlyList<string> DefaultAllowedContentTypes { get; } =
    [
        MediaTypeNames.Image.Jpeg,
        "image/png",
        "image/gif",
        "image/webp",
        "image/bmp",
        "image/svg+xml",
        MediaTypeNames.Application.Pdf,
        MediaTypeNames.Text.Plain,
        "text/csv",
        "text/markdown",
        "text/html",
        "text/css",
        "text/javascript",
        "text/xml",
        "text/yaml",
        "text/x-yaml",
        "application/json",
        "application/xml",
        "application/javascript",
        "application/x-javascript",
        "application/x-yaml",
        "application/x-ndjson",
        "application/rtf",
        "application/msword",
        "application/vnd.ms-excel",
        "application/vnd.ms-powerpoint",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        "application/vnd.oasis.opendocument.text",
        "application/vnd.oasis.opendocument.spreadsheet",
        "application/vnd.oasis.opendocument.presentation",
        "application/ofd",
        "application/dxf",
        "image/vnd.dxf",
        "application/zip",
        "application/x-zip-compressed",
        "video/mp4",
        "video/x-m4v",
        "video/quicktime",
        "video/webm",
        "video/x-msvideo",
        "video/x-ms-wmv",
        "video/mpeg"
    ];

    public static string ResolveFromExtension(string? extension)
    {
        return (extension ?? string.Empty).ToLowerInvariant() switch
        {
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".jpg" or ".jpeg" => MediaTypeNames.Image.Jpeg,
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".pdf" => MediaTypeNames.Application.Pdf,
            ".txt" => MediaTypeNames.Text.Plain,
            ".csv" => "text/csv",
            ".md" or ".markdown" => "text/markdown",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" or ".mjs" => "text/javascript",
            ".json" => "application/json",
            ".jsonl" or ".ndjson" => "application/x-ndjson",
            ".xml" => "application/xml",
            ".yml" or ".yaml" => "text/yaml",
            ".rtf" => "application/rtf",
            ".odt" => "application/vnd.oasis.opendocument.text",
            ".ods" => "application/vnd.oasis.opendocument.spreadsheet",
            ".odp" => "application/vnd.oasis.opendocument.presentation",
            ".ofd" => "application/ofd",
            ".dxf" => "application/dxf",
            ".zip" => "application/zip",
            ".mp4" => "video/mp4",
            ".m4v" => "video/x-m4v",
            ".mov" => "video/quicktime",
            ".webm" => "video/webm",
            ".avi" => "video/x-msvideo",
            ".wmv" => "video/x-ms-wmv",
            ".mpeg" or ".mpg" => "video/mpeg",
            _ => MediaTypeNames.Application.Octet
        };
    }

    public static string Normalize(string? contentType)
    {
        var normalized = (contentType ?? string.Empty).Split(';', 2)[0].Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? MediaTypeNames.Application.Octet
            : normalized;
    }

    public static string ResolvePreviewContentType(string contentType)
    {
        var normalized = Normalize(contentType);
        return IsTextContentType(normalized) && !normalized.Contains("charset=", StringComparison.OrdinalIgnoreCase)
            ? $"{normalized}; charset=utf-8"
            : normalized;
    }

    public static bool IsTextContentType(string contentType)
    {
        var normalized = Normalize(contentType).ToLowerInvariant();
        return normalized.StartsWith("text/") ||
               normalized is "application/json" or
                   "application/javascript" or
                   "application/xml" or
                   "application/x-javascript" or
                   "application/x-ndjson" or
                   "application/x-yaml" or
                   "image/svg+xml";
    }
}
