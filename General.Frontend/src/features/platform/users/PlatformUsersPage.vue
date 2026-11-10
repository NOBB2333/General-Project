<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, Plus, RefreshCw, Search, ShieldCheck, UserRound, UsersRound } from 'lucide-vue-next';
import { deleteUserApi, createUserApi, resetUserPasswordApi, updateUserApi, type UserApi } from '@/api/core/user';
import {
  UiButton,
  UiDialog,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
} from '@/components/ui';
import { canAccess } from '@/directives/permission';
import { hasCurrentTenantScope } from '@/session/token-storage';
import { sessionState } from '@/session/session-store';
import PlatformUserEditorDialog from './PlatformUserEditorDialog.vue';
import PlatformUserResetPasswordDialog from './PlatformUserResetPasswordDialog.vue';
import PlatformUserTable from './PlatformUserTable.vue';
import { usePlatformUsers } from './usePlatformUsers';
import PlatformOnlineUsersPanel from '../monitoring/PlatformOnlineUsersPanel.vue';
import { formatMonitoringDate } from '../monitoring/monitoring.model';

const route = useRoute();
const router = useRouter();
const activeTab = ref(route.query.tab === 'online' ? 'online' : 'accounts');
const detailOpen = ref(false);
const selectedUser = ref<UserApi.ListItem | null>(null);
const editorOpen = ref(false);
const editorUser = ref<UserApi.ListItem | null>(null);
const activeOverride = ref<boolean | null>(null);
const editorSaving = ref(false);
const editorError = ref('');
const resetPasswordOpen = ref(false);
const resetPasswordUser = ref<UserApi.ListItem | null>(null);
const resetPasswordSaving = ref(false);
const resetPasswordError = ref('');
const deleteOpen = ref(false);
const deleteUser = ref<UserApi.ListItem | null>(null);
const deleteConfirmation = ref('');
const deleteSaving = ref(false);
const deleteError = ref('');
const canManageUsers = computed(() => canAccess('Platform.User.Manage'));
const hostGlobalAdminView = computed(() => {
  const currentUser = sessionState.user;
  return Boolean(
    currentUser &&
    !currentUser.isHostTenantOperation &&
    !hasCurrentTenantScope() &&
    currentUser.roles.some((role) => role.toLocaleLowerCase() === 'admin'),
  );
});

const {
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
} = usePlatformUsers();

watch(
  () => route.query.tab,
  (tab) => {
    const nextTab = tab === 'online' ? 'online' : 'accounts';
    if (activeTab.value !== nextTab) activeTab.value = nextTab;
  },
);

watch(activeTab, (tab) => {
  const currentTab = route.query.tab === 'online' ? 'online' : undefined;
  const nextTab = tab === 'online' ? 'online' : undefined;
  if (currentTab === nextTab) return;
  void router.replace({ query: { ...route.query, tab: nextTab } });
});

const statusOptions = [
  { label: '全部状态', value: '' },
  { label: '启用', value: 'true' },
  { label: '停用', value: 'false' },
];
const pageSizeOptions = [
  { label: '10 条/页', value: '10' },
  { label: '20 条/页', value: '20' },
  { label: '50 条/页', value: '50' },
  { label: '100 条/页', value: '100' },
];

const metrics = computed(
  () =>
    [
      { id: 'total', label: '筛选结果', value: totalCount.value, icon: UsersRound, tone: 'primary' },
      { id: 'page', label: '本页用户', value: users.value.length, icon: UserRound, tone: 'success' },
      {
        id: 'enabled',
        label: '本页启用',
        value: users.value.filter((user) => user.isActive).length,
        icon: ShieldCheck,
        tone: 'success',
      },
      {
        id: 'disabled',
        label: '本页停用',
        value: users.value.filter((user) => !user.isActive).length,
        icon: ShieldCheck,
        tone: 'warning',
      },
    ] as const,
);

function viewUser(user: UserApi.ListItem) {
  selectedUser.value = user;
  detailOpen.value = true;
}

function canWriteUser(_user: UserApi.ListItem): boolean {
  return canManageUsers.value && !hostGlobalAdminView.value;
}

function canDeleteSelectedUser(user: UserApi.ListItem): boolean {
  return canWriteUser(user) && user.username.toLocaleLowerCase() !== sessionState.user?.username.toLocaleLowerCase();
}

function openEditor(user: UserApi.ListItem | null, nextActiveState: boolean | null = null) {
  if (user && !canWriteUser(user)) return;
  editorUser.value = user;
  activeOverride.value = nextActiveState;
  editorError.value = '';
  editorOpen.value = true;
}

function toWriteError(error: unknown, fallback: string): string {
  const message = error instanceof Error ? error.message : fallback;
  if (
    /username.*(already|taken|exists)|(already|taken|exists).*username|用户名.*(已存在|占用)|账号.*已存在/iu.test(
      message,
    )
  ) {
    return '该登录账号已存在，请更换账号后重试。';
  }
  if (/internal error occurred|internal server error/iu.test(message)) {
    return '服务端拒绝了本次操作。请检查登录账号是否重复、字段格式和密码规则后重试。';
  }
  return message || fallback;
}

async function submitUser(input: UserApi.SaveInput) {
  if (!canManageUsers.value) return;
  if (editorUser.value && !canWriteUser(editorUser.value)) return;
  editorSaving.value = true;
  editorError.value = '';
  const isCreate = !editorUser.value;
  try {
    if (editorUser.value) await updateUserApi(editorUser.value.id, input);
    else await createUserApi(input);
    editorOpen.value = false;
    toast.success(isCreate ? '用户已创建。' : '用户信息已保存。');
    await loadUsers();
  } catch (error) {
    editorError.value = toWriteError(
      error,
      isCreate ? '用户创建失败，请检查输入后重试。' : '用户保存失败，请检查输入后重试。',
    );
  } finally {
    editorSaving.value = false;
  }
}

function openResetPassword(user: UserApi.ListItem) {
  if (!canWriteUser(user)) return;
  resetPasswordUser.value = user;
  resetPasswordError.value = '';
  resetPasswordOpen.value = true;
}

async function submitResetPassword(password: string) {
  if (!resetPasswordUser.value || !canWriteUser(resetPasswordUser.value)) return;
  resetPasswordSaving.value = true;
  resetPasswordError.value = '';
  try {
    await resetUserPasswordApi(resetPasswordUser.value.id, { newPassword: password });
    resetPasswordOpen.value = false;
    toast.success('用户密码已重置。');
  } catch (error) {
    resetPasswordError.value = toWriteError(error, '密码重置失败，请检查密码规则后重试。');
  } finally {
    resetPasswordSaving.value = false;
  }
}

function openDeleteUser(user: UserApi.ListItem) {
  if (!canDeleteSelectedUser(user)) return;
  deleteUser.value = user;
  deleteConfirmation.value = '';
  deleteError.value = '';
  deleteOpen.value = true;
}

async function confirmDeleteUser() {
  if (!deleteUser.value || !canDeleteSelectedUser(deleteUser.value)) return;
  if (deleteConfirmation.value.trim() !== deleteUser.value.username) {
    deleteError.value = '输入的账号与待删除账号不一致。';
    return;
  }
  deleteSaving.value = true;
  deleteError.value = '';
  try {
    await deleteUserApi(deleteUser.value.id);
    deleteOpen.value = false;
    toast.success('用户已删除。');
    await loadUsers();
  } catch (error) {
    deleteError.value = toWriteError(error, '用户删除失败，请稍后重试。');
  } finally {
    deleteSaving.value = false;
  }
}

function formatLastLogin(value: null | string | undefined): string {
  return formatMonitoringDate(value ?? null, { dateStyle: 'medium', timeStyle: 'short' });
}
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="list"
    data-page-route="/platform/users"
    data-platform-users-page
  >
    <header :class="$style.pageHeader">
      <div :class="$style.titleBlock">
        <div :class="$style.breadcrumb"><span>平台中心</span><span>›</span><strong>用户管理</strong></div>
        <div :class="$style.titleRow">
          <ArrowLeft
            :size="22"
            :stroke-width="1.8"
            :class="$style.titleArrow"
          />
          <h1>用户管理</h1>
        </div>
      </div>
    </header>

    <section
      v-if="activeTab === 'accounts'"
      :class="$style.metricGrid"
      aria-label="用户查询结果摘要"
    >
      <article
        v-for="metric in metrics"
        :key="metric.id"
        :class="$style.metricCard"
        :data-tone="metric.tone"
      >
        <div :class="$style.metricLabel">
          <component
            :is="metric.icon"
            :size="17"
            :stroke-width="1.8"
          />
          <span>{{ metric.label }}</span>
        </div>
        <strong>{{ metric.value.toLocaleString('zh-CN') }}</strong>
      </article>
    </section>

    <UiTabs v-model="activeTab">
      <div :class="$style.contentHeader">
        <UiTabsList
          label="用户管理视图"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            value="accounts"
            :class="$style.tabTrigger"
          >
            <UserRound
              :size="15"
              :stroke-width="1.8"
            />用户列表
          </UiTabsTrigger>
          <UiTabsTrigger
            v-if="canManageUsers"
            value="online"
            :class="$style.tabTrigger"
          >
            <UsersRound
              :size="15"
              :stroke-width="1.8"
            />在线用户
          </UiTabsTrigger>
        </UiTabsList>
        <UiButton
          v-if="activeTab === 'accounts'"
          size="md"
          :disabled="loading"
          @click="loadUsers()"
        >
          <template #leading
            ><RefreshCw
              :size="15"
              :stroke-width="1.8"
          /></template>
          刷新列表
        </UiButton>
        <UiButton
          v-if="canManageUsers && activeTab === 'accounts'"
          variant="primary"
          size="md"
          @click="openEditor(null)"
        >
          <template #leading
            ><Plus
              :size="15"
              :stroke-width="1.8"
          /></template>
          新增用户
        </UiButton>
      </div>

      <UiTabPanel value="accounts">
        <form
          :class="$style.filterBar"
          role="search"
          @submit.prevent="searchUsers"
        >
          <label :class="$style.searchField">
            <Search
              :size="15"
              :stroke-width="1.8"
            />
            <input
              v-model="keyword"
              type="search"
              aria-label="关键词"
              placeholder="搜索姓名、账号、邮箱..."
            />
          </label>
          <UiInput
            v-model="userName"
            aria-label="用户名筛选"
            placeholder="用户名"
            :class="$style.filterInput"
          />
          <UiInput
            v-model="roleName"
            aria-label="角色筛选"
            placeholder="角色名称"
            :class="$style.filterInput"
          />
          <UiInput
            v-model="employeeNo"
            aria-label="员工编号筛选"
            placeholder="员工编号"
            :class="$style.filterInputNarrow"
          />
          <UiInput
            v-model="phoneNumber"
            aria-label="手机号筛选"
            placeholder="手机号"
            :class="$style.filterInputNarrow"
          />
          <UiSelect
            v-model="statusFilter"
            label="状态筛选"
            :options="statusOptions"
            size="md"
          />
          <UiButton
            type="submit"
            variant="primary"
            size="md"
            :disabled="loading"
          >
            <template #leading
              ><Search
                :size="15"
                :stroke-width="1.8"
            /></template>
            查询
          </UiButton>
          <UiButton
            type="button"
            size="md"
            :disabled="loading"
            @click="resetFilters"
            >重置</UiButton
          >
        </form>

        <div
          v-if="errorMessage"
          :class="$style.errorState"
          role="alert"
        >
          <span>{{ errorMessage }}</span>
          <UiButton
            size="sm"
            @click="loadUsers()"
            >重试</UiButton
          >
        </div>

        <PlatformUserTable
          :rows="users"
          :loading="loading"
          :can-manage="canManageUsers"
          :is-writable="canWriteUser"
          :can-delete="canDeleteSelectedUser"
          @view="viewUser"
          @edit="openEditor"
          @set-active="(user, isActive) => openEditor(user, isActive)"
          @reset-password="openResetPassword"
          @delete="openDeleteUser"
        />

        <footer
          :class="$style.pagination"
          aria-label="用户列表分页"
        >
          <span>共 {{ totalCount.toLocaleString('zh-CN') }} 人 · 第 {{ pageIndex + 1 }} / {{ pageCount }} 页</span>
          <div :class="$style.paginationActions">
            <UiSelect
              :model-value="String(pageSize)"
              label="每页条数"
              :options="pageSizeOptions"
              size="sm"
              :disabled="loading"
              @update:model-value="changePageSize"
            />
            <UiButton
              size="sm"
              :disabled="loading || pageIndex === 0"
              @click="changePage(pageIndex - 1)"
              >上一页</UiButton
            >
            <UiButton
              size="sm"
              :disabled="loading || pageIndex + 1 >= pageCount"
              @click="changePage(pageIndex + 1)"
              >下一页</UiButton
            >
          </div>
        </footer>
      </UiTabPanel>
      <UiTabPanel
        v-if="activeTab === 'online' && canManageUsers"
        value="online"
      >
        <PlatformOnlineUsersPanel />
      </UiTabPanel>
    </UiTabs>

    <UiDialog
      v-model:open="detailOpen"
      title="用户详情"
      description="以下字段来自 General.Admin 用户列表 DTO。"
    >
      <div
        v-if="selectedUser"
        :class="$style.detailGrid"
      >
        <strong>姓名</strong><span>{{ selectedUser.displayName || selectedUser.username }}</span> <strong>用户名</strong
        ><span>{{ selectedUser.username }}</span> <strong>邮箱</strong><span>{{ selectedUser.email || '—' }}</span>
        <strong>手机号</strong><span>{{ selectedUser.phoneNumber || '—' }}</span> <strong>员工编号</strong
        ><span>{{ selectedUser.employeeNo || '—' }}</span> <strong>组织</strong
        ><span>{{ selectedUser.organizationUnitNames.join('、') || '—' }}</span> <strong>角色</strong
        ><span>{{ selectedUser.roles.join('、') || '—' }}</span> <strong>账号状态</strong
        ><span>{{ selectedUser.isActive ? '启用' : '停用' }}</span> <strong>在线状态</strong
        ><span>{{ selectedUser.isOnline ? '在线' : '离线' }}</span> <strong>最近登录</strong
        ><span>{{ formatLastLogin(selectedUser.lastLoginTime) }}</span> <strong>租户</strong
        ><span>{{ selectedUser.tenantName || '—' }}</span>
      </div>
    </UiDialog>

    <PlatformUserEditorDialog
      v-model:open="editorOpen"
      :user="editorUser"
      :active-override="activeOverride"
      :host-scope="hostGlobalAdminView"
      :saving="editorSaving"
      :save-error="editorError"
      @submit="submitUser"
    />

    <PlatformUserResetPasswordDialog
      v-model:open="resetPasswordOpen"
      :user="resetPasswordUser"
      :saving="resetPasswordSaving"
      :error="resetPasswordError"
      @submit="submitResetPassword"
    />

    <UiDialog
      v-model:open="deleteOpen"
      title="删除用户"
      description="此操作会清理账号档案、组织关系和外部账号绑定。"
      size="md"
    >
      <div :class="$style.deleteDialog">
        <p>
          请确认要删除用户 <strong>{{ deleteUser?.username }}</strong
          >。此操作无法在本页撤销。
        </p>
        <label :class="$style.deleteField">
          <span>输入用户账号以确认</span>
          <UiInput
            v-model="deleteConfirmation"
            :disabled="deleteSaving"
            aria-label="输入用户账号以确认删除"
            autocomplete="off"
          />
        </label>
        <p
          v-if="deleteError"
          :class="$style.deleteError"
          role="alert"
        >
          {{ deleteError }}
        </p>
      </div>
      <template #footer>
        <UiButton
          :disabled="deleteSaving"
          @click="deleteOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="danger"
          :disabled="deleteSaving || !deleteUser || deleteConfirmation.trim() !== deleteUser.username"
          :loading="deleteSaving"
          @click="confirmDeleteUser"
        >
          删除用户
        </UiButton>
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-w-[1100px] pb-6;
  color: var(--ui-text);
}
.pageHeader {
  @apply mb-5 flex items-end justify-between;
}
.breadcrumb {
  @apply mb-2 flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.breadcrumb strong {
  color: var(--ui-text-muted);
}
.titleRow {
  @apply flex items-center gap-3;
}
.titleArrow {
  color: var(--ui-text-muted);
}
.titleRow h1 {
  @apply m-0 text-[22px] font-bold leading-8;
}
.metricGrid {
  @apply mb-5 grid grid-cols-4 gap-3;
}
.metricCard {
  @apply flex min-h-[100px] flex-col border bg-white px-4 py-4;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.metricLabel {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-text-subtle);
}
.metricCard strong {
  @apply mt-3 text-[23px] font-bold leading-7;
  color: var(--app-primary);
}
.metricCard[data-tone='success'] .metricLabel,
.metricCard[data-tone='success'] strong {
  color: var(--ui-success);
}
.metricCard[data-tone='warning'] .metricLabel,
.metricCard[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.metricCard[data-tone='purple'] .metricLabel,
.metricCard[data-tone='purple'] strong {
  color: #6554c0;
}
.contentHeader {
  @apply flex items-center justify-between border-b;
  border-color: var(--ui-border);
}
.tabs {
  @apply gap-2;
}
.tabTrigger {
  @apply min-h-10 rounded-none px-3 py-2;
}
.tabTrigger[data-state='active'] {
  @apply border-b-2;
  border-color: var(--app-primary);
  background: transparent;
}
.tabCount {
  @apply ml-1 flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[10px];
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.filterBar {
  @apply mb-3 flex flex-wrap items-center gap-2 pt-4;
}
.searchField {
  @apply flex h-9 w-[355px] items-center gap-2 border bg-white px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.searchField:focus-within {
  border-color: var(--ui-focus);
  box-shadow: 0 0 0 1px var(--ui-focus);
}
.searchField input {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}
.searchField input::placeholder {
  color: #97a0af;
}
.filterInput {
  @apply max-w-[160px];
}
.filterInputNarrow {
  @apply max-w-[140px];
}
.roleGrid {
  @apply grid grid-cols-3 gap-3 pt-4;
}
.roleCard {
  @apply flex min-h-[175px] flex-col border bg-white p-4;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.roleHeading {
  @apply flex items-start gap-3;
}
.roleIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.roleCard[data-tone='orange'] .roleIcon {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.roleCard[data-tone='green'] .roleIcon {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.roleCard h2 {
  @apply m-0 text-[14px] font-semibold;
}
.roleCard p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.roleStats {
  @apply mt-auto flex gap-4 border-t py-3 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.roleStats strong {
  @apply mr-1 text-[17px];
  color: var(--ui-text);
}
.permissionPanel {
  @apply mt-4 border bg-white p-5;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.permissionPanel header {
  @apply flex items-start gap-3;
  color: var(--app-primary);
}
.permissionPanel h2 {
  @apply m-0 text-[15px];
  color: var(--ui-text);
}
.permissionPanel p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.permissionRows {
  @apply mt-4 border-t;
  border-color: var(--ui-border-subtle);
}
.permissionRow {
  @apply flex min-h-12 items-center gap-4 border-b text-[12px];
  border-color: var(--ui-border-subtle);
}
.permissionRow span {
  @apply flex-1;
  color: var(--ui-text-muted);
}
.permissionRow strong {
  @apply text-[11px] font-normal;
  color: var(--ui-text-subtle);
}
.createForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.dialogActions {
  @apply col-span-2 mt-2 flex justify-end gap-2 border-t pt-4;
  border-color: var(--ui-border-subtle);
}
.detailGrid {
  @apply grid grid-cols-[110px_1fr] gap-x-4 gap-y-3 p-5 text-[12px];
}
.detailGrid strong {
  color: var(--ui-text-subtle);
}
.detailGrid span {
  color: var(--ui-text);
}
.errorState {
  @apply mb-3 flex items-center justify-between gap-3 rounded border bg-white px-4 py-3 text-[12px];
  color: var(--ui-danger);
  border-color: var(--ui-danger);
}
.pagination {
  @apply flex flex-wrap items-center justify-between gap-3 border-t py-3 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.paginationActions {
  @apply flex items-center gap-2;
}
.deleteDialog {
  @apply space-y-4 p-5 text-[12px];
  color: var(--ui-text-muted);
}
.deleteDialog p {
  @apply m-0 leading-5;
}
.deleteDialog strong {
  color: var(--ui-text);
}
.deleteField {
  @apply flex flex-col gap-2 font-medium;
}
.deleteError {
  @apply border px-3 py-2 text-[12px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
  border-color: color-mix(in srgb, var(--ui-danger) 25%, transparent);
  border-radius: var(--app-control-radius);
}
</style>
