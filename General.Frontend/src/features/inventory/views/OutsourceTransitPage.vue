<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiInput, UiSelect, toast, type UiSelectOption } from '@/components/ui';
import { outsourceTransitReferenceFixture } from '../model/outsource-transit.data';
import type { OutsourceFeedDraft, OutsourceTransitRecord } from '../model/outsource-transit.types';
import OutsourceFeedDialog from './outsource-transit/OutsourceFeedDialog.vue';
import OutsourceSupplierSummary from './outsource-transit/OutsourceSupplierSummary.vue';
import OutsourceTransitHero from './outsource-transit/OutsourceTransitHero.vue';
import OutsourceTransitTable from './outsource-transit/OutsourceTransitTable.vue';

type ViewMode = 'detail' | 'supplier';
const rows = ref<OutsourceTransitRecord[]>(cloneReferenceRows(outsourceTransitReferenceFixture.rows));
const query = ref('');
const supplier = ref('全部供应商');
const viewMode = ref<ViewMode>('detail');
const feedOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<OutsourceTransitRecord | null>(null);
const supplierOptions: UiSelectOption[] = ['全部供应商', ...new Set(rows.value.map((row) => row.supplier))].map(
  (value) => ({ value, label: value }),
);

const filteredRows = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesSupplier = supplier.value === '全部供应商' || row.supplier === supplier.value;
    const matchesQuery =
      !needle ||
      `${row.workOrder} ${row.purchaseOrder} ${row.supplier} ${row.productCode} ${row.productName}`
        .toLocaleLowerCase('zh-CN')
        .includes(needle);
    return matchesSupplier && matchesQuery;
  });
});

const totalBalance = computed(() => filteredRows.value.reduce((sum, row) => sum + row.balance, 0));
const totalAmount = computed(() => filteredRows.value.reduce((sum, row) => sum + row.amount, 0));

function openDetail(row: OutsourceTransitRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function saveFeed(draft: OutsourceFeedDraft) {
  rows.value = [
    {
      id: `outsource-stock-local-${Date.now()}`,
      workOrder: '—',
      workOrderName: '本地发料记录',
      purchaseOrder: draft.purchaseOrder,
      supplier: draft.supplier,
      supplierCode: 'LOCAL',
      productCode: draft.productCode,
      productName: draft.productName,
      specification: draft.specification || '—',
      sent: Number(draft.quantity),
      consumed: 0,
      returned: 0,
      balance: Number(draft.quantity),
      ageDays: 0,
      amount: 0,
      unit: draft.unit,
    },
    ...rows.value,
  ];
  feedOpen.value = false;
  toast.success('委外发料已保存到当前本地会话');
}

function exportRows() {
  const body = filteredRows.value.map((row) => [
    row.workOrder,
    row.purchaseOrder,
    row.supplier,
    row.productCode,
    row.productName,
    row.balance,
    row.amount,
    row.unit,
  ]);
  const csv = [['工单号', '采购订单', '供应商', '产品编号', '品名', '在外余量', '在外金额', '单位'], ...body]
    .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = '委外在途库存.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${body.length} 条委外库存记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/inventory/outsource-transit"
    data-page-label="委外厂库存"
    data-page-module="库存管理"
  >
    <OutsourceTransitHero
      @feed="feedOpen = true"
      @export="exportRows"
    />

    <section :class="$style.metrics">
      <article>
        <span>物料明细</span><strong>{{ filteredRows.length }}</strong
        ><small>当前筛选结果</small>
      </article>
      <article>
        <span>在外余量</span><strong>{{ totalBalance }}</strong
        ><small>合计数量</small>
      </article>
      <article>
        <span>在外金额</span><strong>¥{{ totalAmount.toLocaleString('zh-CN') }}</strong
        ><small>按当前筛选</small>
      </article>
      <article>
        <span>供应商</span><strong>{{ new Set(filteredRows.map((row) => row.supplier)).size }}</strong
        ><small>合作外协厂</small>
      </article>
    </section>

    <section :class="$style.toolbar">
      <label :class="$style.search"
        ><span>搜索</span
        ><UiInput
          v-model="query"
          placeholder="工单 / 订单 / 物料 / 供应商"
      /></label>
      <UiSelect
        v-model="supplier"
        :options="supplierOptions"
        label="供应商"
      />
      <div
        :class="$style.viewTabs"
        role="tablist"
        aria-label="库存视图"
      >
        <button
          type="button"
          :data-active="viewMode === 'detail'"
          @click="viewMode = 'detail'"
        >
          工单 / 物料明细
        </button>
        <button
          type="button"
          :data-active="viewMode === 'supplier'"
          @click="viewMode = 'supplier'"
        >
          按供应商汇总
        </button>
      </div>
      <UiButton
        variant="ghost"
        size="sm"
        @click="
          query = '';
          supplier = '全部供应商';
        "
        >重置</UiButton
      >
    </section>

    <OutsourceTransitTable
      v-if="viewMode === 'detail'"
      :rows="filteredRows"
      @view="openDetail"
    />
    <OutsourceSupplierSummary
      v-else
      :rows="filteredRows"
    />

    <OutsourceFeedDialog
      v-model:open="feedOpen"
      @save="saveFeed"
    />
    <UiDialog
      v-model:open="detailOpen"
      title="委外在途详情"
      description="参考记录只读详情"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>采购订单</dt>
          <dd>{{ activeRow.purchaseOrder }}</dd>
        </div>
        <div>
          <dt>供应商</dt>
          <dd>{{ activeRow.supplier }}</dd>
        </div>
        <div>
          <dt>产品</dt>
          <dd>{{ activeRow.productName }}</dd>
        </div>
        <div>
          <dt>规格</dt>
          <dd>{{ activeRow.specification }}</dd>
        </div>
        <div>
          <dt>发料 / 已耗 / 已退</dt>
          <dd>{{ activeRow.sent }} / {{ activeRow.consumed }} / {{ activeRow.returned }}</dd>
        </div>
        <div>
          <dt>在外金额</dt>
          <dd>¥{{ activeRow.amount.toLocaleString('zh-CN') }}</dd>
        </div>
      </dl>
      <template #footer
        ><UiButton
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
  @apply min-w-0 space-y-4 px-5 py-4 text-[#172B4D];
}

.metrics {
  @apply grid grid-cols-4 gap-3;
}

.metrics article {
  @apply flex min-h-[82px] flex-col rounded-lg border border-[#DFE1E6] bg-white p-3;
}

.metrics span,
.metrics small {
  @apply text-[11px] text-[#6B778C];
}

.metrics strong {
  @apply mt-1 text-[21px] font-semibold text-[#0052CC];
}

.metrics small {
  @apply mt-auto text-[10px] text-[#97A0AF];
}

.toolbar {
  @apply flex items-end gap-3 rounded-lg border border-[#DFE1E6] bg-white p-3;
}

.search {
  @apply grid min-w-[320px] gap-1.5 text-[11px] font-semibold text-[#6B778C];
}

.viewTabs {
  @apply ml-auto flex items-center gap-1 rounded border border-[#DFE1E6] bg-[#FAFBFC] p-0.5;
}

.viewTabs button {
  @apply rounded px-3 py-1.5 text-[11px] text-[#6B778C] transition-colors;
}

.viewTabs button[data-active='true'] {
  @apply bg-white font-semibold text-[#0052CC] shadow-sm;
}

.detailGrid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 text-[12px];
}

.detailGrid dt {
  @apply text-[#6B778C];
}

.detailGrid dd {
  @apply mt-1 font-medium text-[#172B4D];
}
</style>
