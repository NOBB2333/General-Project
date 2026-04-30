using System.Text.Json;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.Users;

namespace General.Admin.Platform;

public class PlatformNotificationPublisher : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<AppNotification, Guid> _notificationRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IPlatformNotificationRecipientResolver _recipientResolver;
    private readonly IRepository<IdentityRole, Guid> _roleRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;
    private readonly IRepository<IdentityUserOrganizationUnit> _userOrganizationUnitRepository;
    private readonly IRepository<AppUserNotification, Guid> _userNotificationRepository;

    public PlatformNotificationPublisher(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        ICurrentUser currentUser,
        IRepository<AppNotification, Guid> notificationRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IPlatformNotificationRecipientResolver recipientResolver,
        IRepository<IdentityRole, Guid> roleRepository,
        IRepository<IdentityUser, Guid> userRepository,
        IRepository<IdentityUserOrganizationUnit> userOrganizationUnitRepository,
        IRepository<AppUserNotification, Guid> userNotificationRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _currentUser = currentUser;
        _notificationRepository = notificationRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _recipientResolver = recipientResolver;
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _userOrganizationUnitRepository = userOrganizationUnitRepository;
        _userNotificationRepository = userNotificationRepository;
    }

    public async Task<Guid?> PublishAsync(PlatformNotificationSendInput input)
    {
        var recipientMode = PlatformNotificationRecipientModes.Normalize(input.Recipient.Mode);
        var recipientUserIds = await ResolveRecipientUserIdsAsync(input.Recipient, recipientMode);
        if (recipientUserIds.Count == 0)
        {
            return null;
        }

        var notification = new AppNotification(
            Guid.NewGuid(),
            input.Title,
            input.Content,
            input.Type,
            input.Level,
            input.Link,
            input.Avatar,
            recipientMode,
            BuildRecipientSummary(input.Recipient, recipientMode),
            _currentUser.Id);
        await _notificationRepository.InsertAsync(notification, autoSave: true);

        var userNotifications = recipientUserIds
            .Select(userId => new AppUserNotification(Guid.NewGuid(), notification.Id, userId))
            .ToList();
        await _userNotificationRepository.InsertManyAsync(userNotifications, autoSave: true);
        return notification.Id;
    }

    private async Task<HashSet<Guid>> ResolveRecipientUserIdsAsync(
        PlatformNotificationRecipientInput input,
        string recipientMode)
    {
        return recipientMode switch
        {
            PlatformNotificationRecipientModes.AllUsers => await ResolveAllUserIdsAsync(),
            PlatformNotificationRecipientModes.Roles => await ResolveRoleUserIdsAsync(input.RoleNames),
            PlatformNotificationRecipientModes.Organizations => await ResolveOrganizationUserIdsAsync(input.OrganizationUnitIds, includeDescendants: false),
            PlatformNotificationRecipientModes.OrganizationsAndDescendants => await ResolveOrganizationUserIdsAsync(input.OrganizationUnitIds, includeDescendants: true),
            _ => await ResolveExistingUserIdsAsync(input.UserIds)
        };
    }

    private async Task<HashSet<Guid>> ResolveAllUserIdsAsync()
    {
        var queryable = await _userRepository.GetQueryableAsync();
        return (await _asyncQueryableExecuter.ToListAsync(
                queryable
                    .Where(x => x.IsActive)
                    .Select(x => x.Id)))
            .ToHashSet();
    }

    private async Task<HashSet<Guid>> ResolveExistingUserIdsAsync(IReadOnlyCollection<Guid> userIds)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var idSet = userIds.ToHashSet();
        var queryable = await _userRepository.GetQueryableAsync();
        return (await _asyncQueryableExecuter.ToListAsync(
                queryable
                    .Where(x => x.IsActive && idSet.Contains(x.Id))
                    .Select(x => x.Id)))
            .ToHashSet();
    }

    private async Task<HashSet<Guid>> ResolveOrganizationUserIdsAsync(
        IReadOnlyCollection<Guid> organizationUnitIds,
        bool includeDescendants)
    {
        if (organizationUnitIds.Count == 0)
        {
            return [];
        }

        var targetIds = organizationUnitIds.ToHashSet();
        if (includeDescendants)
        {
            var organizationQueryable = await _organizationUnitRepository.GetQueryableAsync();
            var selectedOrganizations = await _asyncQueryableExecuter.ToListAsync(
                organizationQueryable.Where(x => targetIds.Contains(x.Id)));
            foreach (var selected in selectedOrganizations)
            {
                var descendantQueryable = await _organizationUnitRepository.GetQueryableAsync();
                var descendantIds = await _asyncQueryableExecuter.ToListAsync(
                    descendantQueryable
                        .Where(x => x.Code.StartsWith(selected.Code))
                        .Select(x => x.Id));
                foreach (var childId in descendantIds)
                {
                    targetIds.Add(childId);
                }
            }
        }

        var userOrganizationQueryable = await _userOrganizationUnitRepository.GetQueryableAsync();
        var userIds = (await _asyncQueryableExecuter.ToListAsync(
                userOrganizationQueryable
                    .Where(x => targetIds.Contains(x.OrganizationUnitId))
                    .Select(x => x.UserId)))
            .Distinct()
            .ToList();
        return await ResolveExistingUserIdsAsync(userIds);
    }

    private async Task<HashSet<Guid>> ResolveRoleUserIdsAsync(IReadOnlyCollection<string> roleNames)
    {
        var normalizedRoleNames = roleNames
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (normalizedRoleNames.Count == 0)
        {
            return [];
        }

        var roleQueryable = await _roleRepository.GetQueryableAsync();
        var roles = await _asyncQueryableExecuter.ToListAsync(
            roleQueryable.Where(x => x.NormalizedName != null && normalizedRoleNames.Contains(x.NormalizedName)));
        if (roles.Count == 0)
        {
            return [];
        }

        var roleIds = roles.Select(x => x.Id).ToHashSet();
        var userIds = await _recipientResolver.GetUserIdsInRolesAsync(roleIds);
        return await ResolveExistingUserIdsAsync(userIds);
    }

    private static string BuildRecipientSummary(PlatformNotificationRecipientInput input, string recipientMode)
    {
        var summary = new
        {
            mode = recipientMode,
            organizationUnitIds = input.OrganizationUnitIds.Distinct().OrderBy(x => x).ToList(),
            roleNames = input.RoleNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList(),
            userIds = input.UserIds.Distinct().OrderBy(x => x).ToList()
        };
        return JsonSerializer.Serialize(summary);
    }
}
