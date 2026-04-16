using Volo.Abp.DependencyInjection;
using Volo.Abp.Data;
using Volo.Abp.TenantManagement;

namespace General.Admin.PhaseOne;

public class PhaseOneTenantService : ITransientDependency
{
    private readonly TenantManager _tenantManager;
    private readonly ITenantRepository _tenantRepository;

    public PhaseOneTenantService(
        TenantManager tenantManager,
        ITenantRepository tenantRepository)
    {
        _tenantManager = tenantManager;
        _tenantRepository = tenantRepository;
    }

    public async Task<List<PhaseOneTenantListItemDto>> GetListAsync()
    {
        return (await _tenantRepository.GetListAsync(includeDetails: true))
            .OrderBy(x => x.Name)
            .Select(x => new PhaseOneTenantListItemDto
            {
                Id = x.Id,
                Name = x.Name,
                DefaultConnectionString = x.ConnectionStrings
                    .FirstOrDefault(item => item.Name == ConnectionStrings.DefaultConnectionStringName)
                    ?.Value
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
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetAsync(id);
        if (tenant.Name.Equals(PhaseOneSeedIds.DefaultTenantName, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("默认租户不允许删除。");
        }

        await _tenantRepository.DeleteAsync(id);
    }
}
