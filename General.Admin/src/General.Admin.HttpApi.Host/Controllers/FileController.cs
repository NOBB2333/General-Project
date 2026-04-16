using System.Net.Mime;
using System.IO;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using General.Admin.Infrastructure;
using General.Admin.PhaseOne;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Auditing;

namespace General.Admin.Controllers;

public class FileUploadInput
{
    [Required]
    public IFormFile File { get; set; } = default!;
}

[ApiController]
[DisableAuditing]
[Authorize]
[Route("api/app/file")]
public class FileController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;

    public FileController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    [HttpGet("list")]
    public ActionResult<ApiResponse<List<PhaseOneFileItemDto>>> GetListAsync()
    {
        var files = GetStorageDirectory()
            .EnumerateFiles()
            .OrderByDescending(x => x.LastWriteTimeUtc)
            .Select(x => new PhaseOneFileItemDto
            {
                ContentType = GetContentType(x.Extension),
                FileKey = x.Name,
                FileName = x.Name[(x.Name.IndexOf('_') + 1)..],
                Size = x.Length,
                UploadedAt = DateTime.SpecifyKind(x.LastWriteTimeUtc, DateTimeKind.Utc)
            })
            .ToList();

        return ApiResponse<List<PhaseOneFileItemDto>>.Ok(files);
    }

    [Authorize(Roles = PhaseOneRoleNames.Admin)]
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

        return ApiResponse<PhaseOneFileItemDto>.Ok(new PhaseOneFileItemDto
        {
            ContentType = string.IsNullOrWhiteSpace(file.ContentType)
                ? GetContentType(Path.GetExtension(originalName))
                : file.ContentType,
            FileKey = fileKey,
            FileName = originalName,
            Size = file.Length,
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
    [HttpDelete("{fileKey}")]
    public ActionResult<ApiResponse<bool>> DeleteAsync(string fileKey)
    {
        var filePath = Path.Combine(GetStorageDirectory().FullName, Path.GetFileName(fileKey));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
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
