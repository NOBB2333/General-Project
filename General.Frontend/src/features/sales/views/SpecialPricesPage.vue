<script setup lang="ts">
import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import { specialPricesReferenceFixture } from '../model/special-prices.data';
import type {
  SpecialPriceApprovalFilter,
  SpecialPriceDraft,
  SpecialPriceOrderFilter,
  SpecialPriceRecord,
  SpecialPriceUrgencyFilter,
} from '../model/special-prices.types';
import SpecialPriceDialogs from './special-prices/SpecialPriceDialogs.vue';
import SpecialPricesFilters from './special-prices/SpecialPricesFilters.vue';
import SpecialPricesHero from './special-prices/SpecialPricesHero.vue';
import SpecialPricesOverview from './special-prices/SpecialPricesOverview.vue';
import SpecialPricesTable from './special-prices/SpecialPricesTable.vue';

const rows = ref<SpecialPriceRecord[]>(cloneReferenceRows(specialPricesReferenceFixture.rows));
const query = ref('');
const approvalStatus = ref<SpecialPriceApprovalFilter>('全部');
const urgency = ref<SpecialPriceUrgencyFilter>('全部');
const orderStatus = ref<SpecialPriceOrderFilter>('全部');
const formOpen = ref(false);
const detailOpen = ref(false);
const deleteOpen = ref(false);
const activeRow = ref<SpecialPriceRecord | null>(null);
const editingRow = ref<SpecialPriceRecord | null>(null);
const draft = reactive<SpecialPriceDraft>(emptyDraft());

const filteredRows = computed(() => {
  const needle = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesQuery =
      !needle ||
      `${row.code} ${row.customer} ${row.linkedOrderCode} ${row.createdBy}`.toLocaleLowerCase('zh-CN').includes(needle);
    const matchesApproval = approvalStatus.value === '全部' || row.status === approvalStatus.value;
    const matchesUrgency = urgency.value === '全部' || row.urgency === urgency.value;
    const matchesOrder = orderStatus.value === '全部' || row.orderStatus === orderStatus.value;
    return matchesQuery && matchesApproval && matchesUrgency && matchesOrder;
  });
});

const metrics = computed(() => ({
  pending: rows.value.filter((row) => row.status === '审批中').length,
  approved: rows.value.filter((row) => row.status === '已通过').length,
  ordered: rows.value.filter((row) => row.orderStatus === '已下单').length,
  waiting: rows.value.filter((row) => row.status === '已通过' && row.orderStatus === '未下单').length,
}));

function emptyDraft(): SpecialPriceDraft {
  return {
    customer: '',
    contact: '',
    sourceQuotationCode: '',
    itemCount: '1',
    totalNormalAmount: '',
    totalRequestedAmount: '',
    urgency: '普通',
    createdBy: '张经理',
    reason: '',
  };
}

function updateDraft(value: SpecialPriceDraft) {
  Object.assign(draft, value);
}

function openCreate() {
  editingRow.value = null;
  Object.assign(draft, emptyDraft());
  formOpen.value = true;
}

function openEdit(row: SpecialPriceRecord) {
  editingRow.value = row;
  Object.assign(draft, {
    customer: row.customer,
    contact: row.contact,
    sourceQuotationCode: row.sourceQuotationCode,
    itemCount: String(row.itemCount),
    totalNormalAmount: String(row.totalNormalAmount),
    totalRequestedAmount: String(row.totalRequestedAmount),
    urgency: row.urgency,
    createdBy: row.createdBy,
    reason: row.reason,
  });
  formOpen.value = true;
}

function saveDraft() {
  const itemCount = Number(draft.itemCount);
  const normalAmount = Number(draft.totalNormalAmount);
  const requestedAmount = Number(draft.totalRequestedAmount);
  if (!draft.customer.trim() || !draft.createdBy.trim() || !draft.reason.trim()) {
    toast.warning('请填写客户、申请人和申请原因');
    return;
  }
  if (!Number.isFinite(itemCount) || itemCount < 1 || normalAmount <= 0 || requestedAmount <= 0) {
    toast.warning('请输入有效的物料数和金额');
    return;
  }
  if (requestedAmount > normalAmount) {
    toast.warning('申请总价不能高于正常总价');
    return;
  }

  if (editingRow.value) {
    const id = editingRow.value.id;
    rows.value = rows.value.map((row) =>
      row.id === id
        ? {
            ...row,
            customer: draft.customer.trim(),
            contact: draft.contact.trim(),
            sourceQuotationCode: draft.sourceQuotationCode.trim(),
            itemCount,
            totalNormalAmount: normalAmount,
            totalRequestedAmount: requestedAmount,
            totalLostAmount: normalAmount - requestedAmount,
            urgency: draft.urgency,
            createdBy: draft.createdBy.trim(),
            reason: draft.reason.trim(),
          }
        : row,
    );
    toast.success(`${editingRow.value.code} 已保存`);
  } else {
    const sequence = rows.value.length + 1;
    const row: SpecialPriceRecord = {
      id: `SP-LOCAL-${sequence}`,
      code: `SP-LOCAL-${String(sequence).padStart(4, '0')}`,
      customer: draft.customer.trim(),
      contact: draft.contact.trim(),
      reason: draft.reason.trim(),
      sourceQuotationCode: draft.sourceQuotationCode.trim(),
      itemCount,
      totalNormalAmount: normalAmount,
      totalRequestedAmount: requestedAmount,
      totalLostAmount: normalAmount - requestedAmount,
      urgency: draft.urgency,
      status: '草稿',
      orderStatus: '未下单',
      linkedOrderCode: '',
      createdBy: draft.createdBy.trim(),
      createdAt: '2026-09-16',
    };
    rows.value.unshift(row);
    toast.success(`${row.code} 已保存为草稿`);
  }
  approvalStatus.value = '全部';
  formOpen.value = false;
}

function openDetail(row: SpecialPriceRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function requestDelete(row: SpecialPriceRecord) {
  activeRow.value = row;
  deleteOpen.value = true;
}

function removeDraft() {
  if (!activeRow.value) return;
  const code = activeRow.value.code;
  rows.value = rows.value.filter((row) => row.id !== activeRow.value?.id);
  deleteOpen.value = false;
  activeRow.value = null;
  toast.success(`${code} 已从当前本地会话删除`);
}

function createOrder(row: SpecialPriceRecord) {
  const linkedOrderCode = `SO-LOCAL-${String(rows.value.indexOf(row) + 1).padStart(4, '0')}`;
  rows.value = rows.value.map((item) =>
    item.id === row.id ? { ...item, orderStatus: '已下单', linkedOrderCode } : item,
  );
  activeRow.value = rows.value.find((item) => item.id === row.id) ?? null;
  detailOpen.value = false;
  toast.success(`${linkedOrderCode} 已按 ${row.code} 创建`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/sales/special-prices"
    data-page-label="特价管理"
    data-page-module="销售业务"
    data-special-prices-page
    :data-result-count="filteredRows.length"
    :data-local-row-count="rows.length"
    :data-approval-status="approvalStatus"
    :data-urgency="urgency"
    :data-order-status="orderStatus"
  >
    <SpecialPricesHero />
    <SpecialPricesFilters
      :query="query"
      :approval-status="approvalStatus"
      :urgency="urgency"
      :order-status="orderStatus"
      @update:query="query = $event"
      @update:approval-status="approvalStatus = $event"
      @update:urgency="urgency = $event"
      @update:order-status="orderStatus = $event"
      @create="openCreate"
    />
    <SpecialPricesOverview
      :pending="metrics.pending"
      :approved="metrics.approved"
      :ordered="metrics.ordered"
      :waiting="metrics.waiting"
    />
    <SpecialPricesTable
      :rows="filteredRows"
      @view="openDetail"
      @edit="openEdit"
      @delete="requestDelete"
      @order="createOrder"
    />
  </main>

  <SpecialPriceDialogs
    :form-open="formOpen"
    :detail-open="detailOpen"
    :delete-open="deleteOpen"
    :editing-row="editingRow"
    :active-row="activeRow"
    :draft="draft"
    @update:form-open="formOpen = $event"
    @update:detail-open="detailOpen = $event"
    @update:delete-open="deleteOpen = $event"
    @update:draft="updateDraft"
    @save="saveDraft"
    @order="createOrder"
    @remove="removeDraft"
  />
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-h-full min-w-[1180px] max-w-[1400px] pb-8;
  color: var(--ui-text);
}
</style>
