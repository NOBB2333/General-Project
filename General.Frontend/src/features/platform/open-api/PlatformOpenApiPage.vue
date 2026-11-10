<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref } from 'vue';
import { KeyRound } from 'lucide-vue-next';
import { createPlatformOpenApiApi, type PlatformOpenApiApi } from '@/api/core/platform-open-api';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { toast, UiButton, UiDialog } from '@/components/ui';
import { canAccess } from '@/directives/permission';
import OpenApiApplicationDialog from './OpenApiApplicationDialog.vue';
import OpenApiSecretDialog from './OpenApiSecretDialog.vue';
import {
  createOpenApiApplicationDraft,
  toOpenApiApplicationDraft,
  toOpenApiApplicationRow,
  toOpenApiApplicationSaveInput,
  validateOpenApiApplicationDraft,
  type OpenApiApplicationDraft,
  type OpenApiApplicationRow,
} from './open-api.model';

const api = createPlatformOpenApiApi(apiClient);
const pageSize = 10;
const applications = ref<PlatformOpenApiApi.Application[]>([]);
const currentPage = ref(1);
const totalCount = ref(0);
const searchQuery = ref('');
const loading = ref(false);
const loadError = ref('');
const actionError = ref('');
const saving = ref(false);
const confirmationBusy = ref(false);
const applicationDialogOpen = ref(false);
const applicationDialogError = ref('');
const editingId = ref<null | string>(null);
const draft = reactive<OpenApiApplicationDraft>(createOpenApiApplicationDraft());
const confirmationOpen = ref(false);
const confirmationAction = ref<'delete' | 'rotate' | null>(null);
const confirmationTarget = ref<null | PlatformOpenApiApi.Application>(null);
const confirmationError = ref('');
const secretDialogOpen = ref(false);
const secretResult = ref<null | { appId: string; appSecret: string }>(null);
const listSurfaceKey = ref(0);
const canManage = computed(() => canAccess('Platform.OpenApi.Manage'));
const rows = computed(() => applications.value.map(toOpenApiApplicationRow));
const enabledCount = computed(() => applications.value.filter((item) => item.isEnabled).length);
const scopeCount = computed(() => new Set(applications.value.flatMap((item) => item.scopes)).size);
const confirmationTitle = computed(() =>
  confirmationAction.value === 'delete' ? '删除开放接口应用' : '轮换开放接口密钥',
);
const confirmationDescription = computed(() => {
  const target = confirmationTarget.value;
  if (!target) return '';
  if (confirmationAction.value === 'delete') {
    return `将删除“${target.name}”（${target.appId}）。此操作会使该应用凭证失效，无法撤销。`;
  }
  return `将立即使“${target.name}”（${target.appId}）的旧密钥失效。新密钥只会展示一次；依赖旧密钥的调用会立即失败。`;
});
const confirmationButtonLabel = computed(() => (confirmationAction.value === 'delete' ? '确认删除' : '确认轮换'));
let activeRequest: AbortController | undefined;
let requestVersion = 0;
let searchTimer: number | undefined;

const pageModel = computed<ListPageModel<OpenApiApplicationRow>>(() => ({
  actions: [
    { id: 'reload', label: loading.value ? '读取中' : '刷新', kind: 'refresh' },
    ...(canManage.value
      ? [
          { id: 'create', label: '新建应用', kind: 'create' as const, tone: 'primary' as const },
          { id: 'edit', label: '编辑所选', kind: 'custom' as const, requiresSelection: true },
          { id: 'rotate', label: '轮换密钥', kind: 'custom' as const, requiresSelection: true },
          {
            id: 'delete',
            label: '删除所选',
            kind: 'delete' as const,
            requiresSelection: true,
            tone: 'danger' as const,
          },
        ]
      : []),
  ],
  columns: [
    { key: 'name', title: '应用名称', width: 200 },
    { key: 'appId', title: 'AppId', width: 260 },
    { key: 'scopes', title: '授权范围', width: 220 },
    { key: 'status', title: '状态', width: 90, presentation: 'status' },
    { key: 'remark', title: '用途备注', width: 240 },
    { key: 'createdAt', title: '创建时间', width: 160 },
  ],
  demoPath: '/platform/open-api',
  description: '管理开放接口调用方、授权范围和密钥生命周期。',
  emptyHint: loading.value
    ? '正在读取开放接口应用…'
    : loadError.value
      ? '开放接口应用读取失败，请刷新重试。'
      : '暂无开放接口应用。',
  filters: [],
  key: '平台中心|开放接口',
  label: '开放接口',
  listScopes: [{ id: 'all', label: '开放接口应用', count: totalCount.value }],
  listTabs: [{ id: 'all', label: '全部应用' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'platform-open-api',
  pageSize,
  panels: [],
  path: '/platform/open-api',
  primaryAction: '新建应用',
  replication: { status: 'implemented', referenceUrl: '/platform/open-api' },
  routeName: 'platform-open-api',
  rows: rows.value,
  searchPlaceholder: '搜索应用名称 / AppId / 备注',
  stats: [
    { key: 'total', label: '开放应用总数', value: String(totalCount.value), tone: 'primary' },
    { key: 'enabled', label: '当前页启用', value: String(enabledCount.value), tone: 'success' },
    { key: 'scopes', label: '当前页授权范围', value: String(scopeCount.value), tone: 'neutral' },
    { key: 'secret', label: '密钥管理', value: '仅创建/轮换时展示', tone: 'warning' },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));

const remotePagination = computed(() => ({
  currentPage: currentPage.value,
  pageSize,
  searchQuery: searchQuery.value,
  totalCount: totalCount.value,
}));

async function loadApplications(page = currentPage.value): Promise<boolean> {
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const version = ++requestVersion;
  currentPage.value = Math.max(1, page);
  loading.value = true;
  loadError.value = '';
  try {
    const result = await api.getApplications(
      {
        keyword: searchQuery.value.trim() || undefined,
        maxResultCount: pageSize,
        skipCount: (currentPage.value - 1) * pageSize,
      },
      controller.signal,
    );
    if (version !== requestVersion) return false;
    totalCount.value = result.totalCount;
    const lastPage = Math.max(1, Math.ceil(result.totalCount / pageSize));
    if (currentPage.value > lastPage) return loadApplications(lastPage);
    applications.value = result.items;
    return true;
  } catch (error) {
    if (version !== requestVersion || (error instanceof Error && error.name === 'AbortError')) return false;
    loadError.value = error instanceof Error ? error.message : '开放接口应用读取失败，请重试。';
    return false;
  } finally {
    if (version === requestVersion) loading.value = false;
  }
}

function resetDraft(application?: PlatformOpenApiApi.Application) {
  Object.assign(draft, application ? toOpenApiApplicationDraft(application) : createOpenApiApplicationDraft());
}

function openCreate() {
  if (!canManage.value) return;
  editingId.value = null;
  applicationDialogError.value = '';
  resetDraft();
  applicationDialogOpen.value = true;
}

function openEdit(application: PlatformOpenApiApi.Application) {
  if (!canManage.value) return;
  editingId.value = application.id;
  applicationDialogError.value = '';
  resetDraft(application);
  applicationDialogOpen.value = true;
}

function selectedApplication(selectedRows: ListPageRow[]): PlatformOpenApiApi.Application | null {
  if (selectedRows.length !== 1) {
    toast.warning('每次请选择一个开放接口应用。');
    return null;
  }
  return applications.value.find((item) => item.id === selectedRows[0]?.id) ?? null;
}

function handleAction(actionId: string, selectedRows: ListPageRow[]) {
  if (actionId === 'reload') {
    void loadApplications();
    return;
  }
  if (!canManage.value) return;
  if (actionId === 'create') {
    openCreate();
    return;
  }
  const application = selectedApplication(selectedRows);
  if (!application) {
    if (selectedRows.length === 1) toast.error('所选应用状态已变化，请刷新列表。');
    return;
  }
  if (actionId === 'edit') openEdit(application);
  if (actionId === 'rotate') openConfirmation('rotate', application);
  if (actionId === 'delete') openConfirmation('delete', application);
}

async function findApplicationByAppId(appId: string): Promise<PlatformOpenApiApi.Application | null> {
  const result = await api.getApplications({ keyword: appId, maxResultCount: 10, skipCount: 0 });
  return result.items.find((item) => item.appId === appId) ?? null;
}

function hasExpectedApplication(
  actual: PlatformOpenApiApi.Application | null,
  expected: PlatformOpenApiApi.ApplicationSaveInput,
): boolean {
  if (!actual) return false;
  const expectedScopes = new Set(expected.scopes);
  const actualScopes = new Set(actual.scopes);
  return (
    actual.name === expected.name &&
    actual.remark === expected.remark &&
    actual.isEnabled === expected.isEnabled &&
    actualScopes.size === expectedScopes.size &&
    [...expectedScopes].every((scope) => actualScopes.has(scope))
  );
}

async function verifyAndRefresh(appId: string, expected?: PlatformOpenApiApi.ApplicationSaveInput): Promise<void> {
  const persisted = await findApplicationByAppId(appId);
  if (!persisted || (expected && !hasExpectedApplication(persisted, expected))) {
    throw new Error('服务已返回成功，但重新查询结果与本次修改不一致。请刷新后核对。');
  }
  if ('appSecret' in persisted) throw new Error('服务端列表返回了不应公开的密钥字段，已停止展示列表数据。');
  if (!(await loadApplications(currentPage.value))) throw new Error('操作已提交，但列表刷新失败，请重新读取核对。');
  listSurfaceKey.value += 1;
}

async function saveApplication() {
  if (!canManage.value || saving.value) return;
  const validation = validateOpenApiApplicationDraft(draft);
  if (validation) {
    applicationDialogError.value = validation;
    return;
  }
  const input = toOpenApiApplicationSaveInput(draft);
  saving.value = true;
  applicationDialogError.value = '';
  let savedApplication: PlatformOpenApiApi.Application;
  let oneTimeSecret: null | { appId: string; appSecret: string } = null;
  try {
    if (editingId.value) {
      savedApplication = await api.updateApplication(editingId.value, input);
    } else {
      const created = await api.createApplication(input);
      savedApplication = created;
      oneTimeSecret = { appId: created.appId, appSecret: created.appSecret };
    }
  } catch (error) {
    applicationDialogError.value = error instanceof Error ? error.message : '开放接口应用保存失败，请重试。';
    saving.value = false;
    return;
  }
  saving.value = false;
  applicationDialogOpen.value = false;
  if (oneTimeSecret) showSecret(oneTimeSecret);
  try {
    await verifyAndRefresh(savedApplication.appId, input);
    toast.success(editingId.value ? '开放接口应用已保存并读回确认。' : '开放接口应用已创建并读回确认。');
    actionError.value = '';
  } catch (error) {
    actionError.value = error instanceof Error ? error.message : '操作已提交，但读回核验失败。';
    toast.warning(actionError.value);
  }
}

function openConfirmation(action: 'delete' | 'rotate', application: PlatformOpenApiApi.Application) {
  confirmationAction.value = action;
  confirmationTarget.value = application;
  confirmationError.value = '';
  confirmationOpen.value = true;
}

async function confirmAction() {
  const target = confirmationTarget.value;
  const action = confirmationAction.value;
  if (!target || !action || !canManage.value || confirmationBusy.value) return;
  confirmationBusy.value = true;
  confirmationError.value = '';
  let rotatedSecret: null | { appId: string; appSecret: string } = null;
  try {
    if (action === 'delete') {
      if (!(await api.deleteApplication(target.id))) throw new Error('服务未确认删除结果。');
      const remaining = await findApplicationByAppId(target.appId);
      if (remaining) throw new Error('删除请求已完成，但应用仍出现在服务端列表中。');
    } else {
      const result = await api.resetApplicationSecret(target.id);
      if (result.id !== target.id || result.appId !== target.appId || !result.appSecret) {
        throw new Error('服务返回的密钥轮换结果与目标应用不匹配。');
      }
      rotatedSecret = { appId: result.appId, appSecret: result.appSecret };
      const persisted = await findApplicationByAppId(target.appId);
      if (!persisted || persisted.id !== target.id || 'appSecret' in persisted) {
        throw new Error('密钥已轮换，但应用列表读回未通过；请刷新核对。');
      }
    }
    confirmationOpen.value = false;
    if (rotatedSecret) showSecret(rotatedSecret);
    if (!(await loadApplications(currentPage.value)))
      throw new Error('操作已提交，但应用列表刷新失败，请重新读取核对。');
    listSurfaceKey.value += 1;
    actionError.value = '';
    toast.success(action === 'delete' ? '开放接口应用已删除并确认。' : '密钥已轮换；新密钥仅展示一次。');
    clearConfirmation();
  } catch (error) {
    const message = error instanceof Error ? error.message : '开放接口操作失败，请重试。';
    if (rotatedSecret) {
      confirmationOpen.value = false;
      showSecret(rotatedSecret);
      actionError.value = message;
      toast.warning(message);
      clearConfirmation();
    } else {
      confirmationError.value = message;
    }
  } finally {
    confirmationBusy.value = false;
  }
}

function clearConfirmation() {
  confirmationAction.value = null;
  confirmationTarget.value = null;
  confirmationError.value = '';
}

function showSecret(secret: { appId: string; appSecret: string }) {
  secretResult.value = { appId: secret.appId, appSecret: secret.appSecret };
  secretDialogOpen.value = true;
}

function updateSecretDialogOpen(open: boolean) {
  secretDialogOpen.value = open;
  if (!open) secretResult.value = null;
}

async function copySecret() {
  const secret = secretResult.value?.appSecret;
  if (!secret) return;
  try {
    await navigator.clipboard.writeText(secret);
    toast.success('密钥已复制；请立即粘贴到受控密码库。');
  } catch {
    toast.error('浏览器未允许访问剪贴板，请手动复制后妥善保存。');
  }
}

function updateApplicationDialogOpen(open: boolean) {
  if (saving.value) return;
  applicationDialogOpen.value = open;
  if (!open) applicationDialogError.value = '';
}

function updateConfirmationOpen(open: boolean) {
  if (confirmationBusy.value) return;
  confirmationOpen.value = open;
  if (!open) clearConfirmation();
}

function searchApplications(query: string) {
  searchQuery.value = query;
  currentPage.value = 1;
  listSurfaceKey.value += 1;
  if (searchTimer !== undefined) window.clearTimeout(searchTimer);
  searchTimer = window.setTimeout(() => void loadApplications(1), 250);
}

function changePage(page: number) {
  listSurfaceKey.value += 1;
  void loadApplications(page);
}

function openSelectedRow(row: ListPageRow) {
  const application = applications.value.find((item) => item.id === row.id);
  if (application) openEdit(application);
}

onMounted(() => void loadApplications(1));
onBeforeUnmount(() => {
  activeRequest?.abort();
  if (searchTimer !== undefined) window.clearTimeout(searchTimer);
  secretResult.value = null;
});
</script>

<template>
  <ListPageSurface
    :key="listSurfaceKey"
    :model="pageModel"
    :remote-pagination="remotePagination"
    @action="(action, selectedRows) => handleAction(action.id, selectedRows)"
    @page-change="changePage"
    @row="openSelectedRow"
    @search-change="searchApplications"
  >
    <template #hero-icon>
      <KeyRound
        :size="22"
        aria-hidden="true"
      />
    </template>
    <template #notice>
      <div :class="$style.noticeContent">
        <p>密钥不会出现在列表或编辑数据中；创建和轮换时只展示一次，关闭窗口后前端立即清除。</p>
        <p>当前已登记的公开只读接口：GET /api/open/dict/{dictTypeCode}/items，授权范围为 dict:read。</p>
        <p>
          调用需要 X-GA-AppId、X-GA-Timestamp、X-GA-Nonce、X-GA-Signature 四个签名头；签名窗口为5分钟，nonce不可重放。
        </p>
        <p>
          签名原文依次为大写HTTP方法、路径及查询、时间戳、nonce、请求体SHA-256十六进制摘要，以换行连接；使用AppSecret执行HMAC-SHA256。
        </p>
        <p>OpenDict由外部调用方按签名协议读取，本管理端不提供字典写入口，也不代持应用密钥发起公开请求。</p>
        <p
          v-if="!canManage"
          role="alert"
        >
          当前账号没有开放接口管理权限。
        </p>
        <p
          v-if="loadError"
          role="alert"
        >
          {{ loadError }}
        </p>
        <p
          v-if="actionError"
          role="alert"
        >
          {{ actionError }}
        </p>
      </div>
    </template>
  </ListPageSurface>

  <OpenApiApplicationDialog
    :open="applicationDialogOpen"
    :mode="editingId ? 'edit' : 'create'"
    :draft="draft"
    :error="applicationDialogError"
    :saving="saving"
    @update:open="updateApplicationDialogOpen"
    @submit="saveApplication"
  />

  <UiDialog
    :open="confirmationOpen"
    :title="confirmationTitle"
    :description="confirmationDescription"
    @update:open="updateConfirmationOpen"
  >
    <div :class="$style.confirmationContent">
      <p v-if="confirmationAction === 'rotate'">旧密钥会立即失效，请先确认调用方可以同步更新凭证。</p>
      <p v-else>删除后无法恢复该应用；此操作会阻止它继续访问公开接口。</p>
      <p
        v-if="confirmationError"
        role="alert"
      >
        {{ confirmationError }}
      </p>
    </div>
    <template #footer>
      <UiButton
        :disabled="confirmationBusy"
        @click="updateConfirmationOpen(false)"
        >取消</UiButton
      >
      <UiButton
        variant="danger"
        :loading="confirmationBusy"
        :disabled="confirmationBusy || !canManage"
        @click="confirmAction"
        >{{ confirmationBusy ? '处理中…' : confirmationButtonLabel }}</UiButton
      >
    </template>
  </UiDialog>

  <OpenApiSecretDialog
    :open="secretDialogOpen"
    :app-id="secretResult?.appId ?? ''"
    :app-secret="secretResult?.appSecret ?? ''"
    @update:open="updateSecretDialogOpen"
    @copy="copySecret"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.noticeContent {
  @apply space-y-1;
}

.confirmationContent {
  @apply space-y-3 p-5;
}
</style>
