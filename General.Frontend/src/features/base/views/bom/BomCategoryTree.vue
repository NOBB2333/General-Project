<script setup lang="ts">
import { ChevronDown, ChevronLeft, Folder, FolderOpen, Monitor, Network, Shield, Zap } from 'lucide-vue-next';
import type { BomCategoryFilter } from '../../model/bom.types';

defineProps<{ active: BomCategoryFilter }>();
const emit = defineEmits<{ select: [category: BomCategoryFilter] }>();

const items: readonly {
  id: BomCategoryFilter;
  label: string;
  count?: number;
  level: number;
  icon: 'folder' | 'network' | 'monitor' | 'shield' | 'zap';
}[] = [
  { id: 'all', label: '全部BOM', count: 4, level: 0, icon: 'folder' },
  { id: 'standard', label: '标准BOM', level: 0, icon: 'shield' },
  { id: 'control', label: '控制系统', level: 1, icon: 'monitor' },
  { id: 'dcs', label: 'DCS系统', count: 1, level: 2, icon: 'monitor' },
  { id: 'sis', label: 'SIS系统', count: 1, level: 2, icon: 'monitor' },
  { id: 'drive', label: '驱动系统', level: 1, icon: 'network' },
  { id: 'vfd', label: '高压变频器', count: 1, level: 2, icon: 'monitor' },
  { id: 'project', label: '项目BOM', level: 0, icon: 'folder' },
  { id: 'huaneng', label: '华能电力项目', count: 1, level: 1, icon: 'folder' },
];
</script>

<template>
  <aside :class="$style.panel">
    <header>
      <Network
        :size="14"
        :stroke-width="1.8"
      /><strong>BOM 分类</strong
      ><ChevronLeft
        :size="14"
        :stroke-width="1.8"
      />
    </header>
    <nav aria-label="BOM 分类">
      <button
        v-for="item in items"
        :key="item.id"
        type="button"
        :class="[$style.item, $style[`level${item.level}`], active === item.id && $style.active]"
        :aria-pressed="active === item.id"
        @click="emit('select', item.id)"
      >
        <ChevronDown
          v-if="['standard', 'control', 'drive', 'project'].includes(item.id)"
          :size="11"
          :stroke-width="1.8"
        />
        <FolderOpen
          v-else-if="item.id === 'all'"
          :size="14"
          :stroke-width="1.8"
        />
        <Folder
          v-else-if="item.icon === 'folder'"
          :size="14"
          :stroke-width="1.8"
        />
        <Monitor
          v-else-if="item.icon === 'monitor'"
          :size="14"
          :stroke-width="1.8"
        />
        <Shield
          v-else-if="item.icon === 'shield'"
          :size="14"
          :stroke-width="1.8"
        />
        <Zap
          v-else
          :size="14"
          :stroke-width="1.8"
        />
        <span>{{ item.label }}</span
        ><small v-if="item.count !== undefined">{{ item.count }}</small>
      </button>
    </nav>
  </aside>
</template>

<style module>
@reference "../../../../styles/index.css";
.panel {
  @apply w-[260px] shrink-0 border-r bg-white;
  border-color: var(--ui-border);
}
.panel header {
  @apply flex h-[45px] items-center gap-2 border-b px-3 text-[13px];
  border-color: var(--ui-border);
  color: var(--ui-text);
}
.panel header strong {
  @apply font-semibold;
}
.panel header svg:last-child {
  @apply ml-auto;
  color: var(--ui-text-subtle);
}
.panel nav {
  @apply p-2;
}
.item {
  @apply flex h-8 w-full items-center gap-1.5 rounded px-2 text-left text-[12px] transition-colors;
  color: var(--ui-text-muted);
}
.item:hover {
  background: var(--ui-surface-hover);
}
.item span {
  @apply truncate;
}
.item small {
  @apply ml-auto text-[10px];
  color: var(--ui-text-subtle);
}
.level1 {
  @apply pl-5;
}
.level2 {
  @apply pl-10;
}
.active {
  @apply font-semibold;
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
</style>
