<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { KeyRound, Plus, ShieldCheck, UserRound, UsersRound } from 'lucide-vue-next';
import { createRoleApi, deleteRoleApi, getRoleListApi, type RoleApi } from '@/api/core/role';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
} from '@/components/ui';
import { canAccess } from '@/directives/permission';
import { isBuiltInRoleName } from './role-authorization.model';
import PlatformRoleAuthorizationDialog from './PlatformRoleAuthorizationDialog.vue';

const roles = ref<RoleApi.Item[]>([]);
const loading = ref(false);
const loadError = ref('');
const createOpen = ref(false);
const createName = ref('');
const createError = ref('');
const createSaving = ref(false);
const deleteRole = ref<RoleApi.Item | null>(null);
const deleteError = ref('');
const deleteSaving = ref(false);
const authorizationRole = ref<RoleApi.Item | null>(null);
const authorizationOpen = ref(false);
const canManageRoles = computed(() => canAccess('Platform.Role.Manage'));
let latestRequestId = 0;

const builtInCount = computed(() => roles.value.filter((role) => isBuiltInRoleName(role.name)).length);
const customCount = computed(() => roles.value.length - builtInCount.value);
const membershipCount = computed(() => roles.value.reduce((total, role) => total + role.userCount, 0));
const metrics = computed(
  () =>
    [
      { id: 'total', label: '角色总数', value: roles.value.length, icon: KeyRound, tone: 'primary' },
      { id: 'built-in', label: '内置角色', value: builtInCount.value, icon: ShieldCheck, tone: 'warning' },
      { id: 'custom', label: '自定义角色', value: customCount.value, icon: UserRound, tone: 'purple' },
      { id: 'members', label: '角色成员关系', value: membershipCount.value, icon: UsersRound, tone: 'success' },
    ] as const,
);

function roleTone(role: RoleApi.Item, index: number) {
  if (isBuiltInRoleName(role.name)) {
    const tones: Record<string, string> = {
      admin: 'orange',
      member: 'green',
      pm: 'blue',
      pmo: 'purple',
      viewer: 'gray',
    };
    return tones[role.name.trim().toLocaleLowerCase()] ?? 'blue';
  }
  return ['blue', 'purple', 'green', 'orange', 'gray'][index % 5];
}

async function refreshRoles(): Promise<boolean> {
  const requestId = ++latestRequestId;
  loading.value = true;
  loadError.value = '';
  try {
    const result = await getRoleListApi();
    if (requestId !== latestRequestId) return false;
    roles.value = result;
    return true;
  } catch (error) {
    if (requestId !== latestRequestId) return false;
    loadError.value = error instanceof Error ? error.message : '角色列表加载失败，请重试。';
    return false;
  } finally {
    if (requestId === latestRequestId) loading.value = false;
  }
}

function openCreateDialog() {
  if (!canManageRoles.value) return;
  createName.value = '';
  createError.value = '';
  createOpen.value = true;
}

async function submitCreate() {
  if (!canManageRoles.value || createSaving.value) return;
  const name = createName.value.trim();
  if (!name) {
    createError.value = '请输入角色名称。';
    return;
  }
  if (name.length > 64) {
    createError.value = '角色名称不能超过64个字符。';
    return;
  }

  createSaving.value = true;
  createError.value = '';
  try {
    await createRoleApi({ name });
    createOpen.value = false;
    toast.success('角色已创建。');
    if (!(await refreshRoles())) toast.warning('角色创建请求已成功，但列表刷新失败，请重新加载角色列表。');
  } catch (error) {
    createError.value = error instanceof Error ? error.message : '角色创建失败，请检查角色名称后重试。';
  } finally {
    createSaving.value = false;
  }
}

function requestDeleteRole(role: RoleApi.Item) {
  if (!canManageRoles.value || isBuiltInRoleName(role.name)) return;
  deleteRole.value = role;
  deleteError.value = '';
}

async function confirmDeleteRole() {
  const role = deleteRole.value;
  if (!role || !canManageRoles.value || isBuiltInRoleName(role.name) || deleteSaving.value) return;

  deleteSaving.value = true;
  deleteError.value = '';
  try {
    await deleteRoleApi(role.id);
    deleteRole.value = null;
    toast.success('自定义角色已删除，所属用户已解除该角色关系。');
    if (!(await refreshRoles())) toast.warning('删除请求已成功，但角色列表刷新失败，请重新加载角色列表。');
  } catch (error) {
    deleteError.value = error instanceof Error ? error.message : '角色删除失败，请重试。';
  } finally {
    deleteSaving.value = false;
  }
}

function openAuthorization(role: RoleApi.Item) {
  if (!canManageRoles.value) return;
  authorizationRole.value = role;
  authorizationOpen.value = true;
}

function closeAuthorization() {
  authorizationOpen.value = false;
  authorizationRole.value = null;
}

onMounted(() => void refreshRoles());
onBeforeUnmount(() => {
  latestRequestId += 1;
});
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="list"
    data-page-route="/platform/roles"
    data-platform-roles-page
  >
    <header :class="$style.pageHeader">
      <div :class="$style.titleBlock">
        <div :class="$style.breadcrumb"><span>平台中心</span><span>›</span><strong>角色权限</strong></div>
        <div :class="$style.titleRow">
          <ShieldCheck
            :size="22"
            :stroke-width="1.8"
            :class="$style.titleIcon"
          />
          <h1>角色权限</h1>
        </div>
      </div>
      <UiButton
        v-if="canManageRoles"
        variant="primary"
        size="md"
        @click="openCreateDialog"
      >
        <template #leading
          ><Plus
            :size="16"
            :stroke-width="1.8" /></template
        >新建角色
      </UiButton>
    </header>

    <section
      :class="$style.metricGrid"
      aria-label="角色概览"
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

    <UiTabs model-value="roles">
      <div :class="$style.contentHeader">
        <UiTabsList
          label="角色权限视图"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            value="roles"
            :class="$style.tabTrigger"
          >
            <KeyRound
              :size="15"
              :stroke-width="1.8"
            />角色列表
          </UiTabsTrigger>
        </UiTabsList>
      </div>

      <UiTabPanel value="roles">
        <div
          v-if="loadError"
          :class="$style.errorBanner"
          role="alert"
        >
          <span>{{ loadError }}</span>
          <UiButton
            size="xs"
            variant="secondary"
            :disabled="loading"
            @click="refreshRoles"
            >重试</UiButton
          >
        </div>
        <div
          v-if="loading && roles.length === 0"
          :class="$style.emptyState"
          role="status"
          aria-live="polite"
        >
          正在读取角色…
        </div>
        <div
          v-else-if="!loadError && roles.length === 0"
          :class="$style.emptyState"
        >
          暂无角色
        </div>
        <div
          v-else
          :class="$style.roleGrid"
          :aria-busy="loading"
        >
          <article
            v-for="(role, index) in roles"
            :key="role.id"
            :class="$style.roleCard"
            :data-tone="roleTone(role, index)"
            :data-role-name="role.name"
          >
            <div :class="$style.roleHeading">
              <span :class="$style.roleIcon">
                <ShieldCheck
                  :size="18"
                  :stroke-width="1.8"
                />
              </span>
              <div :class="$style.roleCopy">
                <h2>{{ role.name }}</h2>
                <p>{{ role.description || '自定义角色，可按菜单和数据范围分配权限。' }}</p>
              </div>
            </div>
            <div :class="$style.roleStats">
              <span
                ><strong>{{ role.userCount }}</strong> 名成员</span
              >
              <span
                ><strong>{{ role.menuAuthorizationCount }}</strong> 项菜单</span
              >
            </div>
            <div :class="$style.roleActions">
              <UiButton
                size="sm"
                variant="primary"
                :disabled="loading || !canManageRoles"
                @click="openAuthorization(role)"
                >配置权限</UiButton
              >
              <UiButton
                v-if="canManageRoles && !isBuiltInRoleName(role.name)"
                size="sm"
                variant="link"
                :disabled="loading"
                @click="requestDeleteRole(role)"
                >删除</UiButton
              >
              <span
                v-else-if="isBuiltInRoleName(role.name)"
                :class="$style.builtInLabel"
                >内置角色</span
              >
            </div>
          </article>
        </div>
      </UiTabPanel>
    </UiTabs>

    <UiDialog
      v-model:open="createOpen"
      title="新建角色"
      description="角色名称保存后不可修改；菜单和数据授权可在角色卡片中单独配置。"
      size="md"
    >
      <form
        :class="$style.createForm"
        @submit.prevent="submitCreate"
      >
        <UiFormField
          label="角色名称"
          required
        >
          <UiInput
            v-model="createName"
            autofocus
            maxlength="64"
            autocomplete="off"
            placeholder="请输入角色名称"
            :disabled="createSaving"
          />
        </UiFormField>
        <p :class="$style.formHint">角色名最长64个字符。内置角色由系统维护，不能删除。</p>
        <p
          v-if="createError"
          :class="$style.dialogError"
          role="alert"
        >
          {{ createError }}
        </p>
        <div :class="$style.dialogActions">
          <UiButton
            type="button"
            variant="secondary"
            :disabled="createSaving"
            @click="createOpen = false"
            >取消</UiButton
          >
          <UiButton
            type="submit"
            variant="primary"
            :disabled="createSaving"
            >{{ createSaving ? '正在保存…' : '创建角色' }}</UiButton
          >
        </div>
      </form>
    </UiDialog>

    <UiDialog
      :open="Boolean(deleteRole)"
      title="删除自定义角色"
      description="删除后，所属用户将解除此角色关系；其他角色不会受影响。"
      size="md"
      @update:open="!$event && (deleteRole = null)"
    >
      <div
        v-if="deleteRole"
        :class="$style.deleteContent"
      >
        <p>确定删除角色「{{ deleteRole.name }}」吗？</p>
        <p>当前有 {{ deleteRole.userCount }} 名成员使用此角色。</p>
        <p
          v-if="deleteError"
          :class="$style.dialogError"
          role="alert"
        >
          {{ deleteError }}
        </p>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          :disabled="deleteSaving"
          @click="deleteRole = null"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          :disabled="deleteSaving || !deleteRole"
          @click="confirmDeleteRole"
          >{{ deleteSaving ? '正在删除…' : '确认删除' }}</UiButton
        >
      </template>
    </UiDialog>

    <PlatformRoleAuthorizationDialog
      v-if="authorizationRole"
      v-model:open="authorizationOpen"
      :role="authorizationRole"
      @close="closeAuthorization"
      @saved="refreshRoles"
    />
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
.titleBlock {
  @apply min-w-0;
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
.titleIcon {
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
.roleCopy {
  @apply min-w-0;
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
.roleActions {
  @apply flex items-center justify-between gap-2;
}
.builtInLabel {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.errorBanner {
  @apply mt-4 flex items-center justify-between gap-3 border px-4 py-3 text-[12px];
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
  border-color: var(--ui-danger);
  border-radius: var(--app-control-radius);
}
.emptyState {
  @apply flex min-h-40 items-center justify-center text-[12px];
  color: var(--ui-text-subtle);
}
.createForm {
  @apply grid grid-cols-2 gap-4 p-5;
}
.formHint {
  @apply col-span-2 m-0 text-[11px];
  color: var(--ui-text-subtle);
}
.dialogError {
  @apply col-span-2 m-0 text-[12px];
  color: var(--ui-danger);
}
.dialogActions {
  @apply col-span-2 mt-2 flex justify-end gap-2 border-t pt-4;
  border-color: var(--ui-border-subtle);
}
.deleteContent {
  @apply p-5 text-[12px];
  color: var(--ui-text-muted);
}
.deleteContent p {
  @apply mb-3;
}
</style>
