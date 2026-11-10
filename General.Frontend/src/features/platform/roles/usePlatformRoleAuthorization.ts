import { computed, onBeforeUnmount, ref, watch, type Ref } from 'vue';
import {
  getMenuPermissionTreeApi,
  getPlatformEndpointOptionsApi,
  getRoleMenuIdsApi,
  saveRoleMenusApi,
  type MenuApi,
} from '@/api/core/menu';
import { getOrganizationUnitTreeApi, type OrganizationApi } from '@/api/core/organization';
import { getRoleAuthorizationApi, saveRoleAuthorizationApi, type RoleApi } from '@/api/core/role';
import { getUserListApi, type UserApi } from '@/api/core/user';
import { toast } from '@/components/ui';
import {
  resolveRoleMenuSelection,
  toggleRoleMenuSelection,
  type AccountScopeMode,
  type DataScopeMode,
} from './role-authorization.model';

const USER_PAGE_SIZE = 20;

function errorText(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback;
}

function needsAccountUsers(mode: AccountScopeMode): boolean {
  return mode === 'only_users' || mode === 'data_and_users';
}

export function usePlatformRoleAuthorization(
  roleId: Readonly<Ref<string>>,
  roleName: Readonly<Ref<string>>,
  isOpen: Readonly<Ref<boolean>>,
) {
  const menuTree = ref<MenuApi.PermissionTreeItem[]>([]);
  const selectedMenuIds = ref(new Set<string>());
  const menuLoading = ref(false);
  const menuLoadError = ref('');
  const menuSaving = ref(false);
  const menuSaveError = ref('');
  const menuReady = ref(false);

  const authorization = ref<null | RoleApi.Authorization>(null);
  const dataScopeMode = ref<DataScopeMode>('current_org_and_descendants');
  const accountScopeMode = ref<AccountScopeMode>('data_and_users');
  const customOrganizationUnitIds = ref(new Set<string>());
  const accountUserIds = ref(new Set<string>());
  const apiBlacklist = ref(new Set<string>());
  const authorizationLoading = ref(false);
  const authorizationLoadError = ref('');
  const authorizationSaving = ref(false);
  const authorizationSaveError = ref('');
  const authorizationReady = ref(false);

  const endpointGroups = ref<MenuApi.EndpointGroup[]>([]);
  const endpointOptionsLoading = ref(false);
  const endpointOptionsError = ref('');
  const endpointOptionsReady = ref(false);
  const endpointSearch = ref('');

  const organizations = ref<OrganizationApi.TreeItem[]>([]);
  const organizationsLoading = ref(false);
  const organizationsError = ref('');
  const organizationsReady = ref(false);

  const users = ref<UserApi.ListItem[]>([]);
  const userTotalCount = ref(0);
  const userPageIndex = ref(0);
  const userPageSize = ref(USER_PAGE_SIZE);
  const userKeyword = ref('');
  const usersLoading = ref(false);
  const usersError = ref('');
  const usersReady = ref(false);
  const pageCount = computed(() => Math.max(1, Math.ceil(userTotalCount.value / userPageSize.value)));

  const isAdminRole = computed(() => roleName.value.trim().toLocaleLowerCase() === 'admin');
  const accountUsersRequired = computed(() => needsAccountUsers(accountScopeMode.value));
  const customOrganizationsRequired = computed(() => dataScopeMode.value === 'custom');
  const endpointKeys = computed(() => {
    const result = new Set<string>();
    for (const group of endpointGroups.value) {
      for (const endpoint of group.items) {
        result.add(endpoint.key);
        if (endpoint.capabilityKey) result.add(endpoint.capabilityKey);
      }
    }
    return result;
  });
  const unknownBlacklistKeys = computed(() => [...apiBlacklist.value].filter((key) => !endpointKeys.value.has(key)));
  const filteredEndpointGroups = computed(() => {
    const keyword = endpointSearch.value.trim().toLocaleLowerCase();
    if (!keyword) return endpointGroups.value;

    return endpointGroups.value
      .map((group) => ({
        ...group,
        items: group.items.filter((item) =>
          `${item.key} ${item.label} ${item.capabilityKey ?? ''}`.toLocaleLowerCase().includes(keyword),
        ),
      }))
      .filter((group) => group.items.length > 0);
  });
  const canSaveMenus = computed(() => menuReady.value && !menuLoading.value && !menuSaving.value);
  const canSaveAuthorization = computed(
    () =>
      authorizationReady.value &&
      endpointOptionsReady.value &&
      !endpointOptionsError.value &&
      (!customOrganizationsRequired.value || organizationsReady.value) &&
      (!accountUsersRequired.value || usersReady.value) &&
      !authorizationLoading.value &&
      !authorizationSaving.value &&
      !isAdminRole.value,
  );

  let initializationId = 0;
  let userRequestId = 0;

  function applyAuthorization(result: RoleApi.Authorization) {
    authorization.value = result;
    dataScopeMode.value = result.dataScopeMode as DataScopeMode;
    accountScopeMode.value = result.accountScopeMode as AccountScopeMode;
    customOrganizationUnitIds.value = new Set(result.customOrganizationUnitIds);
    accountUserIds.value = new Set(result.accountUserIds);
    apiBlacklist.value = new Set(result.apiBlacklist);
    authorizationReady.value = true;
  }

  async function loadMenuState(targetInitializationId: number) {
    menuLoading.value = true;
    menuLoadError.value = '';
    menuReady.value = false;
    try {
      const [tree, storedIds, currentAuthorization] = await Promise.all([
        getMenuPermissionTreeApi(),
        getRoleMenuIdsApi(roleId.value),
        getRoleAuthorizationApi(roleId.value),
      ]);
      if (targetInitializationId !== initializationId) return;
      menuTree.value = tree;
      selectedMenuIds.value = new Set(resolveRoleMenuSelection(storedIds, currentAuthorization.menuIds));
      menuReady.value = true;
    } catch (error) {
      if (targetInitializationId !== initializationId) return;
      menuTree.value = [];
      selectedMenuIds.value = new Set();
      menuLoadError.value = errorText(error, '菜单授权读取失败，请重试。');
    } finally {
      if (targetInitializationId === initializationId) menuLoading.value = false;
    }
  }

  async function loadScopeState(targetInitializationId: number) {
    authorizationLoading.value = true;
    authorizationLoadError.value = '';
    authorizationReady.value = false;
    try {
      const result = await getRoleAuthorizationApi(roleId.value);
      if (targetInitializationId !== initializationId) return;
      applyAuthorization(result);
    } catch (error) {
      if (targetInitializationId !== initializationId) return;
      authorization.value = null;
      authorizationLoadError.value = errorText(error, '范围授权读取失败，请重试。');
    } finally {
      if (targetInitializationId === initializationId) authorizationLoading.value = false;
    }
  }

  async function loadEndpointOptions(targetInitializationId: number) {
    endpointOptionsLoading.value = true;
    endpointOptionsError.value = '';
    endpointOptionsReady.value = false;
    try {
      const result = await getPlatformEndpointOptionsApi();
      if (targetInitializationId !== initializationId) return;
      endpointGroups.value = result;
      endpointOptionsReady.value = true;
    } catch (error) {
      if (targetInitializationId !== initializationId) return;
      endpointGroups.value = [];
      endpointOptionsError.value = errorText(error, 'API 黑名单目录加载失败。');
    } finally {
      if (targetInitializationId === initializationId) endpointOptionsLoading.value = false;
    }
  }

  async function loadOrganizations() {
    if (organizationsReady.value || organizationsLoading.value) return;
    organizationsLoading.value = true;
    organizationsError.value = '';
    try {
      organizations.value = await getOrganizationUnitTreeApi();
      organizationsReady.value = true;
    } catch (error) {
      organizations.value = [];
      organizationsError.value = errorText(error, '组织范围加载失败，请重试。');
    } finally {
      organizationsLoading.value = false;
    }
  }

  async function loadUsers(targetPage = userPageIndex.value) {
    const requestId = ++userRequestId;
    userPageIndex.value = Math.max(0, targetPage);
    usersLoading.value = true;
    usersError.value = '';
    usersReady.value = false;
    try {
      const result = await getUserListApi({
        keyword: userKeyword.value.trim() || undefined,
        maxResultCount: userPageSize.value,
        skipCount: userPageIndex.value * userPageSize.value,
      });
      if (requestId !== userRequestId || !isOpen.value) return;
      const lastPageIndex = Math.max(0, Math.ceil(result.totalCount / userPageSize.value) - 1);
      if (userPageIndex.value > lastPageIndex) {
        await loadUsers(lastPageIndex);
        return;
      }
      users.value = result.items;
      userTotalCount.value = result.totalCount;
      usersReady.value = true;
    } catch (error) {
      if (requestId !== userRequestId || !isOpen.value) return;
      users.value = [];
      userTotalCount.value = 0;
      usersError.value = errorText(error, '账号范围用户列表加载失败。');
    } finally {
      if (requestId === userRequestId) usersLoading.value = false;
    }
  }

  async function initialize() {
    const targetInitializationId = ++initializationId;
    userRequestId += 1;
    menuTree.value = [];
    selectedMenuIds.value = new Set();
    menuLoadError.value = '';
    menuReady.value = false;
    authorization.value = null;
    authorizationLoadError.value = '';
    authorizationReady.value = false;
    endpointGroups.value = [];
    endpointOptionsError.value = '';
    endpointOptionsReady.value = false;
    organizations.value = [];
    organizationsError.value = '';
    organizationsReady.value = false;
    users.value = [];
    userTotalCount.value = 0;
    userPageIndex.value = 0;
    userKeyword.value = '';
    usersError.value = '';
    usersReady.value = false;

    await Promise.all([
      loadMenuState(targetInitializationId),
      loadScopeState(targetInitializationId),
      loadEndpointOptions(targetInitializationId),
    ]);
    if (targetInitializationId !== initializationId || !isOpen.value || !authorizationReady.value) return;
    if (customOrganizationsRequired.value) void loadOrganizations();
    if (accountUsersRequired.value) void loadUsers(0);
  }

  function setDataScopeMode(value: string) {
    dataScopeMode.value = value as DataScopeMode;
    if (value === 'custom') void loadOrganizations();
  }

  function setAccountScopeMode(value: string) {
    accountScopeMode.value = value as AccountScopeMode;
    if (needsAccountUsers(value as AccountScopeMode)) void loadUsers(0);
  }

  function toggleOrganization(organizationId: string, selected: boolean) {
    const next = new Set(customOrganizationUnitIds.value);
    if (selected) next.add(organizationId);
    else next.delete(organizationId);
    customOrganizationUnitIds.value = next;
  }

  function toggleAccountUser(userId: string, selected: boolean) {
    const next = new Set(accountUserIds.value);
    if (selected) next.add(userId);
    else next.delete(userId);
    accountUserIds.value = next;
  }

  function toggleEndpointKey(key: string, selected: boolean) {
    const next = new Set(apiBlacklist.value);
    if (selected) next.add(key);
    else next.delete(key);
    apiBlacklist.value = next;
  }

  function toggleMenu(menuId: string, selected: boolean) {
    selectedMenuIds.value = toggleRoleMenuSelection(menuTree.value, selectedMenuIds.value, menuId, selected);
  }

  async function saveMenus() {
    if (!canSaveMenus.value) return;
    menuSaving.value = true;
    menuSaveError.value = '';
    try {
      await saveRoleMenusApi(roleId.value, { menuIds: [...selectedMenuIds.value] });
      const [storedIds, currentAuthorization] = await Promise.all([
        getRoleMenuIdsApi(roleId.value),
        getRoleAuthorizationApi(roleId.value),
      ]);
      selectedMenuIds.value = new Set(resolveRoleMenuSelection(storedIds, currentAuthorization.menuIds));
      toast.success('菜单授权已保存并重新读取。');
      return true;
    } catch (error) {
      menuSaveError.value = errorText(error, '菜单授权保存失败。');
      try {
        const [storedIds, currentAuthorization] = await Promise.all([
          getRoleMenuIdsApi(roleId.value),
          getRoleAuthorizationApi(roleId.value),
        ]);
        selectedMenuIds.value = new Set(resolveRoleMenuSelection(storedIds, currentAuthorization.menuIds));
        menuSaveError.value += ' 已重新读取服务端当前授权。';
      } catch {
        menuSaveError.value += ' 无法重新读取当前授权，请重试读取。';
      }
      return false;
    } finally {
      menuSaving.value = false;
    }
  }

  async function reloadAuthorization() {
    const result = await getRoleAuthorizationApi(roleId.value);
    applyAuthorization(result);
  }

  async function saveAuthorization() {
    if (!canSaveAuthorization.value) return false;
    authorizationSaving.value = true;
    authorizationSaveError.value = '';
    const input: RoleApi.AuthorizationSaveInput = {
      accountScopeMode: accountScopeMode.value,
      accountUserIds: [...accountUserIds.value],
      apiBlacklist: [...apiBlacklist.value],
      customOrganizationUnitIds: [...customOrganizationUnitIds.value],
      dataScopeMode: dataScopeMode.value,
    };

    try {
      await saveRoleAuthorizationApi(roleId.value, input);
      await reloadAuthorization();
      toast.success('数据、账号范围与 API 黑名单已保存并重新读取。');
      return true;
    } catch (error) {
      authorizationSaveError.value = errorText(error, '范围授权保存失败。');
      try {
        await reloadAuthorization();
        authorizationSaveError.value += ' 已重新读取服务端当前授权。';
      } catch {
        authorizationSaveError.value += ' 无法重新读取当前授权，请重试读取。';
      }
      return false;
    } finally {
      authorizationSaving.value = false;
    }
  }

  function changeUserPage(nextPage: number) {
    if (usersLoading.value || nextPage < 0 || nextPage >= pageCount.value || nextPage === userPageIndex.value) return;
    void loadUsers(nextPage);
  }

  function changeUserPageSize(value: string) {
    const nextPageSize = Number(value);
    if (![10, 20, 50, 100].includes(nextPageSize)) return;
    userPageSize.value = nextPageSize;
    void loadUsers(0);
  }

  function searchUsers() {
    void loadUsers(0);
  }

  function retryLoading() {
    if (isOpen.value) void initialize();
  }

  watch(
    () => [isOpen.value, roleId.value] as const,
    ([open]) => {
      if (open) void initialize();
      else {
        initializationId += 1;
        userRequestId += 1;
      }
    },
    { immediate: true },
  );

  onBeforeUnmount(() => {
    initializationId += 1;
    userRequestId += 1;
  });

  return {
    accountScopeMode,
    accountUserIds,
    accountUsersRequired,
    apiBlacklist,
    authorizationLoadError,
    authorizationLoading,
    authorizationSaveError,
    authorizationSaving,
    canSaveAuthorization,
    canSaveMenus,
    changeUserPage,
    changeUserPageSize,
    customOrganizationUnitIds,
    customOrganizationsRequired,
    dataScopeMode,
    endpointGroups,
    endpointOptionsError,
    endpointOptionsLoading,
    endpointSearch,
    filteredEndpointGroups,
    isAdminRole,
    loadOrganizations,
    loadUsers,
    menuLoadError,
    menuLoading,
    menuSaveError,
    menuSaving,
    menuTree,
    organizations,
    organizationsError,
    organizationsLoading,
    organizationsReady,
    pageCount,
    retryLoading,
    searchUsers,
    selectedMenuIds,
    setAccountScopeMode,
    setDataScopeMode,
    toggleAccountUser,
    toggleEndpointKey,
    toggleMenu,
    toggleOrganization,
    unknownBlacklistKeys,
    userKeyword,
    userPageIndex,
    userPageSize,
    userTotalCount,
    users,
    usersError,
    usersLoading,
    usersReady,
    saveAuthorization,
    saveMenus,
  };
}
