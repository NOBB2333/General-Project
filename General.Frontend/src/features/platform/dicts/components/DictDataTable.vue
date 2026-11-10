<script setup lang="ts">
import { UiButton, UiSwitch } from '@/components/ui';
import type { DictApi } from '@/api/core/dict';
import { displayDictText, getDictTagLabel, getDictTagTone } from '../dict.model';

defineProps<{
  rows: readonly DictApi.Data[];
  savingId: null | string;
}>();

const emit = defineEmits<{
  delete: [row: DictApi.Data];
  edit: [row: DictApi.Data];
  toggleEnabled: [row: DictApi.Data, enabled: boolean];
}>();
</script>

<template>
  <div :class="$style.tableFrame">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>显示名称</th>
          <th>字典值</th>
          <th>标签</th>
          <th>排序</th>
          <th>启用</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="row in rows"
          :key="row.id"
          :data-dict-id="row.id"
        >
          <td>
            <strong>{{ displayDictText(row.label) }}</strong>
          </td>
          <td>
            <code>{{ displayDictText(row.value) }}</code>
          </td>
          <td>
            <span
              :class="$style.tag"
              :data-tone="getDictTagTone(row.tagColor)"
              :title="`标签颜色：${getDictTagLabel(row.tagColor)}`"
              >{{ getDictTagLabel(row.tagColor) }}</span
            >
          </td>
          <td>{{ row.sort }}</td>
          <td>
            <UiSwitch
              :model-value="row.isEnabled"
              size="sm"
              :label="`${row.isEnabled ? '停用' : '启用'}${displayDictText(row.label)}`"
              :disabled="savingId === row.id"
              @update:model-value="emit('toggleEnabled', row, $event)"
            />
          </td>
          <td>
            <div :class="$style.actions">
              <UiButton
                size="xs"
                variant="ghost"
                :aria-label="`编辑${displayDictText(row.label)}`"
                @click="emit('edit', row)"
                >编辑</UiButton
              >
              <UiButton
                size="xs"
                variant="danger"
                :aria-label="`删除${displayDictText(row.label)}`"
                @click="emit('delete', row)"
                >删除</UiButton
              >
            </div>
          </td>
        </tr>
        <tr v-if="rows.length === 0">
          <td
            colspan="6"
            :class="$style.emptyState"
          >
            当前字典类型下没有匹配的字典项
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
  @apply w-full border-collapse text-[12px];
}
.table th {
  @apply h-[43px] px-3 text-left font-semibold;
  color: var(--ui-text-muted);
  background: #f4f5f7;
  border-bottom: 1px solid var(--ui-border);
}
.table td {
  @apply h-[50px] px-3;
  color: var(--ui-text);
  border-bottom: 1px solid var(--ui-border-subtle);
}
.table tbody tr:last-child td {
  border-bottom: 0;
}
.table tbody tr:hover {
  background: var(--ui-surface-hover);
}
.table strong {
  @apply font-semibold;
}
.table code {
  @apply text-[10px];
  color: var(--ui-text-subtle);
}
.actions {
  @apply flex items-center gap-1 whitespace-nowrap;
}
.tag {
  @apply inline-flex items-center rounded-full border px-2 py-0.5 text-[10px] font-medium;
  color: var(--ui-text-muted);
  background: #f4f5f7;
  border-color: var(--ui-border-subtle);
}
.tag[data-tone='primary'] {
  color: var(--app-primary);
  background: #deebff;
  border-color: #b3d4ff;
}
.tag[data-tone='success'] {
  color: #006644;
  background: #e3fcef;
  border-color: #abf5d1;
}
.tag[data-tone='warning'] {
  color: #974f0c;
  background: #fff7d6;
  border-color: #ffe380;
}
.tag[data-tone='danger'] {
  color: #bf2600;
  background: #ffebe6;
  border-color: #ffbdad;
}
.emptyState {
  @apply h-24 text-center;
  color: var(--ui-text-subtle);
}
</style>
