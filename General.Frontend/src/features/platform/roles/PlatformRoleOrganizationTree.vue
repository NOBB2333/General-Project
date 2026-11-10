<script setup lang="ts">
import type { OrganizationApi } from '@/api/core/organization';
import PlatformRoleOrganizationNode from './PlatformRoleOrganizationNode.vue';

defineProps<{
  items: readonly OrganizationApi.TreeItem[];
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
  <ul
    :class="$style.tree"
    role="tree"
    aria-label="自定义组织范围"
  >
    <PlatformRoleOrganizationNode
      v-for="item in items"
      :key="item.id"
      :node="item"
      :selected-ids="selectedIds"
      @toggle="bubbleToggle"
    />
  </ul>
</template>

<style module>
@reference "../../../styles/index.css";
.tree {
  @apply m-0 border bg-white p-0;
  border-color: var(--ui-border-subtle);
  border-radius: var(--app-control-radius);
}
</style>
