<script setup lang="ts">
import { computed, ref } from 'vue';
import { CalendarDays, Plus, RefreshCcw, Settings, UserRoundCheck } from 'lucide-vue-next';
import { cloneReferenceRows } from '@/components/page';
import {
  UiAlertDialog,
  UiButton,
  UiDialog,
  UiInput,
  UiSelect,
  UiTabPanel,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
  type UiSelectOption,
} from '@/components/ui';
import { leaveBalances, leaveReferenceFixture } from '../model/leave.data';
import type { LeaveDraft, LeaveRecord, LeaveStatus, LeaveType } from '../model/leave.types';
import LeaveAuxiliaryPanels from './leave/LeaveAuxiliaryPanels.vue';
import LeaveBalanceGrid from './leave/LeaveBalanceGrid.vue';
import LeaveFormDialog from './leave/LeaveFormDialog.vue';
import LeaveHero from './leave/LeaveHero.vue';
import LeaveRecordsTable from './leave/LeaveRecordsTable.vue';

type LeaveTab = '我的假期' | '审批管理' | '调休补假' | '假期配置';

const rows = ref<LeaveRecord[]>(cloneReferenceRows(leaveReferenceFixture.rows));
const tab = ref<LeaveTab>('我的假期');
const query = ref('');
const type = ref<'全部类型' | LeaveType>('全部类型');
const status = ref<'全部状态' | LeaveStatus>('全部状态');
const formOpen = ref(false);
const detailOpen = ref(false);
const withdrawOpen = ref(false);
const activeRow = ref<LeaveRecord | null>(null);

const typeOptions: UiSelectOption[] = [
  '全部类型',
  '年假',
  '病假',
  '事假',
  '婚假',
  '产假',
  '陪产假',
  '丧假',
  '调休',
  '哺乳假',
].map((value) => ({ value, label: value }));
const statusOptions: UiSelectOption[] = ['全部状态', '待审批', '已批准', '已驳回', '已撤回'].map((value) => ({
  value,
  label: value,
}));

const filteredRows = computed(() => {
  const keyword = query.value.trim().toLowerCase();
  return rows.value.filter(
    (row) =>
      (type.value === '全部类型' || row.type === type.value) &&
      (status.value === '全部状态' || row.status === status.value) &&
      (!keyword || row.code.toLowerCase().includes(keyword) || row.reason.toLowerCase().includes(keyword)),
  );
});

function openDetails(row: LeaveRecord) {
  activeRow.value = row;
  detailOpen.value = true;
}

function requestWithdraw(row: LeaveRecord) {
  activeRow.value = row;
  withdrawOpen.value = true;
}

function confirmWithdraw() {
  if (!activeRow.value) return;
  rows.value = rows.value.map((row) => (row.id === activeRow.value?.id ? { ...row, status: '已撤回' } : row));
  withdrawOpen.value = false;
  toast.success('请假申请已撤回');
}

function saveDraft(draft: LeaveDraft) {
  const start = new Date(`${draft.startDate}T00:00:00`);
  const end = new Date(`${draft.endDate}T00:00:00`);
  const days = Math.floor((end.getTime() - start.getTime()) / 86_400_000) + 1;
  rows.value = [
    {
      id: `leave-local-${Date.now()}`,
      code: `LV-2026-${String(rows.value.length + 8).padStart(3, '0')}`,
      type: draft.type,
      startDate: draft.startDate,
      endDate: draft.endDate,
      days,
      reason: draft.reason,
      status: '待审批',
      approver: '李总监',
    },
    ...rows.value,
  ];
  formOpen.value = false;
  tab.value = '我的假期';
  toast.success('请假申请已提交');
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/attendance/leave"
    data-leave-page
    :data-active-tab="tab"
  >
    <LeaveHero />
    <UiTabs v-model="tab">
      <div :class="$style.tabBar">
        <UiTabsList label="请假管理视图">
          <UiTabsTrigger
            value="我的假期"
            :class="$style.tab"
            ><CalendarDays
              :size="14"
              :stroke-width="1.8"
            />我的假期</UiTabsTrigger
          >
          <UiTabsTrigger
            value="审批管理"
            :class="$style.tab"
            ><UserRoundCheck
              :size="14"
              :stroke-width="1.8"
            />审批管理<span :class="$style.badge">3</span></UiTabsTrigger
          >
          <UiTabsTrigger
            value="调休补假"
            :class="$style.tab"
            ><RefreshCcw
              :size="14"
              :stroke-width="1.8"
            />调休补假<span :class="$style.badge">2</span></UiTabsTrigger
          >
          <UiTabsTrigger
            value="假期配置"
            :class="$style.tab"
            ><Settings
              :size="14"
              :stroke-width="1.8"
            />假期配置</UiTabsTrigger
          >
        </UiTabsList>
        <UiButton
          variant="primary"
          size="sm"
          @click="formOpen = true"
          ><template #leading
            ><Plus
              :size="14"
              :stroke-width="1.8" /></template
          >新建请假</UiButton
        >
      </div>

      <UiTabPanel value="我的假期">
        <LeaveBalanceGrid :balances="leaveBalances" />
        <div :class="$style.filters">
          <UiInput
            v-model="query"
            :class="$style.search"
            aria-label="搜索请假记录"
            placeholder="搜索单号/事由..."
          />
          <UiSelect
            v-model="type"
            :class="$style.select"
            :options="typeOptions"
            label="全部类型"
            size="sm"
          />
          <UiSelect
            v-model="status"
            :class="$style.select"
            :options="statusOptions"
            label="全部状态"
            size="sm"
          />
        </div>
        <LeaveRecordsTable
          :rows="filteredRows"
          @view="openDetails"
          @withdraw="requestWithdraw"
        />
      </UiTabPanel>
      <UiTabPanel value="审批管理"><LeaveAuxiliaryPanels tab="审批管理" /></UiTabPanel>
      <UiTabPanel value="调休补假"><LeaveAuxiliaryPanels tab="调休补假" /></UiTabPanel>
      <UiTabPanel value="假期配置"><LeaveAuxiliaryPanels tab="假期配置" /></UiTabPanel>
    </UiTabs>
  </main>

  <LeaveFormDialog
    v-model:open="formOpen"
    @save="saveDraft"
  />
  <UiDialog
    v-model:open="detailOpen"
    :title="activeRow?.code ?? '请假详情'"
    :description="activeRow ? `${activeRow.type} · ${activeRow.status}` : undefined"
    size="sm"
  >
    <dl
      v-if="activeRow"
      :class="$style.details"
    >
      <div>
        <dt>请假日期</dt>
        <dd>{{ activeRow.startDate }} 至 {{ activeRow.endDate }}</dd>
      </div>
      <div>
        <dt>请假天数</dt>
        <dd>{{ activeRow.days }} 天</dd>
      </div>
      <div>
        <dt>审批人</dt>
        <dd>{{ activeRow.approver }}</dd>
      </div>
      <div>
        <dt>请假事由</dt>
        <dd>{{ activeRow.reason }}</dd>
      </div>
    </dl>
    <template #footer
      ><UiButton
        variant="primary"
        size="sm"
        @click="detailOpen = false"
        >关闭</UiButton
      ></template
    >
  </UiDialog>
  <UiAlertDialog
    v-model:open="withdrawOpen"
    title="撤回请假申请"
    :description="`确认撤回 ${activeRow?.code ?? ''}？撤回后可重新提交。`"
    confirm-label="确认撤回"
    tone="warning"
    @confirm="confirmWithdraw"
  />
</template>

<style module>
@reference "../../../styles/index.css";
.page {
  @apply min-h-full w-full pb-8;
  color: var(--ui-text);
}
.tabBar {
  @apply mb-3 flex h-[56px] items-center justify-between border-b;
  border-color: var(--ui-border);
}
.tab {
  @apply h-[56px] rounded-none px-4 text-[12px];
  background: transparent;
}
.tab[data-state='active'] {
  color: var(--app-primary);
  background: transparent;
  box-shadow: inset 0 -2px 0 var(--app-primary);
}
.badge {
  @apply flex h-5 min-w-5 items-center justify-center rounded-full px-1 text-[9px] font-semibold text-white;
  background: #ff5630;
}
.filters {
  @apply mt-4 flex items-center gap-3;
}
.search {
  @apply w-[285px];
}
.select {
  @apply w-[115px];
}
.details {
  @apply grid grid-cols-2 gap-4 p-5;
}
.details dt {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.details dd {
  @apply mt-1 text-[12px] font-medium;
}
</style>
