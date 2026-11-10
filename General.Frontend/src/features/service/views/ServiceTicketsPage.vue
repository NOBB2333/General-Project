<script setup lang="ts">
import { CircleX, Plus, Search, UserRoundCog, UserRoundPlus } from 'lucide-vue-next';
import { computed, ref } from 'vue';
import { cloneReferenceRows } from '@/components/page';
import {
  UiAlertDialog,
  UiButton,
  UiCheckbox,
  UiDialog,
  UiInput,
  UiSelect,
  UiTabs,
  UiTabsList,
  UiTabsTrigger,
  toast,
} from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import { serviceTicketsReferenceFixture } from '../model/service-tickets.data';
import { serviceTicketsModel } from '../model/service-tickets.model';
import type {
  ServiceTicketBatchAction,
  ServiceTicketDraft,
  ServiceTicketRow,
  ServiceTicketStatus,
  ServiceTicketUrgency,
} from '../model/service-tickets.types';
import ServiceTicketCreateDialog from './service-tickets/ServiceTicketCreateDialog.vue';
import ServiceTicketDetailDialog from './service-tickets/ServiceTicketDetailDialog.vue';
import ServiceTicketStats from './service-tickets/ServiceTicketStats.vue';
import ServiceTicketTable from './service-tickets/ServiceTicketTable.vue';
import ServiceTicketsHero from './service-tickets/ServiceTicketsHero.vue';

type CheckedState = boolean | 'indeterminate';

interface TicketTab {
  id: string;
  label: string;
}

const ticketTabs: readonly TicketTab[] = [
  { id: 'all', label: '全部工单' },
  { id: 'mine', label: '我的待处理' },
  { id: 'pending-acceptance', label: '待受理' },
  { id: 'pending-dispatch', label: '待分派' },
  { id: 'pending-order', label: '待接单' },
  { id: 'pending-appointment', label: '待预约' },
  { id: 'processing', label: '处理中' },
  { id: 'pending-confirmation', label: '待客户确认' },
  { id: 'pending-followup', label: '待回访' },
  { id: 'overdue', label: '已超时' },
  { id: 'billable', label: '有偿服务' },
];

const typeOptions = toSelectOptions(serviceTicketsModel.filters[0]?.options ?? []);
const urgencyOptions = toSelectOptions(serviceTicketsModel.filters[1]?.options ?? []);
const statusOptions = toSelectOptions(serviceTicketsModel.filters[2]?.options ?? []);
const engineerOptions = toSelectOptions(['王工', '李工', '赵工', '陈工', '周工']);
const customerServiceOptions = toSelectOptions(['客服组', '客服-小林', '客服-小周']);

const rows = ref(cloneReferenceRows(serviceTicketsReferenceFixture.rows));
const query = ref('');
const activeTab = ref('all');
const typeFilter = ref('全部类型');
const urgencyFilter = ref('全部紧急度');
const statusFilter = ref('全部状态');
const selectedIds = ref(new Set<string>());
const selectedTicket = ref<ServiceTicketRow | null>(null);
const detailOpen = ref(false);
const createOpen = ref(false);
const batchDialogOpen = ref(false);
const closeDialogOpen = ref(false);
const batchAction = ref<Exclude<ServiceTicketBatchAction, 'force-close'>>('dispatch');
const batchAssignee = ref(engineerOptions[0]?.value ?? '王工');

const filteredRows = computed(() => {
  const normalizedQuery = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value.filter((row) => {
    if (!matchesTab(row, activeTab.value)) return false;
    if (typeFilter.value !== '全部类型' && row.type !== typeFilter.value) return false;
    if (urgencyFilter.value !== '全部紧急度' && row.urgency !== urgencyFilter.value) return false;
    if (statusFilter.value !== '全部状态' && row.status !== statusFilter.value) return false;
    if (!normalizedQuery) return true;
    return [row.code, row.title, row.customer, row.contact, row.product, row.serialNumber, row.owner].some((value) =>
      value.toLocaleLowerCase('zh-CN').includes(normalizedQuery),
    );
  });
});

const selectedCount = computed(() => selectedIds.value.size);
const selectionState = computed<CheckedState>(() => {
  if (!filteredRows.value.length || !selectedCount.value) return false;
  return filteredRows.value.every((row) => selectedIds.value.has(row.id)) ? true : 'indeterminate';
});
const referenceCount = Number(serviceTicketsReferenceFixture.defaults.totalCount);
const localCreatedCount = computed(() => Math.max(0, rows.value.length - serviceTicketsReferenceFixture.rows.length));
const resultCount = computed(() => {
  const untouched =
    activeTab.value === 'all' &&
    !query.value.trim() &&
    typeFilter.value === '全部类型' &&
    urgencyFilter.value === '全部紧急度' &&
    statusFilter.value === '全部状态';
  return untouched ? referenceCount + localCreatedCount.value : filteredRows.value.length;
});

function toSelectOptions(values: readonly string[]): UiSelectOption[] {
  return values.map((value) => ({ value, label: value }));
}

function matchesTab(row: ServiceTicketRow, tab: string) {
  const statusByTab: Record<string, ServiceTicketStatus> = {
    'pending-acceptance': '待受理',
    'pending-dispatch': '待分派',
    'pending-order': '待接单',
    'pending-appointment': '待预约',
    processing: '处理中',
    'pending-confirmation': '待客户确认',
    'pending-followup': '待回访',
  };
  if (tab === 'all') return true;
  if (tab === 'mine') return !['已完成', '已关闭', '已驳回'].includes(row.status);
  if (tab === 'overdue') return row.overdue;
  if (tab === 'billable') return row.billable;
  return row.status === statusByTab[tab];
}

function updateSelected(id: string, checked: boolean) {
  const next = new Set(selectedIds.value);
  checked ? next.add(id) : next.delete(id);
  selectedIds.value = next;
}

function updateAllSelected(checked: boolean) {
  const next = new Set(selectedIds.value);
  filteredRows.value.forEach((row) => (checked ? next.add(row.id) : next.delete(row.id)));
  selectedIds.value = next;
}

function openDetails(row: ServiceTicketRow) {
  selectedTicket.value = row;
  detailOpen.value = true;
}

function openBatchAction(action: Exclude<ServiceTicketBatchAction, 'force-close'>) {
  if (!selectedCount.value) return;
  batchAction.value = action;
  batchAssignee.value =
    action === 'dispatch' ? (engineerOptions[0]?.value ?? '王工') : (customerServiceOptions[0]?.value ?? '客服组');
  batchDialogOpen.value = true;
}

function confirmBatchAction() {
  const selected = selectedIds.value;
  rows.value = rows.value.map((row) => {
    if (!selected.has(row.id)) return row;
    return {
      ...row,
      owner: batchAssignee.value,
      status: batchAction.value === 'dispatch' ? '待接单' : '待受理',
    };
  });
  const label = batchAction.value === 'dispatch' ? '批量派工' : '转客服';
  toast.success(`${label}已在本地完成，共 ${selectedCount.value} 单`);
  selectedIds.value = new Set();
  batchDialogOpen.value = false;
}

function confirmForceClose() {
  const selected = selectedIds.value;
  rows.value = rows.value.map((row) => (selected.has(row.id) ? { ...row, status: '已关闭', overdue: false } : row));
  toast.success(`已在本地强关 ${selectedCount.value} 单`);
  selectedIds.value = new Set();
  closeDialogOpen.value = false;
}

function saveTicket(draft: ServiceTicketDraft) {
  if (!draft.title.trim() || !draft.customer.trim()) {
    toast.warning('请填写工单标题和客户');
    return;
  }
  const sequence = String(29 + localCreatedCount.value).padStart(3, '0');
  const newRow: ServiceTicketRow = {
    id: `service-ticket-local-${Date.now()}`,
    code: `SO-20260816-${sequence}`,
    title: draft.title.trim(),
    aiRisk: false,
    type: draft.type,
    customer: draft.customer.trim(),
    contact: draft.contact.trim() || '待补充',
    product: draft.product.trim() || '待关联产品',
    serialNumber: draft.serialNumber.trim() || '未关联 SN',
    warranty: '无质保',
    urgency: draft.urgency as ServiceTicketUrgency,
    status: '待受理',
    overdue: false,
    billable: draft.type === '有偿服务',
    owner: '客服组',
    createdAt: '本地刚刚',
    response: '—',
    description: draft.description.trim() || '暂无补充说明',
  };
  rows.value = [newRow, ...rows.value];
  activeTab.value = 'all';
  typeFilter.value = '全部类型';
  urgencyFilter.value = '全部紧急度';
  statusFilter.value = '全部状态';
  query.value = '';
  createOpen.value = false;
  toast.success(`工单 ${newRow.code} 已保存到本地会话`);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/service/service-tickets"
    data-service-tickets-page
    :data-active-tab="activeTab"
    :data-result-count="filteredRows.length"
  >
    <ServiceTicketsHero />
    <ServiceTicketStats :stats="serviceTicketsModel.stats" />

    <section :class="$style.workspace">
      <UiTabs
        v-model="activeTab"
        :class="$style.tabsRoot"
      >
        <UiTabsList
          :class="$style.tabsList"
          label="服务工单状态"
        >
          <UiTabsTrigger
            v-for="tab in ticketTabs"
            :key="tab.id"
            :class="$style.tabButton"
            :value="tab.id"
          >
            {{ tab.label }}
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <div :class="$style.filtersRow">
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <UiInput
            v-model="query"
            :class="$style.searchInput"
            :placeholder="serviceTicketsModel.searchPlaceholder"
            :aria-label="serviceTicketsModel.searchPlaceholder"
          />
        </label>
        <UiSelect
          v-model="typeFilter"
          :class="$style.filterSelect"
          :options="typeOptions"
          label="服务类型"
          size="md"
        />
        <UiSelect
          v-model="urgencyFilter"
          :class="$style.filterSelect"
          :options="urgencyOptions"
          label="紧急度"
          size="md"
        />
        <UiSelect
          v-model="statusFilter"
          :class="$style.filterSelect"
          :options="statusOptions"
          label="工单状态"
          size="md"
        />
        <span :class="$style.resultCount">共 {{ resultCount }} 单</span>
      </div>

      <div :class="$style.batchBar">
        <div :class="$style.selectionSummary">
          <UiCheckbox
            :model-value="selectionState"
            label="全选当前筛选结果"
            @update:model-value="updateAllSelected($event === true)"
          />
          <strong>全选本页</strong>
          <span>已选 {{ selectedCount }} 单</span>
        </div>
        <div :class="$style.batchActions">
          <UiButton
            :class="$style.dispatchAction"
            variant="primary"
            size="sm"
            :disabled="!selectedCount"
            @click="openBatchAction('dispatch')"
          >
            <template #leading>
              <UserRoundPlus
                :size="14"
                :stroke-width="1.8"
              />
            </template>
            批量派工
          </UiButton>
          <UiButton
            variant="secondary"
            size="sm"
            :disabled="!selectedCount"
            @click="openBatchAction('transfer')"
          >
            <template #leading>
              <UserRoundCog
                :size="14"
                :stroke-width="1.8"
              />
            </template>
            转客服
          </UiButton>
          <UiButton
            :class="$style.closeAction"
            variant="secondary"
            size="sm"
            :disabled="!selectedCount"
            @click="closeDialogOpen = true"
          >
            <template #leading>
              <CircleX
                :size="14"
                :stroke-width="1.8"
              />
            </template>
            批量强关
          </UiButton>
          <UiButton
            variant="primary"
            size="sm"
            @click="createOpen = true"
          >
            <template #leading>
              <Plus
                :size="15"
                :stroke-width="1.8"
              />
            </template>
            新建工单
          </UiButton>
        </div>
      </div>

      <ServiceTicketTable
        :rows="filteredRows"
        :selected-ids="selectedIds"
        @toggle="updateSelected"
        @toggle-all="updateAllSelected"
        @open="openDetails"
      />
    </section>

    <ServiceTicketDetailDialog
      v-model:open="detailOpen"
      :ticket="selectedTicket"
    />
    <ServiceTicketCreateDialog
      v-model:open="createOpen"
      @save="saveTicket"
    />

    <UiDialog
      v-model:open="batchDialogOpen"
      :title="batchAction === 'dispatch' ? '批量派工' : '转客服'"
      :description="`将对已选择的 ${selectedCount} 张工单执行本地操作。`"
      size="sm"
    >
      <div :class="$style.batchDialogBody">
        <span>{{ batchAction === 'dispatch' ? '指派工程师' : '接收客服' }}</span>
        <UiSelect
          v-model="batchAssignee"
          :options="batchAction === 'dispatch' ? engineerOptions : customerServiceOptions"
          :label="batchAction === 'dispatch' ? '指派工程师' : '接收客服'"
        />
      </div>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="batchDialogOpen = false"
          >取消</UiButton
        >
        <UiButton
          variant="primary"
          @click="confirmBatchAction"
          >确认</UiButton
        >
      </template>
    </UiDialog>

    <UiAlertDialog
      v-model:open="closeDialogOpen"
      title="批量强关服务工单"
      :description="`确认强制关闭已选择的 ${selectedCount} 张工单？此操作只影响本地会话。`"
      confirm-label="确认强关"
      tone="danger"
      @confirm="confirmForceClose"
    />
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply min-w-0 pb-6;
  color: var(--ui-text);
}

.workspace {
  @apply mt-5 min-w-0;
}

.tabsRoot {
  @apply block;
}

.tabsList {
  @apply flex flex-wrap gap-1.5;
}

.tabButton {
  @apply min-h-7 rounded-full border px-3 py-1 text-[11px];
  color: var(--ui-text-muted);
  background: white;
  border-color: var(--ui-border);
}

.tabButton:hover {
  background: var(--ui-surface-hover);
}

.tabButton[data-state='active'] {
  color: white;
  background: var(--app-primary);
  border-color: var(--app-primary);
}

.filtersRow {
  @apply mt-5 flex min-w-0 items-center gap-2;
}

.searchField {
  @apply relative block w-[316px] shrink-0;
}

.searchField > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}

.searchInput {
  @apply pl-9;
}

.filterSelect {
  @apply min-w-[122px];
}

.resultCount {
  @apply ml-auto whitespace-nowrap text-[11px];
  color: var(--ui-text-subtle);
}

.batchBar {
  @apply mt-5 flex min-h-[49px] items-center justify-between gap-4 border bg-white px-4;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.selectionSummary,
.batchActions {
  @apply flex items-center;
}

.selectionSummary {
  @apply gap-2 text-[11px];
}

.selectionSummary strong {
  @apply font-semibold;
  color: var(--ui-text-muted);
}

.selectionSummary span {
  color: var(--ui-text-subtle);
}

.batchActions {
  @apply gap-2;
}

.dispatchAction:disabled {
  color: #6554c0;
  background: #d9d2f0;
}

.closeAction {
  color: var(--ui-danger);
  border-color: #ffbdad;
}

.batchDialogBody {
  @apply flex items-center gap-4 p-5;
}

.batchDialogBody > span {
  @apply w-20 shrink-0 text-[11px] font-semibold;
  color: var(--ui-text-muted);
}

.batchDialogBody > :last-child {
  @apply flex-1;
}

@media (max-width: 900px) {
  .filtersRow {
    @apply flex-wrap;
  }

  .resultCount {
    @apply ml-0;
  }

  .batchBar {
    @apply items-start;
  }

  .batchActions {
    @apply flex-wrap justify-end py-2;
  }
}

@media (max-width: 620px) {
  .searchField {
    @apply w-full;
  }

  .batchBar {
    @apply flex-col items-stretch py-3;
  }

  .batchActions {
    @apply justify-start;
  }
}
</style>
