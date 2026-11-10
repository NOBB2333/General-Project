<script setup lang="ts">
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import { UiTabPanel, UiTabs, toast } from '@/components/ui';
import { draftMetrics, draftsReferenceFixture, payableDraftRows } from '../model/drafts.data';
import type {
  DraftPoolStatus,
  DraftRegistration,
  DraftRow,
  DraftSection,
  PayableDraftRegistration,
} from '../model/drafts.types';
import DraftDetailDialog from './drafts/DraftDetailDialog.vue';
import DraftsHero from './drafts/DraftsHero.vue';
import DraftRecognitionDialog from './drafts/DraftRecognitionDialog.vue';
import DraftRegistrationDialog from './drafts/DraftRegistrationDialog.vue';
import DraftsSectionNav from './drafts/DraftsSectionNav.vue';
import DraftsSummary from './drafts/DraftsSummary.vue';
import DraftsTable from './drafts/DraftsTable.vue';
import PayableDraftDialog from './drafts/PayableDraftDialog.vue';

const sectionIds: readonly DraftSection[] = ['receivable', 'warnings', 'payable', 'history'];
const statusMap: Record<Exclude<DraftPoolStatus, 'all'>, DraftRow['status']> = {
  held: '持有中',
  endorsed: '已背书',
  discounted: '已贴现',
};
const localRows = ref<DraftRow[]>(cloneReferenceRows(draftsReferenceFixture.rows));
const localPayableRows = ref<DraftRow[]>(cloneReferenceRows(payableDraftRows));
const activeSection = ref<DraftSection>(draftsReferenceFixture.defaults.activeSection as DraftSection);
const activeStatus = ref<DraftPoolStatus>(draftsReferenceFixture.defaults.activeStatus as DraftPoolStatus);
const acceptance = ref(String(draftsReferenceFixture.defaults.acceptance));
const query = ref('');
const registerOpen = ref(false);
const recognitionOpen = ref(false);
const payableOpen = ref(false);
const detailOpen = ref(false);
const activeRow = ref<DraftRow | null>(null);

const sectionRows = computed(() => {
  if (activeSection.value === 'warnings') {
    return localRows.value.filter((row) => row.status === '持有中' && row.remainingDays <= 30);
  }
  if (activeSection.value === 'payable') return localPayableRows.value;
  if (activeSection.value === 'history') return localRows.value.filter((row) => row.status !== '持有中');
  return localRows.value;
});
const filteredRows = computed(() => {
  const normalized = query.value.trim().toLocaleLowerCase('zh-CN');
  const selectedStatus = activeStatus.value;
  return sectionRows.value.filter((row) => {
    const matchesStatus =
      activeSection.value !== 'receivable' || selectedStatus === 'all' || row.status === statusMap[selectedStatus];
    const matchesAcceptance = !acceptance.value || row.acceptanceKind === acceptance.value;
    const matchesQuery =
      !normalized ||
      `${row.code} ${row.noteNumber} ${row.bank} ${row.drawer} ${row.acceptor}`
        .toLocaleLowerCase('zh-CN')
        .includes(normalized);
    return matchesStatus && matchesAcceptance && matchesQuery;
  });
});
const warningCount = computed(
  () => localRows.value.filter((row) => row.status === '持有中' && row.remainingDays <= 30).length,
);

function updateSection(value: string) {
  activeSection.value = value as DraftSection;
  activeStatus.value = 'all';
  acceptance.value = '';
  query.value = '';
}

function remainingDays(dueDate: string) {
  const milliseconds = new Date(dueDate).getTime() - new Date('2026-09-16').getTime();
  return Math.max(Math.ceil(milliseconds / 86_400_000), 0);
}

function registerReceivable(value: DraftRegistration) {
  const created: DraftRow = {
    id: `note-local-${Date.now()}`,
    code: `NT-LOCAL-${String(localRows.value.length + 1).padStart(4, '0')}`,
    direction: '应收',
    noteNumber: value.noteNumber.trim(),
    bank: value.acceptor.trim() || '待补充承兑机构',
    acceptanceKind: value.acceptanceKind,
    medium: value.medium,
    drawer: value.drawer.trim(),
    acceptor: value.acceptor.trim() || value.drawer.trim(),
    amount: Number(value.amount),
    issueDate: value.issueDate,
    dueDate: value.dueDate,
    remainingDays: remainingDays(value.dueDate),
    status: '持有中',
  };
  localRows.value = [created, ...localRows.value];
  updateSection('receivable');
  registerOpen.value = false;
  toast.success(`${created.code} 已登记到本地应收票据池`);
}

function recognizeFile(fileName: string) {
  const created: DraftRow = {
    id: `note-recognized-${Date.now()}`,
    code: `NT-OCR-${String(localRows.value.length + 1).padStart(4, '0')}`,
    direction: '应收',
    noteNumber: `OCR${Date.now().toString().slice(-12)}`,
    bank: '本地识别 · 待复核',
    acceptanceKind: '银行承兑',
    medium: '电票',
    drawer: '本地识别出票人',
    acceptor: '本地识别承兑行',
    amount: 580_000,
    issueDate: '2026-09-16',
    dueDate: '2027-03-16',
    remainingDays: 181,
    status: '持有中',
  };
  localRows.value = [created, ...localRows.value];
  updateSection('receivable');
  recognitionOpen.value = false;
  toast.success(`已从 ${fileName} 本地识别 1 张票据`);
}

function createPayable(value: PayableDraftRegistration) {
  const created: DraftRow = {
    id: `payable-note-local-${Date.now()}`,
    code: `PN-LOCAL-${String(localPayableRows.value.length + 1).padStart(4, '0')}`,
    direction: '应付',
    noteNumber: `PAY${Date.now().toString().slice(-12)}`,
    bank: '中国银行 · 基本户',
    acceptanceKind: '银行承兑',
    medium: '电票',
    drawer: '本企业',
    acceptor: '中国银行 · 基本户',
    amount: Number(value.amount),
    issueDate: value.issueDate,
    dueDate: value.dueDate,
    remainingDays: remainingDays(value.dueDate),
    status: '持有中',
  };
  localPayableRows.value = [created, ...localPayableRows.value];
  updateSection('payable');
  payableOpen.value = false;
  toast.success(`已为 ${value.payee} 创建本地银承付款票据`);
}

function openDetail(row: DraftRow) {
  activeRow.value = row;
  detailOpen.value = true;
}

function exportRows() {
  const header = ['编号', '方向', '票号', '承兑类型', '出票人', '承兑人', '票面金额', '出票日', '到期日', '状态'];
  const lines = filteredRows.value.map((row) => [
    row.code,
    row.direction,
    row.noteNumber,
    `${row.acceptanceKind}${row.medium}`,
    row.drawer,
    row.acceptor,
    row.amount,
    row.issueDate,
    row.dueDate,
    row.status,
  ]);
  const csv = [header, ...lines]
    .map((line) => line.map((cell) => `"${String(cell).replace(/"/g, '""')}"`).join(','))
    .join('\n');
  const link = document.createElement('a');
  link.href = URL.createObjectURL(new Blob([`\uFEFF${csv}`], { type: 'text/csv;charset=utf-8' }));
  link.download = 'finance-drafts.csv';
  link.click();
  URL.revokeObjectURL(link.href);
  toast.success(`已导出 ${lines.length} 条票据记录`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-module-page
    data-page-family="list"
    data-page-route="/finance/drafts"
    data-page-label="汇票管理"
    data-page-module="财务管理"
    data-drafts-page
    :data-active-section="activeSection"
    :data-local-receivable-count="localRows.length"
    :data-local-payable-count="localPayableRows.length"
    :data-visible-row-count="filteredRows.length"
  >
    <DraftsHero />
    <DraftsSummary :metrics="draftMetrics" />

    <UiTabs
      :model-value="activeSection"
      @update:model-value="updateSection"
    >
      <section :class="$style.workspace">
        <DraftsSectionNav
          :receivable-count="localRows.length"
          :warning-count="warningCount"
          :payable-count="localPayableRows.length"
          @export="exportRows"
          @recognize="recognitionOpen = true"
          @create-payable="payableOpen = true"
          @create-receivable="registerOpen = true"
        />

        <UiTabPanel
          v-for="section in sectionIds"
          :key="section"
          :value="section"
        >
          <DraftsTable
            :rows="filteredRows"
            :active-status="activeStatus"
            :acceptance="acceptance"
            :query="query"
            :show-status-tabs="section === 'receivable'"
            @update:active-status="activeStatus = $event"
            @update:acceptance="acceptance = $event"
            @update:query="query = $event"
            @view="openDetail"
          />
        </UiTabPanel>
      </section>
    </UiTabs>

    <DraftRegistrationDialog
      v-model:open="registerOpen"
      @save="registerReceivable"
    />
    <DraftRecognitionDialog
      v-model:open="recognitionOpen"
      @recognize="recognizeFile"
    />
    <PayableDraftDialog
      v-model:open="payableOpen"
      @save="createPayable"
    />
    <DraftDetailDialog
      v-model:open="detailOpen"
      :row="activeRow"
    />
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
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
</style>
