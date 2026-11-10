<script setup lang="ts">
import { ClipboardCheck, RotateCcw } from 'lucide-vue-next';
import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiFormField, UiInput, UiSelect, UiTabPanel, UiTabs, toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { salesReturnsReferenceFixture } from '../model/sales-returns.data';
import type {
  SalesReturnDraft,
  SalesReturnRow,
  SalesReturnScope,
  SalesReturnStatusFilter,
} from '../model/sales-returns.types';
import SalesReturnsHero from './sales-returns/SalesReturnsHero.vue';
import SalesReturnsOverview from './sales-returns/SalesReturnsOverview.vue';
import SalesReturnsTable from './sales-returns/SalesReturnsTable.vue';

const reasonOptions: UiSelectOption[] = [
  { value: '客户项目取消', label: '客户项目取消' },
  { value: '发货错误', label: '发货错误' },
  { value: '质量问题', label: '质量问题' },
  { value: '规格不匹配', label: '规格不匹配' },
];

const rows = ref(cloneReferenceRows(salesReturnsReferenceFixture.rows));
const activeScope = ref<SalesReturnScope>(salesReturnsReferenceFixture.defaults.activeScope as SalesReturnScope);
const activeStatus = ref<SalesReturnStatusFilter>(
  salesReturnsReferenceFixture.defaults.activeStatus as SalesReturnStatusFilter,
);
const query = ref('');
const detailOpen = ref(false);
const createOpen = ref(false);
const activeRow = ref<SalesReturnRow | null>(null);
const draft = reactive<SalesReturnDraft>(emptyDraft());

const scopeCounts = computed<Record<SalesReturnScope, number>>(() => ({
  all: rows.value.length,
  mine: rows.value.filter((row) => row.scopeGroup === 'mine').length,
  team: rows.value.filter((row) => row.scopeGroup === 'team').length,
  followed: rows.value.filter((row) => row.followed).length,
}));
const pendingCount = computed(
  () => rows.value.filter((row) => row.status === '待审核' || row.status === '已审核').length,
);
const totalAmount = computed(() => rows.value.reduce((total, row) => total + row.amount, 0));
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    const matchesScope =
      activeScope.value === 'all' ||
      (activeScope.value === 'followed' ? row.followed : row.scopeGroup === activeScope.value);
    const matchesStatus = activeStatus.value === '全部' || row.status === activeStatus.value;
    const matchesQuery =
      !normalized ||
      `${row.code} ${row.orderCode} ${row.customer} ${row.reason}`.toLocaleLowerCase('zh-CN').includes(normalized);
    return matchesScope && matchesStatus && matchesQuery;
  });
});

function emptyDraft(): SalesReturnDraft {
  return { customer: '', orderCode: '', reason: '客户项目取消', quantity: '1', amount: '' };
}

function updateStatus(value: string) {
  activeStatus.value = value as SalesReturnStatusFilter;
}

function updateScope(value: SalesReturnScope) {
  activeScope.value = value;
}

function openCreate() {
  Object.assign(draft, emptyDraft());
  createOpen.value = true;
}

function openDetails(row: SalesReturnRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function saveReturn() {
  const amount = Number(draft.amount.replace(/[,¥\s]/g, ''));
  const quantity = Number(draft.quantity);
  if (!draft.customer.trim() || !draft.orderCode.trim()) {
    toast.warning('请填写客户和销售单号');
    return;
  }
  if (!Number.isFinite(quantity) || quantity <= 0 || !Number.isFinite(amount) || amount <= 0) {
    toast.warning('请输入有效的退货数量和金额');
    return;
  }

  const created: SalesReturnRow = {
    id: `sales-return-local-${Date.now()}`,
    scopeGroup: 'mine',
    followed: false,
    code: `SR-LOCAL-${String(rows.value.length + 1).padStart(3, '0')}`,
    orderCode: draft.orderCode.trim(),
    customer: draft.customer.trim(),
    reason: draft.reason,
    quantity,
    amount,
    status: '待审核',
    attachments: 0,
    owner: '张经理',
    requestedAt: '2026-09-16',
  };
  rows.value = [created, ...rows.value];
  activeScope.value = 'all';
  activeStatus.value = '全部';
  createOpen.value = false;
  toast.success(`${created.code} 已保存到当前本地会话`);
}

function approveReturn() {
  if (!activeRow.value) return;
  const rowId = activeRow.value.id;
  rows.value = rows.value.map((row) => (row.id === rowId ? { ...row, status: '已审核' } : row));
  activeRow.value = rows.value.find((row) => row.id === rowId) ?? null;
  toast.success(`${activeRow.value?.code} 已在本地通过审核`);
}

function exportRows() {
  const header = ['退货单号', '销售单号', '客户', '退货原因', '数量', '金额', '状态', '附件'];
  const lines = filteredRows.value.map((row) => [
    row.code,
    row.orderCode,
    row.customer,
    row.reason,
    String(row.quantity),
    `-¥${row.amount.toLocaleString('zh-CN')}`,
    row.status,
    String(row.attachments),
  ]);
  const csv = [header, ...lines]
    .map((line) => line.map((cell) => `"${cell.replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'sales-returns.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${lines.length} 笔销售退货`);
}

function currency(value: number) {
  return `-¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/sales/returns"
    data-page-label="销售退货"
    data-page-module="销售业务"
    data-sales-returns-page
    :data-active-scope="activeScope"
    :data-active-status="activeStatus"
    :data-local-row-count="rows.length"
  >
    <SalesReturnsHero />
    <SalesReturnsOverview
      :active-scope="activeScope"
      :scope-counts="scopeCounts"
      :total="rows.length"
      :pending="pendingCount"
      :amount="totalAmount"
      @update:active-scope="updateScope"
    />

    <UiTabs
      :model-value="activeStatus"
      @update:model-value="updateStatus"
    >
      <UiTabPanel
        v-for="status in ['全部', '待审核', '已审核', '已退货', '已退款']"
        :key="status"
        :value="status"
      >
        <SalesReturnsTable
          :rows="filteredRows"
          :active-status="activeStatus"
          :query="query"
          @update:query="query = $event"
          @export="exportRows"
          @create="openCreate"
          @view="openDetails"
        />
      </UiTabPanel>
    </UiTabs>

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow?.code ?? '销售退货详情'"
      :description="activeRow ? `${activeRow.customer} · ${activeRow.status}` : undefined"
      size="lg"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>销售单号</dt>
          <dd>{{ activeRow.orderCode }}</dd>
        </div>
        <div>
          <dt>申请日期</dt>
          <dd>{{ activeRow.requestedAt }}</dd>
        </div>
        <div>
          <dt>退货原因</dt>
          <dd>{{ activeRow.reason }}</dd>
        </div>
        <div>
          <dt>数量</dt>
          <dd>{{ activeRow.quantity }}</dd>
        </div>
        <div>
          <dt>退货金额</dt>
          <dd>{{ currency(activeRow.amount) }}</dd>
        </div>
        <div>
          <dt>经办人</dt>
          <dd>{{ activeRow.owner }}</dd>
        </div>
        <div>
          <dt>附件</dt>
          <dd>{{ activeRow.attachments }} 个</dd>
        </div>
        <div>
          <dt>处理状态</dt>
          <dd>{{ activeRow.status }}</dd>
        </div>
      </dl>
      <template #icon>
        <span :class="$style.dialogIcon">
          <ClipboardCheck
            :size="18"
            :stroke-width="1.8"
          />
        </span>
      </template>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="detailOpen = false"
          >关闭</UiButton
        >
        <UiButton
          v-if="activeRow?.status === '待审核'"
          variant="primary"
          size="sm"
          @click="approveReturn"
          >通过审核</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="createOpen"
      title="新建销售退货"
      description="登记客户退货申请与责任原因"
      size="lg"
    >
      <form
        :class="$style.formGrid"
        @submit.prevent="saveReturn"
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
          label="销售单号"
          required
        >
          <UiInput
            v-model="draft.orderCode"
            placeholder="例如：SO-2026-0210"
          />
        </UiFormField>
        <UiFormField
          label="退货原因"
          required
        >
          <UiSelect
            v-model="draft.reason"
            :options="reasonOptions"
            label="退货原因"
          />
        </UiFormField>
        <UiFormField
          label="退货数量"
          required
        >
          <UiInput
            v-model="draft.quantity"
            type="number"
          />
        </UiFormField>
        <UiFormField
          label="退货金额"
          required
        >
          <UiInput
            v-model="draft.amount"
            type="number"
            placeholder="0"
          />
        </UiFormField>
      </form>
      <template #icon>
        <span :class="[$style.dialogIcon, $style.returnDialogIcon]">
          <RotateCcw
            :size="18"
            :stroke-width="1.8"
          />
        </span>
      </template>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="createOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="saveReturn"
          >保存申请</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-h-full min-w-[1120px] pb-8;
  color: var(--ui-text);
  background: var(--ui-surface-subtle);
}

.detailGrid {
  @apply m-0 grid grid-cols-2 gap-3 p-5;
}

.detailGrid > div {
  @apply rounded border px-3 py-2;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-subtle);
}

.detailGrid dt,
.detailGrid dd {
  @apply m-0;
}

.detailGrid dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1 text-[12px] font-semibold;
  color: var(--ui-text);
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.returnDialogIcon {
  color: #974f0c;
  background: #fff0b3;
}

.formGrid {
  @apply grid grid-cols-2 gap-4 p-5;
}
</style>
