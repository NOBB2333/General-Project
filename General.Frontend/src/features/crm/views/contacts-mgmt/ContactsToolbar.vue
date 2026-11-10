<script setup lang="ts">
import { Plus, Search } from 'lucide-vue-next';
import { computed } from 'vue';
import { UiButton, UiInput, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { ContactStatus } from '@/features/crm/model/contacts-mgmt.types';

const props = defineProps<{
  query: string;
  status: ContactStatus | 'all';
  owner: string;
  owners: readonly string[];
  counts: Record<'all' | ContactStatus, number>;
  total: number;
  visibleCount: number;
}>();
const emit = defineEmits<{
  'update:query': [value: string];
  'update:status': [value: ContactStatus | 'all'];
  'update:owner': [value: string];
  create: [];
}>();
const tabs: { value: ContactStatus | 'all'; label: string }[] = [
  { value: 'all', label: '全部' },
  { value: '在职', label: '在职' },
  { value: '已跳槽', label: '已跳槽' },
  { value: '已离职', label: '已离职' },
  { value: '停用', label: '停用' },
];
const ownerOptions = computed<UiSelectOption[]>(() => [
  { value: '', label: '维护人' },
  ...props.owners.map((owner) => ({ value: owner, label: owner })),
]);
</script>

<template>
  <section :class="$style.section">
    <div
      :class="$style.tabs"
      role="tablist"
      aria-label="联系人状态"
    >
      <button
        v-for="tab in tabs"
        :key="tab.value"
        type="button"
        role="tab"
        :aria-selected="status === tab.value"
        :class="[$style.tab, status === tab.value && $style.tabActive]"
        @click="emit('update:status', tab.value)"
      >
        {{ tab.label }} <span>{{ counts[tab.value] }}</span>
      </button>
    </div>
    <section :class="$style.listCard">
      <div :class="$style.actions">
        <UiButton
          variant="primary"
          size="sm"
          :class="$style.createButton"
          @click="emit('create')"
        >
          <template #leading><Plus :size="14" /></template>
          新增联系人
        </UiButton>
        <label :class="$style.search">
          <Search :size="14" />
          <UiInput
            :model-value="query"
            :class="$style.searchInput"
            placeholder="搜索姓名、公司、电话、邮箱..."
            aria-label="搜索联系人"
            @update:model-value="emit('update:query', $event)"
          />
        </label>
        <UiSelect
          :model-value="owner"
          :options="ownerOptions"
          label="维护人"
          size="sm"
          :class="$style.owner"
          @update:model-value="emit('update:owner', $event)"
        />
      </div>
      <div :class="$style.count">共 {{ visibleCount }} 条记录</div>
      <slot />
    </section>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.section {
  @apply mt-4;
}
.tabs {
  @apply flex items-end gap-1 border-b;
  border-color: var(--ui-border);
}
.tab {
  @apply flex h-10 items-center gap-1 px-4 text-[13px] font-medium transition-colors;
  color: var(--ui-text-subtle);
  border-bottom: 2px solid transparent;
}
.tab:hover {
  color: var(--ui-text);
}
.tab span {
  @apply rounded-full px-1.5 py-0.5 text-[10px];
  background: var(--ui-surface-hover);
}
.tabActive {
  color: var(--app-primary);
  border-color: var(--app-primary);
}
.tabActive span {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.listCard {
  @apply overflow-hidden rounded-b-lg border border-t-0 bg-white;
  border-color: var(--ui-border);
}
.actions {
  @apply flex min-h-[60px] items-center gap-3 border-b px-4 py-3;
  border-color: var(--ui-border-subtle);
}
.createButton {
  @apply h-[34px] min-h-[34px] px-4 text-[13px];
}
.search {
  @apply ml-auto flex h-8 min-w-[250px] items-center gap-2 rounded border px-3;
  color: #97a0af;
  background: white;
  border-color: var(--ui-border);
}
.searchInput {
  @apply h-auto border-0 p-0;
  box-shadow: none;
}
.searchInput:focus {
  box-shadow: none;
}
.owner {
  @apply min-w-[100px];
}
.count {
  @apply border-b px-4 py-2 text-[12px];
  color: var(--ui-text-subtle);
  border-color: var(--ui-border-subtle);
}
@media (max-width: 800px) {
  .search {
    @apply ml-0 flex-1;
  }
  .actions {
    @apply flex-wrap;
  }
}
</style>
