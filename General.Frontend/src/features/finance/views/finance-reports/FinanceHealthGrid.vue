<script setup lang="ts">
import { Activity, ShieldCheck, TrendingUp, WalletCards } from 'lucide-vue-next';
import type { HealthScore } from '../../model/finance-reports.types';
defineProps<{ scores: readonly HealthScore[] }>();
const emit = defineEmits<{ inspect: [score: HealthScore] }>();
</script>

<template>
  <section :class="$style.grid">
    <button
      v-for="(score, index) in scores"
      :key="score.label"
      type="button"
      :class="[$style.card, $style[score.tone], index === 0 && $style.featured]"
      @click="emit('inspect', score)"
    >
      <header>
        <span
          ><ShieldCheck
            v-if="index === 0"
            :size="15"
            :stroke-width="1.8"
          /><TrendingUp
            v-else-if="index === 1 || index === 4"
            :size="15"
            :stroke-width="1.8"
          /><WalletCards
            v-else-if="index === 2"
            :size="15"
            :stroke-width="1.8"
          /><Activity
            v-else
            :size="15"
            :stroke-width="1.8"
          />{{ score.label }}</span
        ><small v-if="index > 0">{{ score.status }}</small>
      </header>
      <strong>{{ score.value }}<em>/100</em></strong>
      <div :class="$style.progress"><i /></div>
      <p>{{ score.detail }}</p>
      <small :class="$style.benchmark">{{ score.benchmark }}</small>
    </button>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.grid {
  @apply mt-4 grid grid-cols-5 gap-3;
}
.card {
  @apply h-[156px] min-w-0 rounded-lg border bg-white p-4 text-left transition-colors;
  border-color: var(--ui-border);
}
.card:hover {
  border-color: var(--app-primary);
}
.card header {
  @apply flex items-center justify-between;
}
.card header span {
  @apply flex items-center gap-1.5 text-[11px];
  color: var(--ui-text-subtle);
}
.card header small {
  @apply rounded px-1.5 py-0.5 text-[9px] font-semibold;
  color: #00875a;
  background: #e3fcef;
}
.card strong {
  @apply mt-3 block text-[25px];
}
.card strong em {
  @apply ml-0.5 text-[10px] not-italic font-medium;
  color: #97a0af;
}
.progress {
  @apply mt-2 h-1 overflow-hidden rounded bg-[#EBECF0];
}
.progress i {
  @apply block h-full w-[72%] rounded bg-[#00875A];
}
.card p {
  @apply m-0 mt-2 text-[10px] font-semibold;
  color: var(--ui-text-muted);
}
.benchmark {
  @apply mt-1 block text-[9px];
  color: #97a0af;
}
.warning header small {
  color: #ff8b00;
  background: #fff7d6;
}
.warning .progress i {
  @apply w-[48%];
  background: #ff8b00;
}
.featured {
  @apply border-0 text-white;
  background: linear-gradient(130deg, #0c66e4, #5b46c6);
}
.featured header span,
.featured p,
.featured .benchmark {
  color: white;
}
.featured strong {
  @apply text-[36px];
}
.featured strong em {
  color: white;
}
.featured .progress {
  background: rgb(255 255 255 / 25%);
}
.featured .progress i {
  @apply w-[67%] bg-white;
}
</style>
