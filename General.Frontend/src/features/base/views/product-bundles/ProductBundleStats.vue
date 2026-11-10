<script setup lang="ts">
import { Box, CheckCircle2, Eye, Layers3 } from 'lucide-vue-next';

defineProps<{ total: number; active: number; productKinds: number; references: number }>();

const stats = [
  { key: 'total', label: '组合总数', icon: Layers3, tone: 'primary' },
  { key: 'active', label: '已启用', icon: CheckCircle2, tone: 'success' },
  { key: 'productKinds', label: '总含商品种类', icon: Box, tone: 'purple' },
  { key: 'references', label: '总引用次数', icon: Eye, tone: 'warning' },
] as const;
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="商品组合统计"
  >
    <article
      v-for="stat in stats"
      :key="stat.key"
      :class="$style.card"
    >
      <span :class="[$style.label, $style[stat.tone]]">
        <component
          :is="stat.icon"
          :size="15"
          :stroke-width="1.8"
        />{{ stat.label }}
      </span>
      <strong :class="$style[stat.tone]">{{ $props[stat.key] }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply grid grid-cols-4 gap-3;
}
.card {
  @apply min-h-[84px] rounded border bg-white px-4 py-3;
  border-color: var(--ui-border);
}
.label {
  @apply flex items-center gap-2 text-[12px];
}
.card strong {
  @apply mt-2 block text-[22px] font-bold leading-none;
}
.primary {
  color: #0052cc;
}
.success {
  color: #00875a;
}
.purple {
  color: #6554c0;
}
.warning {
  color: #ff8b00;
}

@media (max-width: 900px) {
  .grid {
    @apply grid-cols-2;
  }
}
</style>
