using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace General.Desktop;

internal static class LocalWebRootServer
{
    private static readonly Lock SyncRoot = new();
    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".css"] = "text/css; charset=utf-8",
        [".html"] = "text/html; charset=utf-8",
        [".ico"] = "image/x-icon",
        [".js"] = "application/javascript; charset=utf-8",
        [".json"] = "application/json; charset=utf-8",
        [".map"] = "application/json; charset=utf-8",
        [".png"] = "image/png",
        [".svg"] = "image/svg+xml",
        [".txt"] = "text/plain; charset=utf-8",
        [".woff"] = "font/woff",
        [".woff2"] = "font/woff2",
    };

    private static string? _baseUrl;
    private static CancellationTokenSource? _cancellationTokenSource;
    private static string? _rootDirectory;

    public static string Start(string rootDirectory)
    {
        lock (SyncRoot)
        {
            if (_baseUrl is not null &&
                _rootDirectory is not null &&
                string.Equals(_rootDirectory, rootDirectory, StringComparison.Ordinal))
            {
                return _baseUrl;
            }

            StopUnsafe();

            var listener = new HttpListener();
            var port = GetAvailablePort();
            var prefix = $"http://127.0.0.1:{port}/";
            listener.Prefixes.Add(prefix);
            listener.Start();

            _rootDirectory = rootDirectory;
            _baseUrl = prefix;
            _cancellationTokenSource = new CancellationTokenSource();
            _ = Task.Run(() => RunAsync(listener, rootDirectory, _cancellationTokenSource.Token));

            return _baseUrl;
        }
    }

    private static async Task RunAsync(HttpListener listener, string rootDirectory, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                HttpListenerContext context;
                try
                {
                    context = await listener.GetContextAsync();
                }
                catch (HttpListenerException)
                {
                    break;
                }
                catch (ObjectDisposedException)
                {
                    break;
                }

                _ = Task.Run(() => ProcessRequestAsync(context, rootDirectory, cancellationToken), cancellationToken);
            }
        }
        finally
        {
            listener.Close();
        }
    }

    private static async Task ProcessRequestAsync(
        HttpListenerContext context,
        string rootDirectory,
        CancellationToken cancellationToken)
    {
        try
        {
            var localPath = context.Request.Url?.LocalPath ?? "/";
            var relativePath = Uri.UnescapeDataString(localPath.TrimStart('/'));
            var fullPath = string.IsNullOrWhiteSpace(relativePath)
                ? Path.Combine(rootDirectory, "index.html")
                : Path.GetFullPath(Path.Combine(rootDirectory, relativePath.Replace('/', Path.DirectorySeparatorChar)));

            if (!fullPath.StartsWith(Path.GetFullPath(rootDirectory), StringComparison.Ordinal))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            if (!File.Exists(fullPath))
            {
                var isSpaRoute = !Path.HasExtension(relativePath);
                if (isSpaRoute)
                {
                    fullPath = Path.Combine(rootDirectory, "index.html");
                }
            }

            if (!File.Exists(fullPath))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            context.Response.ContentType = GetContentType(fullPath);

            await using var fileStream = File.OpenRead(fullPath);
            context.Response.ContentLength64 = fileStream.Length;
            await fileStream.CopyToAsync(context.Response.OutputStream, cancellationToken);
        }
        catch
        {
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }

    private static string GetContentType(string path)
    {
        var extension = Path.GetExtension(path);
        return ContentTypes.TryGetValue(extension, out var contentType)
            ? contentType
            : "application/octet-stream";
    }

    private static int GetAvailablePort()
    {
        using var tcpListener = new TcpListener(IPAddress.Loopback, 0);
        tcpListener.Start();
        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
        tcpListener.Stop();
        return port;
    }

    private static void StopUnsafe()
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _baseUrl = null;
        _rootDirectory = null;
    }
}
