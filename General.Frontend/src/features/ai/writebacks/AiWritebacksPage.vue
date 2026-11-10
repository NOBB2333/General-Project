<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { ClipboardCheck } from 'lucide-vue-next';
import type { PageAction } from '@/components/page/page.contracts';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel, ListPageRow } from '@/components/page/families/list/list-page.types';
import { createAiApi, type AiApi } from '@/api/core/ai';
import { apiClient } from '@/api/request';
import { canAccess } from '@/directives/permission';
import { toast, UiAlertDialog, UiButton, UiDialog, UiFormField, UiInput, UiTextarea } from '@/components/ui';

const api = createAiApi(apiClient);
const canView = computed(() => canAccess('Ai.Writeback.View'));
const canApprove = computed(() => canAccess('Ai.Writeback.Approve'));
const requests = ref<AiApi.WritebackRequest[]>([]);
const loading = ref(false);
const actionBusy = ref(false);
const loadError = ref('');
const totalCount = ref(0);
const currentPage = ref(1);
const pageSize = 20;
const searchQuery = ref('');
const selectedStatus = ref('待确认');
const appliedFrom = ref('');
const appliedTo = ref('');
const draftFrom = ref('');
const draftTo = ref('');
const detailOpen = ref(false);
const selectedRequest = ref<AiApi.WritebackRequest | null>(null);
const decisionDialogOpen = ref(false);
const decisionKind = ref<'approve' | 'reject' | 'cancel' | null>(null);
const rejectReason = ref('');
let activeController: AbortController | null = null;
let loadVersion = 0;
let searchTimer: ReturnType<typeof setTimeout> | null = null;

const statusToApi: Record<string, string> = {
  全部: 'All',
  待确认: 'Pending',
  已批准: 'Approved',
  已拒绝: 'Rejected',
  已取消: 'Cancelled',
  失败: 'Failed',
};
const statusFromApi: Record<string, string> = {
  Approved: '已批准（尚未执行）',
  Cancelled: '已取消',
  Failed: '执行失败',
  Pending: '待确认',
  Rejected: '已拒绝',
};
const statusOptions = ['全部', '待确认', '已批准', '已拒绝', '已取消', '失败'];

function errorText(error: unknown): string {
  return error instanceof Error ? error.message : '请求失败，请重试。';
}

function formatTime(value: null | string): string {
  return value ? new Date(value).toLocaleString('zh-CN') : '—';
}

function statusLabel(value: string): string {
  return statusFromApi[value] ?? (value || '未知');
}

const rows = computed<ListPageRow[]>(() =>
  requests.value.map((item) => ({
    actionCode: item.actionCode,
    businessModule: item.businessModule,
    creationTime: formatTime(item.creationTime),
    id: item.id,
    status: statusLabel(item.status),
    summary: item.summary,
    targetEntity: `${item.targetEntityType}${item.targetEntityId ? ` · ${item.targetEntityId}` : ''}`,
  })),
);

const pageModel = computed<ListPageModel>(() => ({
  actions: [{ id: 'reload', kind: 'refresh', label: loading.value ? '读取中' : '刷新' }],
  columns: [
    { key: 'creationTime', title: '申请时间', width: 180 },
    { key: 'businessModule', title: '业务模块', width: 130 },
    { key: 'actionCode', title: '动作', width: 170 },
    { key: 'targetEntity', title: '目标记录', width: 270 },
    { key: 'summary', title: '申请说明', width: 360 },
    { key: 'status', title: '处理状态', width: 150, presentation: 'status' },
  ],
  demoPath: '/ai/writebacks',
  description: '集中查看 AI 生成的业务写回申请，查看差异后由有权限的人员确认状态。',
  emptyHint: loading.value ? '正在读取数据…' : loadError.value || '暂无符合条件的写回申请。',
  filters: [{ key: 'status', label: '状态', options: statusOptions }],
  key: 'AI管理|写回确认',
  label: '写回确认',
  listScopes: [{ id: 'all', label: '全部申请', count: totalCount.value }],
  listTabs: [{ id: 'all', label: '全部申请' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: 'AI 管理',
  moduleSlug: 'ai',
  pageSlug: 'ai-writebacks',
  pageSize,
  panels: [],
  path: '/ai/writebacks',
  primaryAction: '刷新',
  replication: { status: 'implemented', referenceUrl: '/ai/writebacks' },
  routeName: 'ai-writebacks',
  rows: rows.value,
  searchPlaceholder: '搜索申请说明、动作码或目标记录',
  stats: [
    { key: 'total', label: '筛选后申请', value: String(totalCount.value), tone: 'primary' },
    {
      key: 'pending',
      label: '本页待确认',
      value: String(requests.value.filter((item) => item.status === 'Pending').length),
      tone: 'warning',
    },
    {
      key: 'approved',
      label: '本页已批准',
      value: String(requests.value.filter((item) => item.status === 'Approved').length),
      tone: 'success',
    },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));
const remoteFilterValues = computed(() => ({ status: selectedStatus.value }));
const remotePagination = computed(() => ({
  currentPage: currentPage.value,
  pageSize,
  searchQuery: searchQuery.value,
  totalCount: totalCount.value,
}));
const decisionTitle = computed(() => {
  if (decisionKind.value === 'approve') return '确认批准写回申请';
  if (decisionKind.value === 'reject') return '退回写回申请';
  if (decisionKind.value === 'cancel') return '取消写回申请';
  return '处理写回申请';
});

async function load(): Promise<void> {
  if (!canView.value) return;
  const version = ++loadVersion;
  activeController?.abort();
  const controller = new AbortController();
  activeController = controller;
  loading.value = true;
  loadError.value = '';
  const apiStatus = statusToApi[selectedStatus.value] ?? 'Pending';
  try {
    const result = await api.getWritebacks(
      {
        createdFrom: appliedFrom.value || undefined,
        createdTo: appliedTo.value || undefined,
        keyword: searchQuery.value.trim() || undefined,
        maxResultCount: pageSize,
        skipCount: (currentPage.value - 1) * pageSize,
        status: apiStatus,
      },
      controller.signal,
    );
    if (version !== loadVersion) return;
    requests.value = result.items;
    totalCount.value = result.totalCount;
    const pageCount = Math.max(1, Math.ceil(totalCount.value / pageSize));
    if (currentPage.value > pageCount) currentPage.value = pageCount;
  } catch (error) {
    if (!controller.signal.aborted && version === loadVersion) loadError.value = errorText(error);
  } finally {
    if (version === loadVersion) {
      loading.value = false;
      activeController = null;
    }
  }
}

function handleAction(action: PageAction): void {
  if (action.id === 'reload') void load();
}

function handleFilterChange(key: string, value: string): void {
  if (key !== 'status') return;
  selectedStatus.value = value;
  currentPage.value = 1;
  void load();
}

function handleSearch(value: string): void {
  searchQuery.value = value;
  currentPage.value = 1;
  if (searchTimer) clearTimeout(searchTimer);
  searchTimer = setTimeout(() => void load(), 280);
}

function changePage(page: number): void {
  currentPage.value = page;
  void load();
}

function applyDateFilters(): void {
  if (draftFrom.value && draftTo.value && draftFrom.value > draftTo.value) {
    loadError.value = '开始日期不能晚于结束日期。';
    return;
  }
  appliedFrom.value = draftFrom.value;
  appliedTo.value = draftTo.value;
  currentPage.value = 1;
  void load();
}

function resetDateFilters(): void {
  draftFrom.value = '';
  draftTo.value = '';
  appliedFrom.value = '';
  appliedTo.value = '';
  currentPage.value = 1;
  void load();
}

function openRequest(row: ListPageRow): void {
  selectedRequest.value = requests.value.find((item) => item.id === row.id) ?? null;
  detailOpen.value = Boolean(selectedRequest.value);
}

function askDecision(kind: 'approve' | 'reject' | 'cancel'): void {
  if (!canApprove.value || !selectedRequest.value || selectedRequest.value.status !== 'Pending' || actionBusy.value)
    return;
  decisionKind.value = kind;
  rejectReason.value = '';
  decisionDialogOpen.value = true;
}

async function confirmDecision(): Promise<void> {
  const item = selectedRequest.value;
  const kind = decisionKind.value;
  if (!item || !kind || !canApprove.value || actionBusy.value) return;
  if (kind === 'reject' && rejectReason.value.length > 512) {
    toast.error('退回原因不能超过 512 个字符。');
    return;
  }
  actionBusy.value = true;
  try {
    if (kind === 'approve') await api.approveWriteback(item.id);
    else if (kind === 'reject') await api.rejectWriteback(item.id, { reason: rejectReason.value.trim() });
    else await api.cancelWriteback(item.id);
    decisionDialogOpen.value = false;
    detailOpen.value = false;
    toast.success(
      kind === 'approve' ? '申请已批准，业务数据尚未执行写入。' : kind === 'reject' ? '申请已退回。' : '申请已取消。',
    );
    await load();
  } catch (error) {
    toast.error(errorText(error));
  } finally {
    actionBusy.value = false;
    decisionKind.value = null;
  }
}

onMounted(() => void load());
onBeforeUnmount(() => {
  loadVersion += 1;
  activeController?.abort();
  if (searchTimer) clearTimeout(searchTimer);
});
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    :remote-filter-values="remoteFilterValues"
    :remote-pagination="remotePagination"
    @action="handleAction"
    @filter-change="handleFilterChange"
    @page-change="changePage"
    @row="openRequest"
    @search-change="handleSearch"
  >
    <template #hero-icon
      ><ClipboardCheck
        :size="22"
        aria-hidden="true"
    /></template>
    <template #notice>
      <p>
        当前后端提供待确认、批准、退回、取消状态流转。批准只表示审核通过，不会执行项目或经营数据写入，也没有模拟执行接口。
      </p>
      <p v-if="!canApprove">当前账号只有查看权限；处理申请需要 Ai.Writeback.Approve。</p>
      <section aria-label="申请日期筛选">
        <UiFormField label="申请开始日期"
          ><UiInput
            v-model="draftFrom"
            type="date"
            aria-label="申请开始日期"
        /></UiFormField>
        <UiFormField label="申请结束日期"
          ><UiInput
            v-model="draftTo"
            type="date"
            aria-label="申请结束日期"
        /></UiFormField>
        <UiButton
          variant="primary"
          @click="applyDateFilters"
          >应用日期</UiButton
        >
        <UiButton @click="resetDateFilters">重置日期</UiButton>
      </section>
      <p
        v-if="loadError"
        role="alert"
      >
        {{ loadError }}
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    :open="detailOpen"
    :title="selectedRequest ? `写回申请：${selectedRequest.summary}` : '写回申请详情'"
    description="申请载荷与差异预览以纯文本展示。批准不会自动修改业务记录。"
    size="lg"
    @update:open="detailOpen = $event"
  >
    <UiFormField label="处理状态"
      ><UiInput
        :model-value="selectedRequest ? statusLabel(selectedRequest.status) : ''"
        readonly
    /></UiFormField>
    <UiFormField label="业务模块"
      ><UiInput
        :model-value="selectedRequest?.businessModule ?? ''"
        readonly
    /></UiFormField>
    <UiFormField label="动作码"
      ><UiInput
        :model-value="selectedRequest?.actionCode ?? ''"
        readonly
    /></UiFormField>
    <UiFormField label="目标记录"
      ><UiInput
        :model-value="
          selectedRequest ? `${selectedRequest.targetEntityType} · ${selectedRequest.targetEntityId || '未指定'}` : ''
        "
        readonly
    /></UiFormField>
    <UiFormField label="申请时间"
      ><UiInput
        :model-value="selectedRequest ? formatTime(selectedRequest.creationTime) : ''"
        readonly
    /></UiFormField>
    <UiFormField
      v-if="selectedRequest?.approvedTime"
      label="审核时间"
      ><UiInput
        :model-value="formatTime(selectedRequest.approvedTime)"
        readonly
    /></UiFormField>
    <UiFormField
      v-if="selectedRequest?.rejectReason"
      label="退回原因"
      wide
      ><UiTextarea
        :model-value="selectedRequest.rejectReason"
        rows="3"
        readonly
    /></UiFormField>
    <UiFormField
      label="差异预览 JSON（纯文本）"
      wide
      ><UiTextarea
        :model-value="selectedRequest?.diffPreviewJson ?? '{}'"
        rows="7"
        readonly
    /></UiFormField>
    <UiFormField
      label="申请载荷 JSON（纯文本）"
      wide
      ><UiTextarea
        :model-value="selectedRequest?.payloadJson ?? '{}'"
        rows="10"
        readonly
    /></UiFormField>
    <UiFormField
      v-if="selectedRequest?.errorMessage"
      label="错误信息"
      wide
      ><UiTextarea
        :model-value="selectedRequest.errorMessage"
        rows="3"
        readonly
    /></UiFormField>
    <template #footer>
      <UiButton @click="detailOpen = false">关闭</UiButton>
      <template v-if="selectedRequest?.status === 'Pending' && canApprove">
        <UiButton
          :disabled="actionBusy"
          @click="askDecision('cancel')"
          >取消申请</UiButton
        >
        <UiButton
          :disabled="actionBusy"
          @click="askDecision('reject')"
          >退回</UiButton
        >
        <UiButton
          variant="primary"
          :disabled="actionBusy"
          @click="askDecision('approve')"
          >批准申请</UiButton
        >
      </template>
    </template>
  </UiDialog>

  <UiDialog
    v-if="decisionKind === 'reject'"
    :open="decisionDialogOpen"
    :title="decisionTitle"
    description="退回后申请进入已拒绝状态，业务数据不会被写入。"
    @update:open="decisionDialogOpen = $event"
  >
    <UiFormField label="退回原因"
      ><UiTextarea
        v-model="rejectReason"
        rows="4"
        maxlength="512"
        placeholder="填写退回原因（可选）"
    /></UiFormField>
    <template #footer>
      <UiButton
        :disabled="actionBusy"
        @click="decisionDialogOpen = false"
        >返回</UiButton
      >
      <UiButton
        variant="primary"
        :disabled="actionBusy"
        @click="confirmDecision"
        >{{ actionBusy ? '处理中…' : '确认退回' }}</UiButton
      >
    </template>
  </UiDialog>
  <UiAlertDialog
    v-else
    :open="decisionDialogOpen"
    :title="decisionTitle"
    :description="
      decisionKind === 'approve' ? '批准只更新审核状态，当前后端不会执行写回。' : '取消后申请不可再次处理。'
    "
    :confirm-label="decisionKind === 'approve' ? '确认批准' : '确认取消'"
    :tone="decisionKind === 'cancel' ? 'warning' : 'default'"
    @update:open="decisionDialogOpen = $event"
    @confirm="confirmDecision"
  />
</template>
