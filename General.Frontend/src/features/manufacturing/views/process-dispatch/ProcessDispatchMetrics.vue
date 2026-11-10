<script setup lang="ts">
import { AlertTriangle, Clock3, UserRoundPlus, UsersRound } from 'lucide-vue-next';

defineProps<{
  pendingCount: number;
  pendingHours: number;
  assignedCount: number;
  overloadedCount: number;
}>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="派工概览"
  >
    <article :class="$style.card">
      <div :class="$style.label">
        <UserRoundPlus
          :size="16"
          :stroke-width="1.8"
        />
        <span>待派工序</span>
      </div>
      <strong :class="[$style.value, $style.pendingValue]"> {{ pendingCount }}<small>道</small> </strong>
    </article>

    <article :class="$style.card">
      <div :class="$style.label">
        <Clock3
          :size="16"
          :stroke-width="1.8"
        />
        <span>待派定额工时</span>
      </div>
      <strong :class="$style.value"> {{ Math.round(pendingHours) }}<small>h</small> </strong>
    </article>

    <article :class="$style.card">
      <div :class="$style.label">
        <UsersRound
          :size="16"
          :stroke-width="1.8"
        />
        <span>已派/在制工序</span>
      </div>
      <strong :class="[$style.value, $style.assignedValue]"> {{ assignedCount }}<small>道</small> </strong>
    </article>

    <article :class="$style.card">
      <div :class="$style.label">
        <AlertTriangle
          :size="16"
          :stroke-width="1.8"
        />
        <span>超载工作中心</span>
      </div>
      <strong :class="$style.value"> {{ overloadedCount }}<small>个</small> </strong>
      <p>本周已派工时 &gt; 可用工时</p>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mb-4 grid shrink-0 grid-cols-4 gap-4;
}

.card {
  @apply h-[102.5px] rounded border bg-white p-4;
  border-color: var(--ui-border);
  border-radius: 3.5px;
}

.label {
  @apply flex items-center gap-2 text-[12px];
  color: var(--ui-text-subtle);
}

.value {
  @apply mt-2 block text-[24px] font-bold leading-7;
  color: var(--ui-text);
}

.value small {
  @apply ml-1 text-[11px] font-medium;
  color: var(--ui-text-subtle);
}

.pendingValue {
  color: #ff8b00;
}

.assignedValue {
  color: var(--app-primary);
}

.card p {
  @apply mt-1 text-[10px];
  color: #97a0af;
}

@media (max-width: 1050px) {
  .grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
