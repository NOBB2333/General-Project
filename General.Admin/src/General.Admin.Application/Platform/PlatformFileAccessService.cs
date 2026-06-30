using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.Users;
using General.Admin.Permissions;
using General.Admin.Project;
using ProjectEntity = General.Admin.Project.Project;

namespace General.Admin.Platform;

public class PlatformFileAccessService : ITransientDependency
{
    private static readonly HashSet<string> BusinessTypes =
    [
        "BusinessAcceptance",
        "BusinessBudget",
        "BusinessChain",
        "BusinessContract",
        "BusinessInvoice",
        "BusinessProcurement",
        "BusinessReceivable",
        "Acceptance",
        "Budget",
        "Chain",
        "Contract",
        "Invoice",
        "Procurement",
        "Receivable"
    ];

    private readonly ICurrentUser _currentUser;
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly IReadOnlyList<IFileBusinessLinkResolver> _fileBusinessLinkResolvers;
    private readonly IPermissionChecker _permissionChecker;
    private readonly ProjectAccessGuard _projectAccessGuard;
    private readonly IRepository<BusinessAcceptance, Guid> _businessAcceptanceRepository;
    private readonly IRepository<BusinessBudgetExecution, Guid> _businessBudgetRepository;
    private readonly IRepository<BusinessChain, Guid> _businessChainRepository;
    private readonly IRepository<BusinessContract, Guid> _businessContractRepository;
    private readonly IRepository<BusinessInvoice, Guid> _businessInvoiceRepository;
    private readonly IRepository<BusinessProcurement, Guid> _businessProcurementRepository;
    private readonly IRepository<BusinessReceivable, Guid> _businessReceivableRepository;
    private readonly IRepository<ProjectSprint, Guid> _projectSprintRepository;
    private readonly IRepository<ProjectMeetingRecord, Guid> _projectMeetingRecordRepository;
    private readonly IRepository<ProjectMember, Guid> _projectMemberRepository;
    private readonly IRepository<ProjectEntity, Guid> _projectRepository;
    private readonly IRepository<ProjectReport, Guid> _projectReportRepository;
    private readonly IRepository<ProjectRequirement, Guid> _projectRequirementRepository;
    private readonly IRepository<ProjectTaskParticipant, Guid> _projectTaskParticipantRepository;
    private readonly IRepository<ProjectTask, Guid> _projectTaskRepository;

    public PlatformFileAccessService(
        ICurrentUser currentUser,
        IAsyncQueryableExecuter asyncQueryableExecuter,
        CurrentUserDataScopeService dataScopeService,
        IEnumerable<IFileBusinessLinkResolver> fileBusinessLinkResolvers,
        IPermissionChecker permissionChecker,
        ProjectAccessGuard projectAccessGuard,
        IRepository<BusinessAcceptance, Guid> businessAcceptanceRepository,
        IRepository<BusinessBudgetExecution, Guid> businessBudgetRepository,
        IRepository<BusinessChain, Guid> businessChainRepository,
        IRepository<BusinessContract, Guid> businessContractRepository,
        IRepository<BusinessInvoice, Guid> businessInvoiceRepository,
        IRepository<BusinessProcurement, Guid> businessProcurementRepository,
        IRepository<BusinessReceivable, Guid> businessReceivableRepository,
        IRepository<ProjectSprint, Guid> projectSprintRepository,
        IRepository<ProjectMeetingRecord, Guid> projectMeetingRecordRepository,
        IRepository<ProjectMember, Guid> projectMemberRepository,
        IRepository<ProjectEntity, Guid> projectRepository,
        IRepository<ProjectReport, Guid> projectReportRepository,
        IRepository<ProjectRequirement, Guid> projectRequirementRepository,
        IRepository<ProjectTaskParticipant, Guid> projectTaskParticipantRepository,
        IRepository<ProjectTask, Guid> projectTaskRepository)
    {
        _currentUser = currentUser;
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _dataScopeService = dataScopeService;
        _fileBusinessLinkResolvers = fileBusinessLinkResolvers.ToList();
        _permissionChecker = permissionChecker;
        _projectAccessGuard = projectAccessGuard;
        _businessAcceptanceRepository = businessAcceptanceRepository;
        _businessBudgetRepository = businessBudgetRepository;
        _businessChainRepository = businessChainRepository;
        _businessContractRepository = businessContractRepository;
        _businessInvoiceRepository = businessInvoiceRepository;
        _businessProcurementRepository = businessProcurementRepository;
        _businessReceivableRepository = businessReceivableRepository;
        _projectSprintRepository = projectSprintRepository;
        _projectMeetingRecordRepository = projectMeetingRecordRepository;
        _projectMemberRepository = projectMemberRepository;
        _projectRepository = projectRepository;
        _projectReportRepository = projectReportRepository;
        _projectRequirementRepository = projectRequirementRepository;
        _projectTaskParticipantRepository = projectTaskParticipantRepository;
        _projectTaskRepository = projectTaskRepository;
    }

    public async Task<IQueryable<AppPlatformFile>> ApplyReadFilterAsync(IQueryable<AppPlatformFile> queryable)
    {
        if (await _permissionChecker.IsGrantedAsync(AdminPermissions.Platform.FileManage))
        {
            return queryable;
        }

        if (!_currentUser.Id.HasValue)
        {
            return queryable.Where(file => file.IsPublic);
        }

        var currentUserId = _currentUser.Id.Value;
        var accessibleProjectIds = await ResolveAccessibleProjectIdsAsync();
        var projectBusinessKeys = await ResolveProjectBusinessKeysAsync(accessibleProjectIds);
        var canViewBusinessFiles = await _permissionChecker.IsGrantedAsync(AdminPermissions.Business.ReportView);
        var businessKeys = canViewBusinessFiles
            ? await ResolveBusinessKeysAsync(accessibleProjectIds, projectBusinessKeys)
            : [];

        return queryable.Where(file =>
            file.IsPublic ||
            (file.UploadedByUserId.HasValue && file.UploadedByUserId.Value == currentUserId) ||
            (file.BusinessType != null &&
             file.BusinessId != null &&
             (
                 (file.Category != "Business" &&
                  !file.Category.StartsWith("Business/") &&
                  projectBusinessKeys.Contains(file.BusinessType + ":" + file.BusinessId)) ||
                 (canViewBusinessFiles &&
                  businessKeys.Contains(file.BusinessType + ":" + file.BusinessId))
             )));
    }

    public async Task EnsureCanReadAsync(AppPlatformFile file)
    {
        if (file.IsPublic || IsUploadedByCurrentUser(file))
        {
            return;
        }

        if (await _permissionChecker.IsGrantedAsync(AdminPermissions.Platform.FileManage))
        {
            return;
        }

        if (await CanCurrentUserAccessTaskFileAsync(file))
        {
            return;
        }

        var projectId = await ResolveProjectIdAsync(file);
        if (IsBusinessFile(file))
        {
            if (!await _permissionChecker.IsGrantedAsync(AdminPermissions.Business.ReportView))
            {
                throw NoAccess();
            }

            if (projectId.HasValue)
            {
                await _projectAccessGuard.EnsureProjectAccessAsync(projectId.Value);
                return;
            }
        }

        if (projectId.HasValue)
        {
            await _projectAccessGuard.EnsureProjectAccessAsync(projectId.Value);
            return;
        }

        throw NoAccess();
    }

    public async Task EnsureCanUploadAsync(string? businessType, string? businessId, string category)
    {
        if (await _permissionChecker.IsGrantedAsync(AdminPermissions.Platform.FileManage))
        {
            return;
        }

        if (!_currentUser.Id.HasValue)
        {
            throw NoAccess();
        }

        var probe = new AppPlatformFile(
            Guid.NewGuid(),
            "access-probe",
            "access-probe",
            "application/octet-stream",
            0,
            category,
            "access-probe",
            "access-probe",
            PlatformFileStorageNames.Local,
            _currentUser.Id,
            businessType: businessType,
            businessId: businessId);
        var projectId = await ResolveProjectIdAsync(probe);
        if (IsBusinessFile(probe))
        {
            if (!await _permissionChecker.IsGrantedAsync(AdminPermissions.Business.ReportView))
            {
                throw NoAccess();
            }

            if (projectId.HasValue)
            {
                await _projectAccessGuard.EnsureProjectAccessAsync(projectId.Value);
                return;
            }

            throw NoAccess();
        }

        if (await CanCurrentUserAccessTaskFileAsync(probe))
        {
            return;
        }

        if (projectId.HasValue)
        {
            await _projectAccessGuard.EnsureProjectAccessAsync(projectId.Value);
            return;
        }

        throw NoAccess();
    }

    private async Task<HashSet<Guid>> ResolveAccessibleProjectIdsAsync()
    {
        var projectQueryable = await _projectRepository.GetQueryableAsync();
        if (_currentUser.IsInRole(PlatformRoleNames.Admin) || _currentUser.IsInRole(PlatformRoleNames.Pmo))
        {
            return (await _asyncQueryableExecuter.ToListAsync(projectQueryable.Select(project => project.Id)))
                .ToHashSet();
        }

        if (!_currentUser.Id.HasValue)
        {
            return [];
        }

        var currentUserId = _currentUser.Id.Value;
        var accessibleOrganizationUnitIds = await _dataScopeService.GetAccessibleOrganizationUnitIdsAsync();
        var projectIds = (await _asyncQueryableExecuter.ToListAsync(
                projectQueryable
                    .Where(project =>
                        accessibleOrganizationUnitIds.Contains(project.OrganizationUnitId) ||
                        project.ManagerUserId == currentUserId ||
                        project.SponsorUserId == currentUserId)
                    .Select(project => project.Id)))
            .ToHashSet();

        var today = DateTime.Today;
        var memberQueryable = await _projectMemberRepository.GetQueryableAsync();
        var memberProjectIds = await _asyncQueryableExecuter.ToListAsync(
            memberQueryable
                .Where(member =>
                    member.UserId == currentUserId &&
                    (!member.LeaveDate.HasValue || member.LeaveDate.Value.Date >= today || member.AllowHistoricalRead))
                .Select(member => member.ProjectId));
        foreach (var projectId in memberProjectIds)
        {
            projectIds.Add(projectId);
        }

        var taskQueryable = await _projectTaskRepository.GetQueryableAsync();
        var taskProjectIds = await _asyncQueryableExecuter.ToListAsync(
            taskQueryable
                .Where(task =>
                    task.OwnerUserId == currentUserId ||
                    task.ProductOwnerUserId == currentUserId ||
                    task.DeveloperOwnerUserId == currentUserId ||
                    task.TesterOwnerUserId == currentUserId)
                .Select(task => task.ProjectId));
        foreach (var projectId in taskProjectIds)
        {
            projectIds.Add(projectId);
        }

        var taskParticipantQueryable = await _projectTaskParticipantRepository.GetQueryableAsync();
        var participantProjectIds = await _asyncQueryableExecuter.ToListAsync(
            taskParticipantQueryable
                .Where(participant => participant.UserId == currentUserId)
                .Select(participant => participant.ProjectId));
        foreach (var projectId in participantProjectIds)
        {
            projectIds.Add(projectId);
        }

        return projectIds;
    }

    private async Task<HashSet<string>> ResolveProjectBusinessKeysAsync(HashSet<Guid> projectIds)
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var projectId in projectIds)
        {
            keys.Add(PlatformFileBusinessLinkResolver.BuildBusinessKey("Project", projectId.ToString()));
        }

        if (projectIds.Count == 0)
        {
            return keys;
        }

        var sprintQueryable = await _projectSprintRepository.GetQueryableAsync();
        AddKeys(
            keys,
            "ProjectSprint",
            await _asyncQueryableExecuter.ToListAsync(
                sprintQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var requirementQueryable = await _projectRequirementRepository.GetQueryableAsync();
        AddKeys(
            keys,
            "ProjectRequirement",
            await _asyncQueryableExecuter.ToListAsync(
                requirementQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var taskQueryable = await _projectTaskRepository.GetQueryableAsync();
        AddKeys(
            keys,
            "ProjectTask",
            await _asyncQueryableExecuter.ToListAsync(
                taskQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var reportQueryable = await _projectReportRepository.GetQueryableAsync();
        AddKeys(
            keys,
            "ProjectReport",
            await _asyncQueryableExecuter.ToListAsync(
                reportQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var meetingQueryable = await _projectMeetingRecordRepository.GetQueryableAsync();
        AddKeys(
            keys,
            "ProjectMeetingRecord",
            await _asyncQueryableExecuter.ToListAsync(
                meetingQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        return keys;
    }

    private async Task<HashSet<string>> ResolveBusinessKeysAsync(
        HashSet<Guid> projectIds,
        HashSet<string> projectBusinessKeys)
    {
        var keys = new HashSet<string>(projectBusinessKeys, StringComparer.OrdinalIgnoreCase);
        if (projectIds.Count == 0)
        {
            return keys;
        }

        var acceptanceQueryable = await _businessAcceptanceRepository.GetQueryableAsync();
        AddAliasKeys(
            keys,
            "BusinessAcceptance",
            await _asyncQueryableExecuter.ToListAsync(
                acceptanceQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var budgetQueryable = await _businessBudgetRepository.GetQueryableAsync();
        AddAliasKeys(
            keys,
            "BusinessBudget",
            await _asyncQueryableExecuter.ToListAsync(
                budgetQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var chainQueryable = await _businessChainRepository.GetQueryableAsync();
        AddAliasKeys(
            keys,
            "BusinessChain",
            await _asyncQueryableExecuter.ToListAsync(
                chainQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var contractQueryable = await _businessContractRepository.GetQueryableAsync();
        AddAliasKeys(
            keys,
            "BusinessContract",
            await _asyncQueryableExecuter.ToListAsync(
                contractQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var invoiceQueryable = await _businessInvoiceRepository.GetQueryableAsync();
        AddAliasKeys(
            keys,
            "BusinessInvoice",
            await _asyncQueryableExecuter.ToListAsync(
                invoiceQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var procurementQueryable = await _businessProcurementRepository.GetQueryableAsync();
        AddAliasKeys(
            keys,
            "BusinessProcurement",
            await _asyncQueryableExecuter.ToListAsync(
                procurementQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        var receivableQueryable = await _businessReceivableRepository.GetQueryableAsync();
        AddAliasKeys(
            keys,
            "BusinessReceivable",
            await _asyncQueryableExecuter.ToListAsync(
                receivableQueryable.Where(item => projectIds.Contains(item.ProjectId)).Select(item => item.Id)));

        return keys;
    }

    private async Task<Guid?> ResolveProjectIdAsync(AppPlatformFile file)
    {
        if (string.IsNullOrWhiteSpace(file.BusinessType) || string.IsNullOrWhiteSpace(file.BusinessId))
        {
            return null;
        }

        var businessType = file.BusinessType.Trim();
        var resolver = _fileBusinessLinkResolvers.FirstOrDefault(item => item.CanResolve(businessType));
        return resolver == null ? null : await resolver.ResolveProjectIdAsync(businessType, file.BusinessId);
    }

    private static bool IsBusinessFile(AppPlatformFile file)
    {
        return IsKnownType(file, BusinessTypes) || HasCategoryPrefix(file, "Business");
    }

    private async Task<bool> CanCurrentUserAccessTaskFileAsync(AppPlatformFile file)
    {
        if (!_currentUser.Id.HasValue ||
            !IsProjectTaskFile(file) ||
            !Guid.TryParse(file.BusinessId, out var taskId))
        {
            return false;
        }

        var currentUserId = _currentUser.Id.Value;
        var task = await _projectTaskRepository.FindAsync(taskId);
        if (task == null)
        {
            return false;
        }

        if (task.OwnerUserId == currentUserId ||
            task.ProductOwnerUserId == currentUserId ||
            task.DeveloperOwnerUserId == currentUserId ||
            task.TesterOwnerUserId == currentUserId)
        {
            return true;
        }

        return await _projectTaskParticipantRepository.AnyAsync(participant =>
            participant.TaskId == taskId &&
            participant.UserId == currentUserId);
    }

    private static bool IsProjectTaskFile(AppPlatformFile file)
    {
        return !string.IsNullOrWhiteSpace(file.BusinessType) &&
               file.BusinessType.Equals("ProjectTask", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsUploadedByCurrentUser(AppPlatformFile file)
    {
        return _currentUser.Id.HasValue &&
               file.UploadedByUserId.HasValue &&
               file.UploadedByUserId.Value == _currentUser.Id.Value;
    }

    private static bool IsKnownType(AppPlatformFile file, HashSet<string> candidates)
    {
        return !string.IsNullOrWhiteSpace(file.BusinessType) &&
               candidates.Contains(file.BusinessType.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    private static bool HasCategoryPrefix(AppPlatformFile file, string prefix)
    {
        return file.Category.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
               file.Category.StartsWith($"{prefix}/", StringComparison.OrdinalIgnoreCase);
    }

    private static AbpAuthorizationException NoAccess()
    {
        return new AbpAuthorizationException("无权访问该文件。");
    }

    private static void AddKeys(HashSet<string> keys, string businessType, IEnumerable<Guid> businessIds)
    {
        foreach (var businessId in businessIds)
        {
            var key = PlatformFileBusinessLinkResolver.BuildBusinessKey(businessType, businessId.ToString());
            if (key != string.Empty)
            {
                keys.Add(key);
            }
        }
    }

    private static void AddAliasKeys(HashSet<string> keys, string businessType, IEnumerable<Guid> businessIds)
    {
        foreach (var businessId in businessIds)
        {
            foreach (var key in PlatformFileBusinessLinkResolver.ResolveBusinessKeyAliases(businessType, businessId.ToString()))
            {
                if (key != string.Empty)
                {
                    keys.Add(key);
                }
            }
        }
    }
}
