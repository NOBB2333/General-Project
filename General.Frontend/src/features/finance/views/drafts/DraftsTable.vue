<script setup lang="ts">
import { ArrowLeftRight, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { DraftPoolStatus, DraftRow } from '../../model/drafts.types';

withDefaults(
  defineProps<{
    rows: readonly DraftRow[];
    activeStatus: DraftPoolStatus;
    acceptance: string;
    query: string;
    showStatusTabs?: boolean;
  }>(),
  { showStatusTabs: true },
);

const emit = defineEmits<{
  'update:activeStatus': [value: DraftPoolStatus];
  'update:acceptance': [value: string];
  'update:query': [value: string];
  view: [row: DraftRow];
}>();

const statusFilters = [
  { value: 'all', label: '全部', count: null },
  { value: 'held', label: '持有中', count: 5 },
  { value: 'endorsed', label: '已背书', count: 2 },
  { value: 'discounted', label: '已贴现', count: 1 },
] as const;
const acceptanceOptions: UiSelectOption[] = [
  { value: '', label: '全部承兑类型' },
  { value: '银行承兑', label: '银行承兑' },
  { value: '商业承兑', label: '商业承兑' },
];

function currency(value: number) {
  return `¥${value.toLocaleString('zh-CN')}`;
}
</script>

<template>
  <div :class="$style.controls">
    <div
      v-if="showStatusTabs"
      :class="$style.statusTabs"
      role="group"
      aria-label="票据状态"
    >
      <button
        v-for="item in statusFilters"
        :key="item.value"
        type="button"
        :class="[$style.statusButton, activeStatus === item.value && $style.statusActive]"
        :aria-pressed="activeStatus === item.value"
        @click="emit('update:activeStatus', item.value)"
      >
        {{ item.label }}<span v-if="item.count !== null">({{ item.count }})</span>
      </button>
    </div>
    <div :class="$style.filters">
      <UiSelect
        :model-value="acceptance"
        :options="acceptanceOptions"
        label="承兑类型"
        size="sm"
        @update:model-value="emit('update:acceptance', $event)"
      />
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          placeholder="票号/出票人/承兑人/客户..."
          aria-label="搜索汇票"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
    </div>
  </div>

  <div :class="$style.tableScroller">
    <span :class="$style.scrollHint">
      <ArrowLeftRight
        :size="13"
        :stroke-width="1.8"
      />
      左右滑动
    </span>
    <table :class="$style.table">
      <thead>
        <tr>
          <th>编号</th>
          <th>方向</th>
          <th>票号</th>
          <th>承兑类型</th>
          <th>出票人 / 承兑人</th>
          <th>票面金额</th>
          <th>出票日</th>
          <th>到期日</th>
          <th>状态</th>
          <th>操作</th>
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
              @click="emit('view', row)"
            >
              {{ row.code }}
            </button>
          </td>
          <td>
            <span
              :class="$style.direction"
              :data-direction="row.direction"
              >{{ row.direction }}</span
            >
          </td>
          <td>
            <strong :class="$style.primaryText">{{ row.noteNumber }}</strong>
            <small>{{ row.bank }}</small>
          </td>
          <td>
            <span
              :class="$style.acceptance"
              :data-acceptance="row.acceptanceKind"
              >{{ row.acceptanceKind }}</span
            >
            <span :class="$style.medium">{{ row.medium }}</span>
          </td>
          <td>
            <strong :class="$style.primaryText">{{ row.drawer }}</strong>
            <small>承兑：{{ row.acceptor }}</small>
          </td>
          <td>
            <strong :class="$style.amount">{{ currency(row.amount) }}</strong>
          </td>
          <td>{{ row.issueDate }}</td>
          <td>
            <span :class="$style.primaryText">{{ row.dueDate }}</span>
            <small
              v-if="row.status === '持有中'"
              :class="$style.remaining"
              :data-urgent="row.remainingDays <= 7 || undefined"
              >剩 {{ row.remainingDays }} 天</small
            >
          </td>
          <td>
            <span
              :class="$style.status"
              :data-status="row.status"
              >{{ row.status }}</span
            >
          </td>
          <td>
            <UiButton
              variant="link"
              size="xs"
              @click="emit('view', row)"
              >详情</UiButton
            >
          </td>
        </tr>
        <tr v-if="!rows.length">
          <td
            :class="$style.empty"
            colspan="10"
          >
            当前筛选下没有票据记录
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.controls {
  @apply flex min-h-[65px] items-center px-4;
}

.statusTabs {
  @apply flex h-full items-center gap-1;
}

.statusButton {
  @apply min-h-10 border-b-2 border-transparent px-3 text-[11px] font-semibold transition-colors;
  color: var(--ui-text-subtle);
}

.statusButton:hover,
.statusActive {
  color: var(--app-primary);
}

.statusActive {
  border-bottom-color: var(--app-primary);
}

.statusButton span {
  @apply ml-1 font-normal;
  color: var(--ui-text-subtle);
}

.filters {
  @apply ml-auto flex items-center gap-2;
}

.searchField {
  @apply flex h-8 w-[245px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: var(--ui-border);
  background: var(--ui-surface-hover);
}

.searchField:focus-within {
  border-color: var(--app-primary-light);
  background: var(--ui-surface);
}

.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}

.tableScroller {
  @apply relative overflow-x-auto;
}

.scrollHint {
  @apply absolute right-3 top-2 z-10 inline-flex items-center gap-1 rounded-full border px-2 py-1 text-[9px] font-semibold;
  color: var(--app-primary);
  border-color: var(--app-primary-light);
  background: var(--app-primary-soft);
}

.table {
  @apply w-full min-w-[1420px] border-collapse text-left text-[10px];
  color: var(--ui-text-muted);
}

.table th {
  @apply h-10 border-y px-4 font-semibold;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}

.table td {
  @apply h-[61px] border-b px-4 align-middle;
  border-color: var(--ui-border-subtle);
}

.table tbody tr:hover {
  background: var(--ui-surface-hover);
}

.table th:nth-child(1) {
  width: 150px;
}

.table th:nth-child(2) {
  width: 75px;
}

.table th:nth-child(3) {
  width: 260px;
}

.table th:nth-child(4) {
  width: 150px;
}

.table th:nth-child(5) {
  width: 250px;
}

.table th:nth-child(6) {
  width: 120px;
}

.table th:nth-child(7),
.table th:nth-child(8) {
  width: 125px;
}

.table th:nth-child(9),
.table th:nth-child(10) {
  width: 90px;
}

.code {
  @apply font-semibold;
  color: var(--app-primary);
}

.primaryText,
.amount {
  @apply block font-semibold;
  color: var(--ui-text);
}

.table small {
  @apply mt-1 block text-[9px];
  color: var(--ui-text-subtle);
}

.direction,
.acceptance,
.medium,
.status,
.remaining {
  @apply inline-flex rounded px-2 py-0.5 font-semibold;
}

.direction {
  color: var(--ui-success);
  background: #e3fcef;
}

.direction[data-direction='应付'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.acceptance {
  color: var(--ui-success);
  background: #e3fcef;
}

.acceptance[data-acceptance='商业承兑'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.medium {
  @apply ml-1;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.remaining {
  @apply w-fit;
  color: var(--ui-success);
  background: #e3fcef;
}

.remaining[data-urgent='true'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.status {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.status[data-status='已背书'] {
  color: #6554c0;
  background: #eae6ff;
}

.status[data-status='已贴现'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.empty {
  @apply h-32 text-center;
  color: var(--ui-text-subtle);
}
</style>
