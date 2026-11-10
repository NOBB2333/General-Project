<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { salesDeliveryReferenceFixture } from '../model/sales-delivery.data';
import type {
  DeliveryDetailRow,
  DeliveryDraft,
  DeliveryScope,
  DeliveryStatus,
  DeliveryView,
  SalesDeliveryRecord,
} from '../model/sales-delivery.types';
import DeliveryOverview from './sales-delivery/DeliveryOverview.vue';
import DeliveryWorkspace from './sales-delivery/DeliveryWorkspace.vue';
import SalesDeliveryHero from './sales-delivery/SalesDeliveryHero.vue';

const currentOperator = '张明';
const teamOperators = ['李强', '王芳', '赵敏'] as const;
const rows = ref<SalesDeliveryRecord[]>(cloneReferenceRows(salesDeliveryReferenceFixture.rows));
const scope = ref<DeliveryScope>('all');
const view = ref<DeliveryView>('list');
const status = ref<'全部' | DeliveryStatus>('全部');
const query = ref('');
const selectedRow = ref<SalesDeliveryRecord | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const shipOpen = ref(false);
const draft = reactive<DeliveryDraft>(emptyDraft());
const warehouse = ref('上海主仓');
const carrier = ref('顺丰速运');

const warehouseOptions: readonly UiSelectOption[] = [
  { value: '上海主仓', label: '上海主仓' },
  { value: '苏州分仓', label: '苏州分仓' },
];
const carrierOptions: readonly UiSelectOption[] = [
  { value: '顺丰速运', label: '顺丰速运' },
  { value: '中通快递', label: '中通快递' },
  { value: '德邦物流', label: '德邦物流' },
];

const scopeCounts = computed<Record<DeliveryScope, number>>(() => ({
  all: rows.value.length,
  mine: rows.value.filter((row) => row.operator === currentOperator).length,
  team: rows.value.filter((row) => teamOperators.includes(row.operator as (typeof teamOperators)[number])).length,
  watched: rows.value.filter((row) => row.watched).length,
}));

const scopedRows = computed(() => rows.value.filter(matchesScope));
const statusCounts = computed<Record<'全部' | DeliveryStatus, number>>(() => ({
  全部: scopedRows.value.length,
  待发货: scopedRows.value.filter((row) => row.status === '待发货').length,
  部分发货: scopedRows.value.filter((row) => row.status === '部分发货').length,
  已发货: scopedRows.value.filter((row) => row.status === '已发货').length,
  已签收: scopedRows.value.filter((row) => row.status === '已签收').length,
}));
const filteredRows = computed(() => scopedRows.value.filter(matchesFilters));
const allDetailRows = computed(() => scopedRows.value.flatMap(toDetailRows));
const filteredDetailRows = computed(() => allDetailRows.value.filter(matchesDetailFilters));
const pendingCount = computed(() => scopedRows.value.filter((row) => row.status === '待发货').length);
const transitCount = computed(
  () => scopedRows.value.filter((row) => row.status === '部分发货' || row.status === '已发货').length,
);
const signedCount = computed(() => scopedRows.value.filter((row) => row.status === '已签收').length);
const shippedQty = computed(() => scopedRows.value.reduce((sum, row) => sum + row.shippedQty, 0));
const totalQty = computed(() => scopedRows.value.reduce((sum, row) => sum + row.totalQty, 0));

function emptyDraft(): DeliveryDraft {
  return { customer: '', order: '', receiver: '', date: '2026-02-20' };
}

function matchesScope(row: SalesDeliveryRecord) {
  if (scope.value === 'mine') return row.operator === currentOperator;
  if (scope.value === 'team') return teamOperators.includes(row.operator as (typeof teamOperators)[number]);
  if (scope.value === 'watched') return row.watched;
  return true;
}

function matchesFilters(row: SalesDeliveryRecord) {
  if (status.value !== '全部' && row.status !== status.value) return false;
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  if (!keyword) return true;
  return [row.id, row.customer, row.soId, ...(row.soIds ?? [])].some((value) =>
    value.toLocaleLowerCase('zh-CN').includes(keyword),
  );
}

function matchesDetailFilters(row: DeliveryDetailRow) {
  if (status.value !== '全部' && row.shipmentStatus !== status.value) return false;
  const keyword = query.value.trim().toLocaleLowerCase('zh-CN');
  if (!keyword) return true;
  return [row.shipmentId, row.customer, row.itemCode, row.name, row.spec].some((value) =>
    value.toLocaleLowerCase('zh-CN').includes(keyword),
  );
}

function toDetailRows(row: SalesDeliveryRecord): DeliveryDetailRow[] {
  return row.items.map((item, index) => ({
    ...item,
    key: `${row.id}-${index}`,
    shipmentId: row.id,
    soId: item.sourceOrderId ?? row.soId,
    date: row.date,
    customer: row.customer,
    operator: row.operator,
    shipmentStatus: row.status,
    itemCode: materialCode(item.name, index),
    pendingQty: Math.max(0, item.qty - item.shippedQty),
    amount: item.qty * item.price,
  }));
}

function materialCode(name: string, index: number) {
  const sum = Array.from(name).reduce((total, character) => total + character.charCodeAt(0), 0);
  return `${String(2100 + (sum % 40)).padStart(4, '0')}${String(index + 1).padStart(4, '0')}`;
}

function changeScope(nextScope: DeliveryScope) {
  scope.value = nextScope;
  status.value = '全部';
}

function openDetails(row: SalesDeliveryRecord) {
  selectedRow.value = row;
  detailOpen.value = true;
}

function openCreate() {
  Object.assign(draft, emptyDraft());
  createOpen.value = true;
}

function createDelivery() {
  if (!draft.customer.trim() || !draft.order.trim()) {
    toast.warning('请填写客户和关联订单');
    return;
  }
  rows.value.unshift({
    id: `FH-LOCAL-${String(rows.value.length + 1).padStart(4, '0')}`,
    soId: draft.order.trim(),
    customer: draft.customer.trim(),
    project: '本地演示项目',
    receiver: draft.receiver.trim() || '待确认',
    phone: '-',
    address: '待确认',
    totalQty: 1,
    shippedQty: 0,
    status: '待发货',
    date: draft.date,
    amount: 0,
    operator: currentOperator,
    watched: false,
    outbounds: [],
    items: [],
  });
  createOpen.value = false;
  toast.success('发货单已保存到当前本地会话');
}

function openShip(row: SalesDeliveryRecord) {
  selectedRow.value = row;
  warehouse.value = '上海主仓';
  carrier.value = '顺丰速运';
  shipOpen.value = true;
}

function confirmShipment() {
  if (!selectedRow.value) return;
  const row = rows.value.find((entry) => entry.id === selectedRow.value?.id);
  if (!row) return;
  row.shippedQty = row.totalQty;
  row.status = '已发货';
  row.items.forEach((item) => {
    item.shippedQty = item.qty;
    item.outboundStatus = '已出库';
    item.warehouse = warehouse.value;
  });
  row.outbounds.push({
    id: `CK-LOCAL-${Date.now().toString().slice(-6)}`,
    warehouse: warehouse.value,
    qty: row.totalQty,
    status: '已出库',
    date: '2026-02-20',
    carrier: carrier.value,
    tracking: `LOCAL${Date.now().toString().slice(-8)}`,
    operator: currentOperator,
    batchNo: 'LOCAL-BATCH',
  });
  shipOpen.value = false;
  toast.success(`${row.id} 已完成本地发货登记`);
}

function exportRows() {
  const source = view.value === 'list' ? filteredRows.value : filteredDetailRows.value;
  const csv = source
    .map((row) =>
      Object.values(row)
        .filter((value) => typeof value !== 'object')
        .join(','),
    )
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = view.value === 'list' ? 'sales-deliveries.csv' : 'sales-delivery-items.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${source.length} 条记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/sales/deliveries"
    data-sales-deliveries-page
    :data-active-view="view"
    :data-active-scope="scope"
    :data-result-count="view === 'list' ? filteredRows.length : filteredDetailRows.length"
  >
    <SalesDeliveryHero />
    <DeliveryOverview
      :scope="scope"
      :counts="scopeCounts"
      :pending="pendingCount"
      :transit="transitCount"
      :signed="signedCount"
      :shipped-qty="shippedQty"
      :total-qty="totalQty"
      @update:scope="changeScope"
    />
    <DeliveryWorkspace
      :view="view"
      :status="status"
      :query="query"
      :rows="filteredRows"
      :detail-rows="filteredDetailRows"
      :detail-count="allDetailRows.length"
      :status-counts="statusCounts"
      @update:view="view = $event"
      @update:status="status = $event"
      @update:query="query = $event"
      @create="openCreate"
      @export="exportRows"
      @ship="openShip"
      @open="openDetails"
    />
  </main>

  <UiDialog
    v-model:open="detailOpen"
    :title="selectedRow?.id ?? '销售发货单详情'"
    :description="selectedRow ? `${selectedRow.customer} · ${selectedRow.status}` : undefined"
    size="xl"
  >
    <div
      v-if="selectedRow"
      :class="$style.detailBody"
    >
      <dl :class="$style.detailGrid">
        <div>
          <dt>关联订单</dt>
          <dd>{{ (selectedRow.soIds ?? [selectedRow.soId]).join(' / ') }}</dd>
        </div>
        <div>
          <dt>收货人</dt>
          <dd>{{ selectedRow.receiver }} · {{ selectedRow.phone }}</dd>
        </div>
        <div>
          <dt>收货地址</dt>
          <dd>{{ selectedRow.address }}</dd>
        </div>
        <div>
          <dt>发货进度</dt>
          <dd>{{ selectedRow.shippedQty }} / {{ selectedRow.totalQty }} 件</dd>
        </div>
      </dl>
      <h3>出库与物流</h3>
      <p
        v-if="!selectedRow.outbounds.length"
        :class="$style.emptyDetail"
      >
        尚未生成出库单
      </p>
      <article
        v-for="outbound in selectedRow.outbounds"
        :key="outbound.id"
        :class="$style.outboundCard"
      >
        <strong>{{ outbound.id }}</strong
        ><span>{{ outbound.warehouse }}</span
        ><span>{{ outbound.carrier }}</span> <span>{{ outbound.tracking }}</span
        ><span>{{ outbound.date }}</span>
      </article>
    </div>
    <template #footer><UiButton @click="detailOpen = false">关闭</UiButton></template>
  </UiDialog>

  <UiDialog
    v-model:open="createOpen"
    title="新建销售发货单"
    description="仅保存到当前本地复刻会话。"
    size="lg"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="createDelivery"
    >
      <UiFormField
        label="客户"
        required
        wide
        ><UiInput
          v-model="draft.customer"
          placeholder="请输入客户名称"
      /></UiFormField>
      <UiFormField
        label="关联订单"
        required
        ><UiInput
          v-model="draft.order"
          placeholder="SO-2026-0000"
      /></UiFormField>
      <UiFormField label="收货人"
        ><UiInput
          v-model="draft.receiver"
          placeholder="姓名"
      /></UiFormField>
      <UiFormField label="发货日期"
        ><UiInput
          v-model="draft.date"
          type="date"
      /></UiFormField>
    </form>
    <template #footer
      ><UiButton @click="createOpen = false">取消</UiButton
      ><UiButton
        variant="primary"
        @click="createDelivery"
        >保存发货单</UiButton
      ></template
    >
  </UiDialog>

  <UiDialog
    v-model:open="shipOpen"
    :title="`发货 · ${selectedRow?.id ?? ''}`"
    description="登记本次出库与物流信息。"
    size="md"
  >
    <div :class="$style.formGrid">
      <UiFormField label="出库仓库"
        ><UiSelect
          v-model="warehouse"
          :options="warehouseOptions"
          label="出库仓库"
      /></UiFormField>
      <UiFormField label="承运商"
        ><UiSelect
          v-model="carrier"
          :options="carrierOptions"
          label="承运商"
      /></UiFormField>
    </div>
    <template #footer
      ><UiButton @click="shipOpen = false">取消</UiButton
      ><UiButton
        variant="primary"
        @click="confirmShipment"
        >确认发货</UiButton
      ></template
    >
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-w-0 pb-8;
  max-width: 1400px;
}
.detailBody {
  @apply space-y-4 p-5;
}
.detailGrid {
  @apply grid grid-cols-2 gap-3;
}
.detailGrid div {
  @apply rounded border px-3 py-2;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.detailGrid dt {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.detailGrid dd {
  @apply mt-1 text-[12px] font-medium;
}
.detailBody h3 {
  @apply text-[13px] font-semibold;
}
.outboundCard {
  @apply grid grid-cols-5 gap-3 rounded border px-3 py-2 text-[11px];
  border-color: var(--ui-border);
}
.outboundCard strong {
  color: var(--app-primary);
}
.emptyDetail {
  @apply rounded border border-dashed p-6 text-center text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
