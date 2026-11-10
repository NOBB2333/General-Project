<script setup lang="ts">
import { computed } from 'vue';
import { CalendarDays, Filter, List, Plus, Search, SortDesc, UserRound, UsersRound } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { FollowUpRole, FollowUpRow, FollowUpType, FollowUpView } from '@/features/crm/model/follow-ups.types';
import FollowUpTimeline from './FollowUpTimeline.vue';

const props = defineProps<{
  query: string;
  type: FollowUpType | 'all';
  owner: string;
  role: FollowUpRole;
  view: FollowUpView;
  rows: readonly FollowUpRow[];
  types: readonly FollowUpType[];
  owners: readonly string[];
}>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:type': [value: FollowUpType | 'all'];
  'update:owner': [value: string];
  'update:role': [value: FollowUpRole];
  'update:view': [value: FollowUpView];
  create: [];
}>();

const ownerOptions = computed<UiSelectOption[]>(() => [
  { value: 'all', label: '负责人: 全部' },
  ...props.owners.map((owner) => ({ value: owner, label: owner })),
]);
const groupedCalendar = computed(() =>
  props.rows.reduce<Record<string, number>>((result, row) => {
    result[row.date] = (result[row.date] ?? 0) + 1;
    return result;
  }, {}),
);
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.toolbar">
      <label :class="$style.search">
        <Search :size="14" />
        <UiInput
          :model-value="query"
          :class="$style.searchInput"
          placeholder="搜索客户、内容、商机..."
          aria-label="搜索跟进记录"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <div :class="$style.typeFilters">
        <span><Filter :size="14" />类型:</span>
        <button
          type="button"
          :class="[$style.typeButton, type === 'all' && $style.typeActive]"
          @click="emit('update:type', 'all')"
        >
          全部
        </button>
        <button
          v-for="item in types"
          :key="item"
          type="button"
          :class="[$style.typeButton, type === item && $style.typeActive]"
          @click="emit('update:type', item)"
        >
          {{ item }}
        </button>
      </div>
      <UiSelect
        :model-value="owner"
        :options="ownerOptions"
        label="负责人筛选"
        size="sm"
        :class="$style.ownerSelect"
        @update:model-value="emit('update:owner', $event)"
      />
      <UiButton size="sm">
        <template #leading><SortDesc :size="14" /></template>
        最新优先
      </UiButton>
      <div :class="$style.roleSwitch">
        <button
          v-for="item in [
            { value: 'admin', label: '管理员视角', icon: UsersRound },
            { value: 'manager', label: '主管视角', icon: UsersRound },
            { value: 'sales', label: '销售视角', icon: UserRound },
          ] as const"
          :key="item.value"
          type="button"
          :class="[$style.segmentButton, role === item.value && $style.segmentActive]"
          @click="emit('update:role', item.value)"
        >
          <component
            :is="item.icon"
            :size="13"
          />
          {{ item.label }}
        </button>
      </div>
      <div :class="$style.viewSwitch">
        <button
          type="button"
          :class="[$style.segmentButton, view === 'list' && $style.segmentActive]"
          aria-label="列表视图"
          @click="emit('update:view', 'list')"
        >
          <List :size="14" />列表
        </button>
        <button
          type="button"
          :class="[$style.segmentButton, view === 'calendar' && $style.segmentActive]"
          aria-label="日历视图"
          @click="emit('update:view', 'calendar')"
        >
          <CalendarDays :size="14" />日历
        </button>
      </div>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading><Plus :size="14" /></template>
        新增跟进
      </UiButton>
    </div>

    <FollowUpTimeline
      v-if="view === 'list'"
      :rows="rows"
    />
    <div
      v-else
      :class="$style.calendar"
    >
      <header>
        <strong>2026 年 2 月</strong><span>共 {{ rows.length }} 条跟进</span>
      </header>
      <div :class="$style.weekdays">
        <span
          v-for="day in ['一', '二', '三', '四', '五', '六', '日']"
          :key="day"
          >周{{ day }}</span
        >
      </div>
      <div :class="$style.calendarGrid">
        <button
          v-for="day in 28"
          :key="day"
          type="button"
          :class="groupedCalendar[`2026-02-${String(day).padStart(2, '0')}`] > 0 ? $style.calendarActive : undefined"
        >
          <span>{{ day }}</span>
          <strong v-if="groupedCalendar[`2026-02-${String(day).padStart(2, '0')}`]">
            {{ groupedCalendar[`2026-02-${String(day).padStart(2, '0')}`] }} 条跟进
          </strong>
        </button>
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply mt-4;
}
.toolbar {
  @apply flex min-h-[68px] items-center gap-2 overflow-x-auto rounded border bg-white px-4;
  border-color: var(--ui-border);
}
.search {
  @apply flex h-9 w-[240px] shrink-0 items-center gap-2 rounded border px-3;
  color: #97a0af;
  border-color: var(--ui-border);
}
.searchInput {
  @apply h-auto border-0 bg-transparent p-0;
  box-shadow: none;
}
.searchInput:focus {
  box-shadow: none;
}
.typeFilters {
  @apply flex shrink-0 items-center gap-1;
}
.typeFilters > span {
  @apply flex items-center gap-1 text-[11px];
  color: var(--ui-text-subtle);
}
.typeButton {
  @apply h-8 rounded px-2.5 text-[11px] transition-colors;
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.typeActive {
  @apply font-semibold text-white;
  background: var(--app-primary);
}
.ownerSelect {
  @apply min-w-[136px];
}
.roleSwitch,
.viewSwitch {
  @apply flex shrink-0 rounded border p-0.5;
  border-color: var(--ui-border);
}
.segmentButton {
  @apply flex h-7 items-center gap-1 whitespace-nowrap rounded px-2 text-[11px] transition-colors;
  color: var(--ui-text-subtle);
}
.segmentActive {
  @apply font-medium text-white;
  background: var(--app-primary);
}
.calendar {
  @apply mt-3 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.calendar > header {
  @apply flex h-12 items-center justify-between border-b px-4;
  border-color: var(--ui-border);
}
.calendar > header span {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.weekdays,
.calendarGrid {
  @apply grid grid-cols-7;
}
.weekdays span {
  @apply py-2 text-center text-[11px] font-medium;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.calendarGrid button {
  @apply flex min-h-[76px] flex-col items-start border-b border-r p-2 text-[11px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
.calendarGrid button strong {
  @apply mt-2 rounded px-2 py-1 text-[10px];
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.calendarGrid .calendarActive {
  background: #f7faff;
}
</style>
