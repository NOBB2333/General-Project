<script setup lang="ts">
import { computed } from 'vue';
import { Columns3, Filter, List, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type {
  OpportunityRow,
  OpportunitySource,
  OpportunityStage,
  OpportunityView,
} from '@/features/crm/model/opportunities.types';
import OpportunitiesBoard from './OpportunitiesBoard.vue';
import OpportunitiesTable from './OpportunitiesTable.vue';

const props = defineProps<{
  query: string;
  stage: OpportunityStage | 'all';
  source: OpportunitySource | 'all';
  view: OpportunityView;
  rows: readonly OpportunityRow[];
  stages: readonly OpportunityStage[];
  sources: readonly OpportunitySource[];
}>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:stage': [value: OpportunityStage | 'all'];
  'update:source': [value: OpportunitySource | 'all'];
  'update:view': [value: OpportunityView];
  create: [];
}>();

const sourceOptions = computed<UiSelectOption[]>(() => [
  { value: 'all', label: '来源: 全部' },
  ...props.sources.map((value) => ({ value, label: value })),
]);
</script>

<template>
  <section :class="$style.workspace">
    <div :class="$style.toolbar">
      <label :class="$style.search">
        <Search
          :size="14"
          :stroke-width="1.8"
        />
        <UiInput
          :model-value="query"
          :class="$style.searchInput"
          placeholder="搜索商机名称、客户..."
          aria-label="搜索商机"
          @update:model-value="emit('update:query', $event)"
        />
      </label>
      <div :class="$style.stageFilter">
        <span><Filter :size="14" />阶段:</span>
        <button
          type="button"
          :class="[$style.stageButton, stage === 'all' && $style.stageActive]"
          :aria-pressed="stage === 'all'"
          @click="emit('update:stage', 'all')"
        >
          全部
        </button>
        <button
          v-for="item in stages"
          :key="item"
          type="button"
          :class="[$style.stageButton, stage === item && $style.stageActive]"
          :aria-pressed="stage === item"
          @click="emit('update:stage', item)"
        >
          {{ item }}
        </button>
      </div>
      <UiSelect
        :model-value="source"
        :options="sourceOptions"
        label="商机来源"
        size="sm"
        :class="$style.sourceSelect"
        @update:model-value="emit('update:source', $event as OpportunitySource | 'all')"
      />
      <div
        :class="$style.views"
        aria-label="商机视图"
      >
        <button
          type="button"
          :class="[$style.viewButton, view === 'list' && $style.viewActive]"
          :aria-pressed="view === 'list'"
          @click="emit('update:view', 'list')"
        >
          <List :size="14" />列表
        </button>
        <button
          type="button"
          :class="[$style.viewButton, view === 'board' && $style.viewActive]"
          :aria-pressed="view === 'board'"
          @click="emit('update:view', 'board')"
        >
          <Columns3 :size="14" />看板
        </button>
      </div>
      <UiButton
        variant="primary"
        size="sm"
        @click="emit('create')"
      >
        <template #leading><Plus :size="14" /></template>
        新建商机
      </UiButton>
    </div>
    <OpportunitiesTable
      v-if="view === 'list'"
      :rows="rows"
    />
    <div
      v-else
      :class="$style.boardScroll"
    >
      <OpportunitiesBoard
        :rows="rows"
        :stages="stages"
      />
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply overflow-hidden rounded border bg-white;
  margin-top: 17.5px;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-[68px] items-center gap-2 border-b px-4 py-3;
  border-color: var(--ui-border);
}
.search {
  @apply flex h-9 w-[330px] shrink-0 items-center gap-2 rounded border px-3;
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
.stageFilter {
  @apply flex items-center gap-1;
}
.stageFilter > span {
  @apply flex items-center gap-1 whitespace-nowrap text-[11px];
  color: var(--ui-text-subtle);
}
.stageButton {
  @apply h-8 whitespace-nowrap rounded px-2 text-[11px] font-medium transition-colors;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}
.stageButton:hover {
  color: var(--ui-text);
}
.stageActive,
.stageActive:hover {
  color: white;
  background: var(--app-primary);
}
.sourceSelect {
  @apply w-[130px] shrink-0;
}
.views {
  @apply ml-auto inline-flex shrink-0 overflow-hidden rounded;
  background: var(--ui-surface-hover);
}
.viewButton {
  @apply flex h-8 items-center gap-1.5 px-3 text-[11px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.viewActive {
  color: var(--ui-text);
  background: white;
  box-shadow: 0 1px 2px rgb(9 30 66 / 12%);
}
.boardScroll {
  @apply overflow-x-auto;
}
@media (max-width: 1300px) {
  .toolbar {
    @apply overflow-x-auto;
  }
  .search {
    @apply w-[260px];
  }
}
</style>
