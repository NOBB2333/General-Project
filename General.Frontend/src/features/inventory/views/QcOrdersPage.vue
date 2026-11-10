<script setup lang="ts">
import { ClipboardCheck, Download, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { qcOrdersReferenceFixture } from '../model/qc-orders.data';
import type { QcOrderDraft, QcOrderRecord, QcOrderStatus } from '../model/qc-orders.types';
import QcOrderFormDialog from './qc-orders/QcOrderFormDialog.vue';
import QcOrdersTable from './qc-orders/QcOrdersTable.vue';
import InventoryQualityHero from './shared/InventoryQualityHero.vue';

type StatusFilter = '全部' | QcOrderStatus;
const statuses: readonly StatusFilter[] = ['全部', '待检验', '检验中', '待审核', '已完成', '已关闭'];
const rows = ref<QcOrderRecord[]>(cloneReferenceRows(qcOrdersReferenceFixture.rows));
const activeStatus = ref<StatusFilter>('全部');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<QcOrderRecord | null>(null);
const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter(
    (row) =>
      (activeStatus.value === '全部' || row.status === activeStatus.value) &&
      (!keyword ||
        [row.id, row.material, row.supplier, row.sourceCode].some((value) =>
          value.toLocaleLowerCase('zh-CN').includes(keyword),
        )),
  );
});

function openDetails(row: QcOrderRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}
function saveOrder(draft: QcOrderDraft) {
  rows.value.unshift({
    id: `IQC-2026-${String(rows.value.length + 22).padStart(4, '0')}`,
    material: draft.material,
    specification: draft.specification || '-',
    quantity: Number(draft.quantity),
    supplier: draft.supplier,
    sourceCode: draft.sourceCode,
    arrivalDate: '2026-03-18',
    status: '待检验',
    result: '-',
    inspector: '李质检',
  });
  activeStatus.value = '全部';
  query.value = '';
  formOpen.value = false;
}
function exportRows() {
  const header = [
    '检验单号',
    '物料名称',
    '规格型号',
    '数量',
    '供应商',
    '来源单号',
    '到货日期',
    '状态',
    '检验结果',
    '检验员',
  ];
  const body = filteredRows.value.map((row) => [
    row.id,
    row.material,
    row.specification,
    row.quantity,
    row.supplier,
    row.sourceCode,
    row.arrivalDate,
    row.status,
    row.result,
    row.inspector,
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '检验单.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/qc-orders"
    data-page-label="检验单"
    data-page-module="库存管理"
    data-qc-orders-page
    :data-local-row-count="rows.length"
    :data-filtered-row-count="filteredRows.length"
  >
    <InventoryQualityHero
      title="检验单"
      description="每条检验单对应一种物料的来料检验"
      ><template #icon
        ><ClipboardCheck
          :size="22"
          :stroke-width="1.8" /></template
    ></InventoryQualityHero>
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
        >+ 新建检验单</UiButton
      >
    </div>
    <section :class="$style.listPanel">
      <div :class="$style.toolbar">
        <label :class="$style.searchField"
          ><Search
            :size="14"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索检验单"
            placeholder="搜索检验单号、物料名称、供应商..." /></label
        ><UiTabs v-model="activeStatus"
          ><UiTabsList label="检验单状态筛选"
            ><UiTabsTrigger
              v-for="status in statuses"
              :key="status"
              :value="status"
              >{{ status }}</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
      </div>
      <QcOrdersTable
        :rows="filteredRows"
        @view="openDetails"
      />
    </section>
    <QcOrderFormDialog
      v-model:open="formOpen"
      @save="saveOrder"
    />
    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.id ?? '检验详情'"
      :description="activeRow ? `${activeRow.material} · ${activeRow.status}` : undefined"
      size="md"
      ><dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>规格型号</dt>
          <dd>{{ activeRow.specification }}</dd>
        </div>
        <div>
          <dt>数量</dt>
          <dd>{{ activeRow.quantity }}</dd>
        </div>
        <div>
          <dt>供应商</dt>
          <dd>{{ activeRow.supplier }}</dd>
        </div>
        <div>
          <dt>来源 / 到货</dt>
          <dd>{{ activeRow.sourceCode }} · {{ activeRow.arrivalDate }}</dd>
        </div>
        <div>
          <dt>检验结果</dt>
          <dd>{{ activeRow.result }}</dd>
        </div>
        <div>
          <dt>检验员</dt>
          <dd>{{ activeRow.inspector }}</dd>
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
.listPanel {
  @apply overflow-hidden rounded-lg border bg-white shadow-sm;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex h-[64px] items-center gap-3 border-b px-5;
  border-color: var(--ui-border);
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
</style>
