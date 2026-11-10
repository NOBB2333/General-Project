<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { revenueMetrics, revenueRecognitionFixture } from '../model/revenue-recognition.data';
import type {
  RevenueRecognitionDraft,
  RevenueRecognitionRow,
  RevenueStatus,
  RevenueStatusFilter,
} from '../model/revenue-recognition.types';
import RevenueRecognitionCreateDialog from './revenue-recognition/RevenueRecognitionCreateDialog.vue';
import RevenueRecognitionDetailDialog from './revenue-recognition/RevenueRecognitionDetailDialog.vue';
import RevenueRecognitionHero from './revenue-recognition/RevenueRecognitionHero.vue';
import RevenueRecognitionOverview from './revenue-recognition/RevenueRecognitionOverview.vue';
import RevenueRecognitionWorkspace from './revenue-recognition/RevenueRecognitionWorkspace.vue';

const rows = ref<RevenueRecognitionRow[]>(cloneReferenceRows(revenueRecognitionFixture.rows));
const activeStatus = ref<RevenueStatusFilter>(revenueRecognitionFixture.defaults?.activeStatus as RevenueStatusFilter);
const query = ref('');
const startDate = ref('');
const endDate = ref('');
const customer = ref('');
const project = ref('');
const method = ref('');
const creator = ref('');
const selectedIds = ref<string[]>([]);
const createOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<RevenueRecognitionRow | null>(null);

const statusCounts = computed<Record<RevenueStatus, number>>(() => ({
  草稿: rows.value.filter((row) => row.status === '草稿').length,
  待审核: rows.value.filter((row) => row.status === '待审核').length,
  已审核: rows.value.filter((row) => row.status === '已审核').length,
  已驳回: rows.value.filter((row) => row.status === '已驳回').length,
  已作废: rows.value.filter((row) => row.status === '已作废').length,
}));
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !normalized || `${row.code} ${row.contract} ${row.customer}`.toLocaleLowerCase('zh-CN').includes(normalized);
    const matchesStart = !startDate.value || row.confirmedAt >= startDate.value;
    const matchesEnd = !endDate.value || row.confirmedAt <= endDate.value;
    const matchesCustomer = !customer.value || row.customer === customer.value;
    const matchesProject = !project.value || row.project === project.value;
    const matchesMethod = !method.value || row.method === method.value;
    const matchesCreator = !creator.value || row.creator === creator.value;
    return (
      matchesStatus &&
      matchesQuery &&
      matchesStart &&
      matchesEnd &&
      matchesCustomer &&
      matchesProject &&
      matchesMethod &&
      matchesCreator
    );
  });
});

function toggleRow(id: string, checked: boolean) {
  selectedIds.value = checked
    ? [...new Set([...selectedIds.value, id])]
    : selectedIds.value.filter((item) => item !== id);
}

function toggleAll(checked: boolean) {
  const visibleIds = filteredRows.value.map((row) => row.id);
  selectedIds.value = checked
    ? [...new Set([...selectedIds.value, ...visibleIds])]
    : selectedIds.value.filter((id) => !visibleIds.includes(id));
}

function openDetail(row: RevenueRecognitionRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function saveDraft(draft: RevenueRecognitionDraft) {
  const amount = Number(draft.amount);
  const created: RevenueRecognitionRow = {
    id: `revenue-local-${Date.now()}`,
    code: `RR-LOCAL-${String(rows.value.length + 1).padStart(4, '0')}`,
    contract: draft.contract.trim(),
    customer: draft.customer.trim(),
    project: draft.project.trim() || '未关联项目',
    method: draft.method,
    amount,
    accumulated: amount,
    contractAmount: amount * 5,
    remaining: amount * 4,
    confirmedAt: draft.confirmedAt,
    period: draft.period.trim(),
    invoiceStatus: '-',
    status: '草稿',
    creator: '当前用户',
    createdAt: '2026-09-16 10:00',
    reviewer: '-',
    reviewedAt: '-',
  };
  rows.value = [created, ...rows.value];
  activeStatus.value = '全部';
  query.value = '';
  createOpen.value = false;
  toast.success(`${created.code} 已保存到本地草稿`);
}

function updateStatus(row: RevenueRecognitionRow, status: RevenueStatus) {
  rows.value = rows.value.map((item) =>
    item.id === row.id
      ? {
          ...item,
          status,
          reviewer: status === '已审核' ? '当前审核人' : item.reviewer,
          reviewedAt: status === '已审核' ? '2026-09-16 10:10' : item.reviewedAt,
        }
      : item,
  );
  activeRow.value = rows.value.find((item) => item.id === row.id) ?? null;
  toast.success(`${row.code} 已更新为${status}`);
}

function exportRows() {
  const header = [
    '确认单号',
    '来源合同',
    '客户名称',
    '确认方式',
    '本次确认金额',
    '累计确认',
    '合同金额',
    '剩余待确认',
    '确认日期',
    '财务期间',
    '开票状态',
    '状态',
    '创建人',
    '审核人',
  ];
  const body = filteredRows.value.map((row) => [
    row.code,
    row.contract,
    row.customer,
    row.method,
    row.amount ?? '',
    row.accumulated,
    row.contractAmount,
    row.remaining,
    row.confirmedAt,
    row.period,
    row.invoiceStatus,
    row.status,
    row.creator,
    row.reviewer,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '销售收入确认.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${body.length} 条收入确认记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/revenue-recognition"
    data-page-label="销售收入确认"
    data-page-module="财务管理"
    data-revenue-recognition-page
    :data-active-status="activeStatus"
    :data-result-count="filteredRows.length"
    :data-total-count="rows.length"
    :data-selected-count="selectedIds.length"
  >
    <RevenueRecognitionHero />
    <RevenueRecognitionOverview :metrics="revenueMetrics" />
    <RevenueRecognitionWorkspace
      :rows="filteredRows"
      :status="activeStatus"
      :status-counts="statusCounts"
      :query="query"
      :start-date="startDate"
      :end-date="endDate"
      :customer="customer"
      :project="project"
      :method="method"
      :creator="creator"
      :selected-ids="selectedIds"
      @update:status="activeStatus = $event"
      @update:query="query = $event"
      @update:start-date="startDate = $event"
      @update:end-date="endDate = $event"
      @update:customer="customer = $event"
      @update:project="project = $event"
      @update:method="method = $event"
      @update:creator="creator = $event"
      @toggle="toggleRow"
      @toggle-all="toggleAll"
      @export="exportRows"
      @create="createOpen = true"
      @view="openDetail"
    />
    <RevenueRecognitionCreateDialog
      v-model:open="createOpen"
      @save="saveDraft"
    />
    <RevenueRecognitionDetailDialog
      v-model:open="detailOpen"
      :row="activeRow"
      @submit="updateStatus($event, '待审核')"
      @approve="updateStatus($event, '已审核')"
    />
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
</style>
