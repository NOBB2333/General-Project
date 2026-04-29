using System.Net.Mime;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using Microsoft.AspNetCore.Http;
using General.Admin.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Volo.Abp.Linq;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Auditing;
using Volo.Abp.Users;

namespace General.Admin.Controllers;

public class FileUploadInput
{
    [Required]
    public IFormFile File { get; set; } = default!;

    public string? Category { get; set; }

    public string? ParentPath { get; set; }
}

[ApiController]
[Authorize]
[ApiExplorerSettings(GroupName = ApiDocGroups.Platform)]
[Route("api/app/file")]
public class FileController : ControllerBase
{
    private readonly ICurrentUser _currentUser;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IPlatformFileStorageProviderResolver _fileStorageProviderResolver;
    private readonly PlatformFileStorageOptions _fileStorageOptions;
    private readonly IRepository<AppPlatformFile, Guid> _platformFileRepository;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public FileController(
        ICurrentUser currentUser,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IPlatformFileStorageProviderResolver fileStorageProviderResolver,
        IOptions<PlatformFileStorageOptions> fileStorageOptions,
        IRepository<AppPlatformFile, Guid> platformFileRepository,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _fileStorageProviderResolver = fileStorageProviderResolver;
        _fileStorageOptions = fileStorageOptions.Value;
        _platformFileRepository = platformFileRepository;
        _userRepository = userRepository;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PlatformFileItemDto>>>> GetListAsync([FromQuery] PlatformFileListInput input)
    {
        var queryable = await _platformFileRepository.GetQueryableAsync();
        var keyword = input.Keyword?.Trim();
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            queryable = queryable.Where(x =>
                x.FileName.Contains(keyword) ||
                x.ContentType.Contains(keyword) ||
                x.StorageLocation.Contains(keyword) ||
                (x.ParentPath != null && x.ParentPath.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(input.Category))
        {
            var category = input.Category.Trim();
            queryable = queryable.Where(x => x.Category == category);
        }

        if (input.UploadedFrom.HasValue)
        {
            queryable = queryable.Where(x => x.CreationTime >= input.UploadedFrom.Value);
        }

        if (input.UploadedTo.HasValue)
        {
            var uploadedTo = input.UploadedTo.Value.Date.AddDays(1);
            queryable = queryable.Where(x => x.CreationTime < uploadedTo);
        }

        if (!string.IsNullOrWhiteSpace(input.UploadedBy))
        {
            var uploadedBy = input.UploadedBy.Trim();
            var userQueryable = await _userRepository.GetQueryableAsync();
            var uploaderIds = await _asyncQueryableExecuter.ToListAsync(
                userQueryable
                    .Where(x =>
                        (x.UserName != null && x.UserName.Contains(uploadedBy)) ||
                        (x.Name != null && x.Name.Contains(uploadedBy)))
                    .Select(x => x.Id));

            if (uploaderIds.Count == 0)
            {
                return ApiResponse<List<PlatformFileItemDto>>.Ok([]);
            }

            queryable = queryable.Where(x => x.UploadedByUserId.HasValue && uploaderIds.Contains(x.UploadedByUserId.Value));
        }

        var files = await _asyncQueryableExecuter.ToListAsync(
            queryable.OrderByDescending(x => x.CreationTime));
        var userIds = files
            .Where(x => x.UploadedByUserId.HasValue)
            .Select(x => x.UploadedByUserId!.Value)
            .Distinct()
            .ToList();
        var users = userIds.Count == 0
            ? new Dictionary<Guid, Volo.Abp.Identity.IdentityUser>()
            : (await _userRepository.GetListAsync())
                .Where(x => userIds.Contains(x.Id))
                .ToDictionary(x => x.Id);

        var result = files.Select(x =>
        {
            users.TryGetValue(x.UploadedByUserId ?? Guid.Empty, out var user);
            return new PlatformFileItemDto
            {
                Category = x.Category,
                ContentType = x.ContentType,
                FileKey = x.FileKey,
                FileName = x.FileName,
                ParentPath = x.ParentPath,
                RelativePath = BuildRelativePath(x.Category, x.ParentPath, x.FileName),
                Size = x.Size,
                StorageLocation = x.StorageLocation,
                StorageProvider = x.StorageProvider,
                UploadedAt = x.CreationTime,
                UploadedBy = user?.UserName
            };
        }).ToList();

        return ApiResponse<List<PlatformFileItemDto>>.Ok(result);
    }

    [Authorize(AdminPermissions.Platform.FileManage)]
    [PlatformEndpoint("Platform.File.Manage")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)]
    public async Task<ActionResult<ApiResponse<PlatformFileItemDto>>> UploadAsync([FromForm] FileUploadInput input)
    {
        var file = input.File;
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("上传文件不能为空。");
        }

        if (_fileStorageOptions.MaxFileSizeBytes > 0 && file.Length > _fileStorageOptions.MaxFileSizeBytes)
        {
            throw new InvalidOperationException("上传文件超过大小限制。");
        }

        var originalName = Path.GetFileName(file.FileName);
        var contentType = NormalizeContentType(string.IsNullOrWhiteSpace(file.ContentType)
            ? GetContentType(Path.GetExtension(originalName))
            : file.ContentType);
        if (_fileStorageOptions.AllowedContentTypes.Length > 0 &&
            !_fileStorageOptions.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("上传文件类型不允许。");
        }

        var storageProvider = _fileStorageProviderResolver.Resolve();
        await using (var validationStream = file.OpenReadStream())
        {
            await ValidateFileSignatureAsync(validationStream, contentType, HttpContext.RequestAborted);
        }

        await using var inputStream = file.OpenReadStream();
        var storageResult = await storageProvider.SaveAsync(
            inputStream,
            originalName,
            contentType,
            string.IsNullOrWhiteSpace(input.Category) ? "default" : input.Category,
            input.ParentPath,
            HttpContext.RequestAborted);

        AppPlatformFile metadata;
        try
        {
            metadata = new AppPlatformFile(
                Guid.NewGuid(),
                storageResult.FileKey,
                originalName,
                contentType,
                file.Length,
                string.IsNullOrWhiteSpace(input.Category) ? "default" : input.Category,
                input.ParentPath,
                storageResult.StorageLocation,
                storageResult.Provider,
                _currentUser.Id);
            await _platformFileRepository.InsertAsync(metadata, autoSave: true);
        }
        catch
        {
            await storageProvider.DeleteAsync(
                storageResult.FileKey,
                storageResult.StorageLocation,
                HttpContext.RequestAborted);
            throw;
        }

        return ApiResponse<PlatformFileItemDto>.Ok(new PlatformFileItemDto
        {
            Category = metadata.Category,
            ContentType = contentType,
            FileKey = storageResult.FileKey,
            FileName = originalName,
            ParentPath = metadata.ParentPath,
            RelativePath = BuildRelativePath(metadata.Category, metadata.ParentPath, originalName),
            Size = file.Length,
            StorageLocation = storageResult.StorageLocation,
            StorageProvider = storageResult.Provider,
            UploadedBy = _currentUser.UserName,
            UploadedAt = DateTime.UtcNow
        });
    }

    [HttpGet("download/{fileKey}")]
    public async Task<IActionResult> DownloadAsync(string fileKey)
    {
        var metadata = await _platformFileRepository.FindAsync(x => x.FileKey == fileKey);
        if (metadata == null)
        {
            return NotFound();
        }

        var storageProvider = _fileStorageProviderResolver.Resolve(metadata.StorageProvider);
        var stream = await storageProvider.OpenReadAsync(
            metadata.FileKey,
            metadata.StorageLocation,
            HttpContext.RequestAborted);
        return File(stream, metadata.ContentType, metadata.FileName);
    }

    [Authorize(AdminPermissions.Platform.FileManage)]
    [PlatformEndpoint("Platform.File.Manage")]
    [HttpDelete("{fileKey}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(string fileKey)
    {
        var metadata = await _platformFileRepository.FindAsync(x => x.FileKey == fileKey);
        if (metadata != null)
        {
            var storageProvider = _fileStorageProviderResolver.Resolve(metadata.StorageProvider);
            await storageProvider.DeleteAsync(
                metadata.FileKey,
                metadata.StorageLocation,
                HttpContext.RequestAborted);
            await _platformFileRepository.DeleteAsync(metadata, autoSave: true);
        }

        return ApiResponse<bool>.Ok(true);
    }

    private static string BuildRelativePath(string category, string? parentPath, string fileName)
    {
        return string.Join(
            '/',
            new[] { category, parentPath, fileName }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim().Replace('\\', '/').Trim('/')));
    }

    private static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".doc" => MediaTypeNames.Application.Octet,
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".jpg" or ".jpeg" => MediaTypeNames.Image.Jpeg,
            ".pdf" => MediaTypeNames.Application.Pdf,
            ".png" => "image/png",
            ".txt" => MediaTypeNames.Text.Plain,
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => MediaTypeNames.Application.Octet
        };
    }

    private static string NormalizeContentType(string contentType)
    {
        var normalized = contentType.Split(';', 2)[0].Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? MediaTypeNames.Application.Octet
            : normalized;
    }

    private static async Task ValidateFileSignatureAsync(
        Stream stream,
        string contentType,
        CancellationToken cancellationToken)
    {
        var buffer = new byte[512];
        var read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken);
        if (!IsAllowedSignature(buffer.AsSpan(0, read), contentType))
        {
            throw new InvalidOperationException("上传文件内容与文件类型不匹配。");
        }
    }

    private static bool IsAllowedSignature(ReadOnlySpan<byte> header, string contentType)
    {
        return contentType.ToLowerInvariant() switch
        {
            MediaTypeNames.Image.Jpeg => HasPrefix(header, [0xFF, 0xD8, 0xFF]),
            "image/png" => HasPrefix(header, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]),
            MediaTypeNames.Application.Pdf => HasPrefix(header, [0x25, 0x50, 0x44, 0x46]),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => IsZipHeader(header),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => IsZipHeader(header),
            MediaTypeNames.Text.Plain => IsPlainTextHeader(header),
            _ => true
        };
    }

    private static bool HasPrefix(ReadOnlySpan<byte> header, ReadOnlySpan<byte> prefix)
    {
        return header.Length >= prefix.Length && header[..prefix.Length].SequenceEqual(prefix);
    }

    private static bool IsZipHeader(ReadOnlySpan<byte> header)
    {
        return HasPrefix(header, [0x50, 0x4B, 0x03, 0x04]) ||
               HasPrefix(header, [0x50, 0x4B, 0x05, 0x06]) ||
               HasPrefix(header, [0x50, 0x4B, 0x07, 0x08]);
    }

    private static bool IsPlainTextHeader(ReadOnlySpan<byte> header)
    {
        if (header.IsEmpty)
        {
            return false;
        }

        foreach (var value in header)
        {
            if (value == 0)
            {
                return false;
            }
        }

        return true;
    }
}
