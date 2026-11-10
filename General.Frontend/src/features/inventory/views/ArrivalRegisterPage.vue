<script setup lang="ts">
import { AlertTriangle, CheckCircle2, Clock3, Download, Search, Truck } from 'lucide-vue-next';
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
import { arrivalRegisterReferenceFixture } from '../model/arrival-register.data';
import type { ArrivalDraft, ArrivalRecord, ArrivalStatus, ArrivalType } from '../model/arrival-register.types';
import ArrivalFormDialog from './arrival-register/ArrivalFormDialog.vue';
import ArrivalRegisterTable from './arrival-register/ArrivalRegisterTable.vue';
import InventoryLogisticsHero from './shared/InventoryLogisticsHero.vue';

type StatusFilter = '全部' | ArrivalStatus;

const statusTabs: readonly StatusFilter[] = [
  '全部',
  '待检',
  '检验中',
  '免检-已入库',
  '已检验-合格',
  '已检验-不合格',
  '已入库',
];

const typeOptions: UiSelectOption[] = [
  { value: '全部', label: '全部' },
  { value: '采购到货', label: '采购到货' },
  { value: '生产到货', label: '生产到货' },
  { value: '退货到货', label: '退货到货' },
  { value: '其他到货', label: '其他到货' },
];

const rows = ref<ArrivalRecord[]>(cloneReferenceRows(arrivalRegisterReferenceFixture.rows));
const activeStatus = ref<StatusFilter>('全部');
const activeType = ref<'全部' | ArrivalType>('全部');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<ArrivalRecord | null>(null);

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');

  return rows.value.filter((row) => {
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesType = activeType.value === '全部' || row.type === activeType.value;
    const matchesQuery =
      !keyword ||
      [row.id, row.party, row.relatedCode, row.inspectionCode, row.inboundCode].some((value) =>
        value.toLocaleLowerCase('zh-CN').includes(keyword),
      );

    return matchesStatus && matchesType && matchesQuery;
  });
});

const summaries = [
  { label: '待检', value: 3, tone: 'warning', icon: Clock3 },
  { label: '检验中', value: 2, tone: 'primary', icon: Search },
  { label: '合格/已入库', value: 3, tone: 'success', icon: CheckCircle2 },
  { label: '不合格', value: 1, tone: 'danger', icon: AlertTriangle },
] as const;

function openDetails(row: ArrivalRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function saveArrival(draft: ArrivalDraft) {
  const nextNumber = rows.value.length + 1;
  rows.value.unshift({
    id: `DH-2026-${String(nextNumber).padStart(4, '0')}`,
    type: draft.type,
    date: draft.date,
    warehouse: draft.warehouse,
    party: draft.party,
    relatedCode: draft.relatedCode,
    quantity: Number(draft.quantity),
    amount: Number(draft.amount),
    status: '待检',
    inspectionCode: '-',
    inboundCode: '-',
  });
  activeStatus.value = '全部';
  activeType.value = '全部';
  query.value = '';
  formOpen.value = false;
}

function exportRows() {
  const header = [
    '到货单号',
    '类型',
    '到货日期',
    '仓库',
    '对方',
    '关联单号',
    '数量',
    '金额',
    '状态',
    '检验单',
    '入库单',
  ];
  const body = filteredRows.value.map((row) => [
    row.id,
    row.type,
    row.date,
    row.warehouse,
    row.party,
    row.relatedCode,
    row.quantity,
    row.amount,
    row.status,
    row.inspectionCode,
    row.inboundCode,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '到货登记.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/arrivals"
    data-page-label="到货登记"
    data-page-module="库存管理"
    data-arrival-register-page
    :data-local-row-count="rows.length"
    :data-filtered-row-count="filteredRows.length"
  >
    <InventoryLogisticsHero
      title="到货登记"
      description="登记到货物料，进入待检或免检流程"
    >
      <template #icon>
        <Truck
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
      >
        + 新建到货登记
      </UiButton>
    </div>

    <section
      :class="$style.summaryGrid"
      aria-label="到货状态摘要"
    >
      <article
        v-for="item in summaries"
        :key="item.label"
        :class="$style.summaryCard"
        :data-tone="item.tone"
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
      </article>
    </section>

    <section :class="$style.listPanel">
      <div :class="$style.toolbar">
        <UiTabs v-model="activeStatus">
          <UiTabsList label="到货状态筛选">
            <UiTabsTrigger
              v-for="status in statusTabs"
              :key="status"
              :value="status"
            >
              {{ status }}
            </UiTabsTrigger>
          </UiTabsList>
        </UiTabs>

        <span :class="$style.divider" />

        <UiSelect
          v-model="activeType"
          :options="typeOptions"
          label="到货类型"
          :class="$style.typeSelect"
        />

        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
          />
          <UiInput
            v-model="query"
            aria-label="搜索到货登记"
            placeholder="搜索到货单号/供应商/客户..."
          />
        </label>
      </div>

      <ArrivalRegisterTable
        :rows="filteredRows"
        @view="openDetails"
      />
    </section>

    <ArrivalFormDialog
      v-model:open="formOpen"
      @save="saveArrival"
    />

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.id ?? '到货详情'"
      :description="activeRow ? `${activeRow.type} · ${activeRow.status}` : undefined"
      size="md"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>到货日期</dt>
          <dd>{{ activeRow.date }}</dd>
        </div>
        <div>
          <dt>仓库</dt>
          <dd>{{ activeRow.warehouse }}</dd>
        </div>
        <div>
          <dt>对方</dt>
          <dd>{{ activeRow.party }}</dd>
        </div>
        <div>
          <dt>关联单号</dt>
          <dd>{{ activeRow.relatedCode }}</dd>
        </div>
        <div>
          <dt>数量</dt>
          <dd>{{ activeRow.quantity }}</dd>
        </div>
        <div>
          <dt>金额</dt>
          <dd>¥{{ activeRow.amount.toLocaleString('zh-CN') }}</dd>
        </div>
        <div>
          <dt>检验单</dt>
          <dd>{{ activeRow.inspectionCode }}</dd>
        </div>
        <div>
          <dt>入库单</dt>
          <dd>{{ activeRow.inboundCode }}</dd>
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
  @apply grid grid-cols-4 gap-4;
}

.summaryCard {
  @apply flex h-[88px] flex-col justify-center rounded-lg border bg-white px-5;
  border-color: var(--ui-border);
}

.summaryLabel {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-muted);
}

.summaryCard strong {
  @apply mt-2 text-[22px] leading-none;
  color: var(--ui-text);
}

.summaryCard[data-tone='warning'] .summaryLabel {
  color: var(--ui-warning);
}

.summaryCard[data-tone='primary'] .summaryLabel {
  color: var(--app-primary);
}

.summaryCard[data-tone='success'] .summaryLabel {
  color: var(--ui-success);
}

.summaryCard[data-tone='danger'] .summaryLabel {
  color: var(--ui-danger);
}

.listPanel {
  @apply mt-6 overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}

.toolbar {
  @apply flex h-[60px] items-center gap-3 border-b px-5;
  border-color: var(--ui-border);
}

.divider {
  @apply h-6 w-px;
  background: var(--ui-border);
}

.typeSelect {
  @apply w-[88px];
}

.searchField {
  @apply relative ml-auto block w-[225px];
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
