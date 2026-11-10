<script setup lang="ts">
import {
  Archive,
  CheckCircle2,
  Clock3,
  Download,
  FileText,
  PackageCheck,
  PackageOpen,
  RotateCcw,
  Search,
  ShoppingCart,
  Wrench,
} from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { inboundReferenceFixture } from '../model/inbound.data';
import type { InboundDraft, InboundRecord, InboundStatus, InboundType } from '../model/inbound.types';
import InboundFormDialog from './inbound/InboundFormDialog.vue';
import InboundTable from './inbound/InboundTable.vue';
import InventoryLogisticsHero from './shared/InventoryLogisticsHero.vue';

type SourceFilter = '全部' | InboundType | '入库明细';
type StatusFilter = '全部' | InboundStatus;

const sourceTabs: readonly { value: SourceFilter; label: string; count: number }[] = [
  { value: '全部', label: '全部', count: 21 },
  { value: '期初入库', label: '期初入库', count: 3 },
  { value: '采购入库', label: '采购入库', count: 7 },
  { value: '生产入库', label: '生产入库', count: 3 },
  { value: '退货入库', label: '退货入库', count: 2 },
  { value: '其他入库', label: '其他入库', count: 6 },
  { value: '入库明细', label: '入库明细', count: 44 },
];
const statusTabs: readonly StatusFilter[] = ['全部', '草稿', '待审批', '已审批', '已入库', '已驳回', '已作废'];
const summaries = [
  { label: '期初入库', value: 3, filter: '期初入库', tone: 'initial', icon: Archive },
  { label: '采购入库', value: 7, filter: '采购入库', tone: 'primary', icon: ShoppingCart },
  { label: '生产入库', value: 3, filter: '生产入库', tone: 'success', icon: Wrench },
  { label: '退货入库', value: 2, filter: '退货入库', tone: 'danger', icon: RotateCcw },
  { label: '其他入库', value: 6, filter: '其他入库', tone: 'purple', icon: PackageOpen },
  { label: '待审批', value: 4, filter: '待审批', tone: 'warning', icon: Clock3 },
  { label: '已入库', value: 13, filter: '已入库', tone: 'success', icon: CheckCircle2 },
] as const;

const rows = ref<InboundRecord[]>(cloneReferenceRows(inboundReferenceFixture.rows));
const activeSource = ref<SourceFilter>('全部');
const activeStatus = ref<StatusFilter>('全部');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<InboundRecord | null>(null);

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesSource =
      activeSource.value === '全部' || activeSource.value === '入库明细' || row.type === activeSource.value;
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !keyword ||
      [row.id, row.batchCode, row.arrivalCode, row.supplier, row.remark].some((value) =>
        value.toLocaleLowerCase('zh-CN').includes(keyword),
      );
    return matchesSource && matchesStatus && matchesQuery;
  });
});

function selectSummary(filter: string) {
  if (filter === '待审批' || filter === '已入库') {
    activeSource.value = '全部';
    activeStatus.value = filter;
    return;
  }
  activeSource.value = filter as InboundType;
  activeStatus.value = '全部';
}

function openDetails(row: InboundRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function saveInbound(draft: InboundDraft) {
  rows.value.unshift({
    id: `RK-2026-${String(rows.value.length + 1).padStart(4, '0')}`,
    source: '手工入库',
    batchCode: '-',
    attachmentCount: 0,
    arrivalCode: '-',
    supplier: draft.supplier,
    type: draft.type,
    status: '待审批',
    date: draft.date,
    warehouse: draft.warehouse,
    quantity: Number(draft.quantity),
    amount: Number(draft.amount),
    handler: '张经理',
    remark: draft.remark || '-',
  });
  activeSource.value = '全部';
  activeStatus.value = '全部';
  query.value = '';
  formOpen.value = false;
}

function exportRows() {
  const header = [
    '入库单号',
    '入库来源',
    '到货登记单号',
    '供应商',
    '类型',
    '状态',
    '入库日期',
    '仓库',
    '数量',
    '金额',
    '经办人',
    '备注',
  ];
  const body = filteredRows.value.map((row) => [
    row.id,
    row.source,
    row.arrivalCode,
    row.supplier,
    row.type,
    row.status,
    row.date,
    row.warehouse,
    row.quantity,
    row.amount,
    row.handler,
    row.remark,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '入库管理.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/inbound"
    data-page-label="入库管理"
    data-page-module="库存管理"
    data-inbound-page
    :data-local-row-count="rows.length"
    :data-filtered-row-count="filteredRows.length"
  >
    <InventoryLogisticsHero
      title="入库单"
      description="管理所有入库单据"
    >
      <template #icon>
        <PackageCheck
          :size="22"
          :stroke-width="1.8"
        />
      </template>
    </InventoryLogisticsHero>

    <div :class="$style.actions">
      <UiButton
        size="sm"
        variant="secondary"
        @click="exportRows"
      >
        <Download
          :size="14"
          :stroke-width="1.8"
        />
        导出
      </UiButton>
      <UiButton
        size="sm"
        variant="primary"
        @click="formOpen = true"
        >+ 新建入库</UiButton
      >
    </div>

    <section
      :class="$style.summaryGrid"
      aria-label="入库业务摘要"
    >
      <button
        v-for="item in summaries"
        :key="item.label"
        :class="$style.summaryCard"
        :data-tone="item.tone"
        :aria-pressed="activeSource === item.filter || activeStatus === item.filter"
        @click="selectSummary(item.filter)"
      >
        <span :class="$style.summaryLabel">
          <component
            :is="item.icon"
            :size="15"
            :stroke-width="1.8"
          />
          {{ item.label }}
        </span>
        <strong>{{ item.value }}</strong>
      </button>
    </section>

    <section :class="$style.listPanel">
      <div :class="$style.sourceToolbar">
        <UiTabs v-model="activeSource">
          <UiTabsList label="入库来源视图">
            <UiTabsTrigger
              v-for="item in sourceTabs"
              :key="item.value"
              :value="item.value"
            >
              <FileText
                v-if="item.value === '入库明细'"
                :size="13"
                :stroke-width="1.8"
              />
              {{ item.label }}
              <span :class="$style.count">{{ item.count }}</span>
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>
      </div>

      <div :class="$style.filterToolbar">
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            v-model="query"
            aria-label="搜索入库单"
            placeholder="搜索入库单号、供应商、客户、批次号..."
          />
        </label>

        <UiTabs v-model="activeStatus">
          <UiTabsList label="入库状态筛选">
            <UiTabsTrigger
              v-for="status in statusTabs"
              :key="status"
              :value="status"
              >{{ status }}</UiTabsTrigger
            >
          </UiTabsList>
        </UiTabs>
      </div>

      <InboundTable
        :rows="filteredRows"
        @view="openDetails"
      />
    </section>

    <InboundFormDialog
      v-model:open="formOpen"
      @save="saveInbound"
    />

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.id ?? '入库单详情'"
      :description="activeRow ? `${activeRow.type} · ${activeRow.status}` : undefined"
      size="md"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>入库来源</dt>
          <dd>{{ activeRow.source }}</dd>
        </div>
        <div>
          <dt>到货登记单号</dt>
          <dd>{{ activeRow.arrivalCode }}</dd>
        </div>
        <div>
          <dt>供应商</dt>
          <dd>{{ activeRow.supplier }}</dd>
        </div>
        <div>
          <dt>仓库</dt>
          <dd>{{ activeRow.warehouse }}</dd>
        </div>
        <div>
          <dt>数量 / 金额</dt>
          <dd>{{ activeRow.quantity }} / ¥{{ activeRow.amount.toLocaleString('zh-CN') }}</dd>
        </div>
        <div>
          <dt>经办信息</dt>
          <dd>{{ activeRow.date }} · {{ activeRow.handler }}</dd>
        </div>
        <div :class="$style.detailWide">
          <dt>备注</dt>
          <dd>{{ activeRow.remark }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          size="sm"
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        >
      </template>
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
  @apply grid grid-cols-7 gap-3;
}

.summaryCard {
  @apply flex h-[84px] flex-col justify-center rounded-lg border bg-white px-4 text-left outline-none transition-colors;
  border-color: var(--ui-border);
}

.summaryCard:hover,
.summaryCard:focus-visible,
.summaryCard[aria-pressed='true'] {
  border-color: var(--app-primary);
}

.summaryCard[data-tone='initial'] {
  border-color: #ffcf57;
  background: #fffaf0;
}

.summaryLabel {
  @apply flex items-center gap-2 text-[11px] font-medium;
  color: var(--ui-text-muted);
}

.summaryCard strong {
  @apply mt-2 text-[21px] leading-none;
}

.summaryCard[data-tone='initial'] .summaryLabel,
.summaryCard[data-tone='initial'] strong {
  color: #a54800;
}

.summaryCard[data-tone='primary'] .summaryLabel,
.summaryCard[data-tone='primary'] strong {
  color: var(--app-primary);
}

.summaryCard[data-tone='success'] .summaryLabel,
.summaryCard[data-tone='success'] strong {
  color: var(--ui-success);
}

.summaryCard[data-tone='danger'] .summaryLabel,
.summaryCard[data-tone='danger'] strong {
  color: var(--ui-danger);
}

.summaryCard[data-tone='warning'] .summaryLabel,
.summaryCard[data-tone='warning'] strong {
  color: var(--ui-warning);
}

.summaryCard[data-tone='purple'] .summaryLabel,
.summaryCard[data-tone='purple'] strong {
  color: #6554c0;
}

.listPanel {
  @apply mt-4 overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}

.sourceToolbar,
.filterToolbar {
  @apply flex items-center border-b px-5;
  border-color: var(--ui-border);
}

.sourceToolbar {
  @apply h-[50px];
}

.filterToolbar {
  @apply h-[64px] gap-3;
}

.count {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px];
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}

.searchField {
  @apply relative block w-[320px];
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

.detailWide {
  @apply col-span-2;
}
</style>
