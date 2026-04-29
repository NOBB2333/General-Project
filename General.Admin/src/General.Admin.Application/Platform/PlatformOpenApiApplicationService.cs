using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;

namespace General.Admin.Platform;

public class PlatformOpenApiApplicationService : ITransientDependency
{
    private const string ScopeAll = "*";
    private readonly IStringEncryptionService _encryptionService;
    private readonly IRepository<AppOpenApiApplication, Guid> _repository;

    public PlatformOpenApiApplicationService(
        IStringEncryptionService encryptionService,
        IRepository<AppOpenApiApplication, Guid> repository)
    {
        _encryptionService = encryptionService;
        _repository = repository;
    }

    public async Task<List<PlatformOpenApiApplicationDto>> GetListAsync()
    {
        return (await _repository.GetListAsync())
            .OrderBy(x => x.Name)
            .Select(Map)
            .ToList();
    }

    public async Task<PlatformOpenApiApplicationSecretDto> CreateAsync(PlatformOpenApiApplicationSaveInput input)
    {
        var appId = await GenerateUniqueAppIdAsync();
        var appSecret = GenerateSecret();
        var entity = new AppOpenApiApplication(
            Guid.NewGuid(),
            appId,
            input.Name,
            EncryptSecret(appSecret),
            SerializeScopes(input.Scopes),
            input.IsEnabled,
            input.Remark);

        await _repository.InsertAsync(entity, autoSave: true);
        return MapWithSecret(entity, appSecret);
    }

    public async Task<PlatformOpenApiApplicationDto> UpdateAsync(Guid id, PlatformOpenApiApplicationSaveInput input)
    {
        var entity = await _repository.GetAsync(id);
        entity.Update(input.Name, SerializeScopes(input.Scopes), input.IsEnabled, input.Remark);
        await _repository.UpdateAsync(entity, autoSave: true);
        return Map(entity);
    }

    public async Task<PlatformOpenApiApplicationSecretDto> ResetSecretAsync(Guid id)
    {
        var entity = await _repository.GetAsync(id);
        var appSecret = GenerateSecret();
        entity.ResetSecret(EncryptSecret(appSecret));
        await _repository.UpdateAsync(entity, autoSave: true);
        return MapWithSecret(entity, appSecret);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _repository.DeleteAsync(id, autoSave: true);
    }

    public async Task<PlatformOpenApiSignatureValidationResult> ValidateSignatureAsync(
        string appId,
        string? requiredScope,
        string canonicalText,
        string signature)
    {
        var application = await FindEnabledByAppIdAsync(appId);
        if (application == null)
        {
            return PlatformOpenApiSignatureValidationResult.ApplicationUnavailable;
        }

        if (!HasScope(DeserializeScopes(application.Scopes), requiredScope))
        {
            return PlatformOpenApiSignatureValidationResult.ScopeDenied;
        }

        var expectedSignature = ComputeSignature(DecryptSecret(application), canonicalText);
        if (!FixedTimeEquals(signature, expectedSignature))
        {
            return PlatformOpenApiSignatureValidationResult.SignatureInvalid;
        }

        await MarkUsedAsync(application);
        return PlatformOpenApiSignatureValidationResult.Success;
    }

    private async Task<AppOpenApiApplication?> FindEnabledByAppIdAsync(string appId)
    {
        if (string.IsNullOrWhiteSpace(appId))
        {
            return null;
        }

        return await _repository.FindAsync(x => x.AppId == appId.Trim() && x.IsEnabled);
    }

    private string DecryptSecret(AppOpenApiApplication application)
    {
        return _encryptionService.Decrypt(application.EncryptedSecret) ?? string.Empty;
    }

    private async Task MarkUsedAsync(AppOpenApiApplication application)
    {
        application.Touch();
        await _repository.UpdateAsync(application, autoSave: true);
    }

    private async Task<string> GenerateUniqueAppIdAsync()
    {
        for (var i = 0; i < 8; i++)
        {
            var appId = $"ga_{RandomNumberGenerator.GetHexString(16).ToLowerInvariant()}";
            if (!await _repository.AnyAsync(x => x.AppId == appId))
            {
                return appId;
            }
        }

        throw new InvalidOperationException("生成开放接口 AppId 失败，请重试。");
    }

    private string EncryptSecret(string appSecret)
    {
        return _encryptionService.Encrypt(appSecret) ?? throw new InvalidOperationException("开放接口密钥加密失败。");
    }

    private static string GenerateSecret()
    {
        return $"gas_{RandomNumberGenerator.GetHexString(32).ToLowerInvariant()}";
    }

    private static string SerializeScopes(IEnumerable<string> scopes)
    {
        var normalized = scopes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            normalized.Add(ScopeAll);
        }

        return JsonSerializer.Serialize(normalized);
    }

    private static List<string> DeserializeScopes(string scopes)
    {
        if (string.IsNullOrWhiteSpace(scopes))
        {
            return [ScopeAll];
        }

        return JsonSerializer.Deserialize<List<string>>(scopes) ?? [ScopeAll];
    }

    private static bool HasScope(IReadOnlyCollection<string> scopes, string? requiredScope)
    {
        return string.IsNullOrWhiteSpace(requiredScope) ||
               scopes.Contains(ScopeAll, StringComparer.OrdinalIgnoreCase) ||
               scopes.Contains(requiredScope, StringComparer.OrdinalIgnoreCase);
    }

    private static string ComputeSignature(string secret, string canonicalText)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonicalText))).ToLowerInvariant();
    }

    private static bool FixedTimeEquals(string actualSignature, string expectedSignature)
    {
        try
        {
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(actualSignature),
                Convert.FromHexString(expectedSignature));
        }
        catch
        {
            return false;
        }
    }

    private static PlatformOpenApiApplicationDto Map(AppOpenApiApplication item)
    {
        return new PlatformOpenApiApplicationDto
        {
            AppId = item.AppId,
            CreationTime = item.CreationTime,
            Id = item.Id,
            IsEnabled = item.IsEnabled,
            LastModificationTime = item.LastModificationTime,
            Name = item.Name,
            Remark = item.Remark,
            Scopes = DeserializeScopes(item.Scopes)
        };
    }

    private static PlatformOpenApiApplicationSecretDto MapWithSecret(AppOpenApiApplication item, string appSecret)
    {
        var dto = new PlatformOpenApiApplicationSecretDto
        {
            AppSecret = appSecret,
            AppId = item.AppId,
            CreationTime = item.CreationTime,
            Id = item.Id,
            IsEnabled = item.IsEnabled,
            LastModificationTime = item.LastModificationTime,
            Name = item.Name,
            Remark = item.Remark,
            Scopes = DeserializeScopes(item.Scopes)
        };
        return dto;
    }
}

public enum PlatformOpenApiSignatureValidationResult
{
    Success,
    ApplicationUnavailable,
    ScopeDenied,
    SignatureInvalid
}
