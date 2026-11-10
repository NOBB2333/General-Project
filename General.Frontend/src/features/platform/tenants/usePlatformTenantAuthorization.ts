import { computed, onBeforeUnmount, ref, watch, type Ref } from 'vue';
import { getMenuPermissionTreeApi, getPlatformEndpointOptionsApi, type MenuApi } from '@/api/core/menu';
import {
  getTenantAuthorizationApi,
  getTenantUsersPagedApi,
  saveTenantAuthorizationApi,
  type TenantApi,
} from '@/api/core/tenant';
import { toast, type UiSelectOption } from '@/components/ui';
import {
  getTenantEndpointUnknownKeys,
  getTenantMenuUnknownCount,
  toggleTenantApiBlacklist,
  toggleTenantMenuSelection,
  toTenantAuthorizationInput,
} from './tenant-authorization.model';

const TENANT_USER_PAGE_SIZE = 20;

function errorText(error: unknown, fallback: string): string {
  return error instanceof Error ? error.message : fallback;
}

export function usePlatformTenantAuthorization(
  tenant: Readonly<Ref<TenantApi.ListItem>>,
  isOpen: Readonly<Ref<boolean>>,
) {
  const authorization = ref<TenantApi.Authorization | null>(null);
  const selectedAdminUserId = ref('');
  const remark = ref('');
  const selectedMenuIds = ref(new Set<string>());
  const selectedApiBlacklist = ref(new Set<string>());
  const menuTree = ref<MenuApi.PermissionTreeItem[]>([]);
  const endpointGroups = ref<MenuApi.EndpointGroup[]>([]);
  const users = ref<TenantApi.UserItem[]>([]);
  const userTotalCount = ref(0);
  const userPageIndex = ref(0);
  const userKeyword = ref('');
  const endpointSearch = ref('');
  const isLoading = ref(false);
  const isUserLoading = ref(false);
  const isSaving = ref(false);
  const loadError = ref('');
  const userLoadError = ref('');
  const saveError = ref('');
  const dataReady = ref(false);
  const endpointOptionsReady = ref(false);
  let initializationId = 0;
  let userRequestId = 0;

  const userPageCount = computed(() => Math.max(1, Math.ceil(userTotalCount.value / TENANT_USER_PAGE_SIZE)));
  const selectedMenuCount = computed(() => selectedMenuIds.value.size);
  const unknownMenuCount = computed(() => getTenantMenuUnknownCount(menuTree.value, selectedMenuIds.value));
  const unknownEndpointKeys = computed(() =>
    getTenantEndpointUnknownKeys(endpointGroups.value, selectedApiBlacklist.value),
  );
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
  const visibleEndpointCount = computed(() =>
    filteredEndpointGroups.value.reduce((total, group) => total + group.items.length, 0),
  );
  const userOptions = computed<UiSelectOption[]>(() => {
    const options: UiSelectOption[] = users.value.map((item) => ({
      value: item.id,
      label: `${item.displayName || item.username} · ${item.username}${item.isActive ? '' : '（已停用）'}`,
      disabled: !item.isActive,
    }));
    if (selectedAdminUserId.value && !options.some((item) => item.value === selectedAdminUserId.value)) {
      options.unshift({
        value: selectedAdminUserId.value,
        label: `${tenant.value.adminUserName || '当前租户管理员'}（当前配置）`,
      });
    }
    return [{ value: '', label: '不指定管理员' }, ...options];
  });
  const canSave = computed(() => dataReady.value && endpointOptionsReady.value && !isLoading.value && !isSaving.value);

  function resetDialogState() {
    authorization.value = null;
    selectedAdminUserId.value = '';
    remark.value = '';
    selectedMenuIds.value = new Set();
    selectedApiBlacklist.value = new Set();
    menuTree.value = [];
    endpointGroups.value = [];
    users.value = [];
    userTotalCount.value = 0;
    userPageIndex.value = 0;
    userKeyword.value = '';
    endpointSearch.value = '';
    loadError.value = '';
    userLoadError.value = '';
    saveError.value = '';
    dataReady.value = false;
    endpointOptionsReady.value = false;
  }

  async function loadUsers(targetInitializationId: number, pageIndex: number, keyword: string) {
    const requestId = ++userRequestId;
    isUserLoading.value = true;
    userLoadError.value = '';
    try {
      const result = await getTenantUsersPagedApi(tenant.value.id, {
        keyword: keyword.trim() || undefined,
        maxResultCount: TENANT_USER_PAGE_SIZE,
        skipCount: pageIndex * TENANT_USER_PAGE_SIZE,
      });
      if (targetInitializationId !== initializationId || requestId !== userRequestId) return;
      users.value = result.items;
      userTotalCount.value = result.totalCount;
      userPageIndex.value = pageIndex;
      userKeyword.value = keyword;
    } catch (error) {
      if (targetInitializationId !== initializationId || requestId !== userRequestId) return;
      users.value = [];
      userLoadError.value = errorText(error, '租户用户读取失败，请重试。');
    } finally {
      if (targetInitializationId === initializationId && requestId === userRequestId) {
        isUserLoading.value = false;
      }
    }
  }

  async function load() {
    const targetInitializationId = ++initializationId;
    resetDialogState();
    isLoading.value = true;
    try {
      const [current, tree, endpoints] = await Promise.all([
        getTenantAuthorizationApi(tenant.value.id),
        getMenuPermissionTreeApi(),
        getPlatformEndpointOptionsApi(),
      ]);
      if (targetInitializationId !== initializationId) return;
      authorization.value = current;
      selectedAdminUserId.value = current.adminUserId ?? '';
      remark.value = current.remark ?? '';
      selectedMenuIds.value = new Set(current.menuIds);
      selectedApiBlacklist.value = new Set(current.apiBlacklist);
      menuTree.value = tree;
      endpointGroups.value = endpoints;
      dataReady.value = true;
      endpointOptionsReady.value = true;
      void loadUsers(targetInitializationId, 0, tenant.value.adminUserName ?? '');
    } catch (error) {
      if (targetInitializationId !== initializationId) return;
      loadError.value = errorText(error, '租户授权、菜单或API目录读取失败，请重试。');
    } finally {
      if (targetInitializationId === initializationId) isLoading.value = false;
    }
  }

  function searchUsers() {
    void loadUsers(initializationId, 0, userKeyword.value);
  }

  function setUserPage(pageIndex: number) {
    if (pageIndex < 0 || pageIndex >= userPageCount.value || pageIndex === userPageIndex.value) return;
    void loadUsers(initializationId, pageIndex, userKeyword.value);
  }

  function toggleMenu(menuId: string, selected: boolean) {
    selectedMenuIds.value = toggleTenantMenuSelection(menuTree.value, selectedMenuIds.value, menuId, selected);
  }

  function toggleEndpointKey(key: string, selected: boolean) {
    selectedApiBlacklist.value = toggleTenantApiBlacklist(selectedApiBlacklist.value, key, selected);
  }

  async function save(): Promise<boolean> {
    if (!authorization.value || !canSave.value) return false;
    isSaving.value = true;
    saveError.value = '';
    try {
      const input = toTenantAuthorizationInput(
        authorization.value,
        selectedMenuIds.value,
        selectedApiBlacklist.value,
        remark.value,
        selectedAdminUserId.value || null,
      );
      await saveTenantAuthorizationApi(tenant.value.id, input);
      authorization.value = input;
      toast.success('租户权限已保存。');
      return true;
    } catch (error) {
      saveError.value = errorText(error, '租户权限保存失败，请重试。');
      return false;
    } finally {
      isSaving.value = false;
    }
  }

  watch(
    [isOpen, () => tenant.value.id],
    ([open]) => {
      if (open) void load();
      else {
        initializationId += 1;
        userRequestId += 1;
        isLoading.value = false;
        isUserLoading.value = false;
        isSaving.value = false;
        resetDialogState();
      }
    },
    { immediate: true },
  );

  onBeforeUnmount(() => {
    initializationId += 1;
    userRequestId += 1;
  });

  return {
    authorization,
    canSave,
    dataReady,
    endpointGroups,
    endpointOptionsReady,
    endpointSearch,
    filteredEndpointGroups,
    isLoading,
    isSaving,
    isUserLoading,
    load,
    loadError,
    menuTree,
    remark,
    save,
    saveError,
    searchUsers,
    selectedAdminUserId,
    selectedApiBlacklist,
    selectedMenuCount,
    selectedMenuIds,
    setUserPage,
    toggleEndpointKey,
    toggleMenu,
    unknownEndpointKeys,
    unknownMenuCount,
    userKeyword,
    userLoadError,
    userOptions,
    userPageCount,
    userPageIndex,
    userTotalCount,
    users,
    visibleEndpointCount,
  };
}
