<script setup lang="ts">
import {
  BriefcaseBusiness,
  CalendarDays,
  Eye,
  FileCheck2,
  LockKeyhole,
  Search,
  ShieldCheck,
  UserRound,
  UserRoundCog,
  UsersRound,
  Workflow,
} from 'lucide-vue-next';
import { UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import { policyCategories } from '../../model/policies.data';
import type { PolicyRecord } from '../../model/policies.types';

defineProps<{
  query: string;
  category: string;
  status: string;
  creator: string;
  sort: string;
  rows: readonly PolicyRecord[];
}>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:category': [value: string];
  'update:status': [value: string];
  'update:creator': [value: string];
  'update:sort': [value: string];
  open: [row: PolicyRecord];
}>();
const categoryCounts: Record<string, number> = {
  全部: 7,
  人事制度: 2,
  财务制度: 1,
  安全管理: 1,
  行政管理: 1,
  保密制度: 1,
  质量管理: 1,
};
const statusOptions: readonly UiSelectOption[] = ['全部状态', '生效中', '修订中'].map((value) => ({
  value,
  label: value,
}));
const creatorOptions: readonly UiSelectOption[] = ['全部创建人', '孙丽萍', '刘晓芳', '李志强', '张明远'].map(
  (value) => ({ value, label: value }),
);
const sortOptions: readonly UiSelectOption[] = ['生效日期', '发布日期', '版本'].map((value) => ({
  value,
  label: `排序: ${value}`,
}));
const iconByCategory = {
  人事制度: UserRoundCog,
  财务制度: BriefcaseBusiness,
  安全管理: ShieldCheck,
  行政管理: Workflow,
  保密制度: LockKeyhole,
  质量管理: FileCheck2,
} as const;
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.categories">
      <button
        v-for="item in policyCategories"
        :key="item"
        type="button"
        :class="{ [$style.active]: category === item }"
        @click="emit('update:category', item)"
      >
        {{ item }} ({{ categoryCounts[item] }})
      </button>
    </div>
    <div :class="$style.panel">
      <header>
        <div :class="$style.search">
          <Search
            :size="14"
            :stroke-width="1.8"
          /><UiInput
            :model-value="query"
            placeholder="搜索制度名称、摘要、编号..."
            aria-label="搜索制度"
            @update:model-value="emit('update:query', $event)"
          />
        </div>
        <UiSelect
          :model-value="status"
          :options="statusOptions"
          label="状态筛选"
          @update:model-value="emit('update:status', $event)"
        />
        <UiSelect
          :model-value="creator"
          :options="creatorOptions"
          label="创建人筛选"
          @update:model-value="emit('update:creator', $event)"
        />
        <UiSelect
          :model-value="sort"
          :options="sortOptions"
          label="制度排序"
          @update:model-value="emit('update:sort', $event)"
        />
      </header>
      <div :class="$style.list">
        <article
          v-for="row in rows"
          :key="row.id"
          :data-category="row.category"
        >
          <span :class="$style.icon"
            ><component
              :is="iconByCategory[row.category]"
              :size="15"
              :stroke-width="1.8"
          /></span>
          <div :class="$style.copy">
            <p>
              <strong>{{ row.title }}</strong
              ><small>{{ row.version }}</small>
            </p>
            <p>{{ row.summary }}</p>
            <ul>
              <li>
                <CalendarDays
                  :size="10"
                  :stroke-width="1.8"
                />{{ row.effectiveAt }} 生效
              </li>
              <li>
                <UserRound
                  :size="10"
                  :stroke-width="1.8"
                />{{ row.creator }}
              </li>
              <li>
                <UsersRound
                  :size="10"
                  :stroke-width="1.8"
                />{{ row.audience }}
              </li>
              <li>{{ row.code }}</li>
            </ul>
          </div>
          <div :class="$style.tags">
            <span>{{ row.category }}</span
            ><b :data-status="row.status">{{ row.status }}</b>
          </div>
          <button
            type="button"
            :aria-label="`查看${row.title}`"
            @click="emit('open', row)"
          >
            <Eye
              :size="14"
              :stroke-width="1.8"
            />
          </button>
        </article>
        <p
          v-if="rows.length === 0"
          :class="$style.empty"
        >
          当前筛选下没有规章制度
        </p>
      </div>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.workspace {
  @apply mt-4;
}
.categories {
  @apply flex gap-2;
}
.categories button {
  @apply h-9 rounded border bg-white px-3 text-[10px] font-medium;
  border-color: var(--ui-border);
  color: var(--ui-text-muted);
}
.categories button:hover {
  background: var(--ui-surface-hover);
}
.categories button.active {
  @apply border-2;
  border-color: var(--app-primary);
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.panel {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}
.panel > header {
  @apply flex h-[58px] items-center gap-2 border-b px-4;
  border-color: var(--ui-border);
}
.search {
  @apply relative mr-auto w-[420px];
}
.search > svg {
  @apply absolute left-3 top-[10px] z-10;
  color: var(--ui-text-subtle);
}
.search input {
  @apply pl-9;
}
.panel > header > [role='combobox'] {
  @apply min-w-[128px];
}
.list article {
  @apply relative grid min-h-[80px] grid-cols-[32px_1fr_auto_24px] items-start gap-3 border-b px-4 py-3;
  border-color: var(--ui-border-subtle);
}
.list article:last-child {
  @apply border-b-0;
}
.icon {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.copy p {
  @apply m-0 text-[10px];
  color: var(--ui-text-muted);
}
.copy p:first-child {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-text);
}
.copy small {
  color: var(--ui-text-subtle);
}
.copy p + p {
  @apply mt-1;
}
.copy ul {
  @apply m-0 mt-2 flex list-none gap-4 p-0 text-[9px];
  color: var(--ui-text-muted);
}
.copy li {
  @apply flex items-center gap-1;
}
.copy li:last-child {
  color: var(--app-primary);
}
.tags {
  @apply flex gap-2 pt-1;
}
.tags span,
.tags b {
  @apply rounded px-2 py-1 text-[8px] font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.tags b {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.tags b[data-status='修订中'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.list article > button {
  @apply mt-1 border-0 bg-transparent p-1;
  color: var(--ui-text-subtle);
}
.list article > button:hover {
  color: var(--app-primary);
}
.empty {
  @apply m-0 py-16 text-center text-[11px];
  color: var(--ui-text-subtle);
}
</style>
