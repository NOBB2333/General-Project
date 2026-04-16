using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace General.Admin.PhaseOne;

public class PhaseOneRoleService : ITransientDependency
{
    private static readonly IReadOnlyList<string> BuiltInRoleNames =
    [
        PhaseOneRoleNames.Admin,
        PhaseOneRoleNames.Pmo,
        PhaseOneRoleNames.Pm,
        PhaseOneRoleNames.Member,
        PhaseOneRoleNames.Viewer,
    ];

    private readonly IdentityUserManager _userManager;
    private readonly IdentityRoleManager _roleManager;
    private readonly IRepository<AppRoleMenu, Guid> _roleMenuRepository;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;

    public PhaseOneRoleService(
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        IRepository<AppRoleMenu, Guid> roleMenuRepository,
        IRepository<IdentityRole, Guid> roleRepository)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _roleMenuRepository = roleMenuRepository;
        _roleRepository = roleRepository;
    }

    public async Task<List<PhaseOneRoleDto>> GetListAsync()
    {
        var roles = (await _roleRepository.GetListAsync())
            .OrderBy(x => GetOrder(x.Name))
            .ThenBy(x => x.Name)
            .ToList();

        var roleMenus = await _roleMenuRepository.GetListAsync();
        var result = new List<PhaseOneRoleDto>();

        foreach (var role in roles)
        {
            var users = await _userManager.GetUsersInRoleAsync(role.Name);
            result.Add(new PhaseOneRoleDto
            {
                Description = GetDescription(role.Name),
                HomePath = PhaseOneUserService.ResolveHomePath([role.Name]),
                Id = role.Id,
                MenuCount = roleMenus.Count(x => x.RoleId == role.Id),
                Name = role.Name,
                UserCount = users.Count
            });
        }

        return result;
    }

    public async Task CreateAsync(PhaseOneRoleSaveInput input)
    {
        var roleName = NormalizeRoleName(input.Name);
        if (await _roleManager.FindByNameAsync(roleName) != null)
        {
            throw new InvalidOperationException($"角色 {roleName} 已存在。");
        }

        var role = new IdentityRole(Guid.NewGuid(), roleName);
        EnsureSucceeded(await _roleManager.CreateAsync(role));
    }

    public async Task DeleteAsync(Guid id)
    {
        var role = await _roleManager.GetByIdAsync(id);
        if (BuiltInRoleNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("内置角色不允许删除。");
        }

        var users = await _userManager.GetUsersInRoleAsync(role.Name);
        foreach (var user in users)
        {
            EnsureSucceeded(await _userManager.RemoveFromRoleAsync(user, role.Name));
        }

        var mappings = (await _roleMenuRepository.GetListAsync())
            .Where(x => x.RoleId == id)
            .ToList();
        if (mappings.Count > 0)
        {
            await _roleMenuRepository.DeleteManyAsync(mappings, autoSave: true);
        }

        EnsureSucceeded(await _roleManager.DeleteAsync(role));
    }

    private static string GetDescription(string roleName)
    {
        return roleName switch
        {
            PhaseOneRoleNames.Admin => "系统管理员，可维护平台与全量权限。",
            PhaseOneRoleNames.Pmo => "PMO 角色，可查看项目汇总与经营预警。",
            PhaseOneRoleNames.Pm => "项目经理角色，可负责项目执行闭环。",
            PhaseOneRoleNames.Member => "项目成员角色，可处理任务与协作事项。",
            PhaseOneRoleNames.Viewer => "经营查看角色，仅查看汇总与只读经营信息。",
            _ => "自定义角色，可按菜单授权灵活分配。"
        };
    }

    private static int GetOrder(string roleName)
    {
        var index = BuiltInRoleNames
            .Select((name, order) => new { name, order })
            .FirstOrDefault(x => string.Equals(x.name, roleName, StringComparison.OrdinalIgnoreCase));
        return index?.order ?? 1000;
    }

    private static string NormalizeRoleName(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new InvalidOperationException("角色名称不能为空。");
        }

        return roleName.Trim().ToLowerInvariant();
    }

    private static void EnsureSucceeded(Microsoft.AspNetCore.Identity.IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
    }
}
