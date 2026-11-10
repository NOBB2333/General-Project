<script setup lang="ts">
import { BarChart3, CalendarDays, Download, ListFilter, Search } from 'lucide-vue-next';
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
import { stockLedgerReferenceFixture } from '../model/stock-ledger.data';
import type { StockLedgerDirection, StockLedgerRecord } from '../model/stock-ledger.types';
import StockLedgerHero from './stock-ledger/StockLedgerHero.vue';
import StockLedgerReport from './stock-ledger/StockLedgerReport.vue';
import StockLedgerTable from './stock-ledger/StockLedgerTable.vue';

type PageView = 'ledger' | 'detail' | 'report';
type DirectionFilter = '全部' | StockLedgerDirection;

const rows = ref<StockLedgerRecord[]>(cloneReferenceRows(stockLedgerReferenceFixture.rows));
const activeView = ref<PageView>('ledger');
const direction = ref<DirectionFilter>('全部');
const startDate = ref('');
const endDate = ref('');
const warehouse = ref('全部仓库');
const query = ref('');
const detailOpen = ref(false);
const activeRow = ref<StockLedgerRecord | null>(null);
const warehouseOptions: UiSelectOption[] = ['全部仓库', '上海主仓', '备品备件仓'].map((value) => ({
  value,
  label: value,
}));

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (row) =>
      (direction.value === '全部' || row.direction === direction.value) &&
      (!startDate.value || row.date >= startDate.value) &&
      (!endDate.value || row.date <= endDate.value) &&
      (warehouse.value === '全部仓库' || row.warehouse === warehouse.value) &&
      (!keyword ||
        [row.flowCode, row.sourceCode, row.counterparty, row.businessType].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        )),
  );
});

const summaries = [
  { label: '入库笔数', value: '15', tone: 'success' },
  { label: '出库笔数', value: '10', tone: 'danger' },
  { label: '入库总额', value: '¥106.1万', tone: 'neutral' },
  { label: '出库总额', value: '¥53.2万', tone: 'neutral' },
] as const;

function openDetails(row: StockLedgerRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function exportRows() {
  const header = ['流水号', '类型', '业务类型', '日期', '仓库', '操作人', '数量', '金额', '状态', '关联单号', '对方'];
  const body = filteredRows.value.map((row) => [
    row.flowCode,
    row.direction,
    row.businessType,
    row.date,
    row.warehouse,
    row.operator,
    row.quantity,
    row.amount,
    row.status,
    row.sourceCode,
    row.counterparty,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '库存流水.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/stock-ledger"
    data-page-label="库存流水"
    data-page-module="库存管理"
    data-stock-ledger-page
    :data-active-view="activeView"
    :data-filtered-row-count="filteredRows.length"
  >
    <StockLedgerHero />
    <div :class="$style.actions">
      <UiButton
        size="sm"
        variant="secondary"
        @click="exportRows"
        ><Download
          :size="14"
          :stroke-width="1.8"
        />导出</UiButton
      >
    </div>
    <section :class="$style.panel">
      <div :class="$style.viewTabs">
        <UiTabs v-model="activeView"
          ><UiTabsList label="库存流水视图"
            ><UiTabsTrigger
              value="ledger"
              :class="$style.viewTab"
              ><ListFilter
                :size="14"
                :stroke-width="1.8"
              />库存流水</UiTabsTrigger
            ><UiTabsTrigger
              value="detail"
              :class="$style.viewTab"
              ><CalendarDays
                :size="14"
                :stroke-width="1.8"
              />流水明细</UiTabsTrigger
            ><UiTabsTrigger
              value="report"
              :class="$style.viewTab"
              ><BarChart3
                :size="14"
                :stroke-width="1.8"
              />报表</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
      </div>

      <template v-if="activeView !== 'report'">
        <div
          v-if="activeView === 'ledger'"
          :class="$style.summaryGrid"
        >
          <article
            v-for="item in summaries"
            :key="item.label"
            :data-tone="item.tone"
          >
            <span>{{ item.label }}</span
            ><strong>{{ item.value }}</strong>
          </article>
        </div>
        <div
          v-else
          :class="$style.detailIntro"
        >
          <div><strong>流水明细</strong><span>查看每笔业务引起的库存数量与金额变化</span></div>
          <b>{{ filteredRows.length }} 条</b>
        </div>
        <div :class="$style.filters">
          <UiTabs v-model="direction"
            ><UiTabsList label="出入库方向"
              ><UiTabsTrigger
                v-for="item in ['全部', '入库', '出库']"
                :key="item"
                :value="item"
                >{{ item }}</UiTabsTrigger
              ></UiTabsList
            ></UiTabs
          >
          <span :class="$style.divider" />
          <CalendarDays
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            v-model="startDate"
            type="date"
            aria-label="开始日期"
            :class="$style.dateInput"
          />
          <span>至</span>
          <UiInput
            v-model="endDate"
            type="date"
            aria-label="结束日期"
            :class="$style.dateInput"
          />
          <UiSelect
            v-model="warehouse"
            :options="warehouseOptions"
            label="仓库"
            :class="$style.warehouseSelect"
          />
          <label :class="$style.searchField"
            ><Search
              :size="14"
              :stroke-width="1.8" /><UiInput
              v-model="query"
              aria-label="搜索库存流水"
              placeholder="搜索流水号/关联单号/客户..."
          /></label>
        </div>
        <StockLedgerTable
          :rows="filteredRows"
          @view="openDetails"
        />
      </template>
      <StockLedgerReport
        v-else
        :rows="filteredRows"
      />
    </section>

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.flowCode ?? '流水详情'"
      :description="activeRow ? `${activeRow.businessType} · ${activeRow.status}` : undefined"
      size="md"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>出入库 / 数量</dt>
          <dd>{{ activeRow.direction }} · {{ activeRow.quantity }}</dd>
        </div>
        <div>
          <dt>日期 / 仓库</dt>
          <dd>{{ activeRow.date }} · {{ activeRow.warehouse }}</dd>
        </div>
        <div>
          <dt>操作人</dt>
          <dd>{{ activeRow.operator }}</dd>
        </div>
        <div>
          <dt>金额</dt>
          <dd>¥{{ Math.abs(activeRow.amount).toLocaleString('zh-CN') }}</dd>
        </div>
        <div>
          <dt>关联单号</dt>
          <dd>{{ activeRow.sourceCode }}</dd>
        </div>
        <div>
          <dt>对方</dt>
          <dd>{{ activeRow.counterparty }}</dd>
        </div>
      </dl>
      <template #footer
        ><UiButton
          size="sm"
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        ></template
      >
    </UiDialog>
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
  @apply flex h-[62px] items-center justify-end;
}
.panel {
  @apply overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.viewTabs {
  @apply flex h-[54px] items-end border-b px-4;
  border-color: var(--ui-border);
}
.viewTab {
  @apply min-h-[43px] rounded-none border-b-2 border-transparent px-4;
}
.viewTab[data-state='active'] {
  border-color: var(--app-primary);
  background: transparent;
}
.summaryGrid {
  @apply grid grid-cols-4 gap-4 border-b p-4;
  border-color: var(--ui-border-subtle);
}
.summaryGrid article {
  @apply h-[76px] rounded border px-3 py-3;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.summaryGrid article[data-tone='success'] {
  border-color: #abf5d1;
  background: var(--ui-success-soft);
}
.summaryGrid article[data-tone='danger'] {
  border-color: #ffbdad;
  background: var(--ui-danger-soft);
}
.summaryGrid span {
  @apply text-[11px];
  color: var(--ui-text-muted);
}
.summaryGrid article[data-tone='success'] span,
.summaryGrid article[data-tone='success'] strong {
  color: var(--ui-success);
}
.summaryGrid article[data-tone='danger'] span,
.summaryGrid article[data-tone='danger'] strong {
  color: var(--ui-danger);
}
.summaryGrid strong {
  @apply mt-2 block text-[21px] leading-none;
}
.detailIntro {
  @apply flex h-[76px] items-center justify-between border-b px-5;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}
.detailIntro strong {
  @apply block text-[13px];
}
.detailIntro span {
  @apply mt-1 block text-[10px];
  color: var(--ui-text-muted);
}
.detailIntro b {
  @apply text-[18px];
  color: var(--app-primary);
}
.filters {
  @apply flex h-[60px] items-center gap-3 border-b px-4 text-[11px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.divider {
  @apply h-5 w-px;
  background: var(--ui-border);
}
.dateInput {
  @apply w-[145px];
}
.warehouseSelect {
  @apply w-[135px];
}
.searchField {
  @apply relative ml-auto block w-[270px];
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
