<script setup lang="ts">
import { AlertTriangle, Clock3, CreditCard, ShieldCheck } from 'lucide-vue-next';

defineProps<{
  total: number;
  active: number;
  expiring: number;
  expired: number;
}>();
</script>

<template>
  <section
    :class="$style.overview"
    aria-label="服务权益卡概览"
  >
    <article>
      <span><CreditCard :size="13" />服务卡总数</span>
      <strong>{{ total }}</strong>
    </article>
    <article :class="$style.active">
      <span><ShieldCheck :size="13" />生效中</span>
      <strong>{{ active }}</strong>
    </article>
    <article :class="$style.expiring">
      <span><Clock3 :size="13" />即将到期</span>
      <strong>{{ expiring }}</strong>
    </article>
    <article :class="$style.expired">
      <span><AlertTriangle :size="13" />已过期</span>
      <strong>{{ expired }}</strong>
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

.expiring span,
.expiring strong {
  color: var(--ui-warning);
}

.expired span,
.expired strong {
  color: var(--ui-danger);
}

@media (max-width: 900px) {
  .overview {
    @apply grid-cols-2;
  }
}
</style>
