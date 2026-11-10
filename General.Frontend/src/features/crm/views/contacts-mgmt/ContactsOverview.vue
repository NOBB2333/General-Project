<script setup lang="ts">
import { ContactRound, KeyRound, UserCheck, UserRoundX } from 'lucide-vue-next';
defineProps<{ total: number; active: number; away: number; keyPeople: number }>();
</script>

<template>
  <section
    :class="$style.overview"
    aria-label="联系人概览"
  >
    <article>
      <span><ContactRound :size="13" />联系人总数</span><strong>{{ total }}</strong>
    </article>
    <article :class="$style.active">
      <span><UserCheck :size="13" />在职</span><strong>{{ active }}</strong>
    </article>
    <article :class="$style.away">
      <span><UserRoundX :size="13" />不在原公司</span><strong>{{ away }}</strong>
    </article>
    <article :class="$style.key">
      <span><KeyRound :size="13" />关键决策人(KP)</span><strong>{{ keyPeople }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.overview {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.overview article {
  @apply flex min-h-[84px] flex-col justify-center rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.overview span {
  @apply flex items-center gap-1.5 text-[12px];
  color: var(--ui-text-subtle);
}
.overview strong {
  @apply mt-1 text-[22px] font-bold;
  color: var(--ui-text);
}
.active span,
.active strong {
  color: var(--ui-success);
}
.away span,
.away strong {
  color: var(--ui-warning);
}
.key span,
.key strong {
  color: var(--app-primary);
}
@media (max-width: 900px) {
  .overview {
    @apply grid-cols-2;
  }
}
</style>
