using System.Net.Mime;
using System.IO;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentUser _currentUser;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly IRepository<AppPlatformFile, Guid> _platformFileRepository;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public FileController(
        IWebHostEnvironment environment,
        ICurrentUser currentUser,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IRepository<AppPlatformFile, Guid> platformFileRepository,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        _environment = environment;
        _currentUser = currentUser;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _platformFileRepository = platformFileRepository;
        _userRepository = userRepository;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneFileItemDto>>>> GetListAsync([FromQuery] PhaseOneFileListInput input)
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
                return ApiResponse<List<PhaseOneFileItemDto>>.Ok([]);
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
            return new PhaseOneFileItemDto
            {
                Category = x.Category,
                ContentType = x.ContentType,
                FileKey = x.FileKey,
                FileName = x.FileName,
                ParentPath = x.ParentPath,
                Size = x.Size,
                StorageLocation = x.StorageLocation,
                UploadedAt = x.CreationTime,
                UploadedBy = user?.UserName
            };
        }).ToList();

        return ApiResponse<List<PhaseOneFileItemDto>>.Ok(result);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.File.Manage")]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(104_857_600)]
    public async Task<ActionResult<ApiResponse<PhaseOneFileItemDto>>> UploadAsync([FromForm] FileUploadInput input)
    {
        var file = input.File;
        if (file == null || file.Length == 0)
        {
            throw new InvalidOperationException("上传文件不能为空。");
        }

        var originalName = Path.GetFileName(file.FileName);
        var fileKey = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}_{Guid.NewGuid():N}{Path.GetExtension(originalName)}";
        var filePath = Path.Combine(GetStorageDirectory().FullName, fileKey);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream);
        }

        var metadata = new AppPlatformFile(
            Guid.NewGuid(),
            fileKey,
            originalName,
            string.IsNullOrWhiteSpace(file.ContentType)
                ? GetContentType(Path.GetExtension(originalName))
                : file.ContentType,
            file.Length,
            string.IsNullOrWhiteSpace(input.Category) ? "default" : input.Category,
            input.ParentPath,
            filePath,
            _currentUser.Id);
        await _platformFileRepository.InsertAsync(metadata, autoSave: true);

        return ApiResponse<PhaseOneFileItemDto>.Ok(new PhaseOneFileItemDto
        {
            Category = metadata.Category,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? GetContentType(Path.GetExtension(originalName))
                : file.ContentType,
            FileKey = fileKey,
            FileName = originalName,
            ParentPath = metadata.ParentPath,
            Size = file.Length,
            StorageLocation = filePath,
            UploadedBy = _currentUser.UserName,
            UploadedAt = DateTime.UtcNow
        });
    }

    [HttpGet("download/{fileKey}")]
    public IActionResult DownloadAsync(string fileKey)
    {
        var filePath = Path.Combine(GetStorageDirectory().FullName, Path.GetFileName(fileKey));
        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        var originalName = Path.GetFileName(filePath);
        var downloadName = originalName[(originalName.IndexOf('_') + 1)..];
        return PhysicalFile(filePath, GetContentType(Path.GetExtension(filePath)), downloadName);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
    [PlatformEndpoint("Platform.File.Manage")]
    [HttpDelete("{fileKey}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(string fileKey)
    {
        var filePath = Path.Combine(GetStorageDirectory().FullName, Path.GetFileName(fileKey));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        var metadata = await _platformFileRepository.FindAsync(x => x.FileKey == fileKey);
        if (metadata != null)
        {
            await _platformFileRepository.DeleteAsync(metadata, autoSave: true);
        }

        return ApiResponse<bool>.Ok(true);
    }

    private DirectoryInfo GetStorageDirectory()
    {
        var directoryPath = Path.Combine(_environment.ContentRootPath, "App_Data", "phase-one-files");
        Directory.CreateDirectory(directoryPath);
        return new DirectoryInfo(directoryPath);
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
}
