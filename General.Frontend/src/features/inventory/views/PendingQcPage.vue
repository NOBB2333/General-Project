<script setup lang="ts">
import { ClipboardCheck, Download, Search } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import { pendingQcReferenceFixture } from '../model/pending-qc.data';
import type { PendingQcDraft, PendingQcRecord, PendingQcStatus } from '../model/pending-qc.types';
import PendingQcFormDialog from './pending-qc/PendingQcFormDialog.vue';
import PendingQcTable from './pending-qc/PendingQcTable.vue';
import InventoryQualityHero from './shared/InventoryQualityHero.vue';

type StatusFilter = '全部' | PendingQcStatus;
const statuses: readonly StatusFilter[] = [
  '全部',
  '草稿',
  '已提交',
  '检验中',
  '已通过',
  '部分合格',
  '不合格',
  '已入库',
  '已退回',
];
const rows = ref<PendingQcRecord[]>(cloneReferenceRows(pendingQcReferenceFixture.rows));
const activeStatus = ref<StatusFilter>('全部');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<PendingQcRecord | null>(null);

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !keyword ||
      [row.id, row.material, row.supplier, row.batchCode, row.inspectionCode].some((value) =>
        value.toLocaleLowerCase('zh-CN').includes(keyword),
      );
    return matchesStatus && matchesQuery;
  });
});

function openDetails(row: PendingQcRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function savePendingQc(draft: PendingQcDraft) {
  rows.value.unshift({
    id: `DJ-2026-${String(rows.value.length + 21).padStart(4, '0')}`,
    stage: '',
    attachmentCount: 0,
    material: draft.material,
    specification: draft.specification || '-',
    quantity: draft.quantity,
    batchCode: draft.batchCode,
    supplier: draft.supplier,
    contractCode: '-',
    arrivalDate: '2026-03-18',
    status: '草稿',
    inspectionCode: '-',
    result: [],
  });
  activeStatus.value = '全部';
  query.value = '';
  formOpen.value = false;
}

function exportRows() {
  const header = [
    '待检单号',
    '物料名称',
    '规格型号',
    '数量',
    '批次号',
    '供应商',
    '合同号',
    '到货日期',
    '状态',
    '关联检验单',
    '检验结果',
  ];
  const body = filteredRows.value.map((row) => [
    row.id,
    row.material,
    row.specification,
    row.quantity,
    row.batchCode,
    row.supplier,
    row.contractCode,
    row.arrivalDate,
    row.status,
    row.inspectionCode,
    row.result.join(' '),
  ]);
  const csv = [header, ...body]
    .map((line) => line.map((cell) => `"${String(cell).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = '待检库存.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/pending-qc"
    data-page-label="待检库存"
    data-page-module="库存管理"
    data-pending-qc-page
    :data-local-row-count="rows.length"
    :data-filtered-row-count="filteredRows.length"
  >
    <InventoryQualityHero
      title="待检库存"
      description="每个物料独立一条待检记录，关联来料检验流程"
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
        >+ 新建待检单</UiButton
      >
    </div>
    <section :class="$style.listPanel">
      <div :class="$style.toolbar">
        <label :class="$style.searchField"
          ><Search
            :size="14"
            :stroke-width="1.8" /><UiInput
            v-model="query"
            aria-label="搜索待检记录"
            placeholder="搜索待检单号、物料名称、供应商、批次号..."
        /></label>
        <UiTabs v-model="activeStatus"
          ><UiTabsList label="待检状态筛选"
            ><UiTabsTrigger
              v-for="status in statuses"
              :key="status"
              :value="status"
              >{{ status }}</UiTabsTrigger
            ></UiTabsList
          ></UiTabs
        >
      </div>
      <PendingQcTable
        :rows="filteredRows"
        @view="openDetails"
      />
    </section>
    <PendingQcFormDialog
      v-model:open="formOpen"
      @save="savePendingQc"
    />
    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.id ?? '待检详情'"
      :description="activeRow ? `${activeRow.material} · ${activeRow.status}` : undefined"
      size="md"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>规格型号</dt>
          <dd>{{ activeRow.specification }}</dd>
        </div>
        <div>
          <dt>数量 / 批次</dt>
          <dd>{{ activeRow.quantity }} · {{ activeRow.batchCode }}</dd>
        </div>
        <div>
          <dt>供应商</dt>
          <dd>{{ activeRow.supplier }}</dd>
        </div>
        <div>
          <dt>合同 / 到货日期</dt>
          <dd>{{ activeRow.contractCode }} · {{ activeRow.arrivalDate }}</dd>
        </div>
        <div>
          <dt>关联检验单</dt>
          <dd>{{ activeRow.inspectionCode }}</dd>
        </div>
        <div>
          <dt>检验结果</dt>
          <dd>{{ activeRow.result.join(' / ') || '-' }}</dd>
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
