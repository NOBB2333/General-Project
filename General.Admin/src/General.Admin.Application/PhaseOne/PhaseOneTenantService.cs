using System.IO;
using System.Text.RegularExpressions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace General.Admin.PhaseOne;

public class PhaseOneTenantService : ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IRepository<AppMenu, Guid> _menuRepository;
    private readonly IRepository<AppTenantAuthorization, Guid> _tenantAuthorizationRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly TenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;

    public PhaseOneTenantService(
        ICurrentTenant currentTenant,
        IRepository<AppMenu, Guid> menuRepository,
        IRepository<AppTenantAuthorization, Guid> tenantAuthorizationRepository,
        IRepository<IdentityUser, Guid> userRepository,
        TenantManager tenantManager,
        ITenantRepository tenantRepository)
    {
        _currentTenant = currentTenant;
        _menuRepository = menuRepository;
        _tenantAuthorizationRepository = tenantAuthorizationRepository;
        _userRepository = userRepository;
        _tenantManager = tenantManager;
        _tenantRepository = tenantRepository;
    }

    public async Task<List<PhaseOneTenantListItemDto>> GetListAsync()
    {
        var tenants = (await _tenantRepository.GetListAsync(includeDetails: true))
            .OrderBy(x => x.Name)
            .ToList();
        var authorizations = (await _tenantAuthorizationRepository.GetListAsync())
            .ToDictionary(x => x.TenantId);
        var users = (await _userRepository.GetListAsync()).ToDictionary(x => x.Id);

        return tenants
            .Select(x => new PhaseOneTenantListItemDto
            {
                AdminEmail = authorizations.GetValueOrDefault(x.Id) is { AdminUserId: { } adminUserId } authorizationForAdmin &&
                             users.TryGetValue(adminUserId, out var adminUser)
                    ? adminUser.Email
                    : null,
                AdminUserId = authorizations.GetValueOrDefault(x.Id)?.AdminUserId,
                AdminUserName = authorizations.GetValueOrDefault(x.Id) is { AdminUserId: { } adminUserId2 } authorizationForName &&
                                 users.TryGetValue(adminUserId2, out var adminUserForName)
                    ? adminUserForName.UserName
                    : null,
                ApiBlacklist = authorizations.GetValueOrDefault(x.Id) is { } authorization
                    ? PhaseOneSerializationHelper.DeserializeStringList(authorization.ApiBlacklist)
                    : [],
                CreationTime = x.CreationTime,
                DefaultConnectionStringDisplay = BuildConnectionStringDisplay(
                    x.ConnectionStrings
                        .FirstOrDefault(item => item.Name == ConnectionStrings.DefaultConnectionStringName)
                        ?.Value),
                HasDefaultConnectionString = x.ConnectionStrings
                    .Any(item =>
                        item.Name == ConnectionStrings.DefaultConnectionStringName &&
                        !string.IsNullOrWhiteSpace(item.Value)),
                Id = x.Id,
                HasExplicitAuthorization = authorizations.ContainsKey(x.Id),
                IsActive = authorizations.GetValueOrDefault(x.Id)?.IsActive ?? true,
                Name = x.Name,
                Remark = authorizations.GetValueOrDefault(x.Id)?.Remark
            })
            .ToList();
    }

    public async Task CreateAsync(PhaseOneTenantSaveInput input)
    {
        var tenant = await _tenantManager.CreateAsync(input.Name.Trim());
        if (!string.IsNullOrWhiteSpace(input.DefaultConnectionString))
        {
            tenant.SetDefaultConnectionString(input.DefaultConnectionString.Trim());
        }

        await _tenantRepository.InsertAsync(tenant, autoSave: true);

        if (input.AdminUserId.HasValue || !string.IsNullOrWhiteSpace(input.Remark))
        {
            await SaveAuthorizationAsync(tenant.Id, new PhaseOneTenantAuthorizationSaveInput
            {
                AdminUserId = input.AdminUserId,
                ApiBlacklist = [],
                IsActive = true,
                MenuIds = await ResolveDefaultMenuIdsAsync(),
                Remark = input.Remark
            });
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetAsync(id);
        if (tenant.Name.Equals(PhaseOneSeedIds.DefaultTenantName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("默认租户不允许删除。");
        }

        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == id);
        if (authorization != null)
        {
            await _tenantAuthorizationRepository.DeleteAsync(authorization, autoSave: true);
        }

        await _tenantRepository.DeleteAsync(id);
    }

    public async Task<PhaseOneTenantAuthorizationDto> GetAuthorizationAsync(Guid tenantId)
    {
        await _tenantRepository.GetAsync(tenantId);
        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenantId);
        var defaultMenuIds = await ResolveDefaultMenuIdsAsync();
        return new PhaseOneTenantAuthorizationDto
        {
            AdminUserId = authorization?.AdminUserId,
            ApiBlacklist = authorization == null
                ? []
                : PhaseOneSerializationHelper.DeserializeStringList(authorization.ApiBlacklist),
            IsActive = authorization?.IsActive ?? true,
            MenuIds = authorization == null
                ? defaultMenuIds
                : PhaseOneSerializationHelper.DeserializeGuidList(authorization.MenuIds),
            Remark = authorization?.Remark
        };
    }

    public async Task SaveAuthorizationAsync(Guid tenantId, PhaseOneTenantAuthorizationSaveInput input)
    {
        await _tenantRepository.GetAsync(tenantId);
        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenantId);
        var normalizedMenuIds = PhaseOneSerializationHelper.SerializeGuids(input.MenuIds);
        var normalizedApiBlacklist = PhaseOneSerializationHelper.SerializeStrings(input.ApiBlacklist);

        if (authorization == null)
        {
            authorization = new AppTenantAuthorization(
                Guid.NewGuid(),
                tenantId,
                normalizedMenuIds,
                normalizedApiBlacklist,
                input.IsActive,
                input.AdminUserId,
                input.Remark);
            await _tenantAuthorizationRepository.InsertAsync(authorization, autoSave: true);
            return;
        }

        authorization.Update(normalizedMenuIds, normalizedApiBlacklist, input.IsActive, input.AdminUserId, input.Remark);
        await _tenantAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
    }

    public async Task SetStatusAsync(Guid tenantId, bool isActive)
    {
        await _tenantRepository.GetAsync(tenantId);
        var authorization = await _tenantAuthorizationRepository.FindAsync(x => x.TenantId == tenantId);
        if (authorization == null)
        {
            authorization = new AppTenantAuthorization(
                Guid.NewGuid(),
                tenantId,
                PhaseOneSerializationHelper.SerializeGuids(await ResolveDefaultMenuIdsAsync()),
                "[]",
                isActive);
            await _tenantAuthorizationRepository.InsertAsync(authorization, autoSave: true);
            return;
        }

        authorization.UpdateStatus(isActive);
        await _tenantAuthorizationRepository.UpdateAsync(authorization, autoSave: true);
    }

    private async Task<List<Guid>> ResolveDefaultMenuIdsAsync()
    {
        return (await _menuRepository.GetListAsync())
            .Where(x => x.IsEnabled)
            .Select(x => x.Id)
            .Distinct()
            .OrderBy(x => x)
            .ToList();
    }

    public async Task<List<PhaseOneTenantUserDto>> GetUsersAsync(Guid tenantId)
    {
        await _tenantRepository.GetAsync(tenantId);

        using (_currentTenant.Change(tenantId))
        {
            return (await _userRepository.GetListAsync())
                .OrderBy(x => x.UserName)
                .Select(x => new PhaseOneTenantUserDto
                {
                    DisplayName = string.IsNullOrWhiteSpace($"{x.Name}{x.Surname}".Trim())
                        ? x.UserName ?? string.Empty
                        : $"{x.Name}{x.Surname}".Trim(),
                    Email = x.Email ?? string.Empty,
                    Id = x.Id,
                    IsActive = x.IsActive,
                    Username = x.UserName ?? string.Empty
                })
                .ToList();
        }
    }

    private static string? BuildConnectionStringDisplay(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return null;
        }

        var provider = ResolveProvider(connectionString);
        var database = ResolveDatabaseName(connectionString, provider);
        return string.IsNullOrWhiteSpace(database)
            ? $"已配置（{provider}）"
            : $"已配置（{provider} · {database}）";
    }

    private static string? ExtractConnectionValue(string connectionString, params string[] keys)
    {
        foreach (var key in keys)
        {
            var match = Regex.Match(
                connectionString,
                $@"(?:^|;)\s*{Regex.Escape(key)}\s*=\s*(?<value>[^;]+)",
                RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups["value"].Value.Trim();
            }
        }

        return null;
    }

    private static string? ResolveDatabaseName(string connectionString, string provider)
    {
        if (provider == "SQLite")
        {
            var dataSource = ExtractConnectionValue(connectionString, "Data Source");
            return string.IsNullOrWhiteSpace(dataSource) ? null : Path.GetFileName(dataSource);
        }

        return ExtractConnectionValue(connectionString, "Initial Catalog", "Database");
    }

    private static string ResolveProvider(string connectionString)
    {
        if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
        {
            return "PostgreSQL";
        }

        if (connectionString.Contains("Uid=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("User Id=", StringComparison.OrdinalIgnoreCase))
        {
            return "MySQL";
        }

        if (connectionString.Contains("Initial Catalog=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains("Trusted_Connection=", StringComparison.OrdinalIgnoreCase))
        {
            return "SQL Server";
        }

        if (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains(".db", StringComparison.OrdinalIgnoreCase) ||
            connectionString.Contains(".sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return "SQLite";
        }

        return "数据库";
    }
}
