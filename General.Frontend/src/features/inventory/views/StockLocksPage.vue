<script setup lang="ts">
import { AlertTriangle, Download, FileText, Filter, Hand, LockKeyhole, Search, ShoppingCart } from 'lucide-vue-next';
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
import { stockLocksReferenceFixture } from '../model/stock-locks.data';
import type { StockLockDraft, StockLockRecord, StockLockStatus, StockLockType } from '../model/stock-locks.types';
import StockLockFormDialog from './stock-locks/StockLockFormDialog.vue';
import StockLocksHero from './stock-locks/StockLocksHero.vue';
import StockLocksOverview from './stock-locks/StockLocksOverview.vue';
import StockLocksTable from './stock-locks/StockLocksTable.vue';

type ViewMode = 'records' | 'overview';
type TypeFilter = '全部类型' | StockLockType;
type StatusFilter = '全部状态' | StockLockStatus;

const rows = ref<StockLockRecord[]>(cloneReferenceRows(stockLocksReferenceFixture.rows));
const activeView = ref<ViewMode>('records');
const typeFilter = ref<TypeFilter>('全部类型');
const statusFilter = ref<StatusFilter>('全部状态');
const warehouse = ref('全部仓库');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<StockLockRecord | null>(null);
const warehouseOptions: UiSelectOption[] = ['全部仓库', '上海主仓', '备品备件仓'].map((value) => ({
  value,
  label: value,
}));
const summaries = [
  { label: '总锁定数量', value: '45', hint: '涉及 12 个SKU', tone: 'primary', icon: LockKeyhole },
  { label: '订单锁库', value: '7', hint: '关联 4 个订单', tone: 'success', icon: ShoppingCart },
  { label: '手动锁库', value: '5', hint: '含预留、质检、展示', tone: 'purple', icon: Hand },
  { label: '高锁定率商品', value: '1', hint: '锁定率 > 50%', tone: 'danger', icon: AlertTriangle },
] as const;

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (row) =>
      (typeFilter.value === '全部类型' || row.type === typeFilter.value) &&
      (statusFilter.value === '全部状态' || row.status === statusFilter.value) &&
      (warehouse.value === '全部仓库' || row.warehouse === warehouse.value) &&
      (!keyword ||
        [row.lockCode, row.product, row.reasonCode, row.reasonName].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        )),
  );
});

function openDetails(row: StockLockRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function saveLock(draft: StockLockDraft) {
  const next = String(rows.value.length + 1).padStart(4, '0');
  rows.value.unshift({
    id: `lock-${next}`,
    lockCode: `LK-2026-${next}`,
    note: '',
    product: draft.product,
    specification: draft.specification || '-',
    batchCode: draft.batchCode || '-',
    type: draft.type,
    reasonCode: draft.type === '订单锁库' ? draft.reason : '手动锁定',
    reasonName: draft.reason,
    quantity: Number(draft.quantity),
    unit: '台',
    released: 0,
    status: '锁定中',
    orderStatus: draft.type === '订单锁库' ? '审批中' : '无需审批',
    lockDate: '2026-03-16',
    releaseDate: '2026-04-01',
    countdown: '16天',
    warehouse: draft.warehouse,
  });
  activeView.value = 'records';
  typeFilter.value = '全部类型';
  statusFilter.value = '全部状态';
  warehouse.value = '全部仓库';
  query.value = '';
  formOpen.value = false;
}

function exportRows() {
  const header = [
    '锁定单号',
    '商品信息',
    '批次号',
    '锁定类型',
    '关联订单/原因',
    '锁定数量',
    '已释放',
    '状态',
    '订单状态',
    '锁定日期',
    '到期释放',
  ];
  const body = filteredRows.value.map((row) => [
    row.lockCode,
    row.product,
    row.batchCode,
    row.type,
    `${row.reasonCode} ${row.reasonName}`,
    row.quantity,
    row.released,
    row.status,
    row.orderStatus,
    row.lockDate,
    row.releaseDate,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '库存锁定.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/stock-locks"
    data-page-label="库存锁定"
    data-page-module="库存管理"
    data-stock-locks-page
    :data-active-view="activeView"
    :data-local-row-count="rows.length"
    :data-filtered-row-count="filteredRows.length"
  >
    <StockLocksHero />
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
        >+ 新建锁库</UiButton
      >
    </div>
    <section :class="$style.summaryGrid">
      <article
        v-for="item in summaries"
        :key="item.label"
        :data-tone="item.tone"
      >
        <span :class="$style.summaryIcon"
          ><component
            :is="item.icon"
            :size="17"
            :stroke-width="1.8"
        /></span>
        <div>
          <span>{{ item.label }}</span
          ><strong>{{ item.value }}</strong
          ><small>{{ item.hint }}</small>
        </div>
      </article>
    </section>
    <section :class="$style.panel">
      <div :class="$style.viewToolbar">
        <UiTabs v-model="activeView"
          ><UiTabsList label="库存锁定视图"
            ><UiTabsTrigger
              value="records"
              :class="$style.viewTab"
              ><FileText
                :size="14"
                :stroke-width="1.8"
              />锁定记录</UiTabsTrigger
            ><UiTabsTrigger
              value="overview"
              :class="$style.viewTab"
              >商品锁定概况</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
        <label :class="$style.searchField"
          ><Search
            :size="14"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索库存锁定"
            placeholder="搜索商品/锁定单号/订单号..."
        /></label>
      </div>
      <template v-if="activeView === 'records'">
        <div :class="$style.filters">
          <Filter
            :size="15"
            :stroke-width="1.8"
          /><span>筛选:</span
          ><UiTabs v-model="typeFilter"
            ><UiTabsList label="锁定类型"
              ><UiTabsTrigger
                v-for="item in ['全部类型', '订单锁库', '手动锁库']"
                :key="item"
                :value="item"
                >{{ item }}</UiTabsTrigger
              ></UiTabsList
            ></UiTabs
          ><UiTabs v-model="statusFilter"
            ><UiTabsList label="锁定状态"
              ><UiTabsTrigger
                v-for="item in ['全部状态', '锁定中', '部分释放', '已释放']"
                :key="item"
                :value="item"
                >{{ item }}</UiTabsTrigger
              ></UiTabsList
            ></UiTabs
          ><UiSelect
            v-model="warehouse"
            :options="warehouseOptions"
            label="仓库"
            :class="$style.warehouseSelect"
          /><span :class="$style.recordCount">{{ filteredRows.length }} 条记录</span>
        </div>
        <StockLocksTable
          :rows="filteredRows"
          @view="openDetails"
        />
      </template>
      <StockLocksOverview
        v-else
        :rows="filteredRows"
      />
    </section>

    <StockLockFormDialog
      v-model:open="formOpen"
      @save="saveLock"
    />
    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.lockCode ?? '锁定详情'"
      :description="activeRow ? `${activeRow.product} · ${activeRow.status}` : undefined"
      size="lg"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>商品规格</dt>
          <dd>{{ activeRow.specification }}</dd>
        </div>
        <div>
          <dt>批次 / 仓库</dt>
          <dd>{{ activeRow.batchCode }} · {{ activeRow.warehouse }}</dd>
        </div>
        <div>
          <dt>锁定类型</dt>
          <dd>{{ activeRow.type }}</dd>
        </div>
        <div>
          <dt>锁定 / 已释放</dt>
          <dd>{{ activeRow.quantity }}{{ activeRow.unit }} · {{ activeRow.released }}</dd>
        </div>
        <div>
          <dt>关联订单/原因</dt>
          <dd>{{ activeRow.reasonCode }} · {{ activeRow.reasonName }}</dd>
        </div>
        <div>
          <dt>状态 / 审批</dt>
          <dd>{{ activeRow.status }} · {{ activeRow.orderStatus }}</dd>
        </div>
        <div>
          <dt>锁定日期</dt>
          <dd>{{ activeRow.lockDate }}</dd>
        </div>
        <div>
          <dt>到期释放</dt>
          <dd>{{ activeRow.releaseDate }} · {{ activeRow.countdown }}</dd>
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
  @apply flex h-[62px] items-center justify-end gap-2;
}
.summaryGrid {
  @apply grid grid-cols-4 gap-4;
}
.summaryGrid article {
  @apply flex h-[116px] items-start gap-3 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.summaryIcon {
  @apply flex h-8 w-8 shrink-0 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.summaryGrid article[data-tone='success'] .summaryIcon {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.summaryGrid article[data-tone='purple'] .summaryIcon {
  color: #6554c0;
  background: #eae6ff;
}
.summaryGrid article[data-tone='danger'] .summaryIcon {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.summaryGrid article > div > span {
  @apply block text-[11px];
  color: var(--ui-text-muted);
}
.summaryGrid strong {
  @apply mt-3 block text-[23px] leading-none;
}
.summaryGrid small {
  @apply mt-3 block text-[10px];
  color: var(--ui-text-subtle);
}
.panel {
  @apply mt-4 overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.viewToolbar {
  @apply flex h-[64px] items-center border-b px-5;
  border-color: var(--ui-border);
}
.viewTab {
  @apply border;
  border-color: transparent;
}
.viewTab[data-state='active'] {
  border-color: var(--ui-border);
  background: white;
  box-shadow: var(--ui-shadow-sm);
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
.filters {
  @apply flex h-[60px] items-center gap-2 border-b px-4 text-[10px];
  color: var(--ui-text-muted);
  border-color: var(--ui-border);
}
.warehouseSelect {
  @apply ml-2 w-[128px];
}
.recordCount {
  @apply ml-auto;
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
