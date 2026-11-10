<script setup lang="ts">
import { LayoutGrid, List, Plus } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { WarehouseFilter, WarehouseRow, WarehouseView } from '../../model/warehouses.types';
import WarehouseCards from './WarehouseCards.vue';
import WarehouseTable from './WarehouseTable.vue';

defineProps<{
  rows: readonly WarehouseRow[];
  filter: WarehouseFilter;
  view: WarehouseView;
}>();
const emit = defineEmits<{
  'update:filter': [value: WarehouseFilter];
  'update:view': [value: WarehouseView];
  create: [];
  viewRow: [row: WarehouseRow];
  edit: [row: WarehouseRow];
}>();

const filters: readonly WarehouseFilter[] = ['全部', '主仓', '项目仓', '备品仓', '委外仓'];
</script>

<template>
  <section :class="$style.panel">
    <header :class="$style.toolbar">
      <div :class="$style.headingGroup">
        <h2>仓库列表</h2>
        <span>{{ rows.length }} 个</span>
        <div
          :class="$style.filters"
          role="group"
          aria-label="仓库类型"
        >
          <button
            v-for="item in filters"
            :key="item"
            type="button"
            :class="[$style.filter, filter === item && $style.filterActive]"
            :aria-pressed="filter === item"
            @click="emit('update:filter', item)"
          >
            {{ item }}
          </button>
        </div>
      </div>
      <div :class="$style.actions">
        <div
          :class="$style.viewToggle"
          role="group"
          aria-label="仓库显示方式"
        >
          <button
            type="button"
            :class="[$style.viewButton, view === 'card' && $style.viewButtonActive]"
            :aria-pressed="view === 'card'"
            @click="emit('update:view', 'card')"
          >
            <LayoutGrid
              :size="12"
              :stroke-width="1.8"
            />
            卡片
          </button>
          <button
            type="button"
            :class="[$style.viewButton, view === 'table' && $style.viewButtonActive]"
            :aria-pressed="view === 'table'"
            @click="emit('update:view', 'table')"
          >
            <List
              :size="12"
              :stroke-width="1.8"
            />
            表格
          </button>
        </div>
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
        >
          <template #leading>
            <Plus
              :size="13"
              :stroke-width="1.8"
            />
          </template>
          新增仓库
        </UiButton>
      </div>
    </header>

    <WarehouseCards
      v-if="view === 'card'"
      :rows="rows"
      @view="emit('viewRow', $event)"
      @edit="emit('edit', $event)"
    />
    <WarehouseTable
      v-else
      :rows="rows"
      @view="emit('viewRow', $event)"
      @edit="emit('edit', $event)"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.panel {
  @apply mb-5 overflow-hidden rounded-lg border bg-white;
  border-color: var(--ui-border);
}
.toolbar {
  @apply flex min-h-[52px] items-center justify-between gap-4 border-b px-5 py-3;
  border-color: var(--ui-border-subtle);
}
.headingGroup,
.actions,
.filters,
.viewToggle,
.viewButton {
  @apply flex items-center;
}
.headingGroup {
  @apply min-w-0 gap-3;
}
.headingGroup h2 {
  @apply whitespace-nowrap text-[14px] font-semibold;
  color: var(--ui-text);
}
.headingGroup > span {
  @apply whitespace-nowrap text-[12px];
  color: #97a0af;
}
.filters {
  @apply ml-2 gap-1;
}
.filter {
  @apply rounded px-2.5 py-1 text-[11px] transition-colors;
  color: var(--ui-text-subtle);
}
.filter:hover {
  color: var(--ui-text-muted);
  background: var(--ui-surface-hover);
}
.filterActive {
  @apply font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.actions {
  @apply shrink-0 gap-2;
}
.viewToggle {
  @apply rounded p-0.5;
  background: var(--ui-surface-hover);
}
.viewButton {
  @apply gap-1 rounded px-2.5 py-1 text-[11px] font-medium transition-colors;
  color: var(--ui-text-subtle);
}
.viewButtonActive {
  @apply bg-white shadow-sm;
  color: var(--app-primary);
}
@media (max-width: 900px) {
  .toolbar,
  .headingGroup {
    @apply items-start;
  }
  .toolbar {
    @apply flex-col;
  }
  .headingGroup {
    @apply flex-wrap;
  }
  .filters {
    @apply ml-0 w-full flex-wrap;
  }
}
</style>
