using Volo.Abp.AuditLogging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.TenantManagement;
using Volo.Abp.Users;

namespace General.Admin.PhaseOne;

public class PlatformWorkspaceService : ITransientDependency
{
    private static readonly TimeSpan OnlineThreshold = TimeSpan.FromMinutes(20);

    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly CurrentUserDataScopeService _dataScopeService;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AuditLog, Guid> _auditLogRepository;
    private readonly IRepository<AppPlatformFile, Guid> _fileRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<PlatformScheduledJob, Guid> _platformScheduledJobRepository;
    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<AppTenantAuthorization, Guid> _tenantAuthorizationRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IRepository<AppUpdateLog, Guid> _updateLogRepository;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;

    public PlatformWorkspaceService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        CurrentUserDataScopeService dataScopeService,
        ICurrentUser currentUser,
        IRepository<AuditLog, Guid> auditLogRepository,
        IRepository<AppPlatformFile, Guid> fileRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<PlatformScheduledJob, Guid> platformScheduledJobRepository,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<AppTenantAuthorization, Guid> tenantAuthorizationRepository,
        ITenantRepository tenantRepository,
        IRepository<AppUpdateLog, Guid> updateLogRepository,
        IRepository<AppUserProfile, Guid> userProfileRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _dataScopeService = dataScopeService;
        _currentUser = currentUser;
        _auditLogRepository = auditLogRepository;
        _fileRepository = fileRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _platformScheduledJobRepository = platformScheduledJobRepository;
        _roleAuthorizationRepository = roleAuthorizationRepository;
        _roleRepository = roleRepository;
        _tenantAuthorizationRepository = tenantAuthorizationRepository;
        _tenantRepository = tenantRepository;
        _updateLogRepository = updateLogRepository;
        _userProfileRepository = userProfileRepository;
        _userRepository = userRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
    }

    public async Task<PlatformWorkspaceSummaryDto> GetSummaryAsync()
    {
        var visibleOrganizationUnitIds = await _dataScopeService.GetVisibleOrganizationUnitIdsAsync();
        var userIds = await GetVisibleUserIdsAsync(visibleOrganizationUnitIds);
        var summary = new PlatformWorkspaceSummaryDto
        {
            FileCount = (int)await _fileRepository.GetCountAsync(),
            OnlineUserCount = await CountOnlineUsersAsync(userIds),
            OrganizationCount = visibleOrganizationUnitIds.Count,
            RoleCount = _currentUser.IsInRole(PhaseOneRoleNames.Admin)
                ? (int)await _roleRepository.GetCountAsync()
                : (_currentUser.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).Count() ?? 0),
            UpdateLogCount = (int)await _updateLogRepository.GetCountAsync(),
            UserCount = userIds.Count
        };

        if (!_currentUser.IsInRole(PhaseOneRoleNames.Admin))
        {
            if (summary.UserCount == 0)
            {
                summary.AttentionItems.Add(new PlatformWorkspaceAttentionItemDto
                {
                    Detail = "当前数据范围内没有用户数据，请检查角色与数据权限配置。",
                    Key = "empty-user-scope",
                    Level = "warning",
                    Link = "/platform/roles",
                    Title = "当前账号没有可见用户"
                });
            }

            return summary;
        }

        var tenants = await _tenantRepository.GetListAsync(includeDetails: true);
        var roleAuthorizations = await _roleAuthorizationRepository.GetListAsync();
        var schedulerJobs = await _platformScheduledJobRepository.GetListAsync();
        var recentExceptionCount = await CountRecentExceptionLogsAsync();
        var missingConnectionCount = tenants.Count(tenant =>
            string.IsNullOrWhiteSpace(
                tenant.ConnectionStrings.FirstOrDefault(item => item.Name == Volo.Abp.Data.ConnectionStrings.DefaultConnectionStringName)?.Value));
        var tenantAuthorizations = await _tenantAuthorizationRepository.GetListAsync();
        var missingTenantAdmins = tenants.Count(tenant =>
            tenantAuthorizations.All(item => item.TenantId != tenant.Id || !item.AdminUserId.HasValue));

        summary.ExceptionLogCount = recentExceptionCount;
        summary.TenantCount = tenants.Count;

        if (roleAuthorizations.Count(x => x.ApiBlacklist != "[]") > 0)
        {
            summary.AttentionItems.Add(new PlatformWorkspaceAttentionItemDto
            {
                Detail = $"{roleAuthorizations.Count(x => x.ApiBlacklist != "[]")} 个角色启用了接口黑名单，请确认是否仍然符合当前治理策略。",
                Key = "role-api-blacklist",
                Level = "warning",
                Link = "/platform/roles",
                Title = "角色接口授权存在手工黑名单"
            });
        }

        if (missingConnectionCount > 0)
        {
            summary.AttentionItems.Add(new PlatformWorkspaceAttentionItemDto
            {
                Detail = $"{missingConnectionCount} 个租户未配置默认连接串，建议先补齐环境配置再启用业务。",
                Key = "tenant-connection-missing",
                Level = "warning",
                Link = "/platform/tenants",
                Title = "存在未完成连接配置的租户"
            });
        }

        if (missingTenantAdmins > 0)
        {
            summary.AttentionItems.Add(new PlatformWorkspaceAttentionItemDto
            {
                Detail = $"{missingTenantAdmins} 个租户尚未绑定管理员账号，后续运维可能无法闭环。",
                Key = "tenant-admin-missing",
                Level = "warning",
                Link = "/platform/tenants",
                Title = "存在未绑定管理员的租户"
            });
        }

        if (schedulerJobs.Count(x => !x.IsEnabled) > 0)
        {
            summary.AttentionItems.Add(new PlatformWorkspaceAttentionItemDto
            {
                Detail = $"{schedulerJobs.Count(x => !x.IsEnabled)} 个定时任务当前处于停用状态，请确认是否为预期。",
                Key = "scheduler-disabled",
                Level = "info",
                Link = "/platform/scheduler",
                Title = "存在停用中的平台任务"
            });
        }

        if (recentExceptionCount > 0)
        {
            summary.AttentionItems.Add(new PlatformWorkspaceAttentionItemDto
            {
                Detail = $"最近 7 天共聚合到 {recentExceptionCount} 条异常日志，建议优先排查高频错误。",
                Key = "recent-exception",
                Level = "error",
                Link = "/platform/audit-logs",
                Title = "近期存在异常日志"
            });
        }

        return summary;
    }

    private async Task<int> CountOnlineUsersAsync(IReadOnlyCollection<Guid> userIds)
    {
        if (userIds.Count == 0)
        {
            return 0;
        }

        var onlineSince = DateTime.UtcNow.Subtract(OnlineThreshold);
        var queryable = await _userProfileRepository.GetQueryableAsync();
        var profiles = await _asyncQueryableExecuter.ToListAsync(
            queryable.Where(x =>
                userIds.Contains(x.UserId) &&
                x.LastSeenAt.HasValue &&
                x.LastSeenAt.Value >= onlineSince));
        return profiles
            .Select(x => x.UserId)
            .Distinct()
            .Count();
    }

    private async Task<int> CountRecentExceptionLogsAsync()
    {
        var exceptionSince = DateTime.UtcNow.AddDays(-7);
        var queryable = await _auditLogRepository.GetQueryableAsync();
        var logs = await _asyncQueryableExecuter.ToListAsync(
            queryable.Where(x =>
                x.ExecutionTime >= exceptionSince &&
                !string.IsNullOrWhiteSpace(x.Exceptions)));
        return logs.Count;
    }

    private async Task<List<Guid>> GetVisibleUserIdsAsync(IReadOnlySet<Guid> visibleOrganizationUnitIds)
    {
        if (_currentUser.IsInRole(PhaseOneRoleNames.Admin))
        {
            var queryable = await _userRepository.GetQueryableAsync();
            return await _asyncQueryableExecuter.ToListAsync(queryable.Select(x => x.Id));
        }

        if (visibleOrganizationUnitIds.Count == 0)
        {
            return [];
        }

        var queryableUserOrganizations = await _userOrganizationUnitRepository.GetQueryableAsync();
        return await _asyncQueryableExecuter.ToListAsync(
            queryableUserOrganizations
                .Where(x => visibleOrganizationUnitIds.Contains(x.OrganizationUnitId))
                .Select(x => x.UserId)
                .Distinct());
    }
}
