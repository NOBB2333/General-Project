<script setup lang="ts">
import { computed } from 'vue';
import { Filter, Search } from 'lucide-vue-next';
import { UiInput, UiSelect, UiTabs, UiTabsList, UiTabsTrigger, type UiSelectOption } from '@/components/ui';
import type { MovementFilter, OnboardingRecord } from '../../model/onboarding.types';
const props = defineProps<{
  rows: readonly OnboardingRecord[];
  allRows: readonly OnboardingRecord[];
  type: MovementFilter;
  department: string;
  position: string;
  status: string;
  query: string;
}>();
const emit = defineEmits<{
  'update:type': [value: MovementFilter];
  'update:department': [value: string];
  'update:position': [value: string];
  'update:status': [value: string];
  'update:query': [value: string];
  open: [row: OnboardingRecord];
}>();
const options = (values: readonly string[], emptyLabel: string): UiSelectOption[] => [
  { value: '', label: emptyLabel },
  ...[...new Set(values)].map((value) => ({ value, label: value })),
];
const departmentOptions = computed(() =>
  options(
    props.allRows.map((row) => row.department),
    '全部部门',
  ),
);
const positionOptions = computed(() =>
  options(
    props.allRows.map((row) => row.position),
    '全部岗位',
  ),
);
const statusOptions = computed(() =>
  options(
    props.allRows.map((row) => row.status),
    '全部状态',
  ),
);
const count = (type: MovementFilter) =>
  type === '全部' ? props.allRows.length : props.allRows.filter((row) => row.type === type).length;
const setType = (value: string) => emit('update:type', value as MovementFilter);
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.filters">
      <Filter
        :size="14"
        :stroke-width="1.8"
      /><span>筛选</span
      ><UiSelect
        :model-value="department"
        :options="departmentOptions"
        label="部门"
        size="sm"
        @update:model-value="emit('update:department', $event)"
      /><UiSelect
        :model-value="position"
        :options="positionOptions"
        label="岗位"
        size="sm"
        @update:model-value="emit('update:position', $event)"
      /><UiSelect
        :model-value="status"
        :options="statusOptions"
        label="状态"
        size="sm"
        @update:model-value="emit('update:status', $event)"
      />
    </div>
    <header :class="$style.tabsRow">
      <UiTabs
        :model-value="type"
        @update:model-value="setType"
        ><UiTabsList label="记录类型"
          ><UiTabsTrigger
            v-for="item in ['全部', '入职', '离职'] as const"
            :key="item"
            :value="item"
            >{{ item }} <small>({{ count(item) }})</small></UiTabsTrigger
          ></UiTabsList
        ></UiTabs
      >
      <div :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        /><UiInput
          :model-value="query"
          placeholder="搜索..."
          aria-label="搜索入职离职记录"
          @update:model-value="emit('update:query', $event)"
        />
      </div>
    </header>
    <div :class="$style.tableWrap">
      <table>
        <thead>
          <tr>
            <th>记录编号</th>
            <th>员工姓名</th>
            <th>类型</th>
            <th>部门</th>
            <th>岗位</th>
            <th>日期</th>
            <th>状态</th>
            <th>经办人</th>
            <th>进度</th>
          </tr>
        </thead>
        <tbody>
          <tr
            v-for="row in rows"
            :key="row.id"
          >
            <td>
              <button
                type="button"
                :class="$style.code"
                @click="emit('open', row)"
              >
                {{ row.code }}
              </button>
            </td>
            <td>
              <span
                :class="$style.person"
                :data-type="row.type"
                ><b>{{ row.employeeName.slice(0, 1) }}</b
                ><strong>{{ row.employeeName }}</strong></span
              >
            </td>
            <td>
              <span
                :class="$style.type"
                :data-type="row.type"
                >{{ row.type }}</span
              >
            </td>
            <td>{{ row.department }}</td>
            <td>{{ row.position }}</td>
            <td>{{ row.date }}</td>
            <td>
              <span
                :class="$style.status"
                :data-status="row.status"
                >{{ row.status }}</span
              >
            </td>
            <td>{{ row.owner }}</td>
            <td>
              <span :class="$style.progress"
                ><progress
                  :value="row.completedSteps"
                  :max="row.totalSteps"
                />{{ row.completedSteps }}/{{ row.totalSteps }}</span
              >
            </td>
          </tr>
        </tbody>
      </table>
      <p
        v-if="!rows.length"
        :class="$style.empty"
      >
        当前筛选下没有入职离职记录
      </p>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4;
}
.filters {
  @apply flex h-[54px] items-center gap-2;
  color: var(--ui-text-subtle);
}
.filters > span {
  @apply mr-2 text-[11px] font-semibold;
}
.tabsRow {
  @apply flex h-[44px] items-end border-b;
  border-color: var(--ui-border);
}
.tabsRow [role='tab'] {
  @apply h-10 rounded-none px-4;
}
.tabsRow [role='tab'][data-state='active'] {
  @apply border-b-2 bg-transparent;
  border-color: var(--app-primary);
}
.tabsRow small {
  @apply text-[9px];
  color: #97a0af;
}
.search {
  @apply relative ml-auto mb-1 w-[205px];
}
.search > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-10 -translate-y-1/2;
  color: var(--ui-text-subtle);
}
.search input {
  @apply h-8 border-0 pl-9;
  background: var(--ui-surface-hover);
}
.tableWrap {
  @apply overflow-x-auto rounded-b border border-t-0 bg-white;
  border-color: var(--ui-border);
}
.tableWrap table {
  @apply w-full min-w-[1180px] border-collapse text-left text-[11px];
}
.tableWrap th {
  @apply h-9 border-b px-3 font-semibold;
  color: var(--ui-text-muted);
  background: var(--ui-surface-subtle);
  border-color: var(--ui-border);
}
.tableWrap td {
  @apply h-[49px] border-b px-3;
  color: var(--ui-text-muted);
  border-color: var(--ui-border-subtle);
}
.tableWrap tr:last-child td {
  @apply border-b-0;
}
.code {
  @apply font-medium;
  color: var(--ui-text-muted);
}
.code:hover {
  color: var(--app-primary);
}
.person {
  @apply flex items-center gap-2;
}
.person b {
  @apply flex h-6 w-6 items-center justify-center rounded-full text-[10px] text-white;
  background: var(--app-primary);
}
.person[data-type='离职'] b {
  background: #ff5630;
}
.person strong {
  @apply text-[11px] font-semibold;
  color: var(--ui-text);
}
.type,
.status {
  @apply inline-flex rounded px-2 py-1 text-[9px] font-semibold;
}
.type {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.type[data-type='离职'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
.status {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.status[data-status='待入职'],
.status[data-status='待审批'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.status[data-status='交接中'],
.status[data-status='入职中'] {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.status[data-status='已离职'],
.status[data-status='已取消'],
.status[data-status='已撤回'] {
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.progress {
  @apply flex min-w-[110px] items-center gap-2 text-[9px];
}
.progress progress {
  @apply h-1 w-[70px] overflow-hidden rounded;
  appearance: none;
}
.progress progress::-webkit-progress-bar {
  background: var(--ui-surface-hover);
}
.progress progress::-webkit-progress-value {
  background: var(--ui-success);
}
.progress progress::-moz-progress-bar {
  background: var(--ui-success);
}
.empty {
  @apply m-0 py-14 text-center text-[12px];
  color: var(--ui-text-subtle);
}
</style>
