<script setup lang="ts">
import { Pencil, Trash2 } from 'lucide-vue-next';
import { UiTooltip } from '@/components/ui';
import type { CustomerMaterialRow } from '@/features/crm/model/customer-materials.types';

defineProps<{ row: CustomerMaterialRow }>();
const emit = defineEmits<{
  edit: [row: CustomerMaterialRow];
  remove: [row: CustomerMaterialRow];
}>();
</script>

<template>
  <tr>
    <td>
      <strong :class="$style.customer">{{ row.customer }}</strong>
    </td>
    <td>
      <button
        type="button"
        :class="$style.codeButton"
        @click="emit('edit', row)"
      >
        {{ row.internalCode }}
      </button>
    </td>
    <td>
      <strong :class="$style.materialName">{{ row.internalName }}</strong>
      <span :class="$style.specification">{{ row.internalSpec }} · {{ row.unit }}</span>
    </td>
    <td :class="$style.customerCode">{{ row.customerCode }}</td>
    <td>{{ row.customerName }}</td>
    <td :class="$style.remark">{{ row.remark }}</td>
    <td>
      <time :class="$style.maintainedAt">{{ row.maintainedAt }}</time>
      <span :class="$style.maintainedBy">{{ row.maintainedBy }}</span>
    </td>
    <td :class="$style.actions">
      <UiTooltip content="编辑对照">
        <button
          type="button"
          aria-label="编辑对照"
          @click="emit('edit', row)"
        >
          <Pencil :size="14" />
        </button>
      </UiTooltip>
      <UiTooltip content="删除对照">
        <button
          type="button"
          :class="$style.dangerAction"
          aria-label="删除对照"
          @click="emit('remove', row)"
        >
          <Trash2 :size="14" />
        </button>
      </UiTooltip>
    </td>
  </tr>
</template>

<style module>
@reference "../../../../styles/index.css";

.customer,
.codeButton {
  @apply font-semibold;
  color: var(--app-primary);
}

.codeButton:hover,
.codeButton:focus-visible {
  text-decoration: underline;
}

.materialName,
.specification,
.maintainedAt,
.maintainedBy {
  @apply block;
}

.materialName {
  @apply font-medium;
  color: var(--ui-text);
}

.specification,
.maintainedBy {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}

.customerCode {
  @apply font-mono font-semibold;
  color: var(--ui-text);
}

.remark {
  color: var(--ui-text-muted);
}

.maintainedAt {
  color: var(--ui-text);
}

.actions {
  @apply text-center opacity-0 transition-opacity;
}

tr:hover .actions,
.actions:focus-within {
  @apply opacity-100;
}

.actions button {
  @apply rounded p-1.5 transition-colors;
  color: var(--ui-text-subtle);
}

.actions button:hover {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}

.actions .dangerAction:hover {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
</style>
