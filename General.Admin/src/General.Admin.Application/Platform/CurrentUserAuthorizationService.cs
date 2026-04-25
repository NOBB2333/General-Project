using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Users;
using Volo.Abp.MultiTenancy;

namespace General.Admin.Platform;

public class CurrentUserAuthorizationService : ITransientDependency
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppRoleAuthorization, Guid> _roleAuthorizationRepository;
    private readonly IRepository<AppTenantAuthorization, Guid> _tenantAuthorizationRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;

    public CurrentUserAuthorizationService(
        ICurrentUser currentUser,
        IRepository<AppRoleAuthorization, Guid> roleAuthorizationRepository,
        IRepository<AppTenantAuthorization, Guid> tenantAuthorizationRepository,
        ICurrentTenant currentTenant,
        IRepository<IdentityRole, Guid> roleRepository)
    {
        _currentUser = currentUser;
        _roleAuthorizationRepository = roleAuthorizationRepository;
        _tenantAuthorizationRepository = tenantAuthorizationRepository;
        _currentTenant = currentTenant;
        _roleRepository = roleRepository;
    }

    public async Task<bool> IsBlockedAsync(string endpointKey)
    {
        return await IsBlockedAsync([endpointKey]);
    }

    public async Task<bool> IsBlockedAsync(IReadOnlyCollection<string> endpointKeys)
    {
        if (_currentUser.IsInRole(PlatformRoleNames.Admin))
        {
            return false;
        }

        var endpoints = endpointKeys
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (endpoints.Count == 0)
        {
            return false;
        }

        var currentRoleNames = _currentUser.Roles?.Distinct(StringComparer.OrdinalIgnoreCase).ToList() ?? [];
        if (currentRoleNames.Count == 0)
        {
            return false;
        }

        var roleIds = (await _roleRepository.GetListAsync())
            .Where(x => currentRoleNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
            .Select(x => x.Id)
            .ToHashSet();

        if (roleIds.Count == 0)
        {
            return false;
        }

        var authorizations = (await _roleAuthorizationRepository.GetListAsync())
            .Where(x => roleIds.Contains(x.RoleId))
            .ToList();

        if (authorizations.Any(item =>
            PlatformSerializationHelper.DeserializeStringList(item.ApiBlacklist)
                .Intersect(endpoints, StringComparer.OrdinalIgnoreCase)
                .Any()))
        {
            return true;
        }

        if (_currentTenant.Id.HasValue)
        {
            var tenantAuthorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == _currentTenant.Id.Value);
            if (tenantAuthorization != null)
            {
                return PlatformSerializationHelper.DeserializeStringList(tenantAuthorization.ApiBlacklist)
                    .Intersect(endpoints, StringComparer.OrdinalIgnoreCase)
                    .Any();
            }
        }

        return false;
    }
}
