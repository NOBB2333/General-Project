<script setup lang="ts">
import { Banknote } from 'lucide-vue-next';
import { computed, reactive, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import {
  UiButton,
  UiDialog,
  UiFormField,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTextarea,
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { paymentMetrics, paymentsReferenceFixture } from '../model/payments.data';
import type { PaymentDraft, PaymentSection, PaymentTaskRow } from '../model/payments.types';
import PaymentTaskTable from './payments/PaymentTaskTable.vue';
import PaymentsHero from './payments/PaymentsHero.vue';
import PaymentsSectionNav from './payments/PaymentsSectionNav.vue';

const secondarySections: readonly PaymentSection[] = ['writeoffs', 'advances', 'auto-match', 'unmatched'];
const sectionLabels: Record<PaymentSection, string> = {
  tasks: '付款任务',
  writeoffs: '付款核销',
  advances: '预付款管理',
  'auto-match': '自动流水匹配',
  unmatched: '未匹配付款流水',
};
const accountOptions: UiSelectOption[] = [
  { value: '中国工商银行 · 基本户', label: '中国工商银行 · 基本户' },
  { value: '招商银行 · 一般户', label: '招商银行 · 一般户' },
  { value: '中国建设银行 · 专用户', label: '中国建设银行 · 专用户' },
];

const localRows = ref(cloneReferenceRows(paymentsReferenceFixture.rows));
const activeSection = ref<PaymentSection>(paymentsReferenceFixture.defaults.activeSection as PaymentSection);
const activeStage = ref(String(paymentsReferenceFixture.defaults.activeStage));
const query = ref('');
const dialogOpen = ref(false);
const activeRow = ref<PaymentTaskRow | null>(null);
const draft = reactive<PaymentDraft>({
  amount: '',
  account: accountOptions[0]?.value ?? '',
  date: '2026-09-16',
  note: '',
});

const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  return localRows.value.filter((row) => {
    const matchesStage = activeStage.value === '全部' || row.stage === activeStage.value;
    const matchesQuery =
      !normalized ||
      `${row.code} ${row.referenceCode} ${row.payableCode} ${row.payee}`
        .toLocaleLowerCase('zh-CN')
        .includes(normalized);
    return matchesStage && matchesQuery;
  });
});
const activeSectionLabel = computed(() => sectionLabels[activeSection.value]);

function updateSection(value: string) {
  activeSection.value = value as PaymentSection;
}

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}

function openRegistration(row: PaymentTaskRow) {
  activeRow.value = row;
  draft.amount = String(Math.max(row.requestedAmount - row.registeredAmount, 0));
  draft.account = accountOptions[0]?.value ?? '';
  draft.date = '2026-09-16';
  draft.note = '';
  dialogOpen.value = true;
}

function saveRegistration() {
  if (!activeRow.value) return;
  const amount = Number(draft.amount.replace(/[,¥\s]/g, ''));
  if (!Number.isFinite(amount) || amount <= 0) {
    toast.warning('请输入有效的付款金额');
    return;
  }
  if (!draft.account) {
    toast.warning('请选择付款账户');
    return;
  }
  const taskId = activeRow.value.id;
  localRows.value = localRows.value.map((row) => {
    if (row.id !== taskId) return row;
    const registeredAmount = Math.min(row.registeredAmount + amount, row.requestedAmount);
    const completed = registeredAmount >= row.requestedAmount;
    return {
      ...row,
      registeredAmount,
      approvalStatus: completed ? row.approvalStatus : '部分登记',
      stage: completed ? '已完成' : '待付款',
    };
  });
  dialogOpen.value = false;
  toast.success(`已登记 ${currency(amount)}，仅保存到当前本地会话`);
}

function exportRows() {
  const header = ['付款任务编号', '来源', '关联单据号', '收款方', '申请付款金额', '应付款金额', '付款审批状态'];
  const lines = filteredRows.value.map((row) => [
    row.code,
    row.source,
    `${row.referenceCode} ${row.payableCode}`,
    row.payee,
    currency(row.requestedAmount),
    row.payableAmount === null ? '预付（应付未生成）' : currency(row.payableAmount),
    row.approvalStatus,
  ]);
  const csv = [header, ...lines]
    .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'finance-payments.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${filteredRows.value.length} 条付款任务`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/payments"
    data-page-label="付款管理"
    data-page-module="财务管理"
    data-module-finance-payments
    :data-active-section="activeSection"
    :data-local-row-count="localRows.length"
  >
    <PaymentsHero
      :metrics="paymentMetrics"
      @open-expense-center="toast.info('已定位到费用中心付款申请入口，本地演示不会提交外部数据')"
    />

    <UiTabs
      :model-value="activeSection"
      @update:model-value="updateSection"
    >
      <section :class="$style.workspace">
        <PaymentsSectionNav @export="exportRows" />

        <UiTabPanel value="tasks">
          <PaymentTaskTable
            :rows="filteredRows"
            :active-stage="activeStage"
            :query="query"
            @update:active-stage="activeStage = $event"
            @update:query="query = $event"
            @register="openRegistration"
          />
        </UiTabPanel>

        <UiTabPanel
          v-for="section in secondarySections"
          :key="section"
          :value="section"
        >
          <PaymentTaskTable
            :rows="filteredRows"
            :active-stage="activeStage"
            :query="query"
            :title="activeSectionLabel"
            @update:active-stage="activeStage = $event"
            @update:query="query = $event"
            @register="openRegistration"
          />
        </UiTabPanel>
      </section>
    </UiTabs>

    <UiDialog
      v-model:open="dialogOpen"
      :title="activeRow ? `登记付款 · ${activeRow.code}` : '登记付款'"
      :description="activeRow ? `${activeRow.payee} · 申请 ${currency(activeRow.requestedAmount)}` : undefined"
      size="lg"
    >
      <form
        :class="$style.paymentForm"
        @submit.prevent="saveRegistration"
      >
        <UiFormField
          label="本次付款金额"
          required
        >
          <UiInput
            v-model="draft.amount"
            type="number"
            placeholder="0"
          />
        </UiFormField>
        <UiFormField
          label="付款日期"
          required
        >
          <UiInput
            v-model="draft.date"
            type="date"
          />
        </UiFormField>
        <UiFormField
          label="付款账户"
          required
          wide
        >
          <UiSelect
            v-model="draft.account"
            :options="accountOptions"
            label="付款账户"
          />
        </UiFormField>
        <UiFormField
          label="付款备注"
          wide
        >
          <UiTextarea
            v-model="draft.note"
            placeholder="可选，记录银行流水号或付款说明"
            :rows="3"
          />
        </UiFormField>
      </form>
      <template #icon>
        <span :class="$style.dialogIcon">
          <Banknote
            :size="18"
            :stroke-width="1.8"
          />
        </span>
      </template>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="dialogOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="saveRegistration"
          >确认登记</UiButton
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

.workspace {
  @apply mt-5 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.paymentForm {
  @apply grid grid-cols-2 gap-4 p-5;
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
