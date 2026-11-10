<script setup lang="ts">
import { RotateCcw, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';

defineProps<{
  query: string;
  assignee: string;
  customer: string;
  type: string;
  urgency: string;
  serviceMode: string;
  startDate: string;
  endDate: string;
  assigneeOptions: readonly UiSelectOption[];
  customerOptions: readonly UiSelectOption[];
  typeOptions: readonly UiSelectOption[];
  urgencyOptions: readonly UiSelectOption[];
  serviceModeOptions: readonly UiSelectOption[];
  activeFilterCount: number;
  resultCount: number;
}>();

const emit = defineEmits<{
  'update:query': [value: string];
  'update:assignee': [value: string];
  'update:customer': [value: string];
  'update:type': [value: string];
  'update:urgency': [value: string];
  'update:serviceMode': [value: string];
  'update:startDate': [value: string];
  'update:endDate': [value: string];
  reset: [];
}>();
</script>

<template>
  <section
    :class="$style.filters"
    aria-label="工单筛选"
  >
    <div :class="$style.primaryRow">
      <label :class="$style.searchField">
        <Search
          :size="14"
          :stroke-width="1.8"
          aria-hidden="true"
        />
        <UiInput
          :model-value="query"
          :class="$style.searchInput"
          placeholder="工单号 / 客户 / SN / 型号 / 联系人"
          aria-label="搜索工单"
          @update:model-value="emit('update:query', $event)"
        />
      </label>

      <label :class="$style.filterField">
        <span>负责人</span>
        <UiSelect
          :model-value="assignee"
          :class="$style.assigneeSelect"
          :options="assigneeOptions"
          label="负责人"
          size="sm"
          @update:model-value="emit('update:assignee', $event)"
        />
      </label>

      <label :class="$style.filterField">
        <span>客户</span>
        <UiSelect
          :model-value="customer"
          :class="$style.customerSelect"
          :options="customerOptions"
          label="客户"
          size="sm"
          @update:model-value="emit('update:customer', $event)"
        />
      </label>

      <label :class="$style.filterField">
        <span>类型</span>
        <UiSelect
          :model-value="type"
          :class="$style.typeSelect"
          :options="typeOptions"
          label="类型"
          size="sm"
          @update:model-value="emit('update:type', $event)"
        />
      </label>

      <label :class="$style.filterField">
        <span>紧急度</span>
        <UiSelect
          :model-value="urgency"
          :class="$style.urgencySelect"
          :options="urgencyOptions"
          label="紧急度"
          size="sm"
          @update:model-value="emit('update:urgency', $event)"
        />
      </label>

      <label :class="$style.filterField">
        <span>服务方式</span>
        <UiSelect
          :model-value="serviceMode"
          :class="$style.modeSelect"
          :options="serviceModeOptions"
          label="服务方式"
          size="sm"
          @update:model-value="emit('update:serviceMode', $event)"
        />
      </label>
    </div>

    <div :class="$style.secondaryRow">
      <span :class="$style.dateLabel">创建时间</span>
      <UiInput
        :model-value="startDate"
        :class="$style.dateInput"
        type="date"
        aria-label="创建日期起"
        @update:model-value="emit('update:startDate', $event)"
      />
      <span :class="$style.dateSeparator">—</span>
      <UiInput
        :model-value="endDate"
        :class="$style.dateInput"
        type="date"
        aria-label="创建日期止"
        @update:model-value="emit('update:endDate', $event)"
      />

      <UiButton
        v-if="activeFilterCount"
        :class="$style.resetButton"
        variant="ghost"
        size="xs"
        @click="emit('reset')"
      >
        <template #leading>
          <RotateCcw
            :size="11"
            :stroke-width="1.8"
          />
        </template>
        重置筛选
      </UiButton>

      <span :class="$style.resultCount">
        <span v-if="activeFilterCount">{{ activeFilterCount }} 个筛选生效</span>
        共 <strong>{{ resultCount }}</strong> 单
      </span>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.filters {
  @apply mb-3 space-y-2.5 rounded-lg border bg-white p-3;
  border-color: var(--ui-border);
}

.primaryRow,
.secondaryRow {
  @apply flex flex-wrap items-center gap-2;
}

.searchField {
  @apply relative block w-[300px] shrink-0;
}

.searchField > svg {
  @apply pointer-events-none absolute left-3 top-1/2 z-[1] -translate-y-1/2;
  color: var(--ui-text-subtle);
}

.searchInput {
  @apply h-8 pl-9 text-[12px];
}

.filterField {
  @apply flex shrink-0 items-center gap-1.5;
}

.filterField > span,
.dateLabel {
  @apply text-[12px];
  color: #5e6c84;
}

.assigneeSelect {
  @apply w-[140px];
}

.customerSelect {
  @apply w-[160px];
}

.typeSelect,
.urgencySelect {
  @apply w-[120px];
}

.modeSelect {
  @apply w-[130px];
}

.dateInput {
  @apply h-8 w-[154px] px-2.5 text-[12px];
  color: var(--ui-text-muted);
}

.dateSeparator {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}

.resetButton {
  @apply ml-1;
  color: var(--ui-text-subtle);
}

.resultCount {
  @apply ml-auto text-[12px];
  color: var(--ui-text-subtle);
}

.resultCount > span {
  @apply mr-2;
  color: var(--app-primary);
}

.resultCount strong {
  color: var(--ui-text);
}
</style>
