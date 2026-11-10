<script setup lang="ts">
import type { MenuApi } from '@/api/core/menu';
import TenantMenuTreeNode from './TenantMenuTreeNode.vue';

defineProps<{
  items: readonly MenuApi.PermissionTreeItem[];
  selectedIds: ReadonlySet<string>;
}>();

const emit = defineEmits<{
  toggle: [menuId: string, selected: boolean];
}>();

function bubbleToggle(menuId: string, selected: boolean) {
  emit('toggle', menuId, selected);
}
</script>

<template>
  <ul
    :class="$style.tree"
    role="tree"
    aria-label="租户菜单授权"
  >
    <TenantMenuTreeNode
      v-for="item in items"
      :key="item.id"
      :node="item"
      :selected-ids="selectedIds"
      @toggle="bubbleToggle"
    />
  </ul>
</template>

<style module>
@reference "../../../../styles/index.css";
.tree {
  @apply m-0 border bg-white p-0;
  border-color: var(--ui-border-subtle);
  border-radius: var(--app-control-radius);
}
</style>
