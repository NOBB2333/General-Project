<script setup lang="ts">
import { BadgeCheck, Globe2, Target, Zap } from 'lucide-vue-next';

defineProps<{ total: number; newCount: number; converted: number; publicCount: number }>();
</script>

<template>
  <section :class="$style.overview">
    <article :class="$style.total">
      <span><Target :size="14" />全部线索</span><strong>{{ total }}</strong>
    </article>
    <article :class="$style.newLead">
      <span><Zap :size="14" />新线索</span><strong>{{ newCount }}</strong>
    </article>
    <article :class="$style.converted">
      <span><BadgeCheck :size="14" />已转化</span><strong>{{ converted }}</strong>
    </article>
    <article :class="$style.publicLead">
      <span><Globe2 :size="14" />公海线索</span><strong>{{ publicCount }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.overview article {
  @apply flex min-h-[82px] flex-col justify-center rounded border border-l-[3px] bg-white p-4;
  border-color: var(--ui-border);
}
.overview span {
  @apply flex items-center gap-1.5 text-[12px];
  color: var(--ui-text-subtle);
}
.overview strong {
  @apply mt-2 text-[23px] font-bold;
}
.total,
.total strong {
  color: var(--app-primary);
}
.total {
  border-left-color: var(--app-primary);
}
.newLead,
.newLead strong {
  color: var(--ui-warning);
}
.newLead {
  border-left-color: var(--ui-warning);
}
.converted,
.converted strong {
  color: #6554c0;
}
.converted {
  border-left-color: #6554c0;
}
.publicLead,
.publicLead strong {
  color: #ff5630;
}
.publicLead {
  border-left-color: #ff5630;
}
@media (max-width: 900px) {
  .overview {
    @apply grid-cols-2;
  }
}
</style>
