<script setup lang="ts">
import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';
import { cloneReferenceRows } from '@/components/page';
import { toast } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import {
  dispatchEngineers,
  dispatchMetrics,
  dispatchRegions,
  dispatchServiceMethods,
  dispatchTypes,
  dispatchUrgencies,
  toDispatchReferenceFixture,
} from '../model/to-dispatch.data';
import type { DispatchEngineer, DispatchMetric, DispatchOrderRow } from '../model/to-dispatch.types';
import DispatchOrderCard from './to-dispatch/DispatchOrderCard.vue';
import DispatchSelectionBar from './to-dispatch/DispatchSelectionBar.vue';
import ToDispatchFilters from './to-dispatch/ToDispatchFilters.vue';
import ToDispatchHero from './to-dispatch/ToDispatchHero.vue';
import ToDispatchStats from './to-dispatch/ToDispatchStats.vue';

type CheckedState = boolean | 'indeterminate';

const router = useRouter();
const rows = ref<DispatchOrderRow[]>(cloneReferenceRows(toDispatchReferenceFixture.rows));
const query = ref('');
const region = ref('全部地区');
const type = ref('');
const urgency = ref('');
const selectedIds = ref(new Set<string>());
const engineerSelections = ref<Record<string, string>>(
  Object.fromEntries(rows.value.map((row) => [row.id, engineerById(row.recommendedEngineerId).name])) as Record<
    string,
    string
  >,
);
const methodSelections = ref<Record<string, string>>(
  Object.fromEntries(rows.value.map((row) => [row.id, row.serviceMode])) as Record<string, string>,
);

const regionOptions = toOptions(dispatchRegions);
const typeOptions = withAllOption('全部类型', dispatchTypes);
const urgencyOptions = withAllOption('全部紧急度', dispatchUrgencies);
const engineerOptions: readonly UiSelectOption[] = dispatchEngineers.map((engineer) => ({
  value: engineer.name,
  label: `${engineer.name} · ${engineer.team}（待${engineer.pendingCount}）`,
}));
const serviceMethodOptions = toOptions(dispatchServiceMethods);

const filteredRows = computed(() => {
  const normalizedQuery = query.value.trim().toLocaleLowerCase('zh-CN');
  return rows.value
    .filter((row) => matchesRegion(row, region.value))
    .filter((row) => !type.value || row.type === type.value)
    .filter((row) => !urgency.value || row.urgency === urgency.value)
    .filter((row) => {
      if (!normalizedQuery) return true;
      return [row.code, row.customerName, row.title].some((value) =>
        value.toLocaleLowerCase('zh-CN').includes(normalizedQuery),
      );
    })
    .sort(
      (left, right) =>
        dispatchPriority(left) - dispatchPriority(right) || left.createdAt.localeCompare(right.createdAt),
    );
});

const metricValues = computed<Record<DispatchMetric['id'], number>>(() => ({
  total: rows.value.length,
  urgent: rows.value.filter((row) => row.isShutdown || row.urgency === '紧急').length,
  overdue: rows.value.filter((row) => row.overdue).length,
  today: rows.value.filter((row) => row.createdAt.slice(0, 10) === '2026-08-16').length,
}));

const selectedVisibleIds = computed(() =>
  filteredRows.value.filter((row) => selectedIds.value.has(row.id)).map((row) => row.id),
);
const selectedCount = computed(() => selectedVisibleIds.value.length);
const selectionState = computed<CheckedState>(() => {
  if (!selectedCount.value) return false;
  return selectedCount.value === filteredRows.value.length ? true : 'indeterminate';
});

function toOptions(values: readonly string[]): UiSelectOption[] {
  return values.map((value) => ({ value, label: value }));
}

function withAllOption(label: string, values: readonly string[]): UiSelectOption[] {
  return [{ value: '', label }, ...toOptions(values)];
}

function engineerById(id: string): DispatchEngineer {
  return dispatchEngineers.find((engineer) => engineer.id === id) ?? dispatchEngineers[0]!;
}

function matchesRegion(row: DispatchOrderRow, selectedRegion: string) {
  if (selectedRegion === '全部地区') return true;
  if (selectedRegion === '远程') return row.serviceMode === '远程支持';
  if (selectedRegion === '其他') {
    return !['上海', '北京', '广州'].some((city) => row.address.includes(city)) && row.serviceMode !== '远程支持';
  }
  return row.address.includes(selectedRegion);
}

function dispatchPriority(row: DispatchOrderRow) {
  return (row.isShutdown ? 0 : 1) + (row.urgency === '紧急' ? 0 : row.urgency === '高' ? 1 : 2);
}

function toggleOrder(id: string, checked: boolean) {
  const next = new Set(selectedIds.value);
  checked ? next.add(id) : next.delete(id);
  selectedIds.value = next;
}

function toggleAll(checked: boolean) {
  const next = new Set(selectedIds.value);
  filteredRows.value.forEach((row) => (checked ? next.add(row.id) : next.delete(row.id)));
  selectedIds.value = next;
}

function updateEngineer(id: string, value: string) {
  engineerSelections.value = { ...engineerSelections.value, [id]: value };
}

function updateServiceMethod(id: string, value: string) {
  methodSelections.value = { ...methodSelections.value, [id]: value };
}

function dispatchOrder(order: DispatchOrderRow) {
  const engineer = engineerSelections.value[order.id] ?? engineerById(order.recommendedEngineerId).name;
  rows.value = rows.value.filter((row) => row.id !== order.id);
  toggleOrder(order.id, false);
  toast.success(`工单 ${order.code} 已派给 ${engineer}`);
}

function dispatchSelected() {
  if (!selectedCount.value) {
    toast.error('请先勾选工单');
    return;
  }
  const selected = new Set(selectedVisibleIds.value);
  rows.value = rows.value.filter((row) => !selected.has(row.id));
  selectedIds.value = new Set();
  toast.success(`已批量派工 ${selected.size} 单（采用所选/AI推荐工程师）`);
}

function navigate(route: string) {
  void router.push(route);
}
</script>

<template>
  <main
    :class="$style.page"
    data-page-family="list"
    data-page-route="/service/to-dispatch"
    data-to-dispatch-page
    :data-result-count="filteredRows.length"
    :data-selected-count="selectedCount"
  >
    <ToDispatchHero @navigate="navigate" />
    <ToDispatchStats
      :metrics="dispatchMetrics"
      :values="metricValues"
    />
    <ToDispatchFilters
      v-model:query="query"
      v-model:region="region"
      v-model:type="type"
      v-model:urgency="urgency"
      :region-options="regionOptions"
      :type-options="typeOptions"
      :urgency-options="urgencyOptions"
      :result-count="filteredRows.length"
    />
    <DispatchSelectionBar
      :selection-state="selectionState"
      :selected-count="selectedCount"
      @toggle-all="toggleAll"
      @dispatch="dispatchSelected"
    />

    <section
      v-if="filteredRows.length"
      :class="$style.orderList"
      aria-label="待分派工单"
    >
      <DispatchOrderCard
        v-for="order in filteredRows"
        :key="order.id"
        :order="order"
        :selected="selectedIds.has(order.id)"
        :recommended-engineer="engineerById(order.recommendedEngineerId)"
        :engineer="engineerSelections[order.id] ?? engineerById(order.recommendedEngineerId).name"
        :service-method="methodSelections[order.id] ?? order.serviceMode"
        :engineer-options="engineerOptions"
        :service-method-options="serviceMethodOptions"
        @toggle="toggleOrder(order.id, $event)"
        @update:engineer="updateEngineer(order.id, $event)"
        @update:service-method="updateServiceMethod(order.id, $event)"
        @dispatch="dispatchOrder(order)"
        @open="navigate('/service/service-tickets')"
      />
    </section>
    <section
      v-else
      :class="$style.emptyState"
    >
      当前筛选下暂无待分派工单
    </section>
  </main>
</template>

<style module>
@reference "../../../styles/index.css";

.page {
  @apply mx-auto min-w-0 max-w-[1500px] pb-10;
  color: var(--ui-text);
}

.orderList {
  @apply space-y-3;
}

.emptyState {
  @apply rounded-lg border bg-white py-16 text-center text-[13px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
}
</style>
