<script setup lang="ts">
import { UiButton } from '@/components/ui';
import type { DictApi } from '@/api/core/dict';

defineProps<{
  rows: readonly DictApi.Type[];
  selectedId: string;
}>();

const emit = defineEmits<{
  delete: [row: DictApi.Type];
  edit: [row: DictApi.Type];
  select: [row: DictApi.Type];
}>();
</script>

<template>
  <div :class="$style.tableFrame">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>字典类型</th>
          <th>字典编码</th>
          <th>启用</th>
          <th>总数</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-dict-type-id="row.id"
          :class="selectedId === row.id && $style.selectedRow"
          :aria-selected="selectedId === row.id"
          @click="emit('select', row)"
        >
          <td>
            <button
              type="button"
              :class="$style.typeName"
              @click.stop="emit('select', row)"
            >
              {{ row.name }}
            </button>
          </td>
          <td>
            <code :title="row.code">{{ row.code }}</code>
          </td>
          <td>{{ row.enabledDataCount }}</td>
          <td>{{ row.dataCount }}</td>
          <td>
            <div :class="$style.actions">
              <UiButton
                size="xs"
                variant="ghost"
                :aria-label="`编辑${row.name}`"
                @click.stop="emit('edit', row)"
                >编辑</UiButton
              >
              <UiButton
                size="xs"
                variant="danger"
                :aria-label="`删除${row.name}`"
                @click.stop="emit('delete', row)"
                >删除</UiButton
              >
            </div>
          </td>
        </tr>
        <tr v-if="rows.length === 0">
          <td
            colspan="5"
            :class="$style.emptyState"
          >
            没有匹配的字典类型
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.tableFrame {
  @apply overflow-hidden border bg-white;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}
.table {
  @apply w-full table-fixed border-collapse text-[12px];
}
.table th {
  @apply h-[43px] px-3 text-left font-semibold;
  color: var(--ui-text-muted);
  background: #f4f5f7;
  border-bottom: 1px solid var(--ui-border);
}
.table th:first-child {
  @apply w-[92px];
}
.table th:nth-child(2) {
  @apply w-[110px];
}
.table th:nth-child(3),
.table th:nth-child(4) {
  @apply w-[48px];
}
.table th:nth-child(5) {
  @apply w-[100px];
}
.table td {
  @apply h-[50px] px-3;
  color: var(--ui-text);
  border-bottom: 1px solid var(--ui-border-subtle);
}
.table tbody tr:last-child td {
  border-bottom: 0;
}
.table tbody tr:hover,
.selectedRow {
  background: var(--ui-surface-hover);
}
.table code {
  @apply block truncate text-[10px];
  color: var(--ui-text-subtle);
}
.typeName {
  @apply text-left font-semibold;
  color: var(--ui-text);
}
.actions {
  @apply flex items-center gap-1 whitespace-nowrap;
}
.emptyState {
  @apply h-24 text-center;
  color: var(--ui-text-subtle);
}
</style>
