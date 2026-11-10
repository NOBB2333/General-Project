<script setup lang="ts">
import type { OrganizationApi } from '@/api/core/organization';
import { UiCheckbox } from '@/components/ui';

defineOptions({ name: 'PlatformRoleOrganizationNode' });

const props = defineProps<{
  node: OrganizationApi.TreeItem;
  selectedIds: ReadonlySet<string>;
}>();

const emit = defineEmits<{
  toggle: [organizationId: string, selected: boolean];
}>();

function bubbleToggle(organizationId: string, selected: boolean) {
  emit('toggle', organizationId, selected);
}
</script>

<template>
  <li
    :class="$style.treeNode"
    role="treeitem"
    :aria-checked="selectedIds.has(node.id)"
    :aria-disabled="node.disabled"
  >
    <div :class="$style.nodeRow">
      <UiCheckbox
        :model-value="selectedIds.has(node.id)"
        :disabled="node.disabled"
        :label="`纳入组织${node.displayName}`"
        @update:model-value="emit('toggle', node.id, $event === true)"
      />
      <div :class="$style.nodeCopy">
        <strong>{{ node.displayName }}</strong>
        <small>{{ node.code }} · {{ node.memberCount }} 名成员</small>
      </div>
      <span
        v-if="node.disabled"
        :class="$style.disabledBadge"
        >当前不可配置</span
      >
    </div>
    <ul
      v-if="node.children.length"
      :class="$style.treeChildren"
      role="group"
    >
      <PlatformRoleOrganizationNode
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
@reference "../../../styles/index.css";
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
.nodeCopy strong {
  @apply block font-medium;
  color: var(--ui-text-muted);
}
.nodeCopy small {
  @apply mt-1 block text-[10px];
  color: var(--ui-text-subtle);
}
.disabledBadge {
  @apply rounded px-1.5 py-0.5 text-[10px];
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.treeChildren {
  @apply ml-5 border-l;
  border-color: var(--ui-border-subtle);
}
</style>
