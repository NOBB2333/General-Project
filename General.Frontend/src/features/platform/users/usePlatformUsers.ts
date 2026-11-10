import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { getUserListApi, type UserApi } from '@/api/core/user';

const DEFAULT_PAGE_SIZE = 20;

function optionalText(value: string): string | undefined {
  const normalized = value.trim();
  return normalized || undefined;
}

export function usePlatformUsers() {
  const users = ref<UserApi.ListItem[]>([]);
  const totalCount = ref(0);
  const pageIndex = ref(0);
  const pageSize = ref(DEFAULT_PAGE_SIZE);
  const keyword = ref('');
  const userName = ref('');
  const employeeNo = ref('');
  const phoneNumber = ref('');
  const roleName = ref('');
  const statusFilter = ref('');
  const loading = ref(false);
  const errorMessage = ref('');
  let latestRequestId = 0;

  const pageCount = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)));

  async function loadUsers(targetPage = pageIndex.value) {
    const requestId = ++latestRequestId;
    pageIndex.value = Math.max(0, targetPage);
    loading.value = true;
    errorMessage.value = '';
    users.value = [];

    try {
      const result = await getUserListApi({
        employeeNo: optionalText(employeeNo.value),
        isActive: statusFilter.value === '' ? undefined : statusFilter.value === 'true',
        keyword: optionalText(keyword.value),
        maxResultCount: pageSize.value,
        roleName: optionalText(roleName.value),
        skipCount: pageIndex.value * pageSize.value,
        phoneNumber: optionalText(phoneNumber.value),
        userName: optionalText(userName.value),
      });

      if (requestId !== latestRequestId) return;

      const lastPageIndex = Math.max(0, Math.ceil(result.totalCount / pageSize.value) - 1);
      if (pageIndex.value > lastPageIndex) {
        await loadUsers(lastPageIndex);
        return;
      }

      users.value = result.items;
      totalCount.value = result.totalCount;
    } catch (error) {
      if (requestId !== latestRequestId) return;
      users.value = [];
      totalCount.value = 0;
      errorMessage.value = error instanceof Error ? error.message : '用户列表加载失败，请重试。';
    } finally {
      if (requestId === latestRequestId) loading.value = false;
    }
  }

  function searchUsers() {
    return loadUsers(0);
  }

  function resetFilters() {
    keyword.value = '';
    userName.value = '';
    employeeNo.value = '';
    phoneNumber.value = '';
    roleName.value = '';
    statusFilter.value = '';
    pageSize.value = DEFAULT_PAGE_SIZE;
    return loadUsers(0);
  }

  function changePageSize(value: string) {
    const nextPageSize = Number(value);
    if (![10, 20, 50, 100].includes(nextPageSize)) return;
    pageSize.value = nextPageSize;
    return loadUsers(0);
  }

  function changePage(nextPage: number) {
    if (loading.value || nextPage < 0 || nextPage >= pageCount.value || nextPage === pageIndex.value) return;
    return loadUsers(nextPage);
  }

  onMounted(() => void loadUsers());
  onBeforeUnmount(() => {
    latestRequestId += 1;
  });

  return {
    changePage,
    changePageSize,
    errorMessage,
    employeeNo,
    keyword,
    loadUsers,
    loading,
    pageCount,
    pageIndex,
    pageSize,
    phoneNumber,
    resetFilters,
    roleName,
    searchUsers,
    statusFilter,
    totalCount,
    userName,
    users,
  };
}
