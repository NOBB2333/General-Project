<script setup lang="ts">
import { Clock3, DollarSign, Target, UserMinus, UserPlus, Users } from 'lucide-vue-next';
import type { HrMetric } from '../../model/hr-workbench.types';
defineProps<{ metrics: readonly HrMetric[] }>();
const emit = defineEmits<{ inspect: [metric: HrMetric] }>();
</script>

<template>
  <section :class="$style.grid">
    <button
      v-for="(metric, index) in metrics"
      :key="metric.id"
      type="button"
      :class="[$style.card, $style[metric.tone]]"
      @click="emit('inspect', metric)"
    >
      <header>
        <span :class="$style.icon"
          ><Users
            v-if="index === 0"
            :size="16"
            :stroke-width="1.8" /><Clock3
            v-else-if="index === 1"
            :size="16"
            :stroke-width="1.8" /><UserPlus
            v-else-if="index === 2"
            :size="16"
            :stroke-width="1.8" /><UserMinus
            v-else-if="index === 3"
            :size="16"
            :stroke-width="1.8" /><Target
            v-else-if="index === 4"
            :size="16"
            :stroke-width="1.8" /><DollarSign
            v-else
            :size="16"
            :stroke-width="1.8" /></span
        ><span>{{ metric.label }}</span>
      </header>
      <strong>{{ metric.value }}</strong>
      <p>{{ metric.hint }}</p>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply grid grid-cols-6 gap-3;
}
.card {
  @apply h-[112px] rounded border bg-white p-4 text-left transition-colors;
  border-color: var(--ui-border);
}
.card:hover {
  border-color: var(--app-primary);
}
.card header {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.icon {
  @apply flex h-8 w-8 items-center justify-center rounded;
  color: #0c66e4;
  background: #deebff;
}
.card strong {
  @apply mt-3 block text-[20px];
}
.card p {
  @apply m-0 mt-1 text-[9px];
  color: #97a0af;
}
.orange .icon {
  color: #ff8b00;
  background: #fff7d6;
}
.green .icon {
  color: #00875a;
  background: #e3fcef;
}
.red .icon {
  color: #de350b;
  background: #ffebe6;
}
.purple .icon {
  color: #6554c0;
  background: #eae6ff;
}
.cyan .icon {
  color: #00a3bf;
  background: #e6fcff;
}
</style>
