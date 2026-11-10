<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';
import { supplierPricebookReferenceFixture } from '../model/supplier-pricebook.data';
import type {
  SupplierPricebookDraft,
  SupplierPricebookRow,
  SupplierPricebookStatus,
} from '../model/supplier-pricebook.types';
import SupplierPricebookControls from './supplier-pricebook/SupplierPricebookControls.vue';
import SupplierPricebookFormDialog from './supplier-pricebook/SupplierPricebookFormDialog.vue';
import SupplierPricebookGrid from './supplier-pricebook/SupplierPricebookGrid.vue';
import SupplierPricebookHero from './supplier-pricebook/SupplierPricebookHero.vue';
const rows = ref<SupplierPricebookRow[]>(cloneReferenceRows(supplierPricebookReferenceFixture.rows));
const status = ref<'全部状态' | SupplierPricebookStatus>('全部状态');
const query = ref('');
const formOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<SupplierPricebookRow | null>(null);
const filteredRows = computed(() => {
  const k = query.value.trim().toLowerCase();
  return rows.value.filter(
    (r) =>
      (status.value === '全部状态' || r.status === status.value) &&
      (!k ||
        r.id.toLowerCase().includes(k) ||
        r.name.toLowerCase().includes(k) ||
        r.supplier.toLowerCase().includes(k)),
  );
});
function view(row: SupplierPricebookRow) {
  activeRow.value = row;
  detailOpen.value = true;
}
function save(d: SupplierPricebookDraft) {
  const n = rows.value.length + 1;
  rows.value = [
    {
      id: `PB-LOCAL-${String(n).padStart(3, '0')}`,
      name: d.name.trim(),
      supplier: d.supplier.trim(),
      status: '草稿',
      pricingMethod: d.discountRate ? '多级折扣' : '按物料单独定价',
      discounts: d.discountRate ? [{ label: '一级', rate: Number(d.discountRate) }] : [],
      itemCount: Number(d.itemCount) || 1,
      validFrom: d.validFrom,
      validTo: d.validTo,
      createdBy: '张经理',
      updatedAt: new Date().toISOString().slice(0, 10),
    },
    ...rows.value,
  ];
  formOpen.value = false;
  status.value = '全部状态';
  toast.success('供应商价格本草稿已保存');
}
function exportRows() {
  const head = ['价格本编号', '名称', '供应商', '状态', '物料数', '生效日期', '失效日期'];
  const data = filteredRows.value.map((r) => [r.id, r.name, r.supplier, r.status, r.itemCount, r.validFrom, r.validTo]);
  const csv = [head, ...data]
    .map((line) => line.map((c) => `"${String(c).replaceAll('"', '""')}"`).join(','))
    .join('\n');
  const href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const a = document.createElement('a');
  a.href = href;
  a.download = `供应商价格本_${new Date().toISOString().slice(0, 10)}.csv`;
  a.click();
  URL.revokeObjectURL(href);
  toast.success(`已导出 ${filteredRows.value.length} 条价格本`);
}
</script>
<template>
  <main
    :class="$style.page"
    data-page-family="cards"
    data-page-route="/purchase/pricebook"
    data-supplier-pricebook-page
    :data-active-status="status"
  >
    <SupplierPricebookHero /><SupplierPricebookControls
      v-model:status="status"
      v-model:query="query"
      :rows="rows"
      @export="exportRows"
      @create="formOpen = true"
    /><SupplierPricebookGrid
      :rows="filteredRows"
      @view="view"
    />
  </main>
  <SupplierPricebookFormDialog
    v-model:open="formOpen"
    @save="save"
  /><UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.name ?? '价格本详情'"
    :description="activeRow?.id"
    size="lg"
    ><dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>供应商</dt>
        <dd>{{ activeRow.supplier }}</dd>
      </div>
      <div>
        <dt>状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div>
        <dt>定价方式</dt>
        <dd>{{ activeRow.pricingMethod }}</dd>
      </div>
      <div>
        <dt>物料数</dt>
        <dd>{{ activeRow.itemCount }}</dd>
      </div>
      <div>
        <dt>有效期</dt>
        <dd>{{ activeRow.validFrom }} ~ {{ activeRow.validTo }}</dd>
      </div>
      <div>
        <dt>创建人</dt>
        <dd>{{ activeRow.createdBy }}</dd>
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
</template>
<style module>
@reference "../../../styles/index.css";
.page {
  @apply mx-auto min-h-full w-full max-w-[1294px] pb-8 pt-[20px];
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
</style>
