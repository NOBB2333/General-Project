<script setup lang="ts">
import { BarChart3, Info, ReceiptText, X } from 'lucide-vue-next';
import { computed, reactive, ref } from 'vue';
import { ListPageSurface, cloneReferenceRows } from '@/components/page';
import type { PageAction } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { salesOrdersReferenceFixture } from '@/features/sales/model/sales-orders.data';
import { salesOrdersModel } from '@/features/sales/model/sales-orders.model';
import type { SalesOrderRow } from '@/features/sales/model/sales-orders.types';

interface SalesOrderDraft {
  customer: string;
  amount: string;
  date: string;
  warehouse: string;
  owner: string;
}

const warehouseOptions: UiSelectOption[] = [
  { value: '上海主仓', label: '上海主仓' },
  { value: '苏州分仓', label: '苏州分仓' },
  { value: '客户直收（直运）', label: '客户直收（直运）' },
];
const ownerOptions: UiSelectOption[] = [
  { value: '张经理', label: '张经理' },
  { value: '陈销售', label: '陈销售' },
];

const rows = ref(cloneReferenceRows(salesOrdersReferenceFixture.rows));
const pageModel = computed(() => ({ ...salesOrdersModel, rows: rows.value }));
const activeRow = ref<SalesOrderRow | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const pendingAction = ref<PageAction | null>(null);
const actionOpen = ref(false);
const noticeVisible = ref(true);
const draft = reactive<SalesOrderDraft>(emptyDraft());

function emptyDraft(): SalesOrderDraft {
  return {
    customer: '',
    amount: '',
    date: '2026-04-30',
    warehouse: warehouseOptions[0]?.value ?? '',
    owner: ownerOptions[0]?.value ?? '',
  };
}

function resetDraft() {
  Object.assign(draft, emptyDraft());
}

function openDetails(row: SalesOrderRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function runAction(action: PageAction, selectedRows: SalesOrderRow[]) {
  if (action.kind === 'create') {
    resetDraft();
    createOpen.value = true;
    return;
  }
  if (action.kind === 'export') {
    exportOrders(selectedRows.length ? selectedRows : rows.value);
    return;
  }
  pendingAction.value = action;
  actionOpen.value = true;
}

function saveOrder() {
  if (!draft.customer.trim() || !draft.amount.trim()) {
    toast.warning('请填写客户和订单金额');
    return;
  }

  const sequence = String(230 + rows.value.length).padStart(4, '0');
  rows.value = [
    {
      id: `sales-order-local-${Date.now()}`,
      code: `SO-2026-${sequence}`,
      contract: '未关联',
      customer: draft.customer.trim(),
      amount: draft.amount.trim(),
      invoicePct: '0%',
      shipPct: '0%',
      collectPct: '0%',
      status: '草稿',
      date: draft.date,
      warehouse: draft.warehouse,
      owner: draft.owner,
      updatedAt: '2026-07-15 10:00',
    },
    ...rows.value,
  ];
  createOpen.value = false;
  toast.success('销售订单已保存为本地草稿');
}

function confirmAction() {
  if (!pendingAction.value) return;
  toast.success(`${pendingAction.value.label}已在本地完成`);
  actionOpen.value = false;
  pendingAction.value = null;
}

function exportOrders(exportRows: SalesOrderRow[]) {
  const columns = salesOrdersModel.columns.map((column) => column.key);
  const content = [
    salesOrdersModel.columns.map((column) => column.title),
    ...exportRows.map((row) => columns.map((key) => String(row[key] ?? ''))),
  ]
    .map((line) => line.map((value) => `"${value.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const url = URL.createObjectURL(new Blob([`\uFEFF${content}`], { type: 'text/csv;charset=utf-8' }));
  const link = document.createElement('a');
  link.href = url;
  link.download = 'sales-orders.csv';
  link.click();
  URL.revokeObjectURL(url);
  toast.success(`已导出 ${exportRows.length} 笔销售订单`);
}
</script>

<template>
  <ListPageSurface
    :model="pageModel"
    @action="runAction"
    @row="openDetails"
  >
    <template #hero-icon>
      <span :class="$style.heroIcon">
        <ReceiptText
          :size="22"
          :stroke-width="1.8"
        />
      </span>
    </template>
    <template #hero-art>
      <div
        :class="$style.heroArt"
        aria-hidden="true"
      >
        <BarChart3
          :size="92"
          :stroke-width="1.2"
        />
        <span :class="$style.heroReceipt">
          <ReceiptText
            :size="56"
            :stroke-width="1.2"
          />
        </span>
      </div>
    </template>
    <template
      v-if="noticeVisible"
      #notice
    >
      <div :class="$style.notice">
        <Info
          :size="16"
          :stroke-width="1.8"
        />
        <strong>90% 业务直接用销售订单 · 何时需要先签合同？</strong>
        <button
          type="button"
          :class="$style.noticeLink"
          @click="toast.info('销售订单适用于直接成交，长期框架合作建议先签销售合同')"
        >
          查看说明 →
        </button>
        <button
          type="button"
          :class="$style.noticeClose"
          aria-label="关闭订单说明"
          @click="noticeVisible = false"
        >
          <X
            :size="14"
            :stroke-width="1.8"
          />
        </button>
      </div>
    </template>
  </ListPageSurface>

  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? '销售订单详情'"
    :description="activeRow ? `${activeRow.customer} · ${activeRow.status}` : undefined"
    size="lg"
  >
    <dl
      v-if="activeRow"
      :class="$style.detailGrid"
    >
      <div
        v-for="column in salesOrdersModel.columns"
        :key="column.key"
      >
        <dt>{{ column.title }}</dt>
        <dd>{{ activeRow[column.key] || '—' }}</dd>
      </div>
    </dl>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="detailOpen = false"
        >关闭</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="createOpen"
    title="新建销售订单"
    description="创建仅保存在当前本地会话中的销售订单草稿"
    size="lg"
  >
    <form
      :class="$style.formGrid"
      @submit.prevent="saveOrder"
    >
      <UiFormField
        label="客户"
        required
        wide
      >
        <UiInput
          v-model="draft.customer"
          placeholder="请输入客户名称"
        />
      </UiFormField>
      <UiFormField
        label="订单金额"
        required
      >
        <UiInput
          v-model="draft.amount"
          placeholder="例如：¥128,000"
        />
      </UiFormField>
      <UiFormField label="计划交货">
        <UiInput v-model="draft.date" />
      </UiFormField>
      <UiFormField label="仓库">
        <UiSelect
          v-model="draft.warehouse"
          :options="warehouseOptions"
          label="仓库"
        />
      </UiFormField>
      <UiFormField label="创建人">
        <UiSelect
          v-model="draft.owner"
          :options="ownerOptions"
          label="创建人"
        />
      </UiFormField>
    </form>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="createOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="saveOrder"
        >保存草稿</UiButton
      >
    </template>
  </UiDialog>

  <UiDialog
    v-model:open="actionOpen"
    :title="pendingAction?.label ?? '订单操作'"
    description="该操作仅作用于本地复刻页面，不会请求外部 Demo。"
    size="sm"
  >
    <p :class="$style.actionMessage">确认执行“{{ pendingAction?.label }}”？完成后会通过统一消息反馈结果。</p>
    <template #footer>
      <UiButton
        variant="secondary"
        @click="actionOpen = false"
        >取消</UiButton
      >
      <UiButton
        variant="primary"
        @click="confirmAction"
        >确认执行</UiButton
      >
    </template>
  </UiDialog>
</template>

<style module>
@reference "../../../styles/index.css";

.detailGrid {
  @apply grid grid-cols-2 gap-x-6 gap-y-4 p-5;
}

.heroIcon {
  @apply flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border;
  color: white;
  border-color: color-mix(in srgb, white 35%, transparent);
  background: color-mix(in srgb, white 14%, transparent);
}

.heroArt {
  @apply absolute bottom-0 right-40 flex items-end gap-24;
  color: color-mix(in srgb, white 55%, transparent);
}

.heroReceipt {
  @apply flex h-24 w-24 items-center justify-center border-t;
  background: color-mix(in srgb, white 82%, transparent);
  color: color-mix(in srgb, var(--ui-success) 78%, transparent);
  border-color: color-mix(in srgb, white 62%, transparent);
}

.notice {
  @apply flex min-h-10 w-fit max-w-full items-center gap-2 rounded-full border px-3 text-[12px];
  color: var(--ui-text-muted);
  border-color: color-mix(in srgb, var(--ui-success) 35%, transparent);
  background: color-mix(in srgb, var(--ui-success-soft) 76%, white);
}

.notice svg {
  color: var(--ui-success);
}

.noticeLink {
  @apply font-semibold;
  color: var(--ui-success);
}

.noticeClose {
  @apply flex h-6 w-6 items-center justify-center rounded-full transition-colors;
  color: var(--ui-text-subtle);
}

.noticeClose:hover {
  background: var(--ui-surface-hover);
}

.detailGrid dt {
  @apply text-[11px] font-semibold;
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1 text-[13px];
  color: var(--ui-text);
}

.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}

.actionMessage {
  @apply p-5 text-[13px] leading-6;
  color: var(--ui-text-muted);
}
</style>
