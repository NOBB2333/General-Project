using System.Threading;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.TenantManagement;

namespace General.Admin.PhaseOne;

public class PlatformTenantHealthCheckJobHandler : IPlatformScheduledJobHandler
{
    private readonly IRepository<AppTenantAuthorization, Guid> _tenantAuthorizationRepository;
    private readonly ITenantRepository _tenantRepository;

    public PlatformTenantHealthCheckJobHandler(
        IRepository<AppTenantAuthorization, Guid> tenantAuthorizationRepository,
        ITenantRepository tenantRepository)
    {
        _tenantAuthorizationRepository = tenantAuthorizationRepository;
        _tenantRepository = tenantRepository;
    }

    public string JobKey => "tenant-health-check";

    public async Task<string> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var tenants = await _tenantRepository.GetListAsync(includeDetails: true, cancellationToken: cancellationToken);
        var authorizations = await _tenantAuthorizationRepository.GetListAsync(cancellationToken: cancellationToken);
        var authorizationMap = authorizations
            .GroupBy(x => x.TenantId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(item => item.CreationTime).First());

        var missingConnectionCount = tenants.Count(tenant =>
            string.IsNullOrWhiteSpace(
                tenant.ConnectionStrings?
                    .FirstOrDefault(item => item.Name == Volo.Abp.Data.ConnectionStrings.DefaultConnectionStringName)
                    ?.Value));
        var missingAdminCount = tenants.Count(tenant =>
            !authorizationMap.TryGetValue(tenant.Id, out var authorization) || !authorization.AdminUserId.HasValue);

        return $"巡检完成：租户 {tenants.Count} 个，未配置连接串 {missingConnectionCount} 个，未绑定管理员 {missingAdminCount} 个。";
    }
}
