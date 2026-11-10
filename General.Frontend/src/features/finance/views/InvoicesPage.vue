<script setup lang="ts">
import { FileCheck2, ReceiptText, ShieldCheck } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabPanel, UiTabs, toast } from '@/components/ui';
import {
  invoiceMetrics,
  invoiceTasks,
  invoicesReferenceFixture,
  invoiceTrend,
  uninvoicedItems,
} from '../model/invoices.data';
import type {
  InvoiceDirection,
  InvoiceRange,
  InvoiceRecord,
  InvoiceSection,
  InvoiceTask,
} from '../model/invoices.types';
import InvoiceOverview from './invoices/InvoiceOverview.vue';
import InvoiceRecordsPanel from './invoices/InvoiceRecordsPanel.vue';
import InvoicesHero from './invoices/InvoicesHero.vue';
import InvoicesSectionNav from './invoices/InvoicesSectionNav.vue';
import InvoiceSummary from './invoices/InvoiceSummary.vue';

const localRows = ref(cloneReferenceRows(invoicesReferenceFixture.rows));
const localTasks = ref<InvoiceTask[]>(invoiceTasks.map((task) => ({ ...task })));
const activeSection = ref<InvoiceSection>(invoicesReferenceFixture.defaults.activeSection as InvoiceSection);
const activeRange = ref<InvoiceRange>(invoicesReferenceFixture.defaults.activeRange as InvoiceRange);
const query = ref('');
const detailOpen = ref(false);
const recognitionOpen = ref(false);
const processOpen = ref(false);
const activeRow = ref<InvoiceRecord | null>(null);
const activeTask = ref<InvoiceTask | null>(null);
const recognitionDirection = ref<InvoiceDirection>('销项');
const selectedFileName = ref('');

const normalizedQuery = computed(() => query.value.trim().toLocaleLowerCase('zh-CN'));
const filteredInvoiceRows = computed(() => {
  const direction = activeSection.value === 'outbound' ? '销项' : '进项';
  return localRows.value.filter((row) => {
    const matchesDirection = row.direction === direction;
    const matchesQuery =
      !normalizedQuery.value ||
      `${row.number} ${row.party} ${row.referenceCode} ${row.remark}`
        .toLocaleLowerCase('zh-CN')
        .includes(normalizedQuery.value);
    return matchesDirection && matchesQuery;
  });
});
const filteredUninvoicedRows = computed(() =>
  uninvoicedItems.filter(
    (row) =>
      !normalizedQuery.value ||
      `${row.referenceCode} ${row.party} ${row.direction} ${row.status}`
        .toLocaleLowerCase('zh-CN')
        .includes(normalizedQuery.value),
  ),
);
const filteredTasks = computed(() =>
  localTasks.value.filter(
    (task) =>
      !normalizedQuery.value ||
      `${task.code} ${task.party} ${task.applicant} ${task.status}`
        .toLocaleLowerCase('zh-CN')
        .includes(normalizedQuery.value),
  ),
);
const pendingTasks = computed(() => localTasks.value.filter((task) => task.status === '待处理'));
const pendingAmount = computed(() => pendingTasks.value.reduce((total, task) => total + task.amount, 0));

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}

function updateSection(value: string) {
  activeSection.value = value as InvoiceSection;
  query.value = '';
}

function handleTasks() {
  activeSection.value = 'tasks';
  query.value = '';
}

function openDetail(row: InvoiceRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function openRecognition(direction: InvoiceDirection) {
  recognitionDirection.value = direction;
  selectedFileName.value = '';
  recognitionOpen.value = true;
}

function selectRecognitionFile(event: Event) {
  const input = event.target as HTMLInputElement;
  selectedFileName.value = input.files?.[0]?.name ?? '';
}

function confirmRecognition() {
  if (!selectedFileName.value) {
    toast.warning('请先选择本地发票文件');
    return;
  }

  const isOutbound = recognitionDirection.value === '销项';
  const created: InvoiceRecord = {
    id: `invoice-local-${Date.now()}`,
    number: isOutbound ? '34100226-LOCAL0001' : '31200226-LOCAL0001',
    direction: recognitionDirection.value,
    date: '2026-09-16',
    party: isOutbound ? '本地识别客户' : '本地识别供应商',
    type: '专票',
    amount: 62_000,
    tax: 7_133,
    referenceCode: isOutbound ? 'SO-LOCAL-001' : 'PO-LOCAL-001',
    remark: `由 ${selectedFileName.value} 本地识别`,
    verified: false,
  };
  localRows.value = [created, ...localRows.value];
  activeSection.value = isOutbound ? 'outbound' : 'inbound';
  recognitionOpen.value = false;
  toast.success(`已在本地识别 1 张${recognitionDirection.value}发票`);
}

function openTask(task: InvoiceTask) {
  activeTask.value = task;
  processOpen.value = true;
}

function confirmTask() {
  if (!activeTask.value) return;
  const taskId = activeTask.value.id;
  localTasks.value = localTasks.value.map((task) => (task.id === taskId ? { ...task, status: '已开票' } : task));
  processOpen.value = false;
  toast.success(`${activeTask.value.code} 已在本地标记为已开票`);
}

function exportRows() {
  let header: string[];
  let lines: string[][];

  if (activeSection.value === 'uninvoiced') {
    header = ['方向', '关联单据', '往来单位', '待开票金额', '预计日期', '状态'];
    lines = filteredUninvoicedRows.value.map((row) => [
      row.direction,
      row.referenceCode,
      row.party,
      currency(row.amount),
      row.dueDate,
      row.status,
    ]);
  } else if (activeSection.value === 'tasks') {
    header = ['申请编号', '申请时间', '客户', '发票类型', '开票金额', '申请人', '状态'];
    lines = filteredTasks.value.map((task) => [
      task.code,
      task.requestedAt,
      task.party,
      task.type,
      currency(task.amount),
      task.applicant,
      task.status,
    ]);
  } else {
    const sourceRows = activeSection.value === 'overview' ? localRows.value : filteredInvoiceRows.value;
    header = ['发票号码', '方向', '日期', '对方单位', '类型', '含税金额', '税额', '关联单据', '备注'];
    lines = sourceRows.map((row) => [
      row.number,
      row.direction,
      row.date,
      row.party,
      row.type,
      currency(row.amount),
      currency(row.tax),
      row.referenceCode,
      row.remark,
    ]);
  }

  const csv = [header, ...lines]
    .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'finance-invoices.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${lines.length} 条发票数据`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/invoices"
    data-page-label="发票管理"
    data-page-module="财务管理"
    data-module-finance-invoices
    :data-active-section="activeSection"
    :data-local-row-count="localRows.length"
    :data-pending-task-count="pendingTasks.length"
  >
    <InvoicesHero />
    <InvoiceSummary
      :metrics="invoiceMetrics"
      :pending-count="pendingTasks.length"
      :pending-amount="pendingAmount"
      @handle-tasks="handleTasks"
    />

    <UiTabs
      :model-value="activeSection"
      @update:model-value="updateSection"
    >
      <InvoicesSectionNav
        :active-range="activeRange"
        :pending-task-count="pendingTasks.length"
        @update:active-range="activeRange = $event"
        @recognize="openRecognition"
        @export="exportRows"
      />

      <UiTabPanel value="overview">
        <InvoiceOverview
          :range="activeRange"
          :trend="invoiceTrend"
          :recent-rows="localRows"
        />
      </UiTabPanel>

      <UiTabPanel value="outbound">
        <InvoiceRecordsPanel
          section="outbound"
          :rows="filteredInvoiceRows"
          :uninvoiced-rows="filteredUninvoicedRows"
          :tasks="filteredTasks"
          :query="query"
          @update:query="query = $event"
          @view="openDetail"
          @process="openTask"
        />
      </UiTabPanel>

      <UiTabPanel value="inbound">
        <InvoiceRecordsPanel
          section="inbound"
          :rows="filteredInvoiceRows"
          :uninvoiced-rows="filteredUninvoicedRows"
          :tasks="filteredTasks"
          :query="query"
          @update:query="query = $event"
          @view="openDetail"
          @process="openTask"
        />
      </UiTabPanel>

      <UiTabPanel value="uninvoiced">
        <InvoiceRecordsPanel
          section="uninvoiced"
          :rows="filteredInvoiceRows"
          :uninvoiced-rows="filteredUninvoicedRows"
          :tasks="filteredTasks"
          :query="query"
          @update:query="query = $event"
          @view="openDetail"
          @process="openTask"
        />
      </UiTabPanel>

      <UiTabPanel value="tasks">
        <InvoiceRecordsPanel
          section="tasks"
          :rows="filteredInvoiceRows"
          :uninvoiced-rows="filteredUninvoicedRows"
          :tasks="filteredTasks"
          :query="query"
          @update:query="query = $event"
          @view="openDetail"
          @process="openTask"
        />
      </UiTabPanel>
    </UiTabs>

    <UiDialog
      v-model:open="detailOpen"
      :title="activeRow ? `发票详情 · ${activeRow.number}` : '发票详情'"
      :description="activeRow ? `${activeRow.direction}发票 · ${activeRow.party}` : undefined"
      size="lg"
    >
      <dl
        v-if="activeRow"
        :class="$style.detailGrid"
      >
        <div>
          <dt>开票日期</dt>
          <dd>{{ activeRow.date }}</dd>
        </div>
        <div>
          <dt>发票类型</dt>
          <dd>{{ activeRow.type }}</dd>
        </div>
        <div>
          <dt>含税金额</dt>
          <dd>{{ currency(activeRow.amount) }}</dd>
        </div>
        <div>
          <dt>税额</dt>
          <dd>{{ currency(activeRow.tax) }}</dd>
        </div>
        <div>
          <dt>关联单据</dt>
          <dd>{{ activeRow.referenceCode }}</dd>
        </div>
        <div>
          <dt>税务校验</dt>
          <dd>{{ activeRow.verified ? '已认证' : '待认证' }}</dd>
        </div>
        <div :class="$style.detailWide">
          <dt>备注</dt>
          <dd>{{ activeRow.remark }}</dd>
        </div>
      </dl>
      <template #icon>
        <span :class="$style.dialogIcon">
          <ShieldCheck
            :size="18"
            :stroke-width="1.8"
          />
        </span>
      </template>
      <template #footer>
        <UiButton
          variant="primary"
          size="sm"
          @click="detailOpen = false"
          >关闭</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="recognitionOpen"
      :title="`${recognitionDirection}发票批量识别`"
      description="选择本地发票图片或 PDF，识别结果仅保存到当前本地会话"
      size="md"
    >
      <div :class="$style.recognitionBody">
        <label :class="$style.filePicker">
          <ReceiptText
            :size="28"
            :stroke-width="1.8"
          />
          <strong>{{ selectedFileName || '选择发票文件' }}</strong>
          <small>支持 JPG、PNG、PDF</small>
          <input
            type="file"
            accept="image/jpeg,image/png,application/pdf"
            @change="selectRecognitionFile"
          />
        </label>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="recognitionOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          size="sm"
          @click="confirmRecognition"
          >开始识别</UiButton
        >
      </template>
    </UiDialog>

    <UiDialog
      v-model:open="processOpen"
      :title="activeTask ? `确认开票 · ${activeTask.code}` : '确认开票'"
      :description="activeTask ? `${activeTask.party} · ${currency(activeTask.amount)}` : undefined"
      size="sm"
    >
      <div :class="$style.processBody">
        <FileCheck2
          :size="24"
          :stroke-width="1.8"
        />
        <p>确认已完成该申请的发票开具。确认后当前任务将标记为“已开票”。</p>
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          size="sm"
          @click="processOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="danger"
          size="sm"
          @click="confirmTask"
          >确认已开票</UiButton
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
  @apply m-0 grid grid-cols-2 gap-4 p-5;
}

.detailGrid > div {
  @apply rounded border p-3;
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

.detailWide {
  @apply col-span-2;
}

.dialogIcon {
  @apply flex h-9 w-9 shrink-0 items-center justify-center rounded-full;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.recognitionBody {
  @apply p-5;
}

.filePicker {
  @apply flex min-h-[170px] cursor-pointer flex-col items-center justify-center rounded border border-dashed transition-colors;
  color: var(--ui-text-subtle);
  border-color: var(--app-primary-light);
  background: var(--ui-surface-subtle);
}

.filePicker:hover {
  background: var(--app-primary-soft);
}

.filePicker svg {
  color: var(--app-primary);
}

.filePicker strong {
  @apply mt-3 text-[12px];
  color: var(--ui-text);
}

.filePicker small {
  @apply mt-1 text-[10px];
}

.filePicker input {
  @apply sr-only;
}

.processBody {
  @apply flex items-start gap-3 p-5;
  color: var(--ui-text-muted);
}

.processBody svg {
  @apply shrink-0;
  color: #ff5630;
}

.processBody p {
  @apply m-0 text-[12px] leading-5;
}
</style>
