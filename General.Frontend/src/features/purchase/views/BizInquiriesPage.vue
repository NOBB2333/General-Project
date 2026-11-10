<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiSelect, type UiSelectOption, toast } from '@/components/ui';
import { bizInquiriesReferenceFixture } from '../model/biz-inquiries.data';
import type { BizInquiryDraft, BizInquiryRow, BizInquirySortKey, BizInquiryStatus } from '../model/biz-inquiries.types';
import BizInquiryFormDialog from './biz-inquiries/BizInquiryFormDialog.vue';
import BizInquiryHero from './biz-inquiries/BizInquiryHero.vue';
import BizInquiryOverview from './biz-inquiries/BizInquiryOverview.vue';
import BizInquiryTable from './biz-inquiries/BizInquiryTable.vue';
import BizInquiryToolbar from './biz-inquiries/BizInquiryToolbar.vue';

const rows = ref<BizInquiryRow[]>(cloneReferenceRows(bizInquiriesReferenceFixture.rows));
const status = ref<'全部' | BizInquiryStatus>('全部');
const query = ref('');
const salesPerson = ref('');
const assignedBuyer = ref('');
const sortKey = ref<BizInquirySortKey | ''>('');
const sortDirection = ref<'asc' | 'desc'>('asc');
const formOpen = ref(false);
const detailOpen = ref(false);
const assignOpen = ref(false);
const activeRow = ref<BizInquiryRow | null>(null);
const selectedBuyer = ref('');
const salesOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部销售' },
  ...[...new Set(rows.value.map((row) => row.salesPerson))].map((value) => ({ value, label: value })),
]);
const buyerValues = ['王建华', '陈伟杰', '林国栋', '周天宇'];
const buyerOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部采购' },
  ...buyerValues.map((value) => ({ value, label: value })),
]);
const assignOptions: UiSelectOption[] = buyerValues.map((value) => ({ value, label: value }));
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  const result = rows.value.filter(
    (row) =>
      (status.value === '全部' || row.status === status.value) &&
      (!salesPerson.value || row.salesPerson === salesPerson.value) &&
      (!assignedBuyer.value || row.assignedBuyer === assignedBuyer.value) &&
      (!keyword ||
        row.id.toLowerCase().includes(keyword) ||
        row.title.toLowerCase().includes(keyword) ||
        row.customerName.toLowerCase().includes(keyword) ||
        row.salesPerson.includes(keyword)),
  );
  if (!sortKey.value) return result;
  return [...result].sort((a, b) => {
    const key = sortKey.value as BizInquirySortKey;
    const av = a[key],
      bv = b[key];
    const comparison =
      typeof av === 'number' && typeof bv === 'number' ? av - bv : String(av).localeCompare(String(bv), 'zh-CN');
    return sortDirection.value === 'asc' ? comparison : -comparison;
  });
});
function updateSort(key: BizInquirySortKey) {
  if (sortKey.value !== key) {
    sortKey.value = key;
    sortDirection.value = 'asc';
    return;
  }
  sortDirection.value = sortDirection.value === 'asc' ? 'desc' : 'asc';
}
function openDetail(row: BizInquiryRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function openAssign(row: BizInquiryRow) {
  activeRow.value = row;
  selectedBuyer.value = '王建华';
  assignOpen.value = true;
}
function confirmAssign() {
  if (!activeRow.value || !selectedBuyer.value) return;
  rows.value = rows.value.map((row) =>
    row.id === activeRow.value?.id ? { ...row, assignedBuyer: selectedBuyer.value, status: '已分配' } : row,
  );
  assignOpen.value = false;
  toast.success(`已分配采购：${selectedBuyer.value}`);
}
function saveInquiry(draft: BizInquiryDraft) {
  const sequence = Math.max(...rows.value.map((row) => Number(row.id.slice(-3)))) + 1;
  rows.value = [
    {
      id: `BRQ-2026-${String(sequence).padStart(3, '0')}`,
      title: draft.title.trim(),
      customerName: draft.customerName.trim(),
      salesPerson: draft.salesPerson.trim() || '张明远',
      salesDept: '销售部',
      priority: draft.priority,
      assignedBuyer: draft.assignedBuyer,
      status: draft.assignedBuyer ? '已分配' : '待分配',
      result: '待定',
      materialCount: Number(draft.materialCount) || 1,
      estimatedAmount: Number(draft.estimatedAmount) || 0,
      createdAt: new Date().toISOString().slice(0, 10),
      deadline: draft.deadline,
      note: draft.note.trim(),
      quoteMin: null,
      quoteMax: null,
      quoteCount: 0,
    },
    ...rows.value,
  ];
  formOpen.value = false;
  status.value = '全部';
  toast.success('业务询价已创建到本地列表');
}
function exportRows() {
  const header = [
    '询价单号',
    '标题',
    '客户',
    '负责销售',
    '优先级',
    '分配采购',
    '处理状态',
    '结果',
    '物料数',
    '预估金额',
    '截止日期',
  ];
  const data = filteredRows.value.map((row) => [
    row.id,
    row.title,
    row.customerName,
    row.salesPerson,
    row.priority,
    row.assignedBuyer || '-',
    row.status,
    row.result,
    row.materialCount,
    row.estimatedAmount,
    row.deadline,
  ]);
  const csv = [header, ...data]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const a = document.createElement('a');
  a.href = href;
  a.download = `业务询价_${new Date().toISOString().slice(0, 10)}.csv`;
  a.click();
  URL.revokeObjectURL(href);
  toast.success(`已导出 ${filteredRows.value.length} 条业务询价`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/purchase/biz-inquiries"
    data-biz-inquiries-page
    :data-active-status="status"
  >
    <BizInquiryHero /><BizInquiryOverview :rows="rows" />
    <BizInquiryToolbar
      v-model:status="status"
      v-model:query="query"
      v-model:sales-person="salesPerson"
      v-model:assigned-buyer="assignedBuyer"
      :rows="rows"
      :sales-options="salesOptions"
      :buyer-options="buyerOptions"
      @export="exportRows"
      @create="formOpen = true"
    />
    <BizInquiryTable
      :rows="filteredRows"
      :sort-key="sortKey"
      @sort="updateSort"
      @view="openDetail"
      @assign="openAssign"
    />
  </main>
  <BizInquiryFormDialog
    v-model:open="formOpen"
    @save="saveInquiry"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.title ?? '业务询价详情'"
    :description="activeRow?.id"
    size="lg"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>客户</dt>
        <dd>{{ activeRow.customerName }}</dd>
      </div>
      <div>
        <dt>负责销售</dt>
        <dd>{{ activeRow.salesPerson }}</dd>
      </div>
      <div>
        <dt>分配采购</dt>
        <dd>{{ activeRow.assignedBuyer || '待分配' }}</dd>
      </div>
      <div>
        <dt>处理状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>物料数</dt>
        <dd>{{ activeRow.materialCount }}</dd>
      </div>
      <div>
        <dt>预估金额</dt>
        <dd>¥{{ activeRow.estimatedAmount.toLocaleString('en-US') }}</dd>
      </div>
      <div>
        <dt>截止日期</dt>
        <dd>{{ activeRow.deadline }}</dd>
      </div>
      <div>
        <dt>反馈结果</dt>
        <dd>{{ activeRow.result }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    ></UiDialog
  >
  <UiDialog
    v-model:open="assignOpen"
    title="分配采购人员"
    :description="activeRow?.id"
    size="sm"
    ><div :class="$style.assignBody">
      <p>请选择负责该业务询价的采购人员</p>
      <UiSelect
        v-model="selectedBuyer"
        :options="assignOptions"
        label="采购人员"
      />
    </div>
    <template #footer
      ><UiButton @click="assignOpen = false">取消</UiButton
      ><UiButton
        variant="primary"
        @click="confirmAssign"
        >确认分配</UiButton
      ></template
    ></UiDialog
  >
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}
.assignBody {
  @apply space-y-3 p-5;
}
.assignBody p {
  @apply text-[12px];
  color: var(--ui-text-muted);
}
</style>
