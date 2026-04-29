using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace General.Admin.Infrastructure;

public class PlatformOpenApiSignatureMiddleware : IMiddleware
{
    private const string AppIdHeader = "X-GA-AppId";
    private const string NonceHeader = "X-GA-Nonce";
    private const string SignatureHeader = "X-GA-Signature";
    private const string TimestampHeader = "X-GA-Timestamp";
    private static readonly DistributedCacheEntryOptions NonceCacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    private readonly IDistributedCache _distributedCache;
    private readonly PlatformOpenApiApplicationService _openApiApplicationService;

    public PlatformOpenApiSignatureMiddleware(
        IDistributedCache distributedCache,
        PlatformOpenApiApplicationService openApiApplicationService)
    {
        _distributedCache = distributedCache;
        _openApiApplicationService = openApiApplicationService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Path.StartsWithSegments("/api/open"))
        {
            await next(context);
            return;
        }

        var appId = context.Request.Headers[AppIdHeader].ToString();
        var timestamp = context.Request.Headers[TimestampHeader].ToString();
        var nonce = context.Request.Headers[NonceHeader].ToString();
        var signature = context.Request.Headers[SignatureHeader].ToString();
        if (string.IsNullOrWhiteSpace(appId) ||
            string.IsNullOrWhiteSpace(timestamp) ||
            string.IsNullOrWhiteSpace(nonce) ||
            string.IsNullOrWhiteSpace(signature))
        {
            await RejectAsync(context, StatusCodes.Status401Unauthorized, "开放接口签名头不完整。");
            return;
        }

        if (!IsValidAppId(appId) ||
            !IsValidTimestamp(timestamp, out var timestampSeconds) ||
            !IsValidNonce(nonce) ||
            !IsValidSha256Signature(signature))
        {
            await RejectAsync(context, StatusCodes.Status401Unauthorized, "开放接口签名参数格式无效。");
            return;
        }

        if (Math.Abs(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestampSeconds) > 300)
        {
            await RejectAsync(context, StatusCodes.Status401Unauthorized, "开放接口时间戳无效或已过期。");
            return;
        }

        var nonceKey = $"platform:open-api:nonce:{appId}:{nonce}";
        if (!string.IsNullOrWhiteSpace(await _distributedCache.GetStringAsync(nonceKey)))
        {
            await RejectAsync(context, StatusCodes.Status401Unauthorized, "开放接口 nonce 已被使用。");
            return;
        }

        var requiredScope = context.GetEndpoint()?.Metadata.GetMetadata<OpenApiScopeAttribute>()?.Scope;
        var bodyHash = await ComputeBodyHashAsync(context.Request);
        var canonicalText = BuildCanonicalText(context.Request, timestamp, nonce, bodyHash);

        var validationResult = await _openApiApplicationService.ValidateSignatureAsync(
            appId,
            requiredScope,
            canonicalText,
            signature);
        if (validationResult == PlatformOpenApiSignatureValidationResult.ApplicationUnavailable)
        {
            await RejectAsync(context, StatusCodes.Status401Unauthorized, "开放接口应用不存在或已停用。");
            return;
        }

        if (validationResult == PlatformOpenApiSignatureValidationResult.ScopeDenied)
        {
            await RejectAsync(context, StatusCodes.Status403Forbidden, "开放接口应用无权访问该资源。");
            return;
        }

        if (validationResult == PlatformOpenApiSignatureValidationResult.SignatureInvalid)
        {
            await RejectAsync(context, StatusCodes.Status401Unauthorized, "开放接口签名校验失败。");
            return;
        }

        await _distributedCache.SetStringAsync(nonceKey, "1", NonceCacheOptions);
        await next(context);
    }

    private static string BuildCanonicalText(HttpRequest request, string timestamp, string nonce, string bodyHash)
    {
        return string.Join('\n',
            request.Method.ToUpperInvariant(),
            $"{request.Path}{request.QueryString}",
            timestamp,
            nonce,
            bodyHash);
    }

    private static async Task<string> ComputeBodyHashAsync(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
        {
            return Convert.ToHexString(SHA256.HashData(Array.Empty<byte>())).ToLowerInvariant();
        }

        request.EnableBuffering();
        using var sha256 = SHA256.Create();
        var hash = await sha256.ComputeHashAsync(request.Body);
        request.Body.Position = 0;
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static bool IsValidAppId(string appId)
    {
        return appId.Length is >= 3 and <= 64 &&
               appId.All(x => char.IsAsciiLetterOrDigit(x) || x is '_' or '-');
    }

    private static bool IsValidTimestamp(string timestamp, out long timestampSeconds)
    {
        timestampSeconds = 0;
        return timestamp.Length is >= 10 and <= 11 &&
               timestamp.All(char.IsAsciiDigit) &&
               long.TryParse(timestamp, out timestampSeconds);
    }

    private static bool IsValidNonce(string nonce)
    {
        return nonce.Length is >= 8 and <= 64 &&
               nonce.All(x => char.IsAsciiLetterOrDigit(x) || x is '_' or '-' or '.');
    }

    private static bool IsValidSha256Signature(string signature)
    {
        return signature.Length == 64 && signature.All(Uri.IsHexDigit);
    }

    private static async Task RejectAsync(HttpContext context, int statusCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message
        });
    }
}
