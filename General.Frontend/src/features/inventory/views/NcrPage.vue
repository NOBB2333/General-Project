<script setup lang="ts">
import { AlertTriangle, CheckCircle2, Download, Filter, Play, RefreshCw, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
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
import { ncrReferenceFixture } from '../model/ncr.data';
import type { NcrDraft, NcrRecord, NcrResponsibility, NcrStatus, NcrUrgency } from '../model/ncr.types';
import NcrFormDialog from './ncr/NcrFormDialog.vue';
import NcrTable from './ncr/NcrTable.vue';
import InventoryQualityHero from './shared/InventoryQualityHero.vue';

type StatusFilter = '全部' | NcrStatus;
const statusTabs: readonly { value: StatusFilter; label: string }[] = [
  { value: '全部', label: '全部 (8)' },
  { value: '待提交', label: '待提交 (1)' },
  { value: '待审批', label: '待审批 (1)' },
  { value: '待执行', label: '待执行 (1)' },
  { value: '执行中', label: '执行中 (1)' },
  { value: '已完成', label: '已完成 (3)' },
  { value: '已关闭', label: '已关闭 (1)' },
];
const urgencyOptions: UiSelectOption[] = ['全部紧急度', '普通', '紧急', '重大'].map((value) => ({
  value,
  label: value,
}));
const responsibilityOptions: UiSelectOption[] = ['全部责任方', '供应商', '内部', '运输', '待判定'].map((value) => ({
  value,
  label: value,
}));
const summaries = [
  { label: '待处理', value: 2, tone: 'warning', icon: AlertTriangle },
  { label: '执行中', value: 2, tone: 'primary', icon: Play },
  { label: '已完成', value: 3, tone: 'success', icon: CheckCircle2 },
] as const;
const rows = ref<NcrRecord[]>(cloneReferenceRows(ncrReferenceFixture.rows));
const activeStatus = ref<StatusFilter>('全部');
const urgency = ref<'全部紧急度' | NcrUrgency>('全部紧急度');
const responsibility = ref<'全部责任方' | NcrResponsibility>('全部责任方');
const query = ref('');
const selectedIds = ref<string[]>([]);
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<NcrRecord | null>(null);
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (row) =>
      (activeStatus.value === '全部' || row.status === activeStatus.value) &&
      (urgency.value === '全部紧急度' || row.urgency === urgency.value) &&
      (responsibility.value === '全部责任方' || row.responsibility === responsibility.value) &&
      (!keyword ||
        [row.id, row.inspectionCode, row.supplier].some((value) => value.toLocaleLowerCase('zh-CN').includes(keyword))),
  );
});

function reset() {
  rows.value = cloneReferenceRows(ncrReferenceFixture.rows);
  activeStatus.value = '全部';
  urgency.value = '全部紧急度';
  responsibility.value = '全部责任方';
  query.value = '';
  selectedIds.value = [];
}
function openDetails(row: NcrRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function execute(row: NcrRecord) {
  row.status = '执行中';
  openDetails(row);
}
function toggle(id: string, selected: boolean) {
  selectedIds.value = selected
    ? [...new Set([...selectedIds.value, id])]
    : selectedIds.value.filter((item) => item !== id);
}
function toggleAll(selected: boolean) {
  selectedIds.value = selected ? filteredRows.value.map((row) => row.id) : [];
}
function saveNcr(draft: NcrDraft) {
  rows.value.unshift({
    id: `NCR-2026-${String(rows.value.length + 9).padStart(4, '0')}`,
    inspectionCode: draft.inspectionCode,
    supplier: draft.supplier,
    failedQuantity: Number(draft.failedQuantity),
    urgency: draft.urgency,
    responsibility: draft.responsibility,
    resolution: draft.resolution || '复检',
    status: '待提交',
    createdAt: '2026-03-16',
  });
  activeStatus.value = '全部';
  formOpen.value = false;
}
function exportRows() {
  const header = ['处理单号', '来源检验单', '供应商', '不合格数', '紧急度', '责任方', '处理方式', '状态', '创建日期'];
  const body = filteredRows.value.map((row) => [
    row.id,
    row.inspectionCode,
    row.supplier,
    row.failedQuantity,
    row.urgency,
    row.responsibility,
    row.resolution,
    row.status,
    row.createdAt,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '不合格处理.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/ncr"
    data-page-label="不合格处理"
    data-page-module="库存管理"
    data-ncr-page
    :data-local-row-count="rows.length"
    :data-filtered-row-count="filteredRows.length"
    :data-selected-count="selectedIds.length"
  >
    <InventoryQualityHero
      title="不合格处理单"
      description="处理来料/在线/客退异常物料，共 8 条记录"
      ><template #icon
        ><AlertTriangle
          :size="22"
          :stroke-width="1.8" /></template
    ></InventoryQualityHero>
    <div :class="$style.actions">
      <UiButton
        size="sm"
        variant="secondary"
        @click="reset"
        ><RefreshCw :size="14" />刷新</UiButton
      ><UiButton
        size="sm"
        variant="secondary"
        @click="exportRows"
        ><Download :size="14" />导出</UiButton
      ><UiButton
        size="sm"
        variant="danger"
        @click="formOpen = true"
        >+ 新建处理单</UiButton
      >
    </div>
    <section :class="$style.summaryGrid">
      <article
        v-for="item in summaries"
        :key="item.label"
        :class="$style.summaryCard"
        :data-tone="item.tone"
      >
        <span
          ><component
            :is="item.icon"
            :size="15"
          />{{ item.label }}</span
        ><strong>{{ item.value }}</strong>
      </article>
    </section>
    <section :class="$style.listPanel">
      <div :class="$style.statusToolbar">
        <UiTabs v-model="activeStatus"
          ><UiTabsList label="处理单状态筛选"
            ><UiTabsTrigger
              v-for="item in statusTabs"
              :key="item.value"
              :value="item.value"
              >{{ item.label }}</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
      </div>
      <div :class="$style.filterToolbar">
        <Filter :size="15" /><span>筛选:</span
        ><UiSelect
          v-model="urgency"
          :options="urgencyOptions"
          label="紧急度"
          :class="$style.filterSelect"
        /><UiSelect
          v-model="responsibility"
          :options="responsibilityOptions"
          label="责任方"
          :class="$style.filterSelect"
        /><label :class="$style.searchField"
          ><Search :size="14" /><UiInput
            v-model="query"
            aria-label="搜索处理单"
            placeholder="搜索处理单号/检验单号/供应商..."
        /></label>
      </div>
      <NcrTable
        :rows="filteredRows"
        :selected-ids="selectedIds"
        @view="openDetails"
        @execute="execute"
        @toggle="toggle"
        @toggle-all="toggleAll"
      />
    </section>
    <NcrFormDialog
      v-model:open="formOpen"
      @save="saveNcr"
    />
    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.id ?? '处理单详情'"
      :description="activeRow ? `${activeRow.supplier} · ${activeRow.status}` : undefined"
      size="md"
      ><dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>来源检验单</dt>
          <dd>{{ activeRow.inspectionCode }}</dd>
        </div>
        <div>
          <dt>不合格数 / 紧急度</dt>
          <dd>{{ activeRow.failedQuantity }} · {{ activeRow.urgency }}</dd>
        </div>
        <div>
          <dt>责任方</dt>
          <dd>{{ activeRow.responsibility }}</dd>
        </div>
        <div>
          <dt>处理方式</dt>
          <dd>{{ activeRow.resolution }}</dd>
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
.summaryGrid {
  @apply grid grid-cols-3 gap-4;
}
.summaryCard {
  @apply flex h-[82px] flex-col justify-center rounded-lg border bg-white px-5;
  border-color: var(--ui-border);
}
.summaryCard span {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-muted);
}
.summaryCard strong {
  @apply mt-2 text-[21px] leading-none;
}
.summaryCard[data-tone='warning'] span,
.summaryCard[data-tone='warning'] strong {
  color: var(--ui-warning);
}
.summaryCard[data-tone='primary'] span,
.summaryCard[data-tone='primary'] strong {
  color: #6554c0;
}
.summaryCard[data-tone='success'] span,
.summaryCard[data-tone='success'] strong {
  color: var(--ui-success);
}
.listPanel {
  @apply mt-4 overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.statusToolbar,
.filterToolbar {
  @apply flex items-center border-b px-5;
  border-color: var(--ui-border);
}
.statusToolbar {
  @apply h-[50px];
}
.filterToolbar {
  @apply h-[60px] gap-3 text-[11px];
  color: var(--ui-text-muted);
}
.filterSelect {
  @apply w-[110px];
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
