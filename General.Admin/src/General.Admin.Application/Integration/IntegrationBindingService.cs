using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace General.Admin.Integration;

public class IntegrationBindingService : ITransientDependency
{
    private readonly IAsyncQueryableExecuter _asyncQueryableExecuter;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<IntegrationConnector, Guid> _connectorRepository;
    private readonly IRepository<IntegrationDepartmentBinding, Guid> _departmentBindingRepository;
    private readonly IRepository<IntegrationExternalAccount, Guid> _externalAccountRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IRepository<IntegrationGroupBinding, Guid> _groupBindingRepository;
    private readonly IRepository<IntegrationIdMapping, Guid> _idMappingRepository;
    private readonly IRepository<OrganizationUnit, Guid> _organizationUnitRepository;
    private readonly IRepository<AppUserProfile, Guid> _userProfileRepository;
    private readonly IRepository<IntegrationUserBinding, Guid> _userBindingRepository;
    private readonly IRepository<IdentityUser, Guid> _userRepository;

    public IntegrationBindingService(
        IAsyncQueryableExecuter asyncQueryableExecuter,
        ICurrentTenant currentTenant,
        ICurrentUser currentUser,
        IRepository<IntegrationConnector, Guid> connectorRepository,
        IRepository<IntegrationDepartmentBinding, Guid> departmentBindingRepository,
        IRepository<IntegrationExternalAccount, Guid> externalAccountRepository,
        IGuidGenerator guidGenerator,
        IRepository<IntegrationGroupBinding, Guid> groupBindingRepository,
        IRepository<IntegrationIdMapping, Guid> idMappingRepository,
        IRepository<OrganizationUnit, Guid> organizationUnitRepository,
        IRepository<AppUserProfile, Guid> userProfileRepository,
        IRepository<IntegrationUserBinding, Guid> userBindingRepository,
        IRepository<IdentityUser, Guid> userRepository)
    {
        _asyncQueryableExecuter = asyncQueryableExecuter;
        _currentTenant = currentTenant;
        _currentUser = currentUser;
        _connectorRepository = connectorRepository;
        _departmentBindingRepository = departmentBindingRepository;
        _externalAccountRepository = externalAccountRepository;
        _guidGenerator = guidGenerator;
        _groupBindingRepository = groupBindingRepository;
        _idMappingRepository = idMappingRepository;
        _organizationUnitRepository = organizationUnitRepository;
        _userProfileRepository = userProfileRepository;
        _userBindingRepository = userBindingRepository;
        _userRepository = userRepository;
    }

    public async Task<PlatformPagedResultDto<IntegrationExternalAccountDto>> GetExternalAccountsAsync(IntegrationExternalAccountQueryInput input)
    {
        var queryable = await _externalAccountRepository.GetQueryableAsync();
        queryable = ApplyExternalAccountConnectorFilter(queryable, input.ConnectorId, input.ConnectorCode);
        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.DisplayName.Contains(keyword) ||
                x.ExternalUserId.Contains(keyword) ||
                x.ExternalUnionId.Contains(keyword) ||
                x.Mobile.Contains(keyword) ||
                x.Email.Contains(keyword));
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.LastSyncedTime ?? x.CreationTime)
            .Skip(NormalizeSkip(input.SkipCount))
            .Take(NormalizeTake(input.MaxResultCount)));

        return new PlatformPagedResultDto<IntegrationExternalAccountDto>
        {
            Items = items.Select(MapExternalAccount).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationExternalAccountDto> SaveExternalAccountAsync(IntegrationExternalAccountSaveInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        var externalUserId = Check.NotNullOrWhiteSpace(input.ExternalUserId, nameof(input.ExternalUserId), 256).Trim();
        var account = await _externalAccountRepository.FindAsync(x =>
            x.ConnectorId == connector.Id &&
            x.ExternalUserId == externalUserId);
        if (account == null)
        {
            account = new IntegrationExternalAccount(
                _guidGenerator.Create(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                externalUserId,
                _currentTenant.Id,
                input.DisplayName,
                input.ExternalUnionId,
                input.Mobile,
                input.Email,
                input.AvatarUrl,
                input.DepartmentExternalId,
                input.RawJson,
                input.Status);
            await _externalAccountRepository.InsertAsync(account, autoSave: true);
        }
        else
        {
            account.UpdateProfile(
                input.DisplayName,
                input.ExternalUnionId,
                input.Mobile,
                input.Email,
                input.AvatarUrl,
                input.DepartmentExternalId,
                input.RawJson,
                input.Status);
            await _externalAccountRepository.UpdateAsync(account, autoSave: true);
        }

        return MapExternalAccount(account);
    }

    public async Task<IntegrationBatchResultDto> ImportExternalAccountsAsync(IntegrationExternalAccountBatchInput input)
    {
        var result = new IntegrationBatchResultDto();
        foreach (var accountInput in input.Accounts)
        {
            accountInput.ConnectorId = input.ConnectorId;
            var existed = await _externalAccountRepository.AnyAsync(x =>
                x.ConnectorId == input.ConnectorId &&
                x.ExternalUserId == accountInput.ExternalUserId.Trim());
            await SaveExternalAccountAsync(accountInput);
            if (existed)
            {
                result.UpdatedCount++;
            }
            else
            {
                result.CreatedCount++;
            }
        }

        return result;
    }

    public async Task<PlatformPagedResultDto<IntegrationUserBindingDto>> GetUserBindingsAsync(IntegrationUserBindingQueryInput input)
    {
        var queryable = await _userBindingRepository.GetQueryableAsync();
        queryable = ApplyUserBindingConnectorFilter(queryable, input.ConnectorId, input.ConnectorCode);
        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.ExternalDisplayName.Contains(keyword) ||
                x.ExternalUserId.Contains(keyword) ||
                x.InternalUserName.Contains(keyword) ||
                x.ConflictReason.Contains(keyword));
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(NormalizeSkip(input.SkipCount))
            .Take(NormalizeTake(input.MaxResultCount)));

        return new PlatformPagedResultDto<IntegrationUserBindingDto>
        {
            Items = items.Select(MapUserBinding).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationAutoMatchResultDto> AutoMatchUsersAsync(IntegrationAutoMatchInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        var accounts = await _externalAccountRepository.GetListAsync(x => x.ConnectorId == connector.Id && x.Status != IntegrationBindingStatus.Disabled);
        var users = await _userRepository.GetListAsync();
        var profiles = (await _userProfileRepository.GetListAsync())
            .GroupBy(x => x.UserId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(item => item.LastModificationTime ?? item.CreationTime).First());
        var bindings = (await _userBindingRepository.GetListAsync(x => x.ConnectorId == connector.Id))
            .GroupBy(x => x.ExternalAccountId)
            .ToDictionary(x => x.Key, x => x.OrderByDescending(item => item.CreationTime).First());
        var result = new IntegrationAutoMatchResultDto { ScannedCount = accounts.Count };

        foreach (var account in accounts)
        {
            var candidates = users
                .Where(user => IsMatchedByEmail(account, user) || IsMatchedByMobile(account, user, profiles))
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();
            var matchType = ResolveMatchType(account, candidates.FirstOrDefault(), profiles);
            var status = IntegrationBindingStatus.Pending;
            Guid? internalUserId = null;
            var internalUserName = string.Empty;
            var conflictReason = "未匹配到内部用户，请人工绑定。";

            if (candidates.Count == 1)
            {
                var user = candidates[0];
                status = IntegrationBindingStatus.Bound;
                internalUserId = user.Id;
                internalUserName = ResolveUserDisplayName(user);
                conflictReason = string.Empty;
                result.BoundCount++;
                account.MarkStatus(IntegrationBindingStatus.Bound);
            }
            else if (candidates.Count > 1)
            {
                status = IntegrationBindingStatus.Conflict;
                internalUserName = string.Join("、", candidates.Select(ResolveUserDisplayName).Take(5));
                conflictReason = "手机号或邮箱匹配到多个内部用户，请人工确认。";
                result.ConflictCount++;
                account.MarkStatus(IntegrationBindingStatus.Conflict);
            }
            else
            {
                result.PendingCount++;
                account.MarkStatus(IntegrationBindingStatus.Pending);
            }

            if (bindings.TryGetValue(account.Id, out var binding))
            {
                binding.UpdateBinding(internalUserId, internalUserName, matchType, status, conflictReason, "自动匹配更新");
                await _userBindingRepository.UpdateAsync(binding, autoSave: true);
            }
            else
            {
                binding = new IntegrationUserBinding(
                    _guidGenerator.Create(),
                    connector.Id,
                    connector.ConnectorCode,
                    connector.ProviderType,
                    account.Id,
                    account.ExternalUserId,
                    account.DisplayName,
                    _currentTenant.Id,
                    internalUserId,
                    internalUserName,
                    matchType,
                    status,
                    conflictReason,
                    "自动匹配生成");
                await _userBindingRepository.InsertAsync(binding, autoSave: true);
            }

            await _externalAccountRepository.UpdateAsync(account, autoSave: true);
            if (status == IntegrationBindingStatus.Bound && internalUserId.HasValue)
            {
                await UpsertIdMappingAsync(
                    connector,
                    "User",
                    internalUserId.Value.ToString(),
                    "ExternalUser",
                    account.ExternalUserId,
                    IntegrationBindingMatchTypes.Sync,
                    IntegrationBindingStatus.Bound,
                    "自动匹配生成");
            }
        }

        return result;
    }

    public async Task<IntegrationUserBindingDto> BindUserAsync(IntegrationUserBindingSaveInput input)
    {
        var account = await _externalAccountRepository.GetAsync(input.ExternalAccountId);
        var connector = await _connectorRepository.GetAsync(account.ConnectorId);
        var user = await _userRepository.GetAsync(input.InternalUserId);
        var binding = await _userBindingRepository.FindAsync(x => x.ExternalAccountId == account.Id);
        if (binding == null)
        {
            binding = new IntegrationUserBinding(
                _guidGenerator.Create(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                account.Id,
                account.ExternalUserId,
                account.DisplayName,
                _currentTenant.Id,
                user.Id,
                ResolveUserDisplayName(user),
                input.MatchType,
                IntegrationBindingStatus.Bound,
                string.Empty,
                input.Remark);
            await _userBindingRepository.InsertAsync(binding, autoSave: true);
        }
        else
        {
            binding.UpdateBinding(user.Id, ResolveUserDisplayName(user), input.MatchType, IntegrationBindingStatus.Bound, string.Empty, input.Remark);
            await _userBindingRepository.UpdateAsync(binding, autoSave: true);
        }

        account.MarkStatus(IntegrationBindingStatus.Bound);
        await _externalAccountRepository.UpdateAsync(account, autoSave: true);
        await UpsertIdMappingAsync(connector, "User", user.Id.ToString(), "ExternalUser", account.ExternalUserId, input.MatchType, IntegrationBindingStatus.Bound, input.Remark);
        return MapUserBinding(binding);
    }

    public async Task<IntegrationUserBindingDto> BindCurrentUserAsync(IntegrationSelfBindingInput input)
    {
        if (!_currentUser.Id.HasValue)
        {
            throw new UserFriendlyException("当前用户未登录，不能自助绑定外部账号。");
        }

        var account = await _externalAccountRepository.GetAsync(input.ExternalAccountId);
        var user = await _userRepository.GetAsync(_currentUser.Id.Value);
        var profile = await _userProfileRepository.FindAsync(x => x.UserId == user.Id);
        if (!IsMatchedByEmail(account, user) && !IsMatchedByMobile(account, user, profile))
        {
            throw new UserFriendlyException("外部账号手机号或邮箱与当前用户不一致，不能自助绑定。");
        }

        return await BindUserAsync(new IntegrationUserBindingSaveInput
        {
            ExternalAccountId = input.ExternalAccountId,
            InternalUserId = user.Id,
            MatchType = IntegrationBindingMatchTypes.SelfService,
            Remark = input.Remark
        });
    }

    public async Task<IntegrationUserBindingDto> ResolveUserBindingAsync(Guid id, IntegrationBindingResolveInput input)
    {
        var binding = await _userBindingRepository.GetAsync(id);
        IdentityUser? user = null;
        if (input.Status == IntegrationBindingStatus.Bound)
        {
            if (!input.InternalUserId.HasValue)
            {
                throw new UserFriendlyException("确认绑定必须选择内部用户。");
            }

            user = await _userRepository.GetAsync(input.InternalUserId.Value);
        }

        binding.UpdateBinding(
            user?.Id,
            user == null ? string.Empty : ResolveUserDisplayName(user),
            IntegrationBindingMatchTypes.Manual,
            input.Status,
            string.Empty,
            input.Remark);
        await _userBindingRepository.UpdateAsync(binding, autoSave: true);

        var account = await _externalAccountRepository.GetAsync(binding.ExternalAccountId);
        account.MarkStatus(input.Status);
        await _externalAccountRepository.UpdateAsync(account, autoSave: true);
        if (user != null)
        {
            var connector = await _connectorRepository.GetAsync(binding.ConnectorId);
            await UpsertIdMappingAsync(connector, "User", user.Id.ToString(), "ExternalUser", binding.ExternalUserId, IntegrationBindingMatchTypes.Manual, IntegrationBindingStatus.Bound, input.Remark);
        }

        return MapUserBinding(binding);
    }

    public async Task<PlatformPagedResultDto<IntegrationDepartmentBindingDto>> GetDepartmentBindingsAsync(IntegrationDepartmentBindingQueryInput input)
    {
        var queryable = await _departmentBindingRepository.GetQueryableAsync();
        if (input.ConnectorId.HasValue)
        {
            queryable = queryable.Where(x => x.ConnectorId == input.ConnectorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.ExternalDepartmentId.Contains(keyword) ||
                x.ExternalDepartmentName.Contains(keyword) ||
                x.InternalOrganizationUnitName.Contains(keyword));
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(NormalizeSkip(input.SkipCount))
            .Take(NormalizeTake(input.MaxResultCount)));
        return new PlatformPagedResultDto<IntegrationDepartmentBindingDto>
        {
            Items = items.Select(MapDepartmentBinding).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationDepartmentBindingDto> SaveDepartmentBindingAsync(IntegrationDepartmentBindingSaveInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        var organizationUnitName = string.Empty;
        if (input.InternalOrganizationUnitId.HasValue)
        {
            organizationUnitName = (await _organizationUnitRepository.GetAsync(input.InternalOrganizationUnitId.Value)).DisplayName;
        }

        var externalDepartmentId = Check.NotNullOrWhiteSpace(input.ExternalDepartmentId, nameof(input.ExternalDepartmentId), 256).Trim();
        var binding = await _departmentBindingRepository.FindAsync(x =>
            x.ConnectorId == connector.Id &&
            x.ExternalDepartmentId == externalDepartmentId);
        if (binding == null)
        {
            binding = new IntegrationDepartmentBinding(
                _guidGenerator.Create(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                externalDepartmentId,
                input.ExternalDepartmentName,
                _currentTenant.Id,
                input.InternalOrganizationUnitId,
                organizationUnitName,
                input.ParentExternalDepartmentId,
                input.MatchType,
                input.Status,
                string.Empty,
                input.Remark);
            await _departmentBindingRepository.InsertAsync(binding, autoSave: true);
        }
        else
        {
            binding.UpdateBinding(input.InternalOrganizationUnitId, organizationUnitName, input.ExternalDepartmentName, input.ParentExternalDepartmentId, input.MatchType, input.Status, string.Empty, input.Remark);
            await _departmentBindingRepository.UpdateAsync(binding, autoSave: true);
        }

        if (input.InternalOrganizationUnitId.HasValue)
        {
            await UpsertIdMappingAsync(connector, "OrganizationUnit", input.InternalOrganizationUnitId.Value.ToString(), "ExternalDepartment", externalDepartmentId, input.MatchType, input.Status, input.Remark);
        }

        return MapDepartmentBinding(binding);
    }

    public async Task<PlatformPagedResultDto<IntegrationGroupBindingDto>> GetGroupBindingsAsync(IntegrationGroupBindingQueryInput input)
    {
        var queryable = await _groupBindingRepository.GetQueryableAsync();
        if (input.ConnectorId.HasValue)
        {
            queryable = queryable.Where(x => x.ConnectorId == input.ConnectorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.ExternalGroupId.Contains(keyword) ||
                x.ExternalGroupName.Contains(keyword) ||
                x.InternalObjectName.Contains(keyword));
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(NormalizeSkip(input.SkipCount))
            .Take(NormalizeTake(input.MaxResultCount)));
        return new PlatformPagedResultDto<IntegrationGroupBindingDto>
        {
            Items = items.Select(MapGroupBinding).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationGroupBindingDto> SaveGroupBindingAsync(IntegrationGroupBindingSaveInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        var externalGroupId = Check.NotNullOrWhiteSpace(input.ExternalGroupId, nameof(input.ExternalGroupId), 256).Trim();
        var binding = await _groupBindingRepository.FindAsync(x =>
            x.ConnectorId == connector.Id &&
            x.ExternalGroupId == externalGroupId);
        if (binding == null)
        {
            binding = new IntegrationGroupBinding(
                _guidGenerator.Create(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                externalGroupId,
                input.ExternalGroupName,
                input.InternalObjectType,
                input.InternalObjectId,
                input.InternalObjectName,
                _currentTenant.Id,
                input.GroupType,
                input.RobotCode,
                input.Status,
                input.Remark);
            await _groupBindingRepository.InsertAsync(binding, autoSave: true);
        }
        else
        {
            binding.UpdateBinding(input.ExternalGroupName, input.InternalObjectType, input.InternalObjectId, input.InternalObjectName, input.GroupType, input.RobotCode, input.Status, input.Remark);
            await _groupBindingRepository.UpdateAsync(binding, autoSave: true);
        }

        if (!string.IsNullOrWhiteSpace(input.InternalObjectType) &&
            !string.IsNullOrWhiteSpace(input.InternalObjectId))
        {
            await UpsertIdMappingAsync(connector, input.InternalObjectType, input.InternalObjectId, "ExternalGroup", externalGroupId, "Manual", input.Status, input.Remark);
        }

        return MapGroupBinding(binding);
    }

    public async Task<PlatformPagedResultDto<IntegrationIdMappingDto>> GetIdMappingsAsync(IntegrationIdMappingQueryInput input)
    {
        var queryable = await _idMappingRepository.GetQueryableAsync();
        if (input.ConnectorId.HasValue)
        {
            queryable = queryable.Where(x => x.ConnectorId == input.ConnectorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(input.Status) && !input.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            var status = input.Status.Trim();
            queryable = queryable.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(input.Keyword))
        {
            var keyword = input.Keyword.Trim();
            queryable = queryable.Where(x =>
                x.InternalObjectType.Contains(keyword) ||
                x.InternalObjectId.Contains(keyword) ||
                x.ExternalObjectType.Contains(keyword) ||
                x.ExternalObjectId.Contains(keyword));
        }

        var totalCount = await _asyncQueryableExecuter.CountAsync(queryable);
        var items = await _asyncQueryableExecuter.ToListAsync(queryable
            .OrderByDescending(x => x.CreationTime)
            .Skip(NormalizeSkip(input.SkipCount))
            .Take(NormalizeTake(input.MaxResultCount)));
        return new PlatformPagedResultDto<IntegrationIdMappingDto>
        {
            Items = items.Select(MapIdMapping).ToList(),
            TotalCount = totalCount
        };
    }

    public async Task<IntegrationIdMappingDto> SaveIdMappingAsync(IntegrationIdMappingSaveInput input)
    {
        var connector = await _connectorRepository.GetAsync(input.ConnectorId);
        var mapping = await UpsertIdMappingAsync(
            connector,
            input.InternalObjectType,
            input.InternalObjectId,
            input.ExternalObjectType,
            input.ExternalObjectId,
            input.Source,
            input.Status,
            input.Remark);
        return MapIdMapping(mapping);
    }

    private async Task<IntegrationIdMapping> UpsertIdMappingAsync(
        IntegrationConnector connector,
        string internalObjectType,
        string internalObjectId,
        string externalObjectType,
        string externalObjectId,
        string source,
        string status,
        string remark)
    {
        if (string.IsNullOrWhiteSpace(internalObjectType) ||
            string.IsNullOrWhiteSpace(internalObjectId) ||
            string.IsNullOrWhiteSpace(externalObjectType) ||
            string.IsNullOrWhiteSpace(externalObjectId))
        {
            throw new UserFriendlyException("内外部 ID 映射必须包含完整的对象类型和对象 ID。");
        }

        var mapping = await _idMappingRepository.FindAsync(x =>
            x.ConnectorId == connector.Id &&
            x.InternalObjectType == internalObjectType.Trim() &&
            x.InternalObjectId == internalObjectId.Trim() &&
            x.ExternalObjectType == externalObjectType.Trim() &&
            x.ExternalObjectId == externalObjectId.Trim());
        if (mapping == null)
        {
            mapping = new IntegrationIdMapping(
                _guidGenerator.Create(),
                connector.Id,
                connector.ConnectorCode,
                connector.ProviderType,
                internalObjectType,
                internalObjectId,
                externalObjectType,
                externalObjectId,
                _currentTenant.Id,
                source,
                status,
                remark);
            await _idMappingRepository.InsertAsync(mapping, autoSave: true);
        }
        else
        {
            mapping.Update(source, status, remark);
            await _idMappingRepository.UpdateAsync(mapping, autoSave: true);
        }

        return mapping;
    }

    private static IQueryable<IntegrationExternalAccount> ApplyExternalAccountConnectorFilter(
        IQueryable<IntegrationExternalAccount> queryable,
        Guid? connectorId,
        string connectorCode)
    {
        if (connectorId.HasValue)
        {
            queryable = queryable.Where(x => x.ConnectorId == connectorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(connectorCode))
        {
            var code = connectorCode.Trim();
            queryable = queryable.Where(x => x.ConnectorCode == code);
        }

        return queryable;
    }

    private static IQueryable<IntegrationUserBinding> ApplyUserBindingConnectorFilter(
        IQueryable<IntegrationUserBinding> queryable,
        Guid? connectorId,
        string connectorCode)
    {
        if (connectorId.HasValue)
        {
            queryable = queryable.Where(x => x.ConnectorId == connectorId.Value);
        }

        if (!string.IsNullOrWhiteSpace(connectorCode))
        {
            var code = connectorCode.Trim();
            queryable = queryable.Where(x => x.ConnectorCode == code);
        }

        return queryable;
    }

    private static bool IsMatchedByEmail(IntegrationExternalAccount account, IdentityUser user)
    {
        return !string.IsNullOrWhiteSpace(account.Email) &&
               !string.IsNullOrWhiteSpace(user.Email) &&
               account.Email.Trim().Equals(user.Email.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsMatchedByMobile(IntegrationExternalAccount account, IdentityUser user, IReadOnlyDictionary<Guid, AppUserProfile> profiles)
    {
        return IsMatchedByMobile(account, user, profiles.GetValueOrDefault(user.Id));
    }

    private static bool IsMatchedByMobile(IntegrationExternalAccount account, IdentityUser user, AppUserProfile? profile)
    {
        if (string.IsNullOrWhiteSpace(account.Mobile))
        {
            return false;
        }

        var mobile = NormalizePhone(account.Mobile);
        return (!string.IsNullOrWhiteSpace(user.PhoneNumber) && mobile == NormalizePhone(user.PhoneNumber)) ||
               (!string.IsNullOrWhiteSpace(profile?.PhoneNumber) && mobile == NormalizePhone(profile.PhoneNumber));
    }

    private static string ResolveMatchType(IntegrationExternalAccount account, IdentityUser? user, IReadOnlyDictionary<Guid, AppUserProfile> profiles)
    {
        if (user == null)
        {
            return IntegrationBindingMatchTypes.Sync;
        }

        var emailMatched = IsMatchedByEmail(account, user);
        var mobileMatched = IsMatchedByMobile(account, user, profiles);
        return (emailMatched, mobileMatched) switch
        {
            (true, true) => IntegrationBindingMatchTypes.MobileAndEmail,
            (true, false) => IntegrationBindingMatchTypes.Email,
            (false, true) => IntegrationBindingMatchTypes.Mobile,
            _ => IntegrationBindingMatchTypes.Sync
        };
    }

    private static string NormalizePhone(string value)
    {
        return new string(value.Where(char.IsDigit).ToArray());
    }

    private static string ResolveUserDisplayName(IdentityUser user)
    {
        var fullName = $"{user.Name} {user.Surname}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName : fullName;
    }

    private static int NormalizeSkip(int skipCount)
    {
        return Math.Max(skipCount, 0);
    }

    private static int NormalizeTake(int maxResultCount)
    {
        return Math.Clamp(maxResultCount, 1, 500);
    }

    private static IntegrationExternalAccountDto MapExternalAccount(IntegrationExternalAccount item)
    {
        return new IntegrationExternalAccountDto
        {
            AvatarUrl = item.AvatarUrl,
            ConnectorCode = item.ConnectorCode,
            ConnectorId = item.ConnectorId,
            DepartmentExternalId = item.DepartmentExternalId,
            DisplayName = item.DisplayName,
            Email = item.Email,
            ExternalUnionId = item.ExternalUnionId,
            ExternalUserId = item.ExternalUserId,
            Id = item.Id,
            LastSyncedTime = item.LastSyncedTime,
            Mobile = item.Mobile,
            ProviderType = item.ProviderType,
            RawJson = item.RawJson,
            Status = item.Status
        };
    }

    private static IntegrationUserBindingDto MapUserBinding(IntegrationUserBinding item)
    {
        return new IntegrationUserBindingDto
        {
            BoundTime = item.BoundTime,
            ConnectorCode = item.ConnectorCode,
            ConnectorId = item.ConnectorId,
            ConflictReason = item.ConflictReason,
            ExternalAccountId = item.ExternalAccountId,
            ExternalDisplayName = item.ExternalDisplayName,
            ExternalUserId = item.ExternalUserId,
            Id = item.Id,
            InternalUserId = item.InternalUserId,
            InternalUserName = item.InternalUserName,
            MatchType = item.MatchType,
            ProviderType = item.ProviderType,
            Remark = item.Remark,
            Status = item.Status
        };
    }

    private static IntegrationDepartmentBindingDto MapDepartmentBinding(IntegrationDepartmentBinding item)
    {
        return new IntegrationDepartmentBindingDto
        {
            ConnectorCode = item.ConnectorCode,
            ConnectorId = item.ConnectorId,
            ConflictReason = item.ConflictReason,
            ExternalDepartmentId = item.ExternalDepartmentId,
            ExternalDepartmentName = item.ExternalDepartmentName,
            Id = item.Id,
            InternalOrganizationUnitId = item.InternalOrganizationUnitId,
            InternalOrganizationUnitName = item.InternalOrganizationUnitName,
            MatchType = item.MatchType,
            ParentExternalDepartmentId = item.ParentExternalDepartmentId,
            ProviderType = item.ProviderType,
            Remark = item.Remark,
            Status = item.Status
        };
    }

    private static IntegrationGroupBindingDto MapGroupBinding(IntegrationGroupBinding item)
    {
        return new IntegrationGroupBindingDto
        {
            ConnectorCode = item.ConnectorCode,
            ConnectorId = item.ConnectorId,
            ExternalGroupId = item.ExternalGroupId,
            ExternalGroupName = item.ExternalGroupName,
            GroupType = item.GroupType,
            Id = item.Id,
            InternalObjectId = item.InternalObjectId,
            InternalObjectName = item.InternalObjectName,
            InternalObjectType = item.InternalObjectType,
            ProviderType = item.ProviderType,
            Remark = item.Remark,
            RobotCode = item.RobotCode,
            Status = item.Status
        };
    }

    private static IntegrationIdMappingDto MapIdMapping(IntegrationIdMapping item)
    {
        return new IntegrationIdMappingDto
        {
            ConnectorCode = item.ConnectorCode,
            ConnectorId = item.ConnectorId,
            ExternalObjectId = item.ExternalObjectId,
            ExternalObjectType = item.ExternalObjectType,
            Id = item.Id,
            InternalObjectId = item.InternalObjectId,
            InternalObjectType = item.InternalObjectType,
            ProviderType = item.ProviderType,
            Remark = item.Remark,
            Source = item.Source,
            Status = item.Status
        };
    }
}
