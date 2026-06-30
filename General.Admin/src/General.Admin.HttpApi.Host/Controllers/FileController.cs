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
using ProjectEntity = General.Admin.Project.Project;
using ProjectSprintEntity = General.Admin.Project.ProjectSprint;
using ProjectMeetingRecordEntity = General.Admin.Project.ProjectMeetingRecord;
using ProjectReportEntity = General.Admin.Project.ProjectReport;
using ProjectRequirementEntity = General.Admin.Project.ProjectRequirement;
using ProjectTaskEntity = General.Admin.Project.ProjectTask;

namespace General.Admin.Controllers;

public class FileUploadInput
{
    [Required]
    public IFormFile File { get; set; } = default!;

    public string? BusinessId { get; set; }

    public string? BusinessType { get; set; }

    public string? Category { get; set; }

    public bool IsPublic { get; set; }

    public string? ParentPath { get; set; }

    public Guid? StorageSourceId { get; set; }
}

internal class PlatformFileBusinessObjectDescriptor
{
    public string BusinessId { get; set; } = string.Empty;

    public string BusinessType { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid? ProjectId { get; set; }

    public string? ProjectName { get; set; }

    public string? Tab { get; set; }

    public string TypeName { get; set; } = string.Empty;

    public List<string> SearchTokens { get; set; } = [];
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
    private readonly PlatformFileAccessService _fileAccessService;
    private readonly PlatformFileStorageSourceService _fileStorageSourceService;
    private readonly PlatformFileVersionService _fileVersionService;
    private readonly PlatformFileStorageOptions _fileStorageOptions;
    private readonly IRepository<AppPlatformFile, Guid> _platformFileRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectSprintEntity, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectReportEntity, Guid> _projectReportRepository;
    private readonly IRepository<ProjectMeetingRecordEntity, Guid> _projectMeetingRecordRepository;
    private readonly IRepository<ProjectRequirementEntity, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectTaskEntity, Guid> _projectTaskRepository;
    private readonly IRepository<BusinessAcceptance, Guid> _businessAcceptanceRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _businessBudgetRepository;
    private readonly IRepository<BusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<BusinessInvoice, Guid> _businessInvoiceRepository;
    private readonly IRepository<BusinessProcurement, Guid> _businessProcurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _businessReceivableRepository;
    private readonly IRepository<Volo.Abp.Identity.IdentityUser, Guid> _userRepository;

    public FileController(
        ICurrentUser currentUser,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        IPlatformFileStorageProviderResolver fileStorageProviderResolver,
        PlatformFileAccessService fileAccessService,
        PlatformFileStorageSourceService fileStorageSourceService,
        PlatformFileVersionService fileVersionService,
        IOptions<PlatformFileStorageOptions> fileStorageOptions,
        IRepository<AppPlatformFile, Guid> platformFileRepository,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<ProjectSprintEntity, Guid> projectSprintRepository,
        IRepository<ProjectReportEntity, Guid> projectReportRepository,
        IRepository<ProjectMeetingRecordEntity, Guid> projectMeetingRecordRepository,
        IRepository<ProjectRequirementEntity, Guid> projectRequirementRepository,
        IRepository<ProjectTaskEntity, Guid> projectTaskRepository,
        IRepository<BusinessAcceptance, Guid> businessAcceptanceRepository,
        IRepository<BusinessBudgetExecution, Guid> businessBudgetRepository,
        IRepository<BusinessChain, Guid> businessChainRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<BusinessInvoice, Guid> businessInvoiceRepository,
        IRepository<BusinessProcurement, Guid> businessProcurementRepository,
        IRepository<BusinessReceivable, Guid> businessReceivableRepository,
        IRepository<Volo.Abp.Identity.IdentityUser, Guid> userRepository)
    {
        _currentUser = currentUser;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _fileStorageProviderResolver = fileStorageProviderResolver;
        _fileAccessService = fileAccessService;
        _fileStorageSourceService = fileStorageSourceService;
        _fileVersionService = fileVersionService;
        _fileStorageOptions = fileStorageOptions.Value;
        _platformFileRepository = platformFileRepository;
        _projectRepository = projectRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectReportRepository = projectReportRepository;
        _projectMeetingRecordRepository = projectMeetingRecordRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectTaskRepository = projectTaskRepository;
        _businessAcceptanceRepository = businessAcceptanceRepository;
        _businessBudgetRepository = businessBudgetRepository;
        _businessChainRepository = businessChainRepository;
        _businessContractRepository = businessContractRepository;
        _businessInvoiceRepository = businessInvoiceRepository;
        _businessProcurementRepository = businessProcurementRepository;
        _businessReceivableRepository = businessReceivableRepository;
        _userRepository = userRepository;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<PlatformPagedResultDto<PlatformFileItemDto>>>> GetListAsync([FromQuery] PlatformFileListInput input)
    {
        var queryable = await BuildFilteredQueryableAsync(input);

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var maxResultCount = Math.Clamp(input.MaxResultCount, 1, 100);
        var skipCount = Math.Max(0, input.SkipCount);
        var files = await _asyncQueryableExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(skipCount)
                .Take(maxResultCount));
        var sourceIds = files
            .Where(x => x.StorageSourceId.HasValue)
            .Select(x => x.StorageSourceId!.Value)
            .Distinct()
            .ToHashSet();
        var sourceNames = sourceIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _fileStorageSourceService.ResolveSourceNameMapAsync(sourceIds);
        var userIds = files
            .Where(x => x.UploadedByUserId.HasValue)
            .Select(x => x.UploadedByUserId!.Value)
            .Distinct()
            .ToList();
        var users = userIds.Count == 0
            ? new Dictionary<Guid, Volo.Abp.Identity.IdentityUser>()
            : (await _asyncQueryableExecuter.ToListAsync(
                    (await _userRepository.GetQueryableAsync()).Where(x => userIds.Contains(x.Id))))
                .ToDictionary(x => x.Id);
        var businessDescriptors = await ResolveBusinessDescriptorsAsync(files);

        var result = files.Select(x =>
        {
            users.TryGetValue(x.UploadedByUserId ?? Guid.Empty, out var user);
            businessDescriptors.TryGetValue(PlatformFileBusinessLinkResolver.BuildBusinessKey(x.BusinessType, x.BusinessId), out var businessDescriptor);
            var fileBusinessDescriptor = ResolveFileBusinessDescriptor(x, businessDescriptor);
            return new PlatformFileItemDto
            {
                Category = x.Category,
                BusinessId = x.BusinessId,
                BusinessType = x.BusinessType,
                BusinessObjectCode = fileBusinessDescriptor?.Code,
                BusinessObjectName = fileBusinessDescriptor?.Name,
                BusinessObjectTypeName = fileBusinessDescriptor?.TypeName,
                BusinessProjectId = fileBusinessDescriptor?.ProjectId,
                BusinessProjectName = fileBusinessDescriptor?.ProjectName,
                BusinessJumpPath = ResolveBusinessJumpPath(fileBusinessDescriptor, x.BusinessType, x.BusinessId),
                BusinessJumpQuery = ResolveBusinessJumpQuery(fileBusinessDescriptor, x.BusinessType, x.BusinessId, x.Category),
                BucketName = x.BucketName,
                ContentType = x.ContentType,
                Id = x.Id,
                FileKey = x.FileKey,
                FileName = x.FileName,
                IsCurrentVersion = x.IsCurrentVersion,
                IsPublic = x.IsPublic,
                ParentPath = x.ParentPath,
                PreviousVersionFileId = x.PreviousVersionFileId,
                RelativePath = PlatformFilePathPolicy.BuildRelativePath(x.Category, x.ParentPath, x.FileName),
                ReplacedByFileId = x.ReplacedByFileId,
                Size = x.Size,
                StorageLocation = x.StorageLocation,
                StorageProvider = x.StorageProvider,
                StorageSourceId = x.StorageSourceId,
                StorageSourceName = x.StorageSourceId.HasValue && sourceNames.TryGetValue(x.StorageSourceId.Value, out var sourceName)
                    ? sourceName
                    : null,
                UploadedAt = x.CreationTime,
                UploadedBy = user?.UserName,
                VersionNo = x.VersionNo
            };
        }).ToList();

        return ApiResponse<PlatformPagedResultDto<PlatformFileItemDto>>.Ok(new PlatformPagedResultDto<PlatformFileItemDto>
        {
            Items = result,
            TotalCount = totalCount
        });
    }

    [HttpGet("tree")]
    public async Task<ActionResult<ApiResponse<List<PlatformFileTreeItemDto>>>> GetTreeAsync([FromQuery] PlatformFileListInput input)
    {
        var queryable = await BuildFilteredQueryableAsync(input, applyParentPathFilter: false);
        var items = await _asyncQueryableExecuter.ToListAsync(
            queryable
                .Select(x => new { x.BusinessType, x.Category, x.ParentPath })
                .Distinct()
                .OrderBy(x => x.BusinessType)
                .ThenBy(x => x.Category)
                .ThenBy(x => x.ParentPath));

        return ApiResponse<List<PlatformFileTreeItemDto>>.Ok(items.Select(x => new PlatformFileTreeItemDto
        {
            BusinessType = x.BusinessType,
            Category = x.Category,
            ParentPath = x.ParentPath
        }).ToList());
    }

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
        var requestContentType = PlatformFileContentTypes.Normalize(file.ContentType);
        var extensionContentType = PlatformFileContentTypes.ResolveFromExtension(Path.GetExtension(originalName));
        var contentType = string.IsNullOrWhiteSpace(file.ContentType) ||
                          requestContentType == MediaTypeNames.Application.Octet
            ? extensionContentType
            : requestContentType;
        if (_fileStorageOptions.AllowedContentTypes.Length > 0 &&
            !_fileStorageOptions.AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("上传文件类型不允许。");
        }

        var (category, parentPath) = PlatformFilePathPolicy.NormalizeUploadTarget(
            input.Category,
            input.ParentPath,
            input.BusinessType,
            input.BusinessId);
        await _fileAccessService.EnsureCanUploadAsync(
            input.BusinessType,
            input.BusinessId,
            category);
        var storageSource = await _fileStorageSourceService.ResolveDescriptorAsync(input.StorageSourceId, requireEnabled: true);
        var storageProvider = _fileStorageProviderResolver.Resolve(storageSource?.ProviderName);
        await using (var validationStream = file.OpenReadStream())
        {
            await ValidateFileSignatureAsync(validationStream, contentType, HttpContext.RequestAborted);
        }

        await using var inputStream = file.OpenReadStream();
        var storageResult = await storageProvider.SaveAsync(
            inputStream,
            originalName,
            contentType,
            category,
            parentPath,
            storageSource,
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
                category,
                parentPath,
                storageResult.StorageLocation,
                storageResult.Provider,
                _currentUser.Id,
                storageSource?.SourceId,
                ResolveBucketName(storageResult.Provider, storageSource),
                input.IsPublic || storageSource?.IsPublic == true,
                input.BusinessType,
                input.BusinessId);
            await _fileVersionService.ApplyVersionChainAsync(metadata);
            await _platformFileRepository.InsertAsync(metadata, autoSave: true);
        }
        catch
        {
            await storageProvider.DeleteAsync(
                storageResult.FileKey,
                storageResult.StorageLocation,
                storageSource,
                HttpContext.RequestAborted);
            throw;
        }

        return ApiResponse<PlatformFileItemDto>.Ok(new PlatformFileItemDto
        {
            Category = metadata.Category,
            BusinessId = metadata.BusinessId,
            BusinessType = metadata.BusinessType,
            BucketName = metadata.BucketName,
            ContentType = contentType,
            Id = metadata.Id,
            FileKey = storageResult.FileKey,
            FileName = originalName,
            IsCurrentVersion = metadata.IsCurrentVersion,
            IsPublic = metadata.IsPublic,
            ParentPath = metadata.ParentPath,
            PreviousVersionFileId = metadata.PreviousVersionFileId,
            RelativePath = PlatformFilePathPolicy.BuildRelativePath(metadata.Category, metadata.ParentPath, originalName),
            ReplacedByFileId = metadata.ReplacedByFileId,
            Size = file.Length,
            StorageLocation = storageResult.StorageLocation,
            StorageProvider = storageResult.Provider,
            StorageSourceId = metadata.StorageSourceId,
            StorageSourceName = storageSource?.Name,
            UploadedBy = _currentUser.UserName,
            UploadedAt = DateTime.UtcNow,
            VersionNo = metadata.VersionNo
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

        await _fileAccessService.EnsureCanReadAsync(metadata);

        var storageSource = metadata.StorageSourceId.HasValue
            ? await _fileStorageSourceService.ResolveDescriptorAsync(metadata.StorageSourceId, requireEnabled: true)
            : null;
        var storageProvider = _fileStorageProviderResolver.Resolve(storageSource?.ProviderName ?? metadata.StorageProvider);
        var stream = await storageProvider.OpenReadAsync(
            metadata.FileKey,
            metadata.StorageLocation,
            storageSource,
            HttpContext.RequestAborted);
        return File(stream, metadata.ContentType, metadata.FileName);
    }

    [HttpGet("preview/{fileKey}")]
    public async Task<IActionResult> PreviewAsync(string fileKey)
    {
        var metadata = await _platformFileRepository.FindAsync(x => x.FileKey == fileKey);
        if (metadata == null)
        {
            return NotFound();
        }

        await _fileAccessService.EnsureCanReadAsync(metadata);

        var storageSource = metadata.StorageSourceId.HasValue
            ? await _fileStorageSourceService.ResolveDescriptorAsync(metadata.StorageSourceId, requireEnabled: true)
            : null;
        var storageProvider = _fileStorageProviderResolver.Resolve(storageSource?.ProviderName ?? metadata.StorageProvider);
        var stream = await storageProvider.OpenReadAsync(
            metadata.FileKey,
            metadata.StorageLocation,
            storageSource,
            HttpContext.RequestAborted);
        Response.Headers.ContentDisposition = $"inline; filename=\"{Uri.EscapeDataString(metadata.FileName)}\"";
        return File(stream, PlatformFileContentTypes.ResolvePreviewContentType(metadata.ContentType));
    }

    [Authorize(AdminPermissions.Platform.FileManage)]
    [PlatformEndpoint("Platform.File.Manage")]
    [HttpPut("{fileKey}/metadata")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateMetadataAsync(
        string fileKey,
        [FromBody] PlatformFileMetadataInput input)
    {
        var metadata = await _platformFileRepository.FindAsync(x => x.FileKey == fileKey);
        if (metadata == null)
        {
            return ApiResponse<bool>.Ok(true);
        }

        metadata.UpdateMetadata(input.FileName, input.IsPublic, input.BusinessType, input.BusinessId);
        await _platformFileRepository.UpdateAsync(metadata, autoSave: true);
        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.FileManage)]
    [PlatformEndpoint("Platform.File.Manage")]
    [HttpDelete("{fileKey}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAsync(string fileKey)
    {
        var metadata = await _platformFileRepository.FindAsync(x => x.FileKey == fileKey);
        if (metadata != null)
        {
            await _fileVersionService.RestorePreviousCurrentVersionAsync(metadata);
            await _platformFileRepository.DeleteAsync(metadata, autoSave: true);
        }

        return ApiResponse<bool>.Ok(true);
    }

    [Authorize(AdminPermissions.Platform.FileManage)]
    [PlatformEndpoint("Platform.File.Manage")]
    [HttpPost("batch-delete")]
    public async Task<ActionResult<ApiResponse<bool>>> BatchDeleteAsync([FromBody] PlatformFileBatchDeleteInput input)
    {
        var fileKeys = input.FileKeys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();
        if (fileKeys.Count == 0)
        {
            return ApiResponse<bool>.Ok(true);
        }

        var queryable = await _platformFileRepository.GetQueryableAsync();
        var files = await _asyncQueryableExecuter.ToListAsync(queryable.Where(x => fileKeys.Contains(x.FileKey)));
        if (files.Count > 0)
        {
            await _platformFileRepository.DeleteManyAsync(files, autoSave: true);
        }

        return ApiResponse<bool>.Ok(true);
    }

    private string? ResolveBucketName(string providerName, PlatformFileStorageSourceDescriptor? source)
    {
        if (!string.IsNullOrWhiteSpace(source?.BucketName))
        {
            return source.BucketName;
        }

        return providerName.ToLowerInvariant() switch
        {
            "aliyunoss" => _fileStorageOptions.AliyunOSS.BucketName,
            "minio" => _fileStorageOptions.MinIO.BucketName,
            _ => null
        };
    }

    private async Task<IQueryable<AppPlatformFile>> BuildFilteredQueryableAsync(
        PlatformFileListInput input,
        bool applyParentPathFilter = true)
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
            var category = PlatformFilePathPolicy.NormalizeCategory(input.Category);
            queryable = queryable.Where(x => x.Category == category);
        }
        else if (!string.IsNullOrWhiteSpace(input.CategoryPrefix))
        {
            var categoryPrefix = PlatformFilePathPolicy.NormalizeCategory(input.CategoryPrefix);
            queryable = queryable.Where(x => x.Category == categoryPrefix || x.Category.StartsWith($"{categoryPrefix}/"));
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessScope))
        {
            var businessScope = PlatformFilePathPolicy.NormalizeCategory(input.BusinessScope);
            queryable = queryable.Where(x =>
                x.BusinessType == businessScope ||
                x.Category == businessScope ||
                x.Category.StartsWith($"{businessScope}/"));
        }

        if (applyParentPathFilter && !string.IsNullOrWhiteSpace(input.ParentPath))
        {
            var parentPath = PlatformFilePathPolicy.NormalizeParentPath(input.ParentPath);
            queryable = queryable.Where(x => x.ParentPath == parentPath);
        }

        if (input.StorageSourceId.HasValue)
        {
            queryable = queryable.Where(x => x.StorageSourceId == input.StorageSourceId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessType))
        {
            var businessType = input.BusinessType.Trim();
            queryable = queryable.Where(x => x.BusinessType == businessType);
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessId))
        {
            var businessId = input.BusinessId.Trim();
            queryable = queryable.Where(x => x.BusinessId == businessId);
        }

        if (input.BusinessProjectId.HasValue)
        {
            var matchedKeys = await ResolveBusinessKeysByProjectAsync(input.BusinessProjectId.Value);
            queryable = matchedKeys.Count == 0
                ? queryable.Where(_ => false)
                : queryable.Where(x =>
                    x.BusinessType != null &&
                    x.BusinessId != null &&
                    matchedKeys.Contains(x.BusinessType + ":" + x.BusinessId));
        }

        if (!string.IsNullOrWhiteSpace(input.BusinessKeyword))
        {
            var matchedKeys = await ResolveBusinessKeysByKeywordAsync(input.BusinessKeyword);
            queryable = matchedKeys.Count == 0
                ? queryable.Where(_ => false)
                : queryable.Where(x =>
                    x.BusinessType != null &&
                    x.BusinessId != null &&
                    matchedKeys.Contains(x.BusinessType + ":" + x.BusinessId));
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

            queryable = uploaderIds.Count == 0
                ? queryable.Where(_ => false)
                : queryable.Where(x => x.UploadedByUserId.HasValue && uploaderIds.Contains(x.UploadedByUserId.Value));
        }

        return await _fileAccessService.ApplyReadFilterAsync(queryable);
    }

    private async Task<HashSet<string>> ResolveBusinessKeysByProjectAsync(Guid projectId)
    {
        var descriptors = new List<PlatformFileBusinessObjectDescriptor>();
        descriptors.AddRange(await QueryProjectDescriptorsByIdsAsync([projectId]));
        descriptors.AddRange(await QueryContractDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryProcurementDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryBudgetDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryReceivableDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryInvoiceDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryAcceptanceDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryBusinessChainDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QuerySprintDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryRequirementDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryTaskDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryReportDescriptorsByProjectAsync(projectId));
        descriptors.AddRange(await QueryMeetingRecordDescriptorsByProjectAsync(projectId));

        return descriptors
            .SelectMany(item => PlatformFileBusinessLinkResolver.ResolveBusinessKeyAliases(item.BusinessType, item.BusinessId))
            .Where(item => item != string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<HashSet<string>> ResolveBusinessKeysByKeywordAsync(string? businessKeyword)
    {
        var keyword = businessKeyword?.Trim();
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        var descriptors = new List<PlatformFileBusinessObjectDescriptor>();
        descriptors.AddRange(await QueryProjectDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryContractDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryProcurementDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryBudgetDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryReceivableDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryInvoiceDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryAcceptanceDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryBusinessChainDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QuerySprintDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryRequirementDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryTaskDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryReportDescriptorsByKeywordAsync(keyword));
        descriptors.AddRange(await QueryMeetingRecordDescriptorsByKeywordAsync(keyword));

        return descriptors
            .SelectMany(item => PlatformFileBusinessLinkResolver.ResolveBusinessKeyAliases(item.BusinessType, item.BusinessId))
            .Where(item => item != string.Empty)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    private async Task<Dictionary<string, PlatformFileBusinessObjectDescriptor>> ResolveBusinessDescriptorsAsync(
        IReadOnlyCollection<AppPlatformFile> files)
    {
        var descriptors = new Dictionary<string, PlatformFileBusinessObjectDescriptor>(StringComparer.OrdinalIgnoreCase);
        var groupedIds = files
            .Where(item => !string.IsNullOrWhiteSpace(item.BusinessType) && !string.IsNullOrWhiteSpace(item.BusinessId))
            .GroupBy(item => item.BusinessType!.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                item => item.Key,
                item => item
                    .Select(file => file.BusinessId!.Trim())
                    .Where(id => Guid.TryParse(id, out _))
                    .Select(Guid.Parse)
                    .Distinct()
                    .ToList(),
                StringComparer.OrdinalIgnoreCase);

        async Task AddAsync(string businessType, Func<List<Guid>, Task<List<PlatformFileBusinessObjectDescriptor>>> resolver)
        {
            if (!groupedIds.TryGetValue(businessType, out var ids) || ids.Count == 0)
            {
                return;
            }

            foreach (var descriptor in await resolver(ids))
            {
                descriptor.BusinessType = businessType;
                descriptors[PlatformFileBusinessLinkResolver.BuildBusinessKey(businessType, descriptor.BusinessId)] = descriptor;
            }
        }

        await AddAsync("Project", QueryProjectDescriptorsByIdsAsync);
        await AddAsync("BusinessContract", QueryContractDescriptorsByIdsAsync);
        await AddAsync("Contract", QueryContractDescriptorsByIdsAsync);
        await AddAsync("BusinessProcurement", QueryProcurementDescriptorsByIdsAsync);
        await AddAsync("Procurement", QueryProcurementDescriptorsByIdsAsync);
        await AddAsync("BusinessBudget", QueryBudgetDescriptorsByIdsAsync);
        await AddAsync("Budget", QueryBudgetDescriptorsByIdsAsync);
        await AddAsync("BusinessReceivable", QueryReceivableDescriptorsByIdsAsync);
        await AddAsync("Receivable", QueryReceivableDescriptorsByIdsAsync);
        await AddAsync("BusinessInvoice", QueryInvoiceDescriptorsByIdsAsync);
        await AddAsync("Invoice", QueryInvoiceDescriptorsByIdsAsync);
        await AddAsync("BusinessAcceptance", QueryAcceptanceDescriptorsByIdsAsync);
        await AddAsync("Acceptance", QueryAcceptanceDescriptorsByIdsAsync);
        await AddAsync("BusinessChain", QueryBusinessChainDescriptorsByIdsAsync);
        await AddAsync("Chain", QueryBusinessChainDescriptorsByIdsAsync);
        await AddAsync("ProjectSprint", QuerySprintDescriptorsByIdsAsync);
        await AddAsync("ProjectRequirement", QueryRequirementDescriptorsByIdsAsync);
        await AddAsync("ProjectTask", QueryTaskDescriptorsByIdsAsync);
        await AddAsync("ProjectReport", QueryReportDescriptorsByIdsAsync);
        await AddAsync("ProjectMeetingRecord", QueryMeetingRecordDescriptorsByIdsAsync);

        return descriptors;
    }

    private static PlatformFileBusinessObjectDescriptor? ResolveFileBusinessDescriptor(
        AppPlatformFile file,
        PlatformFileBusinessObjectDescriptor? descriptor)
    {
        if (descriptor == null)
        {
            return null;
        }

        var type = descriptor.BusinessType;
        if (!type.Equals("Project", StringComparison.OrdinalIgnoreCase) &&
            !type.Equals("ProjectSprint", StringComparison.OrdinalIgnoreCase))
        {
            return descriptor;
        }

        var typeName = PlatformFileBusinessLinkResolver.ResolveProjectObjectTypeNameByCategory(file.Category);
        var contextName = type.Equals("ProjectSprint", StringComparison.OrdinalIgnoreCase)
            ? string.Join(
                " / ",
                new[] { descriptor.ProjectName, descriptor.Name }.Where(item => !string.IsNullOrWhiteSpace(item)))
            : descriptor.ProjectName ?? descriptor.Name;
        var objectName = PlatformFileBusinessLinkResolver.ResolveProjectObjectNameByCategory(
            file.Category,
            contextName);
        if (string.IsNullOrWhiteSpace(typeName) && string.IsNullOrWhiteSpace(objectName))
        {
            return descriptor;
        }

        return new PlatformFileBusinessObjectDescriptor
        {
            BusinessId = descriptor.BusinessId,
            BusinessType = descriptor.BusinessType,
            Code = descriptor.Code,
            Name = objectName ?? descriptor.Name,
            ProjectId = descriptor.ProjectId,
            ProjectName = descriptor.ProjectName,
            SearchTokens = descriptor.SearchTokens,
            Tab = PlatformFileBusinessLinkResolver.ResolveProjectTabByCategory(file.Category),
            TypeName = typeName ?? descriptor.TypeName
        };
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryProjectDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _projectRepository.GetQueryableAsync();
        var projects = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item => item.Name.Contains(keyword) || item.ProjectCode.Contains(keyword))
            .Take(200));

        return projects.Select(CreateProjectDescriptor).ToList();
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryProjectDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _projectRepository.GetQueryableAsync();
        var projects = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return projects.Select(CreateProjectDescriptor).ToList();
    }

    private PlatformFileBusinessObjectDescriptor CreateProjectDescriptor(ProjectEntity project)
    {
        return new PlatformFileBusinessObjectDescriptor
        {
            BusinessId = project.Id.ToString(),
            BusinessType = "Project",
            Code = project.ProjectCode,
            Name = project.Name,
            ProjectId = project.Id,
            ProjectName = project.Name,
            Tab = PlatformFileBusinessLinkResolver.ResolveProjectTabByCategory(null),
            TypeName = "项目",
            SearchTokens = [project.ProjectCode, project.Name]
        };
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryContractDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _businessContractRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.ContractCode.Contains(keyword) ||
                item.CounterpartyName.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessContract",
            "合同",
            "income",
            item => item.Id,
            item => item.ProjectId,
            item => item.ContractCode,
            item => item.Title,
            item => [item.ContractCode, item.Title, item.CounterpartyName]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryContractDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _businessContractRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessContract",
            "合同",
            "income",
            item => item.Id,
            item => item.ProjectId,
            item => item.ContractCode,
            item => item.Title,
            item => [item.ContractCode, item.Title, item.CounterpartyName]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryContractDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _businessContractRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessContract",
            "合同",
            "income",
            item => item.Id,
            item => item.ProjectId,
            item => item.ContractCode,
            item => item.Title,
            item => [item.ContractCode, item.Title, item.CounterpartyName]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryProcurementDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _businessProcurementRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.ProcurementCode.Contains(keyword) ||
                item.SupplierName.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessProcurement",
            "采购",
            "income",
            item => item.Id,
            item => item.ProjectId,
            item => item.ProcurementCode,
            item => item.Title,
            item => [item.ProcurementCode, item.Title, item.SupplierName]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryProcurementDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _businessProcurementRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessProcurement",
            "采购",
            "income",
            item => item.Id,
            item => item.ProjectId,
            item => item.ProcurementCode,
            item => item.Title,
            item => [item.ProcurementCode, item.Title, item.SupplierName]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryProcurementDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _businessProcurementRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessProcurement",
            "采购",
            "income",
            item => item.Id,
            item => item.ProjectId,
            item => item.ProcurementCode,
            item => item.Title,
            item => [item.ProcurementCode, item.Title, item.SupplierName]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryBudgetDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _businessBudgetRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item => item.BudgetCode.Contains(keyword) || item.Category.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessBudget",
            "预算",
            "budget",
            item => item.Id,
            item => item.ProjectId,
            item => item.BudgetCode,
            item => item.Category,
            item => [item.BudgetCode, item.Category]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryBudgetDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _businessBudgetRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessBudget",
            "预算",
            "budget",
            item => item.Id,
            item => item.ProjectId,
            item => item.BudgetCode,
            item => item.Category,
            item => [item.BudgetCode, item.Category]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryBudgetDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _businessBudgetRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessBudget",
            "预算",
            "budget",
            item => item.Id,
            item => item.ProjectId,
            item => item.BudgetCode,
            item => item.Category,
            item => [item.BudgetCode, item.Category]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryReceivableDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _businessReceivableRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.ReceivableCode.Contains(keyword) ||
                item.InvoiceCode.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessReceivable",
            "回款",
            "receivable",
            item => item.Id,
            item => item.ProjectId,
            item => item.ReceivableCode,
            item => item.Title,
            item => [item.ReceivableCode, item.Title, item.InvoiceCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryReceivableDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _businessReceivableRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessReceivable",
            "回款",
            "receivable",
            item => item.Id,
            item => item.ProjectId,
            item => item.ReceivableCode,
            item => item.Title,
            item => [item.ReceivableCode, item.Title, item.InvoiceCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryReceivableDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _businessReceivableRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessReceivable",
            "回款",
            "receivable",
            item => item.Id,
            item => item.ProjectId,
            item => item.ReceivableCode,
            item => item.Title,
            item => [item.ReceivableCode, item.Title, item.InvoiceCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryInvoiceDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _businessInvoiceRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.InvoiceCode.Contains(keyword) ||
                item.ReceivableCode.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessInvoice",
            "发票",
            "receivable",
            item => item.Id,
            item => item.ProjectId,
            item => item.InvoiceCode,
            item => item.Title,
            item => [item.InvoiceCode, item.Title, item.ReceivableCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryInvoiceDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _businessInvoiceRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessInvoice",
            "发票",
            "receivable",
            item => item.Id,
            item => item.ProjectId,
            item => item.InvoiceCode,
            item => item.Title,
            item => [item.InvoiceCode, item.Title, item.ReceivableCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryInvoiceDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _businessInvoiceRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessInvoice",
            "发票",
            "receivable",
            item => item.Id,
            item => item.ProjectId,
            item => item.InvoiceCode,
            item => item.Title,
            item => [item.InvoiceCode, item.Title, item.ReceivableCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryAcceptanceDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _businessAcceptanceRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.AcceptanceCode.Contains(keyword) ||
                item.LinkedContractCode.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessAcceptance",
            "验收",
            "profit",
            item => item.Id,
            item => item.ProjectId,
            item => item.AcceptanceCode,
            item => item.Title,
            item => [item.AcceptanceCode, item.Title, item.LinkedContractCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryAcceptanceDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _businessAcceptanceRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessAcceptance",
            "验收",
            "profit",
            item => item.Id,
            item => item.ProjectId,
            item => item.AcceptanceCode,
            item => item.Title,
            item => [item.AcceptanceCode, item.Title, item.LinkedContractCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryAcceptanceDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _businessAcceptanceRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessAcceptance",
            "验收",
            "profit",
            item => item.Id,
            item => item.ProjectId,
            item => item.AcceptanceCode,
            item => item.Title,
            item => [item.AcceptanceCode, item.Title, item.LinkedContractCode]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryBusinessChainDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _businessChainRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.ChainCode.Contains(keyword) ||
                item.Summary.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessChain",
            "经营链路",
            "chain",
            item => item.Id,
            item => item.ProjectId,
            item => item.ChainCode,
            item => item.Title,
            item => [item.ChainCode, item.Title, item.Summary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryBusinessChainDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _businessChainRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessChain",
            "经营链路",
            "chain",
            item => item.Id,
            item => item.ProjectId,
            item => item.ChainCode,
            item => item.Title,
            item => [item.ChainCode, item.Title, item.Summary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryBusinessChainDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _businessChainRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "BusinessChain",
            "经营链路",
            "chain",
            item => item.Id,
            item => item.ProjectId,
            item => item.ChainCode,
            item => item.Title,
            item => [item.ChainCode, item.Title, item.Summary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryRequirementDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _projectRequirementRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.RequirementCode.Contains(keyword) ||
                item.Description.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectRequirement",
            "项目需求",
            "tasks",
            item => item.Id,
            item => item.ProjectId,
            item => item.RequirementCode,
            item => item.Title,
            item => [item.RequirementCode, item.Title, item.Description]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QuerySprintDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _projectSprintRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Name.Contains(keyword) ||
                item.SprintCode.Contains(keyword) ||
                item.Summary.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectSprint",
            "Sprint",
            "delivery",
            item => item.Id,
            item => item.ProjectId,
            item => item.SprintCode,
            item => item.Name,
            item => [item.SprintCode, item.Name, item.Summary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QuerySprintDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _projectSprintRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectSprint",
            "Sprint",
            "delivery",
            item => item.Id,
            item => item.ProjectId,
            item => item.SprintCode,
            item => item.Name,
            item => [item.SprintCode, item.Name, item.Summary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QuerySprintDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _projectSprintRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectSprint",
            "Sprint",
            "delivery",
            item => item.Id,
            item => item.ProjectId,
            item => item.SprintCode,
            item => item.Name,
            item => [item.SprintCode, item.Name, item.Summary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryTaskDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _projectTaskRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.TaskCode.Contains(keyword) ||
                item.Description.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectTask",
            "项目需求",
            "tasks",
            item => item.Id,
            item => item.ProjectId,
            item => item.TaskCode,
            item => item.Title,
            item => [item.TaskCode, item.Title, item.Description]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryTaskDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _projectTaskRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectTask",
            "项目需求",
            "tasks",
            item => item.Id,
            item => item.ProjectId,
            item => item.TaskCode,
            item => item.Title,
            item => [item.TaskCode, item.Title, item.Description]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryTaskDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _projectTaskRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectTask",
            "项目需求",
            "tasks",
            item => item.Id,
            item => item.ProjectId,
            item => item.TaskCode,
            item => item.Title,
            item => [item.TaskCode, item.Title, item.Description]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryRequirementDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _projectRequirementRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectRequirement",
            "项目需求",
            "tasks",
            item => item.Id,
            item => item.ProjectId,
            item => item.RequirementCode,
            item => item.Title,
            item => [item.RequirementCode, item.Title, item.Description]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryRequirementDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _projectRequirementRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectRequirement",
            "项目需求",
            "tasks",
            item => item.Id,
            item => item.ProjectId,
            item => item.RequirementCode,
            item => item.Title,
            item => [item.RequirementCode, item.Title, item.Description]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryReportDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _projectReportRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Title.Contains(keyword) ||
                item.ReportType.Contains(keyword) ||
                item.CompletedSummary.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectReport",
            "项目报告",
            "docs",
            item => item.Id,
            item => item.ProjectId,
            item => item.ReportType,
            item => item.Title,
            item => [item.ReportType, item.Title, item.CompletedSummary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryReportDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _projectReportRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectReport",
            "项目报告",
            "docs",
            item => item.Id,
            item => item.ProjectId,
            item => item.ReportType,
            item => item.Title,
            item => [item.ReportType, item.Title, item.CompletedSummary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryReportDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _projectReportRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectReport",
            "项目报告",
            "docs",
            item => item.Id,
            item => item.ProjectId,
            item => item.ReportType,
            item => item.Title,
            item => [item.ReportType, item.Title, item.CompletedSummary]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryMeetingRecordDescriptorsByKeywordAsync(string keyword)
    {
        var queryable = await _projectMeetingRecordRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .Where(item =>
                item.Topic.Contains(keyword) ||
                item.Minutes.Contains(keyword) ||
                item.Decisions.Contains(keyword))
            .Take(200));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectMeetingRecord",
            "会议记录",
            "docs",
            item => item.Id,
            item => item.ProjectId,
            _ => "会议记录",
            item => item.Topic,
            item => [item.Topic, item.Minutes, item.Decisions]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryMeetingRecordDescriptorsByIdsAsync(List<Guid> ids)
    {
        var queryable = await _projectMeetingRecordRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => ids.Contains(item.Id)));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectMeetingRecord",
            "会议记录",
            "docs",
            item => item.Id,
            item => item.ProjectId,
            _ => "会议记录",
            item => item.Topic,
            item => [item.Topic, item.Minutes, item.Decisions]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> QueryMeetingRecordDescriptorsByProjectAsync(Guid projectId)
    {
        var queryable = await _projectMeetingRecordRepository.GetQueryableAsync();
        var items = await _asyncQueryableExecuter.ToListAsync(queryable.Where(item => item.ProjectId == projectId));
        return await BuildBusinessDescriptorsWithProjectsAsync(
            items,
            "ProjectMeetingRecord",
            "会议记录",
            "docs",
            item => item.Id,
            item => item.ProjectId,
            _ => "会议记录",
            item => item.Topic,
            item => [item.Topic, item.Minutes, item.Decisions]);
    }

    private async Task<List<PlatformFileBusinessObjectDescriptor>> BuildBusinessDescriptorsWithProjectsAsync<T>(
        IReadOnlyCollection<T> items,
        string businessType,
        string typeName,
        string tab,
        Func<T, Guid> idSelector,
        Func<T, Guid> projectIdSelector,
        Func<T, string> codeSelector,
        Func<T, string> nameSelector,
        Func<T, IEnumerable<string>> searchTokensSelector)
    {
        if (items.Count == 0)
        {
            return [];
        }

        var projectIds = items.Select(projectIdSelector).Distinct().ToList();
        var projectQueryable = await _projectRepository.GetQueryableAsync();
        var projects = await _asyncQueryableExecuter.ToListAsync(projectQueryable.Where(item => projectIds.Contains(item.Id)));
        var projectMap = projects.ToDictionary(item => item.Id);

        return items.Select(item =>
        {
            projectMap.TryGetValue(projectIdSelector(item), out var project);
            return new PlatformFileBusinessObjectDescriptor
            {
                BusinessId = idSelector(item).ToString(),
                BusinessType = businessType,
                Code = codeSelector(item),
                Name = nameSelector(item),
                ProjectId = project?.Id ?? projectIdSelector(item),
                ProjectName = project?.Name,
                Tab = tab,
                TypeName = typeName,
                SearchTokens = searchTokensSelector(item)
                    .Append(project?.Name ?? string.Empty)
                    .Append(project?.ProjectCode ?? string.Empty)
                    .Where(token => !string.IsNullOrWhiteSpace(token))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList()
            };
        }).ToList();
    }

    private static string? ResolveBusinessJumpPath(
        PlatformFileBusinessObjectDescriptor? descriptor,
        string? businessType,
        string? businessId)
    {
        var type = descriptor?.BusinessType ?? businessType;
        if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(descriptor?.ProjectId?.ToString() ?? businessId))
        {
            return null;
        }

        return type.Equals("Project", StringComparison.OrdinalIgnoreCase)
            || type.Equals("ProjectReport", StringComparison.OrdinalIgnoreCase)
            || type.Equals("ProjectMeetingRecord", StringComparison.OrdinalIgnoreCase)
            ? "/project/detail"
            : "/business/projects";
    }

    private static Dictionary<string, string?> ResolveBusinessJumpQuery(
        PlatformFileBusinessObjectDescriptor? descriptor,
        string? businessType,
        string? businessId,
        string? category)
    {
        var type = descriptor?.BusinessType ?? businessType;
        if (string.IsNullOrWhiteSpace(type))
        {
            return [];
        }

        if (type.Equals("Project", StringComparison.OrdinalIgnoreCase))
        {
            var projectId = descriptor?.ProjectId?.ToString() ?? businessId;
            return string.IsNullOrWhiteSpace(projectId)
                ? []
                : new Dictionary<string, string?> { ["projectId"] = projectId, ["tab"] = PlatformFileBusinessLinkResolver.ResolveProjectTabByCategory(category) };
        }

        if (descriptor?.ProjectId == null)
        {
            return [];
        }

        return new Dictionary<string, string?>
        {
            ["projectId"] = descriptor.ProjectId.Value.ToString(),
            ["tab"] = descriptor.Tab ?? "overview",
            ["objectId"] = descriptor.BusinessId,
            ["objectType"] = descriptor.BusinessType
        };
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
            "image/gif" => HasPrefix(header, [0x47, 0x49, 0x46, 0x38]),
            "image/bmp" => HasPrefix(header, [0x42, 0x4D]),
            "image/webp" => header.Length >= 12 &&
                             HasPrefix(header, [0x52, 0x49, 0x46, 0x46]) &&
                             header[8..12].SequenceEqual("WEBP"u8),
            MediaTypeNames.Application.Pdf => HasPrefix(header, [0x25, 0x50, 0x44, 0x46]),
            "application/msword" or "application/vnd.ms-excel" or "application/vnd.ms-powerpoint" =>
                HasPrefix(header, [0xD0, 0xCF, 0x11, 0xE0]),
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => IsZipHeader(header),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => IsZipHeader(header),
            "application/vnd.openxmlformats-officedocument.presentationml.presentation" => IsZipHeader(header),
            "application/zip" or "application/x-zip-compressed" => IsZipHeader(header),
            "video/mp4" or "video/x-m4v" or "video/quicktime" => IsMp4FamilyHeader(header),
            "video/webm" => HasPrefix(header, [0x1A, 0x45, 0xDF, 0xA3]),
            "video/x-msvideo" => IsAviHeader(header),
            MediaTypeNames.Text.Plain or "text/csv" or "text/markdown" or "text/html" or "text/css" or
            "text/javascript" or "text/xml" or "text/yaml" or "text/x-yaml" or "application/json" or
            "application/xml" or "application/javascript" or "application/x-javascript" or "application/x-yaml" or
            "application/x-ndjson" or "application/dxf" or "image/svg+xml" => IsPlainTextHeader(header),
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

    private static bool IsMp4FamilyHeader(ReadOnlySpan<byte> header)
    {
        return header.Length >= 12 &&
               header[4..8].SequenceEqual("ftyp"u8);
    }

    private static bool IsAviHeader(ReadOnlySpan<byte> header)
    {
        return header.Length >= 12 &&
               HasPrefix(header, [0x52, 0x49, 0x46, 0x46]) &&
               header[8..12].SequenceEqual("AVI "u8);
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
