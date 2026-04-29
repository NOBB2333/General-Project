using System.IO;
using System.Threading;

namespace General.Admin.Infrastructure;

internal static class CloudPlatformFileStoragePathHelper
{
    public static string BuildObjectName(string pathTemplate, string fileKey, DateTime now)
    {
        var path = RenderPathTemplate(pathTemplate, now);
        return string.IsNullOrWhiteSpace(path)
            ? fileKey
            : $"{path.Trim('/')}/{fileKey}";
    }

    public static async Task<Stream> EnsureSeekableAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (stream.CanSeek)
        {
            stream.Position = 0;
            return stream;
        }

        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
    }

    private static string RenderPathTemplate(string? template, DateTime now)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return string.Empty;
        }

        return template
            .Replace("{yyyy}", now.ToString("yyyy"), StringComparison.OrdinalIgnoreCase)
            .Replace("{MM}", now.ToString("MM"), StringComparison.OrdinalIgnoreCase)
            .Replace("{dd}", now.ToString("dd"), StringComparison.OrdinalIgnoreCase)
            .Trim()
            .Replace('\\', '/')
            .Trim('/');
    }
}
