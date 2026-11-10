<script setup lang="ts">
import { computed, ref, toRef } from 'vue';
import { AlertTriangle, KeyRound, RefreshCw, Search, ShieldCheck, UsersRound } from 'lucide-vue-next';
import type { RoleApi } from '@/api/core/role';
import {
  UiButton,
  UiCheckbox,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
  type UiSelectOption,
} from '@/components/ui';
import { accountScopeOptions, dataScopeOptions, isBuiltInRoleName } from './role-authorization.model';
import PlatformRoleMenuTree from './PlatformRoleMenuTree.vue';
import PlatformRoleOrganizationTree from './PlatformRoleOrganizationTree.vue';
import { usePlatformRoleAuthorization } from './usePlatformRoleAuthorization';

const props = defineProps<{
  open: boolean;
  role: RoleApi.Item;
}>();

const emit = defineEmits<{
  'update:open': [open: boolean];
  close: [];
  saved: [];
}>();

const roleId = computed(() => props.role.id);
const roleName = computed(() => props.role.name);
const activeAuthorizationTab = ref('menus');
const open = toRef(props, 'open');
const {
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
  saveAuthorization,
  saveMenus,
} = usePlatformRoleAuthorization(roleId, roleName, open);

const pageSizeOptions: readonly UiSelectOption[] = [
  { value: '10', label: '每页10条' },
  { value: '20', label: '每页20条' },
  { value: '50', label: '每页50条' },
  { value: '100', label: '每页100条' },
];
const capabilityOptions = computed(() => {
  const labels = new Map<string, string>();
  for (const group of endpointGroups.value) {
    for (const endpoint of group.items) {
      if (endpoint.capabilityKey && !labels.has(endpoint.capabilityKey)) {
        labels.set(endpoint.capabilityKey, endpoint.capabilityKey);
      }
    }
  }
  const keyword = endpointSearch.value.trim().toLocaleLowerCase();
  return [...labels.keys()].filter((key) => !keyword || key.toLocaleLowerCase().includes(keyword));
});
const visibleEndpointCount = computed(() =>
  filteredEndpointGroups.value.reduce((total, group) => total + group.items.length, 0),
);
const selectedMenuCount = computed(() => selectedMenuIds.value.size);
const selectedAccountCount = computed(() => accountUserIds.value.size);
const selectedOrganizationCount = computed(() => customOrganizationUnitIds.value.size);
const visibleOrganizationIds = computed(() => {
  const ids = new Set<string>();
  const visit = (units: typeof organizations.value) => {
    for (const unit of units) {
      ids.add(unit.id);
      visit(unit.children);
    }
  };
  visit(organizations.value);
  return ids;
});
const preservedOrganizationCount = computed(
  () => [...customOrganizationUnitIds.value].filter((id) => !visibleOrganizationIds.value.has(id)).length,
);
const dataScopeHelp = computed(() => {
  if (dataScopeMode.value === 'all') return '按后端策略访问当前租户内全部组织数据。';
  if (dataScopeMode.value === 'current_org') return '访问当前用户所属组织，不包含下级组织。';
  if (dataScopeMode.value === 'self') return '按当前用户所属组织节点计算本人范围。';
  if (dataScopeMode.value === 'custom') return '仅访问下方勾选的组织节点；自定义范围不会自动包含下级组织。';
  return '访问当前用户所属组织及其下级组织。';
});
const accountScopeHelp = computed(() => {
  if (accountScopeMode.value === 'all') return '访问当前租户内全部用户。';
  if (accountScopeMode.value === 'data') return '只访问角色数据范围内的用户。';
  if (accountScopeMode.value === 'only_users') return '只访问指定用户，并包含当前用户本人。';
  return '访问角色数据范围内用户、指定用户和当前用户本人。';
});
const dialogDescription = computed(() => `为角色「${props.role.name}」配置菜单授权与数据范围；两类设置分别保存。`);

async function submitMenuAuthorization() {
  if (await saveMenus()) emit('saved');
}

async function submitScopeAuthorization() {
  if (await saveAuthorization()) emit('saved');
}

function closeDialog() {
  emit('update:open', false);
  emit('close');
}

function updateBlacklist(key: string, selected: boolean) {
  toggleEndpointKey(key, selected);
}

function updateOrganization(organizationId: string, selected: boolean) {
  toggleOrganization(organizationId, selected);
}

function updateAccountUser(userId: string, selected: boolean) {
  toggleAccountUser(userId, selected);
}

function updateMenu(menuId: string, selected: boolean) {
  toggleMenu(menuId, selected);
}

function endpointCheckboxLabel(label: string, kind: string) {
  return `封锁${kind}：${label}`;
}

function displayUser(user: { displayName: string; username: string; email: string; organizationUnitNames: string[] }) {
  return `${user.displayName || user.username} · ${user.username}${user.organizationUnitNames.length ? ` · ${user.organizationUnitNames.join('、')}` : ''}${user.email ? ` · ${user.email}` : ''}`;
}
</script>

<template>
  <UiDialog
    :open="open"
    :title="`角色授权 · ${role.name}`"
    :description="dialogDescription"
    size="xl"
    @update:open="emit('update:open', $event)"
    @close="emit('close')"
  >
    <UiTabs v-model="activeAuthorizationTab">
      <div :class="$style.contentHeader">
        <UiTabsList
          label="角色授权设置"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            value="menus"
            :class="$style.tabTrigger"
          >
            <KeyRound
              :size="15"
              :stroke-width="1.8"
            />菜单授权
          </UiTabsTrigger>
          <UiTabsTrigger
            value="scope"
            :class="$style.tabTrigger"
          >
            <UsersRound
              :size="15"
              :stroke-width="1.8"
            />范围与接口
          </UiTabsTrigger>
        </UiTabsList>
        <span :class="$style.authorizationRole">
          {{ isBuiltInRoleName(role.name) ? '内置角色' : '自定义角色' }}
        </span>
      </div>

      <UiTabPanel value="menus">
        <section :class="$style.permissionPanel">
          <header>
            <KeyRound
              :size="18"
              :stroke-width="1.8"
            />
            <div>
              <h2>菜单与按钮权限</h2>
              <p>选择目录会联动勾选可用子项；保存后由后端补齐父级目录。按钮权限以真实菜单权限码为准。</p>
            </div>
          </header>
          <div :class="$style.panelMeta">
            <span>已选择 {{ selectedMenuCount }} 项</span>
            <UiButton
              size="xs"
              variant="secondary"
              :disabled="menuLoading"
              @click="retryLoading"
            >
              <template #leading
                ><RefreshCw
                  :size="13"
                  :stroke-width="1.8" /></template
              >重新读取
            </UiButton>
          </div>
          <p
            v-if="menuLoadError"
            :class="$style.errorMessage"
            role="alert"
          >
            {{ menuLoadError }}
          </p>
          <p
            v-else-if="menuLoading"
            :class="$style.loadingMessage"
            role="status"
          >
            正在读取菜单授权…
          </p>
          <p
            v-else-if="menuTree.length === 0"
            :class="$style.loadingMessage"
          >
            后端没有返回可配置菜单。
          </p>
          <PlatformRoleMenuTree
            v-else
            :items="menuTree"
            :selected-ids="selectedMenuIds"
            @toggle="updateMenu"
          />
          <p
            v-if="menuSaveError"
            :class="$style.errorMessage"
            role="alert"
          >
            {{ menuSaveError }}
          </p>
          <div :class="$style.panelActions">
            <UiButton
              variant="secondary"
              :disabled="menuSaving"
              @click="closeDialog"
              >关闭</UiButton
            >
            <UiButton
              variant="primary"
              :disabled="!canSaveMenus"
              @click="submitMenuAuthorization"
              >{{ menuSaving ? '正在保存并核对…' : '保存菜单授权' }}</UiButton
            >
          </div>
        </section>
      </UiTabPanel>

      <UiTabPanel value="scope">
        <p
          v-if="authorizationLoadError"
          :class="$style.errorMessage"
          role="alert"
        >
          {{ authorizationLoadError }}
          <UiButton
            size="xs"
            variant="link"
            @click="retryLoading"
            >重新读取</UiButton
          >
        </p>
        <p
          v-else-if="authorizationLoading"
          :class="$style.loadingMessage"
          role="status"
        >
          正在读取角色范围…
        </p>

        <div v-else>
          <p
            v-if="isAdminRole"
            :class="$style.adminNotice"
            role="note"
          >
            <AlertTriangle
              :size="16"
              :stroke-width="1.8"
            />
            系统管理员角色在后端绕过数据范围和 API 黑名单检查；当前保存的范围值仅作记录，不能限制管理员账号。
          </p>

          <section :class="$style.permissionPanel">
            <header>
              <ShieldCheck
                :size="18"
                :stroke-width="1.8"
              />
              <div>
                <h2>数据范围</h2>
                <p>实际数据访问由后端按当前账号组织关系和角色范围计算。</p>
              </div>
            </header>
            <div :class="$style.scopeForm">
              <UiFormField label="数据范围">
                <UiSelect
                  :model-value="dataScopeMode"
                  label="数据范围"
                  :options="dataScopeOptions"
                  :disabled="isAdminRole || authorizationSaving"
                  @update:model-value="setDataScopeMode"
                />
              </UiFormField>
              <p :class="$style.helpText">{{ dataScopeHelp }}</p>
              <div
                v-if="customOrganizationsRequired"
                :class="$style.scopeSelection"
              >
                <div :class="$style.selectionHeader">
                  <strong>自定义组织</strong>
                  <span>已选 {{ selectedOrganizationCount }} 个节点</span>
                </div>
                <p :class="$style.helpText">
                  自定义组织只包含勾选的节点。不可见组织 ID 会保留在已保存值中，不会被本次保存清除。
                </p>
                <p
                  v-if="preservedOrganizationCount > 0"
                  :class="$style.helpText"
                >
                  有 {{ preservedOrganizationCount }} 个已保存组织不在当前可见列表中，仍会原样保留。
                </p>
                <p
                  v-if="organizationsError"
                  :class="$style.errorMessage"
                  role="alert"
                >
                  {{ organizationsError }}
                  <UiButton
                    size="xs"
                    variant="link"
                    @click="loadOrganizations"
                    >重试</UiButton
                  >
                </p>
                <p
                  v-else-if="organizationsLoading"
                  :class="$style.loadingMessage"
                  role="status"
                >
                  正在读取组织…
                </p>
                <PlatformRoleOrganizationTree
                  v-else-if="organizations.length"
                  :items="organizations"
                  :selected-ids="customOrganizationUnitIds"
                  @toggle="updateOrganization"
                />
                <p
                  v-else
                  :class="$style.loadingMessage"
                >
                  暂无可配置组织
                </p>
              </div>
            </div>
          </section>

          <section :class="$style.permissionPanel">
            <header>
              <UsersRound
                :size="18"
                :stroke-width="1.8"
              />
              <div>
                <h2>账号范围</h2>
                <p>多角色账号的最终可见范围由服务端合并各角色策略。</p>
              </div>
            </header>
            <div :class="$style.scopeForm">
              <UiFormField label="账号范围">
                <UiSelect
                  :model-value="accountScopeMode"
                  label="账号范围"
                  :options="accountScopeOptions"
                  :disabled="isAdminRole || authorizationSaving"
                  @update:model-value="setAccountScopeMode"
                />
              </UiFormField>
              <p :class="$style.helpText">{{ accountScopeHelp }}</p>
              <div
                v-if="accountUsersRequired"
                :class="$style.scopeSelection"
              >
                <div :class="$style.selectionHeader">
                  <strong>指定用户</strong>
                  <span>已选 {{ selectedAccountCount }} 人</span>
                </div>
                <div :class="$style.userFilter">
                  <label :class="$style.searchField">
                    <Search
                      :size="15"
                      :stroke-width="1.8"
                    />
                    <UiInput
                      :model-value="userKeyword"
                      aria-label="搜索账号范围用户"
                      placeholder="搜索姓名、账号、组织或邮箱"
                      :disabled="usersLoading || isAdminRole"
                      @update:model-value="userKeyword = $event"
                      @keydown.enter="searchUsers"
                    />
                  </label>
                  <UiButton
                    size="sm"
                    variant="secondary"
                    :disabled="usersLoading || isAdminRole"
                    @click="searchUsers"
                    >查询</UiButton
                  >
                  <UiSelect
                    :model-value="String(userPageSize)"
                    label="账号范围用户每页数量"
                    :options="pageSizeOptions"
                    :disabled="usersLoading || isAdminRole"
                    size="sm"
                    @update:model-value="changeUserPageSize"
                  />
                </div>
                <p
                  v-if="usersError"
                  :class="$style.errorMessage"
                  role="alert"
                >
                  {{ usersError }}
                  <UiButton
                    size="xs"
                    variant="link"
                    @click="loadUsers(0)"
                    >重试</UiButton
                  >
                </p>
                <p
                  v-else-if="usersLoading"
                  :class="$style.loadingMessage"
                  role="status"
                >
                  正在读取用户…
                </p>
                <div
                  v-else-if="users.length"
                  :class="$style.permissionRows"
                  :aria-busy="usersLoading"
                >
                  <label
                    v-for="user in users"
                    :key="user.id"
                    :class="$style.permissionRow"
                  >
                    <UiCheckbox
                      :model-value="accountUserIds.has(user.id)"
                      :disabled="isAdminRole || authorizationSaving"
                      :label="`指定用户：${displayUser(user)}`"
                      @update:model-value="updateAccountUser(user.id, $event === true)"
                    />
                    <span :class="$style.userCopy">
                      <strong>{{ user.displayName || user.username }}</strong>
                      <small>{{ user.username }} · {{ user.organizationUnitNames.join('、') || '未关联组织' }}</small>
                    </span>
                    <span :class="[$style.userStatus, user.isActive ? $style.userEnabled : $style.userDisabled]">
                      {{ user.isActive ? '启用' : '禁用' }}
                    </span>
                  </label>
                </div>
                <p
                  v-else
                  :class="$style.loadingMessage"
                >
                  当前筛选没有可选用户；已保存的其他用户 ID 会保留。
                </p>
                <div :class="$style.pagination">
                  <span>共 {{ userTotalCount }} 人；跨页搜索不会清除已选用户。</span>
                  <div>
                    <UiButton
                      size="xs"
                      variant="secondary"
                      :disabled="usersLoading || userPageIndex === 0"
                      @click="changeUserPage(userPageIndex - 1)"
                      >上一页</UiButton
                    >
                    <span>第 {{ userPageIndex + 1 }} / {{ pageCount }} 页</span>
                    <UiButton
                      size="xs"
                      variant="secondary"
                      :disabled="usersLoading || userPageIndex + 1 >= pageCount"
                      @click="changeUserPage(userPageIndex + 1)"
                      >下一页</UiButton
                    >
                  </div>
                </div>
              </div>
            </div>
          </section>

          <section :class="$style.permissionPanel">
            <header>
              <KeyRound
                :size="18"
                :stroke-width="1.8"
              />
              <div>
                <h2>API 黑名单</h2>
                <p>按后端返回的路由键或能力键精确匹配；能力键会同时封锁使用该能力的接口。</p>
              </div>
            </header>
            <div :class="$style.endpointToolbar">
              <label :class="$style.searchField">
                <Search
                  :size="15"
                  :stroke-width="1.8"
                />
                <UiInput
                  :model-value="endpointSearch"
                  aria-label="搜索 API 黑名单项"
                  placeholder="搜索路由、控制器或能力键"
                  :disabled="endpointOptionsLoading || isAdminRole"
                  @update:model-value="endpointSearch = $event"
                />
              </label>
              <span>{{ visibleEndpointCount }} 个路由项</span>
            </div>
            <p
              v-if="endpointOptionsError"
              :class="$style.errorMessage"
              role="alert"
            >
              {{ endpointOptionsError }}
              <UiButton
                size="xs"
                variant="link"
                @click="retryLoading"
                >重新读取</UiButton
              >
            </p>
            <p
              v-else-if="endpointOptionsLoading"
              :class="$style.loadingMessage"
              role="status"
            >
              正在读取 API 目录…
            </p>
            <template v-else>
              <div
                v-if="capabilityOptions.length"
                :class="$style.endpointGroup"
              >
                <h3>能力键</h3>
                <label
                  v-for="key in capabilityOptions"
                  :key="key"
                  :class="$style.permissionRow"
                >
                  <UiCheckbox
                    :model-value="apiBlacklist.has(key)"
                    :disabled="isAdminRole || authorizationSaving"
                    :label="endpointCheckboxLabel(key, '能力键')"
                    @update:model-value="updateBlacklist(key, $event === true)"
                  />
                  <span :class="$style.endpointCopy">
                    <strong>{{ key }}</strong>
                    <small>封锁所有标注此能力键的后端接口</small>
                  </span>
                </label>
              </div>
              <details
                v-for="group in filteredEndpointGroups"
                :key="group.groupName"
                :class="$style.endpointGroup"
                open
              >
                <summary>{{ group.groupName }}（{{ group.items.length }}）</summary>
                <label
                  v-for="endpoint in group.items"
                  :key="endpoint.key"
                  :class="$style.permissionRow"
                >
                  <UiCheckbox
                    :model-value="apiBlacklist.has(endpoint.key)"
                    :disabled="isAdminRole || authorizationSaving"
                    :label="endpointCheckboxLabel(endpoint.label, '路由')"
                    @update:model-value="updateBlacklist(endpoint.key, $event === true)"
                  />
                  <span :class="$style.endpointCopy">
                    <strong>{{ endpoint.label }}</strong>
                    <small>{{ endpoint.key }}</small>
                  </span>
                </label>
              </details>
              <div
                v-if="unknownBlacklistKeys.length"
                :class="$style.endpointGroup"
              >
                <h3>目录外已保存键</h3>
                <p :class="$style.helpText">这些键不在当前后端目录中，保持勾选可原样保留；取消勾选会在保存时移除。</p>
                <label
                  v-for="key in unknownBlacklistKeys"
                  :key="key"
                  :class="$style.permissionRow"
                >
                  <UiCheckbox
                    :model-value="apiBlacklist.has(key)"
                    :disabled="isAdminRole || authorizationSaving"
                    :label="endpointCheckboxLabel(key, '未识别键')"
                    @update:model-value="updateBlacklist(key, $event === true)"
                  />
                  <span :class="$style.endpointCopy"
                    ><strong>{{ key }}</strong></span
                  >
                </label>
              </div>
              <p
                v-if="endpointGroups.length === 0 && capabilityOptions.length === 0"
                :class="$style.loadingMessage"
              >
                后端没有返回 API 黑名单选项。
              </p>
            </template>
          </section>

          <p
            v-if="authorizationSaveError"
            :class="$style.errorMessage"
            role="alert"
          >
            {{ authorizationSaveError }}
          </p>
          <div :class="$style.panelActions">
            <UiButton
              variant="secondary"
              :disabled="authorizationSaving"
              @click="closeDialog"
              >关闭</UiButton
            >
            <UiButton
              variant="primary"
              :disabled="!canSaveAuthorization"
              @click="submitScopeAuthorization"
              >{{ authorizationSaving ? '正在保存并核对…' : '保存范围与 API 黑名单' }}</UiButton
            >
          </div>
        </div>
      </UiTabPanel>
    </UiTabs>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";
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
.authorizationRole {
  @apply px-3 text-[11px];
  color: var(--ui-text-subtle);
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
.permissionPanel header h2 {
  @apply m-0 text-[15px];
  color: var(--ui-text);
}
.permissionPanel header p {
  @apply mt-1 text-[11px] leading-5;
  color: var(--ui-text-subtle);
}
.panelMeta {
  @apply flex items-center justify-between border-t py-3 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.scopeForm {
  @apply mt-4 grid grid-cols-2 gap-3;
}
.helpText {
  @apply col-span-2 m-0 text-[11px] leading-5;
  color: var(--ui-text-subtle);
}
.scopeSelection {
  @apply col-span-2 min-w-0;
}
.selectionHeader {
  @apply mb-2 flex items-center justify-between text-[12px];
  color: var(--ui-text-muted);
}
.selectionHeader span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.userFilter,
.endpointToolbar {
  @apply mb-3 flex items-center gap-2;
}
.searchField {
  @apply flex h-9 min-w-0 flex-1 items-center gap-2 border bg-white px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.searchField input {
  @apply min-w-0 flex-1 border-0 bg-transparent text-[12px] outline-none;
  color: var(--ui-text);
}
.searchField input::placeholder {
  color: #97a0af;
}
.endpointToolbar > span {
  @apply shrink-0 text-[11px];
  color: var(--ui-text-subtle);
}
.permissionRows {
  @apply border-t;
  border-color: var(--ui-border-subtle);
}
.permissionRow {
  @apply flex min-h-12 items-center gap-3 border-b px-3 text-[12px];
  border-color: var(--ui-border-subtle);
}
.userCopy,
.endpointCopy {
  @apply min-w-0 flex-1;
}
.userCopy strong,
.endpointCopy strong {
  @apply block break-all font-medium;
  color: var(--ui-text-muted);
}
.userCopy small,
.endpointCopy small {
  @apply mt-1 block break-all text-[10px];
  color: var(--ui-text-subtle);
}
.userStatus {
  @apply shrink-0 rounded px-1.5 py-0.5 text-[10px];
}
.userEnabled {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.userDisabled {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}
.pagination {
  @apply flex flex-wrap items-center justify-between gap-2 border-t py-3 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.pagination > div {
  @apply flex items-center gap-2;
}
.endpointGroup {
  @apply mt-3 border;
  border-color: var(--ui-border-subtle);
  border-radius: var(--app-control-radius);
}
.endpointGroup summary,
.endpointGroup h3 {
  @apply m-0 border-b px-3 py-2 text-[12px] font-medium;
  color: var(--ui-text-muted);
  background: #f4f5f7;
  border-color: var(--ui-border-subtle);
}
.endpointGroup h3 {
  @apply border-b;
}
.endpointGroup .permissionRow:last-child {
  border-bottom: 0;
}
.adminNotice {
  @apply mt-4 flex items-start gap-2 border px-4 py-3 text-[12px] leading-5;
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
  border-color: var(--ui-warning);
  border-radius: var(--app-control-radius);
}
.errorMessage {
  @apply mt-3 text-[12px] leading-5;
  color: var(--ui-danger);
}
.loadingMessage {
  @apply my-3 flex min-h-16 items-center justify-center text-[12px];
  color: var(--ui-text-subtle);
}
.panelActions {
  @apply mt-4 flex justify-end gap-2 border-t pt-4;
  border-color: var(--ui-border-subtle);
}
</style>
