<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, toast } from '@/components/ui';
import { warehousePurchaseTrend, warehouseQuantityTrend, warehousesReferenceFixture } from '../model/warehouses.data';
import type { WarehouseDraft, WarehouseFilter, WarehouseRow, WarehouseView } from '../model/warehouses.types';
import WarehouseCollection from './warehouses/WarehouseCollection.vue';
import WarehouseFormDialog from './warehouses/WarehouseFormDialog.vue';
import WarehouseHero from './warehouses/WarehouseHero.vue';
import WarehouseStats from './warehouses/WarehouseStats.vue';
import WarehouseTrendChart from './warehouses/WarehouseTrendChart.vue';

const rows = ref<WarehouseRow[]>(cloneReferenceRows(warehousesReferenceFixture.rows));
const filter = ref<WarehouseFilter>('全部');
const view = ref<WarehouseView>('card');
const formOpen = ref(false);
const detailOpen = ref(false);
const editingRow = ref<WarehouseRow | null>(null);
const activeRow = ref<WarehouseRow | null>(null);

const filteredRows = computed(() =>
  filter.value === '全部' ? rows.value : rows.value.filter((row) => row.type === filter.value),
);

function openCreate() {
  editingRow.value = null;
  formOpen.value = true;
}

function openEdit(row: WarehouseRow) {
  editingRow.value = row;
  formOpen.value = true;
}

function openDetail(row: WarehouseRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function editActiveRow() {
  if (!activeRow.value) return;
  detailOpen.value = false;
  openEdit(activeRow.value);
}

function saveWarehouse(draft: WarehouseDraft) {
  const current = editingRow.value;
  if (current) {
    Object.assign(current, {
      name: draft.name.trim(),
      type: draft.type,
      address: draft.address.trim(),
      manager: draft.manager.trim(),
      phone: draft.phone.trim(),
      remark: draft.remark.trim(),
    });
    toast.success('仓库信息已保存');
  } else {
    rows.value = [
      ...rows.value,
      {
        id: `W-LOCAL-${Date.now()}`,
        name: draft.name.trim(),
        type: draft.type,
        address: draft.address.trim(),
        manager: draft.manager.trim() || '未指定',
        phone: draft.phone.trim(),
        totalSKU: 0,
        totalQty: 0,
        totalValue: 0,
        totalPurchase: 0,
        estimatedSales: 0,
        noPriceSKU: 0,
        noPricePurchaseTotal: 0,
        status: '正常',
        isVirtual: false,
        remark: draft.remark.trim(),
      },
    ];
    toast.success('仓库已创建到本地列表');
  }
  formOpen.value = false;
  editingRow.value = null;
}

function formatMoney(value: number) {
  return `¥${value.toLocaleString('en-US')}`;
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="cards"
    data-page-route="/base/warehouses"
    data-warehouses-page
    :data-active-filter="filter"
    :data-active-view="view"
  >
    <WarehouseHero />
    <WarehouseStats :rows="rows" />
    <section
      :class="$style.trends"
      aria-label="仓库趋势"
    >
      <WarehouseTrendChart
        title="总库存数量趋势"
        subtitle="近 12 个月 · 单位：件"
        :values="warehouseQuantityTrend.values"
        :labels="warehouseQuantityTrend.labels"
        current="755"
        delta="+3 件"
        positive
        tone="blue"
      />
      <WarehouseTrendChart
        title="总采购金额趋势"
        subtitle="近 12 个月 · 单位：万元"
        :values="warehousePurchaseTrend.values"
        :labels="warehousePurchaseTrend.labels"
        current="¥220.5万"
        delta="−19.5 万"
        :positive="false"
        tone="purple"
      />
    </section>
    <WarehouseCollection
      v-model:filter="filter"
      v-model:view="view"
      :rows="filteredRows"
      @create="openCreate"
      @view-row="openDetail"
      @edit="openEdit"
    />
  </main>

  <WarehouseFormDialog
    v-model:open="formOpen"
    :editing="editingRow"
    @save="saveWarehouse"
  />

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.name ?? '仓库详情'"
    :description="activeRow?.id"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>仓库类型</dt>
        <dd>{{ activeRow.type }}{{ activeRow.isVirtual ? ' · 虚拟仓' : '' }}</dd>
      </div>
      <div>
        <dt>状态</dt>
        <dd>{{ activeRow.status }}</dd>
      </div>
      <div :class="$style.detailWide">
        <dt>地址</dt>
        <dd>{{ activeRow.address }}</dd>
      </div>
      <div>
        <dt>{{ activeRow.isVirtual ? '关联供应商' : '负责人' }}</dt>
        <dd>{{ activeRow.manager }}</dd>
      </div>
      <div>
        <dt>联系电话</dt>
        <dd>{{ activeRow.phone || '—' }}</dd>
      </div>
      <div>
        <dt>{{ activeRow.isVirtual ? '在途物料种类' : 'SKU 数量' }}</dt>
        <dd>{{ activeRow.totalSKU }}</dd>
      </div>
      <div>
        <dt>{{ activeRow.isVirtual ? '在途数量' : '库存数量' }}</dt>
        <dd>{{ activeRow.totalQty.toLocaleString('en-US') }}</dd>
      </div>
      <template v-if="!activeRow.isVirtual">
        <div>
          <dt>库存金额</dt>
          <dd>{{ formatMoney(activeRow.totalValue) }}</dd>
        </div>
        <div>
          <dt>预估销售额</dt>
          <dd>{{ formatMoney(activeRow.estimatedSales) }}</dd>
        </div>
      </template>
    </dl>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      >
      <UiButton
        variant="primary"
        @click="editActiveRow"
        >编辑仓库</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full w-full max-w-[1400px] pb-8;
  color: var(--ui-text);
}
.trends {
  @apply mb-5 grid grid-cols-2 gap-4;
}
.details {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 p-5;
}
.details dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[13px] font-medium;
  color: var(--ui-text);
}
.detailWide {
  @apply col-span-2;
}
@media (max-width: 900px) {
  .trends {
    @apply grid-cols-1;
  }
}
</style>
