<script setup lang="ts">
import { computed } from 'vue';
import type { MenuApi } from '@/api/core/menu';
import { UiCheckbox } from '@/components/ui';
import { getTenantMenuCheckedState } from '../tenant-authorization.model';

defineOptions({ name: 'TenantMenuTreeNode' });

const props = defineProps<{
  node: MenuApi.PermissionTreeItem;
  selectedIds: ReadonlySet<string>;
}>();

const emit = defineEmits<{
  toggle: [menuId: string, selected: boolean];
}>();

const checked = computed(() => getTenantMenuCheckedState(props.node, props.selectedIds));
const kindLabel = computed(() => {
  if (props.node.type === 1) return '目录';
  if (props.node.type === 2) return '页面';
  return '按钮';
});

function toggle(value: boolean | 'indeterminate') {
  emit('toggle', props.node.id, value === true);
}

function bubbleToggle(menuId: string, selected: boolean) {
  emit('toggle', menuId, selected);
}
</script>

<template>
  <li
    :class="$style.treeNode"
    role="treeitem"
    :aria-checked="checked === 'indeterminate' ? 'mixed' : checked"
    :aria-disabled="!node.isEnabled"
  >
    <div :class="$style.nodeRow">
      <UiCheckbox
        :model-value="checked"
        :disabled="!node.isEnabled"
        :label="`授权${node.title || node.name}`"
        @update:model-value="toggle"
      />
      <div :class="$style.nodeCopy">
        <div :class="$style.nodeHeading">
          <strong>{{ node.title || node.name }}</strong>
          <span :class="$style.typeBadge">{{ kindLabel }}</span>
          <span
            v-if="!node.isEnabled"
            :class="$style.disabledBadge"
            >已停用</span
          >
        </div>
        <small>{{ node.permissionCode || node.path || node.appCode }}</small>
      </div>
    </div>
    <ul
      v-if="node.children.length"
      :class="$style.treeChildren"
      role="group"
    >
      <TenantMenuTreeNode
        v-for="child in node.children"
        :key="child.id"
        :node="child"
        :selected-ids="selectedIds"
        @toggle="bubbleToggle"
      />
    </ul>
  </li>
</template>

<style module>
@reference "../../../../styles/index.css";
.treeNode {
  @apply list-none;
}
.nodeRow {
  @apply flex min-h-12 items-center gap-3 border-b px-3 text-[12px];
  border-color: var(--ui-border-subtle);
}
.nodeCopy {
  @apply min-w-0 flex-1;
}
.nodeHeading {
  @apply flex flex-wrap items-center gap-2;
}
.nodeHeading strong {
  @apply font-medium;
  color: var(--ui-text-muted);
}
.nodeCopy small {
  @apply mt-1 block break-all text-[10px];
  color: var(--ui-text-subtle);
}
.typeBadge,
.disabledBadge {
  @apply rounded px-1.5 py-0.5 text-[10px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-subtle);
}
.disabledBadge {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.treeChildren {
  @apply ml-5 border-l;
  border-color: var(--ui-border-subtle);
}
</style>
