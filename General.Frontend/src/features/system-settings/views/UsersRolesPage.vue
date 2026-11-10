<script setup lang="ts">
import { ArrowLeft, Database, KeyRound, Plus, ShieldCheck, UserRound, UsersRound } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import {
  toast,
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
} from '@/components/ui';
import { usersRolesReferenceFixture } from '../model/users-roles.data';
import type { UserAccountDraft, UserAccountRow, UserRoleSummary, UsersRolesTab } from '../model/users-roles.types';
import AccountTable from './users-roles/AccountTable.vue';

const accounts = ref<UserAccountRow[]>(cloneReferenceRows(usersRolesReferenceFixture.rows));
const roles = ref<UserRoleSummary[]>(cloneReferenceRows(usersRolesReferenceFixture.roles));
const activeTab = ref<UsersRolesTab>('accounts');
const search = ref('');
const roleFilter = ref('');
const statusFilter = ref('');
const createOpen = ref(false);
const detailOpen = ref(false);
const selectedAccount = ref<UserAccountRow | null>(null);
const draft = ref<UserAccountDraft>(emptyDraft());

const roleOptions = computed(() => [
  { label: '全部角色', value: '' },
  ...roles.value.map((role) => ({ label: role.name, value: role.name })),
]);
const statusOptions = [
  { label: '全部状态', value: '' },
  { label: '启用', value: '启用' },
  { label: '禁用', value: '禁用' },
  { label: '待激活', value: '待激活' },
];
const filteredAccounts = computed(() => {
  const query = search.value.trim().toLocaleLowerCase();
  return accounts.value.filter((account) => {
    const matchesQuery =
      !query ||
      [account.name, account.account, account.email].some((value) => value.toLocaleLowerCase().includes(query));
    return (
      matchesQuery &&
      (!roleFilter.value || account.role === roleFilter.value) &&
      (!statusFilter.value || account.status === statusFilter.value)
    );
  });
});
const enabledCount = computed(() => accounts.value.filter((account) => account.status === '启用').length);
const pendingCount = computed(() => accounts.value.filter((account) => account.status === '待激活').length);
const metrics = computed(
  () =>
    [
      { id: 'total', label: '总账号', value: accounts.value.length, icon: UsersRound, tone: 'primary' },
      { id: 'enabled', label: '已启用', value: enabledCount.value, icon: UserRound, tone: 'success' },
      { id: 'pending', label: '待激活', value: pendingCount.value, icon: ShieldCheck, tone: 'warning' },
      { id: 'roles', label: '角色数', value: roles.value.length, icon: KeyRound, tone: 'purple' },
    ] as const,
);

function emptyDraft(): UserAccountDraft {
  return { name: '', jobTitle: '', account: '', email: '', department: '', role: '普通员工' };
}

function openCreate() {
  draft.value = emptyDraft();
  createOpen.value = true;
}

function saveAccount() {
  const name = draft.value.name.trim();
  const account = draft.value.account.trim();
  if (!name || !account) {
    toast.warning('请填写姓名和账号');
    return;
  }
  accounts.value.unshift({
    id: `account-local-${Date.now()}`,
    name,
    jobTitle: draft.value.jobTitle.trim() || '新建账号',
    account,
    email: draft.value.email.trim() || `${account}@company.com`,
    department: draft.value.department.trim() || '待分配',
    role: draft.value.role,
    status: '待激活',
    lastLogin: '—',
  });
  createOpen.value = false;
  toast.success('账号已保存到本地');
}

function openDetails(account: UserAccountRow) {
  selectedAccount.value = account;
  detailOpen.value = true;
}

function toggleStatus(account: UserAccountRow) {
  account.status = account.status === '禁用' ? '启用' : '禁用';
  toast.info(`${account.name}已${account.status === '启用' ? '启用' : '停用'}`);
}

function resetPassword(account: UserAccountRow) {
  toast.success(`已为${account.name}生成本地重置密码链接`);
}
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="list"
    data-page-route="/system/users-roles"
    data-system-users-roles-page
    :data-active-tab="activeTab"
  >
    <header :class="$style.pageHeader">
      <div :class="$style.titleBlock">
        <div :class="$style.breadcrumb"><span>系统设置</span><span>›</span><strong>账号管理</strong></div>
        <div :class="$style.titleRow">
          <ArrowLeft
            :size="22"
            :stroke-width="1.8"
            :class="$style.titleArrow"
          />
          <h1>账号管理</h1>
        </div>
      </div>
    </header>

    <section
      :class="$style.metricGrid"
      aria-label="账号概览"
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
        <strong>{{ metric.value }}</strong>
      </article>
    </section>

    <UiTabs v-model="activeTab">
      <div :class="$style.contentHeader">
        <UiTabsList
          label="账号管理视图"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            value="accounts"
            :class="$style.tabTrigger"
          >
            <UserRound
              :size="15"
              :stroke-width="1.8"
            />账号列表
          </UiTabsTrigger>
          <UiTabsTrigger
            value="roles"
            :class="$style.tabTrigger"
          >
            <ShieldCheck
              :size="15"
              :stroke-width="1.8"
            />角色权限
          </UiTabsTrigger>
          <UiTabsTrigger
            value="permissions"
            :class="$style.tabTrigger"
          >
            <Database
              :size="15"
              :stroke-width="1.8"
            />数据权限<span :class="$style.tabCount">5</span>
          </UiTabsTrigger>
        </UiTabsList>
        <UiButton
          variant="primary"
          size="md"
          @click="openCreate"
        >
          <template #leading
            ><Plus
              :size="16"
              :stroke-width="1.8" /></template
          >新建账号
        </UiButton>
      </div>

      <UiTabPanel value="accounts">
        <div :class="$style.filterBar">
          <label :class="$style.searchField">
            <UserRound
              :size="15"
              :stroke-width="1.8"
            />
            <input
              v-model="search"
              type="search"
              aria-label="搜索账号"
              placeholder="搜索姓名、账号、邮箱..."
            />
          </label>
          <UiSelect
            v-model="roleFilter"
            label="角色筛选"
            :options="roleOptions"
            size="md"
          />
          <UiSelect
            v-model="statusFilter"
            label="状态筛选"
            :options="statusOptions"
            size="md"
          />
        </div>
        <AccountTable
          :rows="filteredAccounts"
          @view="openDetails"
          @lock="toggleStatus"
          @reset="resetPassword"
        />
      </UiTabPanel>

      <UiTabPanel value="roles">
        <div :class="$style.roleGrid">
          <article
            v-for="role in roles"
            :key="role.id"
            :class="$style.roleCard"
            :data-tone="role.tone"
          >
            <div :class="$style.roleHeading">
              <span :class="$style.roleIcon"
                ><ShieldCheck
                  :size="18"
                  :stroke-width="1.8"
              /></span>
              <div>
                <h2>{{ role.name }}</h2>
                <p>{{ role.description }}</p>
              </div>
            </div>
            <div :class="$style.roleStats">
              <span
                ><strong>{{ role.memberCount }}</strong> 名成员</span
              ><span
                ><strong>{{ role.permissionCount }}</strong> 项权限</span
              >
            </div>
            <UiButton
              size="sm"
              @click="toast.info(`已打开${role.name}权限配置`)"
              >查看权限</UiButton
            >
          </article>
        </div>
      </UiTabPanel>

      <UiTabPanel value="permissions">
        <section :class="$style.permissionPanel">
          <header>
            <Database
              :size="18"
              :stroke-width="1.8"
            />
            <div>
              <h2>数据权限</h2>
              <p>按角色控制可见部门、客户和项目范围。</p>
            </div>
          </header>
          <div :class="$style.permissionRows">
            <div
              v-for="item in [
                '销售部 · 客户与商机',
                '工程部 · 项目与工单',
                '财务部 · 收支与凭证',
                '仓储物流部 · 库存与出库',
                '全公司 · 通知与通讯录',
              ]"
              :key="item"
              :class="$style.permissionRow"
            >
              <span>{{ item }}</span
              ><strong>5 个角色可访问</strong
              ><UiButton
                size="xs"
                @click="toast.info(`已打开${item}的数据范围`)"
                >配置</UiButton
              >
            </div>
          </div>
        </section>
      </UiTabPanel>
    </UiTabs>

    <UiDialog
      v-model:open="createOpen"
      title="新建账号"
      description="账号仅保存到本地复刻页面，初始状态为待激活。"
      size="md"
    >
      <form
        :class="$style.createForm"
        @submit.prevent="saveAccount"
      >
        <UiFormField
          label="姓名"
          required
          ><UiInput
            v-model="draft.name"
            autofocus
            placeholder="请输入姓名"
        /></UiFormField>
        <UiFormField label="职位"
          ><UiInput
            v-model="draft.jobTitle"
            placeholder="例如 项目经理"
        /></UiFormField>
        <UiFormField
          label="账号"
          required
          ><UiInput
            v-model="draft.account"
            placeholder="请输入登录账号"
        /></UiFormField>
        <UiFormField label="邮箱"
          ><UiInput
            v-model="draft.email"
            type="email"
            placeholder="name@company.com"
        /></UiFormField>
        <UiFormField label="部门"
          ><UiInput
            v-model="draft.department"
            placeholder="请输入部门"
        /></UiFormField>
        <UiFormField label="角色"
          ><UiSelect
            v-model="draft.role"
            label="角色"
            :options="roleOptions.slice(1)"
        /></UiFormField>
        <div :class="$style.dialogActions">
          <UiButton @click="createOpen = false">取消</UiButton
          ><UiButton
            type="submit"
            variant="primary"
            >保存</UiButton
          >
        </div>
      </form>
    </UiDialog>

    <UiDialog
      v-model:open="detailOpen"
      title="账号详情"
      description="本地账号信息"
    >
      <div
        v-if="selectedAccount"
        :class="$style.detailGrid"
      >
        <strong>姓名</strong><span>{{ selectedAccount.name }} · {{ selectedAccount.jobTitle }}</span>
        <strong>账号</strong><span>{{ selectedAccount.account }} / {{ selectedAccount.email }}</span>
        <strong>部门</strong><span>{{ selectedAccount.department }}</span> <strong>角色</strong
        ><span>{{ selectedAccount.role }}</span> <strong>状态</strong><span>{{ selectedAccount.status }}</span>
        <strong>最近登录</strong><span>{{ selectedAccount.lastLogin }}</span>
      </div>
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
  @apply mb-3 flex items-center gap-2 pt-4;
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
</style>
