<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { cloneReferenceRows } from '@/components/page';
import { UiButton, UiDialog, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  myQueueAssignees,
  myQueueMetrics,
  myQueueReferenceFixture,
  myQueueServiceModes,
  myQueueTabs,
  myQueueTypes,
  myQueueUrgencies,
} from '../model/my-queue.data';
import type { MyQueueTabId, MyQueueTicketRow } from '../model/my-queue.types';
import MyQueueFilters from './my-queue/MyQueueFilters.vue';
import MyQueueHero from './my-queue/MyQueueHero.vue';
import MyQueueMetrics from './my-queue/MyQueueMetrics.vue';
import MyQueueTable from './my-queue/MyQueueTable.vue';

const router = useRouter();
const rows = ref<MyQueueTicketRow[]>(cloneReferenceRows(myQueueReferenceFixture.rows));
const activeTab = ref<MyQueueTabId>('待接单');
const assignee = ref('张伟');
const query = ref('');
const customer = ref('');
const type = ref('');
const urgency = ref('');
const serviceMode = ref('');
const startDate = ref('');
const endDate = ref('');
const selectedTicket = ref<MyQueueTicketRow | null>(null);
const detailOpen = ref(false);

const assigneeOptions: readonly UiSelectOption[] = [
  { value: '张伟', label: '我（张伟）' },
  { value: '__all__', label: '团队全部' },
  ...myQueueAssignees.filter((name) => name !== '张伟').map((name) => ({ value: name, label: name })),
];
const typeOptions = withAllOption('全部类型', myQueueTypes);
const urgencyOptions = withAllOption('全部紧急度', myQueueUrgencies);
const serviceModeOptions = withAllOption('全部方式', myQueueServiceModes);

const assignedRows = computed(() =>
  rows.value.filter((row) => (assignee.value === '__all__' ? Boolean(row.assignee) : row.assignee === assignee.value)),
);

const groupedRows = computed<Record<MyQueueTabId, MyQueueTicketRow[]>>(() => ({
  待接单: assignedRows.value.filter((row) => row.status === '待接单'),
  待预约: assignedRows.value.filter((row) => row.status === '待预约'),
  服务中: assignedRows.value.filter((row) => row.status === '处理中'),
  我已完成: assignedRows.value.filter((row) => ['待客户确认', '待回访', '已完成', '已关闭'].includes(row.status)),
}));

const counts = computed<Record<MyQueueTabId, number>>(() => ({
  待接单: groupedRows.value.待接单.length,
  待预约: groupedRows.value.待预约.length,
  服务中: groupedRows.value.服务中.length,
  我已完成: groupedRows.value.我已完成.length,
}));

const customerOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '全部客户' },
  ...Array.from(new Set(assignedRows.value.map((row) => row.customerName)))
    .sort((left, right) => left.localeCompare(right, 'zh-CN'))
    .map((name) => ({ value: name, label: name })),
]);

const filteredRows = computed(() => {
  const normalizedQuery = query.value.trim().toLocaleLowerCase('zh-CN');
  return groupedRows.value[activeTab.value].filter((row) => {
    if (customer.value && row.customerName !== customer.value) return false;
    if (type.value && row.type !== type.value) return false;
    if (urgency.value && row.urgency !== urgency.value) return false;
    if (serviceMode.value && row.serviceMode !== serviceMode.value) return false;
    if (startDate.value && row.createdAt.slice(0, 10) < startDate.value) return false;
    if (endDate.value && row.createdAt.slice(0, 10) > endDate.value) return false;
    if (!normalizedQuery) return true;
    return [row.code, row.title, row.customerName, row.serialNumber, row.productModel, row.contactPerson].some(
      (value) => value.toLocaleLowerCase('zh-CN').includes(normalizedQuery),
    );
  });
});

const activeFilterCount = computed(
  () =>
    [query.value, customer.value, type.value, urgency.value, serviceMode.value, startDate.value, endDate.value].filter(
      Boolean,
    ).length,
);
const ownQueue = computed(() => assignee.value === '张伟');
const assigneeLabel = computed(() => (assignee.value === '__all__' ? '团队全部' : assignee.value));

function withAllOption(label: string, values: readonly string[]): UiSelectOption[] {
  return [{ value: '', label }, ...values.map((value) => ({ value, label: value }))];
}

function resetFilters() {
  query.value = '';
  customer.value = '';
  type.value = '';
  urgency.value = '';
  serviceMode.value = '';
  startDate.value = '';
  endDate.value = '';
}

function navigate(route: string) {
  void router.push(route);
}

function openTicket(row: MyQueueTicketRow) {
  selectedTicket.value = row;
  detailOpen.value = true;
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/service/my-queue"
    data-my-queue-page
    :data-active-tab="activeTab"
    :data-result-count="filteredRows.length"
  >
    <MyQueueHero
      :assignee-label="assigneeLabel"
      @navigate="navigate"
    />
    <MyQueueMetrics
      :metrics="myQueueMetrics"
      :counts="counts"
      :active-tab="activeTab"
      @select="activeTab = $event"
    />

    <UiTabs
      v-model="activeTab"
      :class="$style.tabsRoot"
    >
      <UiTabsList
        :class="$style.tabsList"
        label="待我处理状态"
      >
        <UiTabsTrigger
          v-for="tab in myQueueTabs"
          :key="tab.id"
          :class="$style.tabTrigger"
          :value="tab.id"
        >
          {{ tab.label }} <span>({{ counts[tab.id] }})</span>
        </UiTabsTrigger>
      </UiTabsList>
    </UiTabs>

    <MyQueueFilters
      v-model:query="query"
      v-model:assignee="assignee"
      v-model:customer="customer"
      v-model:type="type"
      v-model:urgency="urgency"
      v-model:service-mode="serviceMode"
      v-model:start-date="startDate"
      v-model:end-date="endDate"
      :assignee-options="assigneeOptions"
      :customer-options="customerOptions"
      :type-options="typeOptions"
      :urgency-options="urgencyOptions"
      :service-mode-options="serviceModeOptions"
      :active-filter-count="activeFilterCount"
      :result-count="filteredRows.length"
      @reset="resetFilters"
    />

    <MyQueueTable
      :rows="filteredRows"
      :active-tab="activeTab"
      :own-queue="ownQueue"
      @open="openTicket"
    />

    <UiDialog
      v-model:open="detailOpen"
      :title="selectedTicket?.code ?? '工单详情'"
      description="待处理服务工单摘要"
      size="sm"
    >
      <dl
        v-if="selectedTicket"
        :class="$style.detailGrid"
      >
        <div>
          <dt>工单标题</dt>
          <dd>{{ selectedTicket.title }}</dd>
        </div>
        <div>
          <dt>客户 / 联系人</dt>
          <dd>{{ selectedTicket.customerName }} / {{ selectedTicket.contactPerson }}</dd>
        </div>
        <div>
          <dt>产品 / SN</dt>
          <dd>{{ selectedTicket.productName }} / {{ selectedTicket.serialNumber }}</dd>
        </div>
        <div>
          <dt>状态</dt>
          <dd>{{ selectedTicket.status }}</dd>
        </div>
      </dl>
      <template #footer>
        <UiButton
          variant="secondary"
          @click="detailOpen = false"
          >关闭</UiButton
        >
        <UiButton
          variant="primary"
          @click="navigate('/service/service-tickets')"
          >前往服务工单</UiButton
        >
      </template>
    </UiDialog>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-w-0 max-w-[1500px] pb-10;
  color: var(--ui-text);
}

.tabsRoot {
  @apply mt-5 block;
}

.tabsList {
  @apply flex w-full items-center gap-1 border-b;
  border-color: var(--ui-border);
}

.tabTrigger {
  @apply min-h-[43px] rounded-none px-4 py-2.5 text-[13px] font-normal;
  color: #5e6c84;
}

.tabTrigger:hover {
  @apply bg-transparent;
  color: var(--ui-text-muted);
}

.tabTrigger span {
  color: var(--ui-text-subtle);
}

.tabTrigger[data-state='active'] {
  @apply bg-transparent font-semibold;
  color: var(--app-primary);
}

.tabTrigger[data-state='active']::after {
  @apply absolute inset-x-0 bottom-0 h-0.5;
  content: '';
  background: var(--app-primary);
}

.detailGrid {
  @apply grid gap-4 text-[12px];
}

.detailGrid dt {
  color: var(--ui-text-subtle);
}

.detailGrid dd {
  @apply mt-1 font-medium;
  color: var(--ui-text);
}
</style>
