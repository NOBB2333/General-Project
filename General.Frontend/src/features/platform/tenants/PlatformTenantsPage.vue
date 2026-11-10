<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { ArrowLeft, Building2, KeyRound, Plus, RefreshCw, Search, ShieldCheck, UsersRound } from 'lucide-vue-next';
import {
  createTenantApi,
  deleteTenantApi,
  getTenantListApi,
  setTenantStatusApi,
  updateTenantApi,
  type TenantApi,
} from '@/api/core/tenant';
import { enterTenantOperationApi } from '@/api/core/auth';
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
import { replaceSessionFromOperation, sessionState } from '@/session/session-store';
import { useRouter } from 'vue-router';
import PlatformTenantAuthorizationDialog from './PlatformTenantAuthorizationDialog.vue';
import PlatformTenantEditorDialog from './PlatformTenantEditorDialog.vue';
import PlatformTenantTable from './components/PlatformTenantTable.vue';
import { filterTenants, getTenantMetrics, type TenantFormMode, type TenantStatusFilter } from './tenant.model';

const router = useRouter();
const tenants = ref<TenantApi.ListItem[]>([]);
const loading = ref(false);
const loadError = ref('');
const keyword = ref('');
const statusFilter = ref<TenantStatusFilter>('all');
const pageIndex = ref(0);
const pageSize = ref(10);
const editorOpen = ref(false);
const editorMode = ref<TenantFormMode>('create');
const editorTenant = ref<TenantApi.ListItem | null>(null);
const editorSaving = ref(false);
const editorError = ref('');
const authorizationOpen = ref(false);
const authorizationTenant = ref<TenantApi.ListItem | null>(null);
const statusOpen = ref(false);
const statusTenant = ref<TenantApi.ListItem | null>(null);
const nextTenantStatus = ref(false);
const statusSaving = ref(false);
const deleteOpen = ref(false);
const deleteTenant = ref<TenantApi.ListItem | null>(null);
const deleteConfirmation = ref('');
const deleteSaving = ref(false);
const enteringTenantId = ref('');
const canManage = computed(
  () => canAccess('Platform.Tenant.Manage') && !hasCurrentTenantScope() && !sessionState.user?.isHostTenantOperation,
);
const canEnter = computed(
  () => canManage.value && Boolean(sessionState.user?.roles.some((role) => role.toLocaleLowerCase() === 'admin')),
);
const filteredTenants = computed(() => filterTenants(tenants.value, keyword.value, statusFilter.value));
const pageCount = computed(() => Math.max(1, Math.ceil(filteredTenants.value.length / pageSize.value)));
const pageRows = computed(() =>
  filteredTenants.value.slice(pageIndex.value * pageSize.value, (pageIndex.value + 1) * pageSize.value),
);
const metrics = computed(() => {
  const values = getTenantMetrics(tenants.value);
  const icons = { total: Building2, active: ShieldCheck, inactive: UsersRound, connection: KeyRound };
  return values.map((item) => ({ ...item, icon: icons[item.id] }));
});
const pageSizeOptions = [
  { label: '10 条/页', value: '10' },
  { label: '20 条/页', value: '20' },
  { label: '50 条/页', value: '50' },
];
const statusOptions = [
  { label: '全部状态', value: 'all' },
  { label: '启用', value: 'active' },
  { label: '停用', value: 'inactive' },
];
let latestRequestId = 0;

watch([keyword, statusFilter], () => {
  pageIndex.value = 0;
});
watch(pageCount, (count) => {
  if (pageIndex.value >= count) pageIndex.value = count - 1;
});

async function loadTenants(): Promise<boolean> {
  const requestId = ++latestRequestId;
  loading.value = true;
  loadError.value = '';
  try {
    const result = await getTenantListApi();
    if (requestId !== latestRequestId) return false;
    tenants.value = result;
    return true;
  } catch (error) {
    if (requestId !== latestRequestId) return false;
    loadError.value = error instanceof Error ? error.message : '租户列表读取失败，请重试。';
    return false;
  } finally {
    if (requestId === latestRequestId) loading.value = false;
  }
}

function openCreate() {
  if (!canManage.value) return;
  editorMode.value = 'create';
  editorTenant.value = null;
  editorError.value = '';
  editorOpen.value = true;
}

function openEdit(tenant: TenantApi.ListItem) {
  if (!canManage.value) return;
  editorMode.value = 'edit';
  editorTenant.value = tenant;
  editorError.value = '';
  editorOpen.value = true;
}

async function submitTenant(input: TenantApi.SaveInput) {
  if (!canManage.value || editorSaving.value) return;
  editorSaving.value = true;
  editorError.value = '';
  const isCreate = editorMode.value === 'create';
  try {
    if (isCreate) await createTenantApi(input);
    else if (editorTenant.value) await updateTenantApi(editorTenant.value.id, input);
    else return;
    editorOpen.value = false;
    toast.success(isCreate ? '租户已创建。' : '租户信息已保存。');
    if (!(await loadTenants())) toast.warning('保存请求已成功，但租户列表刷新失败，请手动刷新。');
  } catch (error) {
    editorError.value = error instanceof Error ? error.message : '租户保存失败，请检查输入后重试。';
  } finally {
    editorSaving.value = false;
  }
}

function openAuthorization(tenant: TenantApi.ListItem) {
  if (!canManage.value) return;
  authorizationTenant.value = tenant;
  authorizationOpen.value = true;
}

function openStatusConfirmation(tenant: TenantApi.ListItem, isActive: boolean) {
  if (!canManage.value) return;
  statusTenant.value = tenant;
  nextTenantStatus.value = isActive;
  statusOpen.value = true;
}

async function confirmTenantStatus() {
  const target = statusTenant.value;
  if (!target || !canManage.value || statusSaving.value) return;
  statusSaving.value = true;
  try {
    await setTenantStatusApi(target.id, nextTenantStatus.value);
    statusOpen.value = false;
    toast.success(nextTenantStatus.value ? '租户已启用。' : '租户已停用。');
    if (!(await loadTenants())) toast.warning('状态已更新，但列表刷新失败，请手动刷新。');
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '租户状态更新失败，请重试。');
  } finally {
    statusSaving.value = false;
  }
}

function openDeleteConfirmation(tenant: TenantApi.ListItem) {
  if (!canManage.value || tenant.name.trim().toLocaleLowerCase() === 'default') return;
  deleteTenant.value = tenant;
  deleteConfirmation.value = '';
  deleteOpen.value = true;
}

async function confirmDeleteTenant() {
  const target = deleteTenant.value;
  if (
    !target ||
    !canManage.value ||
    deleteSaving.value ||
    target.name.trim().toLocaleLowerCase() === 'default' ||
    deleteConfirmation.value.trim() !== target.name
  )
    return;
  deleteSaving.value = true;
  try {
    await deleteTenantApi(target.id);
    deleteOpen.value = false;
    toast.success('租户已删除。');
    if (!(await loadTenants())) toast.warning('删除请求已成功，但租户列表刷新失败，请手动刷新。');
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '租户删除失败，请重试。');
  } finally {
    deleteSaving.value = false;
  }
}

async function enterTenant(tenant: TenantApi.ListItem) {
  if (!canEnter.value || !tenant.isActive || enteringTenantId.value) return;
  enteringTenantId.value = tenant.id;
  try {
    const result = await enterTenantOperationApi(tenant.id);
    const homePath = await replaceSessionFromOperation(result);
    if (!homePath) return;
    toast.success(`已进入“${tenant.name}”租户。`);
    await router.replace({ path: homePath });
  } catch (error) {
    toast.error(error instanceof Error ? error.message : '进入租户失败，请检查管理员权限和租户状态。');
    if (!sessionState.user) await router.replace({ name: 'login' });
  } finally {
    enteringTenantId.value = '';
  }
}

function changePageSize(value: string | number) {
  const parsed = Number(value);
  if (![10, 20, 50].includes(parsed)) return;
  pageSize.value = parsed;
  pageIndex.value = 0;
}

onMounted(() => void loadTenants());
onBeforeUnmount(() => {
  latestRequestId += 1;
});
</script>

<template>
  <section
    :class="$style.page"
    data-page-family="list"
    data-page-route="/platform/tenants"
    data-platform-tenants-page
  >
    <header :class="$style.pageHeader">
      <div :class="$style.titleBlock">
        <div :class="$style.breadcrumb"><span>平台中心</span><span>›</span><strong>租户管理</strong></div>
        <div :class="$style.titleRow">
          <ArrowLeft
            :size="22"
            :stroke-width="1.8"
            :class="$style.titleIcon"
          />
          <h1>租户管理</h1>
        </div>
      </div>
      <UiButton
        v-if="canManage"
        variant="primary"
        size="md"
        @click="openCreate"
      >
        <template #leading
          ><Plus
            :size="16"
            :stroke-width="1.8"
        /></template>
        新建租户
      </UiButton>
    </header>

    <section
      :class="$style.metricGrid"
      aria-label="租户概览"
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

    <UiTabs model-value="tenants">
      <div :class="$style.contentHeader">
        <UiTabsList
          label="租户管理视图"
          :class="$style.tabs"
        >
          <UiTabsTrigger
            value="tenants"
            :class="$style.tabTrigger"
          >
            <Building2
              :size="15"
              :stroke-width="1.8"
            />租户列表
          </UiTabsTrigger>
        </UiTabsList>
        <div :class="$style.filters">
          <label :class="$style.searchField">
            <Search
              :size="14"
              :stroke-width="1.8"
            />
            <UiInput
              v-model="keyword"
              aria-label="搜索租户"
              placeholder="搜索租户、管理员、邮箱或备注"
            />
          </label>
          <UiSelect
            v-model="statusFilter"
            label="按租户状态筛选"
            :options="statusOptions"
          />
          <UiButton
            size="md"
            :disabled="loading"
            @click="loadTenants"
          >
            <template #leading
              ><RefreshCw
                :size="15"
                :stroke-width="1.8"
            /></template>
            刷新列表
          </UiButton>
        </div>
      </div>

      <UiTabPanel value="tenants">
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
            @click="loadTenants"
            >重试</UiButton
          >
        </div>
        <PlatformTenantTable
          :rows="pageRows"
          :loading="loading"
          :entering="Boolean(enteringTenantId)"
          :can-manage="canManage"
          :can-enter="canEnter"
          @authorize="openAuthorization"
          @delete="openDeleteConfirmation"
          @edit="openEdit"
          @enter="enterTenant"
          @set-active="openStatusConfirmation"
        />
        <footer :class="$style.pagination">
          <span>共 {{ filteredTenants.length }} 个租户</span>
          <div :class="$style.pageControls">
            <UiSelect
              :model-value="String(pageSize)"
              label="每页租户数量"
              :options="pageSizeOptions"
              @update:model-value="changePageSize"
            />
            <button
              type="button"
              :disabled="pageIndex === 0"
              @click="pageIndex -= 1"
            >
              上一页
            </button>
            <span>第 {{ pageIndex + 1 }} / {{ pageCount }} 页</span>
            <button
              type="button"
              :disabled="pageIndex + 1 >= pageCount"
              @click="pageIndex += 1"
            >
              下一页
            </button>
          </div>
        </footer>
      </UiTabPanel>
    </UiTabs>

    <PlatformTenantEditorDialog
      v-model:open="editorOpen"
      :mode="editorMode"
      :tenant="editorTenant"
      :saving="editorSaving"
      :save-error="editorError"
      @submit="submitTenant"
    />
    <PlatformTenantAuthorizationDialog
      v-if="authorizationTenant"
      v-model:open="authorizationOpen"
      :tenant="authorizationTenant"
      @saved="loadTenants"
    />

    <UiDialog
      :open="statusOpen"
      :title="nextTenantStatus ? '启用租户' : '停用租户'"
      :description="statusTenant ? `确认${nextTenantStatus ? '启用' : '停用'}“${statusTenant.name}”吗？` : ''"
      size="sm"
      @update:open="statusOpen = $event"
    >
      <p :class="$style.confirmCopy">
        {{
          nextTenantStatus
            ? '启用后，租户用户可按授权重新访问平台。'
            : '停用后，该租户将不能继续使用平台。该操作会立即影响租户访问。'
        }}
      </p>
      <template #footer>
        <UiButton
          :disabled="statusSaving"
          @click="statusOpen = false"
          >取消</UiButton
        >
        <UiButton
          :variant="nextTenantStatus ? 'primary' : 'danger'"
          :loading="statusSaving"
          @click="confirmTenantStatus"
        >
          {{ nextTenantStatus ? '确认启用' : '确认停用' }}
        </UiButton>
      </template>
    </UiDialog>

    <UiDialog
      :open="deleteOpen"
      title="删除租户"
      :description="deleteTenant ? `删除“${deleteTenant.name}”会删除 ABP 租户记录及租户授权配置。该操作不可撤销。` : ''"
      size="sm"
      @update:open="deleteOpen = $event"
    >
      <div :class="$style.deleteContent">
        <p :class="$style.inlineWarning">
          租户独立数据库中的实际数据不会由此界面归档或回滚。请先确认备份与数据清理方案，再执行删除。
        </p>
        <label :class="$style.deleteLabel">
          输入租户名称 <strong>{{ deleteTenant?.name }}</strong> 以确认
          <UiInput
            v-model="deleteConfirmation"
            autocomplete="off"
            :placeholder="deleteTenant?.name"
          />
        </label>
      </div>
      <template #footer>
        <UiButton
          :disabled="deleteSaving"
          @click="deleteOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="danger"
          :loading="deleteSaving"
          :disabled="deleteConfirmation.trim() !== deleteTenant?.name"
          @click="confirmDeleteTenant"
          >永久删除</UiButton
        >
      </template>
    </UiDialog>
  </section>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply flex min-h-full flex-col gap-5 p-6;
  background: var(--ui-page-background);
}
.pageHeader {
  @apply flex items-center justify-between gap-4;
}
.titleBlock {
  @apply min-w-0;
}
.breadcrumb {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.breadcrumb strong {
  color: var(--ui-text-muted);
}
.titleRow {
  @apply mt-2 flex items-center gap-2;
  color: var(--app-primary);
}
.titleIcon {
  @apply shrink-0;
}
.titleRow h1 {
  @apply text-[20px] font-semibold;
  color: var(--ui-text);
}
.metricGrid {
  @apply grid grid-cols-4 gap-3;
}
.metricCard {
  @apply min-w-0 rounded border bg-white p-4;
  border-color: var(--ui-border-subtle);
}
.metricLabel {
  @apply flex items-center gap-2 text-[11px] font-medium;
  color: var(--ui-text-subtle);
}
.metricCard strong {
  @apply mt-2 block text-[23px] font-semibold;
  color: var(--ui-text);
}
.metricCard[data-tone='primary'] .metricLabel {
  color: var(--app-primary);
}
.metricCard[data-tone='success'] .metricLabel {
  color: var(--ui-success);
}
.metricCard[data-tone='warning'] .metricLabel {
  color: var(--ui-warning);
}
.metricCard[data-tone='neutral'] .metricLabel {
  color: var(--ui-text-muted);
}
.contentHeader {
  @apply flex flex-wrap items-center justify-between gap-3 border-b pb-3;
  border-color: var(--ui-border);
}
.tabs {
  @apply flex gap-1;
}
.tabTrigger {
  @apply inline-flex items-center gap-2 px-3 py-2 text-[12px] font-medium;
  color: var(--ui-text-subtle);
}
.filters {
  @apply flex flex-wrap items-center justify-end gap-2;
}
.searchField {
  @apply flex h-9 w-[280px] items-center gap-2 rounded border bg-white px-3;
  border-color: var(--ui-border);
  color: var(--ui-text-subtle);
}
.searchField input {
  @apply border-0 bg-transparent p-0 shadow-none;
}
.errorBanner {
  @apply mb-3 flex items-center justify-between gap-3 rounded border px-3 py-2 text-[11px];
  color: var(--ui-danger);
  border-color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.pagination {
  @apply flex flex-wrap items-center justify-between gap-3 py-3 text-[11px];
  color: var(--ui-text-subtle);
}
.pageControls {
  @apply flex items-center gap-2;
}
.pageControls button {
  @apply rounded px-2 py-1 hover:bg-white disabled:cursor-not-allowed disabled:opacity-40;
  color: var(--ui-text-muted);
}
.confirmCopy {
  @apply text-[12px] leading-6;
  color: var(--ui-text-muted);
}
.inlineWarning {
  @apply rounded border px-3 py-2 text-[11px] leading-5;
  color: var(--ui-warning);
  border-color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.deleteContent {
  @apply space-y-4;
}
.deleteLabel {
  @apply block text-[11px] leading-5;
  color: var(--ui-text-subtle);
}
.deleteLabel strong {
  color: var(--ui-text);
}
.deleteLabel input {
  @apply mt-2;
}
@media (max-width: 960px) {
  .filters {
    @apply w-full justify-start;
  }
  .searchField {
    @apply flex-1;
  }
}
@media (max-width: 760px) {
  .page {
    @apply p-4;
  }
  .metricGrid {
    @apply grid-cols-2;
  }
  .pageHeader {
    @apply items-start;
  }
}
@media (max-width: 520px) {
  .filters {
    @apply grid grid-cols-1;
  }
  .searchField {
    @apply w-full;
  }
}
</style>
