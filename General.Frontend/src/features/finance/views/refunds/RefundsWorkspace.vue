<script setup lang="ts">
import { Download, Plus, Search } from 'lucide-vue-next';
import { UiButton, UiInput, UiTabs, UiTabsList, UiTabsTrigger } from '@/components/ui';
import type { RefundDocumentStatus, RefundKind, RefundRow, RefundStatusFilter } from '../../model/refunds.types';
import RefundsTable from './RefundsTable.vue';

defineProps<{
  rows: readonly RefundRow[];
  activeKind: RefundKind;
  activeStatus: RefundStatusFilter;
  statusCounts: Record<RefundDocumentStatus, number>;
  query: string;
}>();

const emit = defineEmits<{
  'update:activeStatus': [value: RefundStatusFilter];
  'update:query': [value: string];
  export: [];
  create: [];
  view: [row: RefundRow];
}>();

const statuses: readonly RefundStatusFilter[] = [
  '全部',
  '草稿',
  '待审批',
  '审批中',
  '已批准',
  '已付款',
  '已核销',
  '已驳回',
];

function tabLabel(status: RefundStatusFilter, counts: Record<RefundDocumentStatus, number>) {
  return status === '全部' ? status : `${status}(${counts[status]})`;
}
</script>

<template>
  <section :class="$style.workspace">
    <header :class="$style.toolbar">
      <UiTabs
        :model-value="activeStatus"
        @update:model-value="emit('update:activeStatus', $event as RefundStatusFilter)"
      >
        <UiTabsList
          label="退款单据状态"
          :class="$style.statusTabs"
        >
          <UiTabsTrigger
            v-for="status in statuses"
            :key="status"
            :value="status"
            :class="$style.statusTab"
          >
            {{ tabLabel(status, statusCounts) }}
          </UiTabsTrigger>
        </UiTabsList>
      </UiTabs>

      <div :class="$style.actions">
        <UiButton
          variant="secondary"
          size="sm"
          @click="emit('export')"
        >
          <template #leading>
            <Download
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          导出
        </UiButton>
        <UiButton
          variant="primary"
          size="sm"
          @click="emit('create')"
        >
          <template #leading>
            <Plus
              :size="14"
              :stroke-width="1.8"
            />
          </template>
          新建{{ activeKind === 'customer' ? '客户' : '供应商' }}退款
        </UiButton>
        <label :class="$style.searchField">
          <Search
            :size="14"
            :stroke-width="1.8"
            aria-hidden="true"
          />
          <UiInput
            :model-value="query"
            :placeholder="activeKind === 'customer' ? '搜索单号/合同/客户…' : '搜索单号/采购单/供应商…'"
            aria-label="搜索退款申请"
            @update:model-value="emit('update:query', $event)"
          />
        </label>
      </div>
    </header>

    <RefundsTable
      :rows="rows"
      :active-kind="activeKind"
      @view="emit('view', $event)"
    />
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workspace {
  @apply mt-4 overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
}

.toolbar {
  @apply flex min-h-[54px] items-center gap-4 border-b px-4;
  border-color: var(--ui-border);
}

.statusTabs {
  @apply gap-1;
}

.statusTab {
  @apply min-h-[52px] rounded-none border-b-2 border-transparent px-3 text-[11px];
  color: var(--ui-text-subtle);
}

.statusTab[data-state='active'] {
  color: var(--app-primary);
  border-bottom-color: var(--app-primary);
  background: transparent;
}

.actions {
  @apply ml-auto flex items-center gap-2;
}

.searchField {
  @apply flex h-8 w-[225px] items-center gap-2 rounded border px-3;
  color: var(--ui-text-subtle);
  border-color: transparent;
  background: var(--ui-surface-hover);
}

.searchField:focus-within {
  border-color: var(--app-primary-light);
  background: var(--ui-surface);
}

.searchField input {
  @apply h-full border-0 bg-transparent p-0 shadow-none;
}
</style>
