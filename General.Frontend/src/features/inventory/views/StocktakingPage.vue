<script setup lang="ts">
import { ClipboardCheck, Download, Filter, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows, type PageStat } from '@/components/page';
import {
  UiButton,
  UiDialog,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  type UiSelectOption,
} from '@/components/ui';
import { stocktakingReferenceFixture } from '../model/stocktaking.data';
import type {
  StocktakingDraft,
  StocktakingRecord,
  StocktakingStatus,
  StocktakingType,
} from '../model/stocktaking.types';
import StocktakingFormDialog from './stocktaking/StocktakingFormDialog.vue';
import StocktakingHero from './stocktaking/StocktakingHero.vue';
import StocktakingTable from './stocktaking/StocktakingTable.vue';

type StatusFilter = '全部' | StocktakingStatus;
type TypeFilter = '全部类型' | StocktakingType;

const rows = ref<StocktakingRecord[]>(cloneReferenceRows(stocktakingReferenceFixture.rows));
const activeStatus = ref<StatusFilter>('全部');
const typeFilter = ref<TypeFilter>('全部类型');
const warehouse = ref('全部仓库');
const startDate = ref('');
const endDate = ref('');
const query = ref('');
const selectedIds = ref<string[]>([]);
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<StocktakingRecord | null>(null);

const warehouseOptions: UiSelectOption[] = [
  '全部仓库',
  '上海主仓',
  '备品备件仓',
  '华能DCS项目仓',
  '中石化仪控项目仓',
].map((value) => ({ value, label: value }));
const typeOptions: UiSelectOption[] = ['全部类型', '全盘', '抽盘', '清盘'].map((value) => ({ value, label: value }));
const statusTabs: readonly { value: StatusFilter; label: string }[] = [
  { value: '全部', label: '全部 (11)' },
  { value: '计划中', label: '计划中 (1)' },
  { value: '盘点中', label: '盘点中 (1)' },
  { value: '待审核', label: '待审核 (1)' },
  { value: '已完成', label: '已完成 (7)' },
  { value: '已作废', label: '已作废 (1)' },
];
const summaries: readonly Pick<PageStat, 'label' | 'value' | 'tone' | 'hint'>[] = [
  { label: '计划中', value: '1', tone: 'neutral' },
  { label: '盘点中', value: '1', tone: 'primary' },
  { label: '待审核', value: '1', tone: 'warning' },
  { label: '已完成', value: '7', tone: 'success' },
  { label: '累计盘差金额', value: '-¥3,180', hint: '涉及 9 个差异项', tone: 'danger' },
] as const;

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (row) =>
      (activeStatus.value === '全部' || row.status === activeStatus.value) &&
      (typeFilter.value === '全部类型' || row.type === typeFilter.value) &&
      (warehouse.value === '全部仓库' || row.warehouse === warehouse.value) &&
      (!startDate.value || row.plannedDate >= startDate.value) &&
      (!endDate.value || row.plannedDate <= endDate.value) &&
      (!keyword ||
        [row.code, row.name, row.warehouse].some((value) => value.toLocaleLowerCase('zh-CN').includes(keyword))),
  );
});

function toggle(id: string, selected: boolean) {
  selectedIds.value = selected
    ? [...new Set([...selectedIds.value, id])]
    : selectedIds.value.filter((item) => item !== id);
}
function toggleAll(selected: boolean) {
  selectedIds.value = selected ? filteredRows.value.map((row) => row.id) : [];
}
function openDetails(row: StocktakingRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function saveStocktaking(draft: StocktakingDraft) {
  const next = String(rows.value.length + 2).padStart(4, '0');
  rows.value.unshift({
    id: `stocktake-${next}`,
    code: `IC-2026-${next}`,
    attachmentCount: 0,
    name: draft.name,
    description: draft.description || '新建盘点计划',
    warehouse: draft.warehouse,
    type: draft.type,
    plannedDate: draft.plannedDate,
    actualDate: '-',
    checked: 0,
    total: draft.type === '全盘' ? 86 : 12,
    differences: 0,
    differenceAmount: 0,
    status: '计划中',
    accountLocked: true,
    lockDeadline: `${draft.plannedDate} 00:00`,
    operator: '孙强',
  });
  activeStatus.value = '全部';
  typeFilter.value = '全部类型';
  warehouse.value = '全部仓库';
  query.value = '';
  formOpen.value = false;
}
function exportRows() {
  const header = [
    '盘点单号',
    '盘点名称',
    '仓库',
    '类型',
    '计划日期',
    '实际日期',
    '进度',
    '差异',
    '盘差',
    '状态',
    '锁账',
    '操作人',
  ];
  const body = filteredRows.value.map((row) => [
    row.code,
    row.name,
    row.warehouse,
    row.type,
    row.plannedDate,
    row.actualDate,
    `${row.checked}/${row.total}`,
    row.differences,
    row.differenceAmount,
    row.status,
    row.accountLocked ? '已锁账' : '已解锁',
    row.operator,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '库存盘点.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/stocktaking"
    data-page-label="库存盘点"
    data-page-module="库存管理"
    data-stocktaking-page
    :data-local-row-count="rows.length"
    :data-filtered-row-count="filteredRows.length"
    :data-selected-count="selectedIds.length"
  >
    <StocktakingHero />
    <div :class="$style.actions">
      <UiButton
        size="sm"
        variant="secondary"
        @click="exportRows"
        ><Download
          :size="14"
          :stroke-width="1.8"
        />导出</UiButton
      ><UiButton
        size="sm"
        variant="primary"
        @click="formOpen = true"
        >+ 新建盘点</UiButton
      >
    </div>
    <section :class="$style.notice">
      <span
        ><ClipboardCheck
          :size="18"
          :stroke-width="1.8"
      /></span>
      <div>
        <strong>盘点提醒：有 1 个盘点计划待执行</strong>
        <p>请确保盘点前完成所有待入库和待出库操作，以保证盘点数据准确性。</p>
      </div>
    </section>
    <section :class="$style.summaryGrid">
      <article
        v-for="item in summaries"
        :key="item.label"
        :data-tone="item.tone"
      >
        <span>{{ item.label }}</span
        ><strong>{{ item.value }}</strong
        ><small v-if="item.hint">{{ item.hint }}</small>
      </article>
    </section>
    <section :class="$style.panel">
      <div :class="$style.statusToolbar">
        <UiTabs v-model="activeStatus"
          ><UiTabsList label="盘点状态筛选"
            ><UiTabsTrigger
              v-for="item in statusTabs"
              :key="item.value"
              :value="item.value"
              :class="$style.statusTab"
              >{{ item.label }}</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
      </div>
      <div :class="$style.filters">
        <Filter
          :size="15"
          :stroke-width="1.8"
        /><span>筛选:</span
        ><UiSelect
          v-model="typeFilter"
          :options="typeOptions"
          label="盘点类型"
          :class="$style.typeSelect"
        /><UiSelect
          v-model="warehouse"
          :options="warehouseOptions"
          label="盘点仓库"
          :class="$style.warehouseSelect"
        /><UiInput
          v-model="startDate"
          type="date"
          aria-label="计划开始日期"
          :class="$style.dateInput"
        /><span>至</span
        ><UiInput
          v-model="endDate"
          type="date"
          aria-label="计划结束日期"
          :class="$style.dateInput"
        /><label :class="$style.searchField"
          ><Search
            :size="14"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索库存盘点"
            placeholder="搜索盘点单号/名称/仓库..." /></label
        ><span :class="$style.recordCount">{{ filteredRows.length }} 条记录</span>
      </div>
      <StocktakingTable
        :rows="filteredRows"
        :selected-ids="selectedIds"
        @view="openDetails"
        @toggle="toggle"
        @toggle-all="toggleAll"
      />
    </section>
    <StocktakingFormDialog
      v-model:open="formOpen"
      @save="saveStocktaking"
    />
    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.code ?? '盘点详情'"
      :description="activeRow ? `${activeRow.name} · ${activeRow.status}` : undefined"
      size="lg"
      ><dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>仓库 / 类型</dt>
          <dd>{{ activeRow.warehouse }} · {{ activeRow.type }}</dd>
        </div>
        <div>
          <dt>计划 / 实际日期</dt>
          <dd>{{ activeRow.plannedDate }} · {{ activeRow.actualDate }}</dd>
        </div>
        <div>
          <dt>盘点进度</dt>
          <dd>{{ activeRow.checked }}/{{ activeRow.total }}</dd>
        </div>
        <div>
          <dt>差异 / 盘差</dt>
          <dd>{{ activeRow.differences || '-' }} · {{ activeRow.differenceAmount || '-' }}</dd>
        </div>
        <div>
          <dt>锁账状态</dt>
          <dd>{{ activeRow.accountLocked ? '已锁账' : '已解锁' }} · {{ activeRow.lockDeadline }}</dd>
        </div>
        <div>
          <dt>操作人</dt>
          <dd>{{ activeRow.operator }}</dd>
        </div>
      </dl>
      <template #footer
        ><UiButton
          size="sm"
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        ></template
      ></UiDialog
    >
  </main>
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply min-h-full min-w-[1180px] pb-8;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}
.actions {
  @apply flex h-[62px] items-center justify-end gap-2;
}
.notice {
  @apply flex h-[66px] items-center gap-3 rounded border px-4;
  color: var(--app-primary);
  border-color: #b3d4ff;
  background: var(--app-primary-soft);
}
.notice > span {
  @apply flex h-8 w-8 items-center justify-center rounded text-white;
  background: var(--app-primary);
}
.notice strong {
  @apply text-[12px];
}
.notice p {
  @apply m-0 mt-1 text-[10px];
  color: #4c9aff;
}
.summaryGrid {
  @apply mt-4 grid grid-cols-5 gap-3;
}
.summaryGrid article {
  @apply h-[96px] rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.summaryGrid span {
  @apply text-[10px];
  color: var(--ui-text-muted);
}
.summaryGrid strong {
  @apply mt-3 block text-[22px] leading-none;
}
.summaryGrid small {
  @apply mt-3 block text-[9px];
  color: var(--ui-text-subtle);
}
.summaryGrid article[data-tone='primary'] strong {
  color: var(--app-primary);
}
.summaryGrid article[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.summaryGrid article[data-tone='success'] strong {
  color: var(--ui-success);
}
.summaryGrid article[data-tone='danger'] {
  border-color: #ffbdad;
}
.summaryGrid article[data-tone='danger'] strong {
  color: var(--ui-danger);
}
.panel {
  @apply mt-4 overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.statusToolbar {
  @apply flex h-[52px] items-end border-b px-4;
  border-color: var(--ui-border);
}
.statusTab {
  @apply min-h-[42px] rounded-none border-b-2 border-transparent px-4;
}
.statusTab[data-state='active'] {
  border-color: var(--app-primary);
  background: transparent;
}
.filters {
  @apply flex h-[64px] items-center gap-2 border-b px-4 text-[10px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.typeSelect {
  @apply w-[110px];
}
.warehouseSelect {
  @apply w-[150px];
}
.dateInput {
  @apply w-[140px];
}
.searchField {
  @apply relative ml-auto block w-[230px];
}
.searchField > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.searchField input {
  @apply pl-9;
}
.recordCount {
  @apply whitespace-nowrap;
  color: var(--ui-text-subtle);
}
.detailGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
.detailGrid div {
  @apply rounded border p-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.detailGrid dd {
  @apply m-0 mt-1 text-[12px] font-medium;
}
</style>
