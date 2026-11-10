<script setup lang="ts">
import { ShieldCheck } from 'lucide-vue-next';
import AuditLogDetailDialog from './AuditLogDetailDialog.vue';
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import { createAuditLogApi, type AuditLogApi } from '@/api/core/audit-log';
import { apiClient } from '@/api/request';
import { ListPageSurface } from '@/components/page';
import type { ListPageModel } from '@/components/page/families/list/list-page.types';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect } from '@/components/ui';
import {
  auditCategoryOptions,
  auditPath,
  buildAuditQuery,
  toAuditLogRow,
  type AuditFilters,
  type AuditLogRow,
} from './audit-log.model';

const api = createAuditLogApi(apiClient);
const filters = reactive<AuditFilters>({ category: '', endDate: '', startDate: '' });
const draft = reactive<AuditFilters>({ ...filters });
const draftPageSize = ref('20');
const pageSize = ref(20);
const currentPage = ref(1);
const searchQuery = ref('');
const logs = ref<AuditLogApi.Item[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const errorMessage = ref('');
const filterOpen = ref(false);
const filterError = ref('');
const detailOpen = ref(false);
const selectedRow = ref<AuditLogRow | null>(null);
const analysisOpen = ref(false);
const analysisLoading = ref(false);
const analysisError = ref('');
const dashboard = ref<AuditLogApi.Dashboard | null>(null);
let requestVersion = 0;
let activeRequest: AbortController | undefined;
let analysisRequest: AbortController | undefined;
let searchTimer: number | undefined;

const categoryLabel = computed(
  () => auditCategoryOptions.find((option) => option.value === filters.category)?.label ?? '日志',
);
const filterSummary = computed(
  () => `${categoryLabel.value} · ${filters.startDate || '不限开始日期'} 至 ${filters.endDate || '不限结束日期'}`,
);
const pageModel = computed<ListPageModel<AuditLogRow>>(() => ({
  actions: [
    { id: 'filters', label: '筛选条件', kind: 'custom' },
    { id: 'reset', label: '重置', kind: 'custom' },
    { id: 'analysis', label: '访问分析', kind: 'custom' },
    { id: 'refresh', label: loading.value ? '刷新中' : '刷新', kind: 'refresh' },
  ],
  columns: [
    { key: 'time', title: '执行时间', width: 190 },
    { key: 'actor', title: '用户', width: 130 },
    { key: 'kind', title: '记录类型', width: 110 },
    { key: 'summary', title: '动作 / 页面', width: 320 },
    { key: 'path', title: '请求路径', width: 280 },
    { key: 'method', title: '方法', width: 90 },
    { key: 'status', title: '状态码', width: 90 },
    { key: 'duration', title: '耗时', width: 100 },
    { key: 'source', title: '会话来源', width: 130 },
  ],
  demoPath: '/platform/audit-logs',
  description: '查询接口访问、操作、异常与页面访问记录。点击记录查看详情。',
  emptyHint: loading.value
    ? '正在加载日志…'
    : errorMessage.value
      ? '日志加载失败，请通过刷新重试。'
      : '当前筛选条件下暂无日志。',
  filters: [],
  key: '平台中心|日志中心',
  label: '日志中心',
  listScopes: [{ id: 'all', label: categoryLabel.value }],
  listTabs: [{ id: 'all', label: '记录' }],
  listViews: [{ id: 'list', label: '列表' }],
  module: '平台中心',
  moduleSlug: 'platform',
  pageSlug: 'platform-audit-logs',
  pageSize: pageSize.value,
  panels: [],
  path: '/platform/audit-logs',
  primaryAction: '筛选条件',
  replication: { status: 'implemented', referenceUrl: '/platform/audit-logs' },
  routeName: 'platform-audit-logs',
  rows: logs.value.map(toAuditLogRow),
  searchPlaceholder: '搜索用户 / 租户 / 接口 / 动作',
  stats: [
    { key: 'total', label: '筛选结果', value: String(totalCount.value), tone: 'primary' },
    { key: 'page', label: '本页记录', value: String(logs.value.length), tone: 'neutral' },
  ],
  surface: 'list',
  tabs: [],
  tools: [],
}));

async function loadLogs() {
  if (searchTimer !== undefined) window.clearTimeout(searchTimer);
  activeRequest?.abort();
  const controller = new AbortController();
  activeRequest = controller;
  const version = ++requestVersion;
  loading.value = true;
  errorMessage.value = '';
  logs.value = [];
  totalCount.value = 0;
  try {
    const result = await api.getPaged(
      buildAuditQuery(filters, searchQuery.value, currentPage.value, pageSize.value),
      controller.signal,
    );
    if (version !== requestVersion) return;
    const lastPage = Math.max(1, Math.ceil(result.totalCount / pageSize.value));
    if (currentPage.value > lastPage) {
      currentPage.value = lastPage;
      await loadLogs();
      return;
    }
    logs.value = result.items;
    totalCount.value = result.totalCount;
  } catch (error) {
    if (version !== requestVersion || (error instanceof Error && error.name === 'AbortError')) return;
    errorMessage.value = error instanceof Error ? error.message : '日志加载失败，请重试。';
  } finally {
    if (version === requestVersion) loading.value = false;
  }
}

function applyFilters() {
  try {
    buildAuditQuery(draft, searchQuery.value, 1, Number(draftPageSize.value));
    Object.assign(filters, draft);
    pageSize.value = Number(draftPageSize.value);
    currentPage.value = 1;
    filterOpen.value = false;
    void loadLogs();
  } catch (error) {
    filterError.value = error instanceof Error ? error.message : '请检查筛选条件。';
  }
}

async function openAnalysis() {
  analysisRequest?.abort();
  const controller = new AbortController();
  analysisRequest = controller;
  analysisOpen.value = true;
  analysisLoading.value = true;
  analysisError.value = '';
  dashboard.value = null;
  try {
    const result = await api.getDashboard(buildAuditQuery(filters, searchQuery.value, 1, 200), controller.signal);
    if (analysisRequest === controller) dashboard.value = result;
  } catch (error) {
    if (error instanceof Error && error.name === 'AbortError') return;
    analysisError.value = error instanceof Error ? error.message : '访问分析加载失败。';
  } finally {
    if (analysisRequest === controller) analysisLoading.value = false;
  }
}

function runAction(action: string) {
  if (action === 'filters') {
    Object.assign(draft, filters);
    draftPageSize.value = String(pageSize.value);
    filterError.value = '';
    filterOpen.value = true;
  } else if (action === 'reset') {
    Object.assign(filters, { category: '', startDate: '', endDate: '' });
    searchQuery.value = '';
    currentPage.value = 1;
    void loadLogs();
  } else if (action === 'analysis') {
    void openAnalysis();
  } else if (action === 'refresh') {
    void loadLogs();
  }
}

function changePage(page: number) {
  if (loading.value || page === currentPage.value) return;
  currentPage.value = page;
  void loadLogs();
}

watch(
  searchQuery,
  () => {
    currentPage.value = 1;
    if (searchTimer !== undefined) window.clearTimeout(searchTimer);
    searchTimer = window.setTimeout(() => void loadLogs(), 300);
  },
  { flush: 'sync' },
);
onMounted(() => void loadLogs());
onBeforeUnmount(() => {
  if (searchTimer !== undefined) window.clearTimeout(searchTimer);
  activeRequest?.abort();
  analysisRequest?.abort();
});
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    :remote-pagination="{ currentPage, pageSize, searchQuery, totalCount }"
    @action="(action) => runAction(action.id)"
    @page-change="changePage"
    @search-change="searchQuery = $event"
    @row="
      (row) => {
        selectedRow = row;
        detailOpen = true;
      }
    "
  >
    <template #hero-icon><ShieldCheck :size="20" /></template>
    <template #notice>
      <p>当前范围：{{ filterSummary }}</p>
      <p
        v-if="errorMessage"
        role="alert"
      >
        {{ errorMessage }}
      </p>
    </template>
  </ListPageSurface>

  <UiDialog
    v-model:open="filterOpen"
    title="日志筛选"
    description="日期按本地时区包含当天全部记录。"
  >
    <UiFormField label="日志类别">
      <UiSelect
        v-model="draft.category"
        label="日志类别"
        :options="auditCategoryOptions"
      />
    </UiFormField>
    <UiFormField label="开始日期"
      ><UiInput
        v-model="draft.startDate"
        type="date"
        aria-label="开始日期"
    /></UiFormField>
    <UiFormField label="结束日期"
      ><UiInput
        v-model="draft.endDate"
        type="date"
        aria-label="结束日期"
    /></UiFormField>
    <UiFormField label="每页条数">
      <UiSelect
        v-model="draftPageSize"
        label="每页条数"
        :options="[
          { value: '20', label: '20条' },
          { value: '50', label: '50条' },
          { value: '100', label: '100条' },
        ]"
      />
    </UiFormField>
    <p
      v-if="filterError"
      role="alert"
    >
      {{ filterError }}
    </p>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="filterOpen = false"
        >取消</UiButton
      >
      <UiButton @click="applyFilters">查询</UiButton>
    </template>
  </UiDialog>

  <AuditLogDetailDialog
    v-model:open="detailOpen"
    :row="selectedRow"
  />

  <UiDialog
    v-model:open="analysisOpen"
    title="访问分析"
    description="按当前筛选条件统计最近最多200条记录。"
  >
    <p
      v-if="analysisLoading"
      role="status"
    >
      正在加载访问分析…
    </p>
    <p
      v-else-if="analysisError"
      role="alert"
    >
      {{ analysisError }}
    </p>
    <template v-else-if="dashboard">
      <UiFormField label="接口访问排行">
        <p v-if="!dashboard.topApis.length">暂无接口访问记录。</p>
        <p
          v-for="item in dashboard.topApis"
          :key="item.key"
        >
          {{ auditPath(item.label) }} · {{ item.count }}次
        </p>
      </UiFormField>
      <UiFormField label="页面 / 动作排行">
        <p v-if="!dashboard.topPages.length && !dashboard.topMenus.length">暂无页面或动作记录。</p>
        <p
          v-for="item in dashboard.topPages.length ? dashboard.topPages : dashboard.topMenus"
          :key="item.key"
        >
          {{ item.label }} · {{ item.count }}次
        </p>
      </UiFormField>
      <UiFormField label="用户访问排行">
        <p v-if="!dashboard.topUsers.length">暂无用户访问记录。</p>
        <p
          v-for="item in dashboard.topUsers"
          :key="item.key"
        >
          {{ item.label }} · {{ item.count }}次
        </p>
      </UiFormField>
    </template>
    <template #footer>
      <UiButton
        variant="secondary"
        :disabled="analysisLoading"
        @click="openAnalysis"
        >刷新</UiButton
      >
      <UiButton
        variant="secondary"
        @click="analysisOpen = false"
        >关闭</UiButton
      >
    </template>
  </UiDialog>
</template>
