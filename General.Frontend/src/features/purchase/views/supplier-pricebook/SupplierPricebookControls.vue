<script setup lang="ts">
import { Download, Plus, Search } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiInput, UiSelect, type UiSelectOption } from '@/components/ui';
import type { SupplierPricebookRow, SupplierPricebookStatus } from '../../model/supplier-pricebook.types';
const props = defineProps<{
  rows: readonly SupplierPricebookRow[];
  status: '全部状态' | SupplierPricebookStatus;
  query: string;
}>();
const emit = defineEmits<{
  'update:status': [v: '全部状态' | SupplierPricebookStatus];
  'update:query': [v: string];
  export: [];
  create: [];
}>();
const statusOptions: UiSelectOption[] = ['全部状态', '草稿', '生效中', '已过期', '已废弃'].map((value) => ({
  value,
  label: value,
}));
const stats = computed(() => ({
  all: props.rows.length,
  active: props.rows.filter((r) => r.status === '生效中').length,
  draft: props.rows.filter((r) => r.status === '草稿').length,
  suppliers: new Set(props.rows.map((r) => r.supplier)).size,
}));
</script>
<template>
  <section :class="$style.stats">
    <article>
      <p>全部价格本</p>
      <strong>{{ stats.all }}</strong>
    </article>
    <article>
      <p>生效中</p>
      <strong :class="$style.active">{{ stats.active }}</strong>
    </article>
    <article>
      <p>草稿</p>
      <strong :class="$style.draft">{{ stats.draft }}</strong>
    </article>
    <article>
      <p>供应商数</p>
      <strong :class="$style.supplier">{{ stats.suppliers }}</strong>
    </article>
  </section>
  <div :class="$style.toolbar">
    <label :class="$style.search"
      ><Search
        :size="14"
        :stroke-width="1.8" /><UiInput
        :model-value="query"
        aria-label="搜索价格本"
        placeholder="搜索价格本名称、编码、供应商..."
        @update:model-value="emit('update:query', String($event))" /></label
    ><UiSelect
      :model-value="status"
      :options="statusOptions"
      label="状态"
      size="sm"
      @update:model-value="emit('update:status', $event as '全部状态' | SupplierPricebookStatus)"
    /><UiButton
      size="sm"
      @click="emit('export')"
      ><template #leading
        ><Download
          :size="13"
          :stroke-width="1.8" /></template
      >导出</UiButton
    ><UiButton
      variant="primary"
      size="sm"
      @click="emit('create')"
      ><template #leading
        ><Plus
          :size="13"
          :stroke-width="1.8" /></template
      >新建价格本</UiButton
    >
  </div>
</template>
<style module>
@reference "../../../../styles/index.css";
.stats {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.stats article {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stats p {
  @apply text-[12px];
  color: #6b778c;
}
.stats strong {
  @apply mt-1 block text-[22px] font-bold;
  color: #172b4d;
}
.stats .active {
  color: #00875a;
}
.stats .draft {
  color: #6b778c;
}
.stats .supplier {
  color: #0052cc;
}
.toolbar {
  @apply mb-4 flex items-center gap-2;
}
.search {
  @apply flex h-9 flex-1 items-center gap-2 rounded border px-3;
  color: #97a0af;
  border-color: var(--ui-border);
  background: #fff;
}
.search :global(input) {
  @apply h-auto border-0 bg-transparent p-0 shadow-none;
}
</style>
