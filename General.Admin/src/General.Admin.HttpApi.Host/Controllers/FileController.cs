using System.Net.Mime;
using System.IO;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
[Route("api/app/file")]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppPlatformFile, Guid> _platformFileRepository;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public FileController(
        IWebHostEnvironment environment,
        ICurrentUser currentUser,
        IRepository<AppPlatformFile, Guid> platformFileRepository,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        _environment = environment;
        _currentUser = currentUser;
        _platformFileRepository = platformFileRepository;
        _userRepository = userRepository;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<List<PhaseOneFileItemDto>>>> GetListAsync()
    {
        var users = (await _userRepository.GetListAsync()).ToDictionary(x => x.Id);
        var files = (await _platformFileRepository.GetListAsync())
            .OrderByDescending(x => x.CreationTime)
            .Select(x =>
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
            })
            .ToList();

        return ApiResponse<List<PhaseOneFileItemDto>>.Ok(files);
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
