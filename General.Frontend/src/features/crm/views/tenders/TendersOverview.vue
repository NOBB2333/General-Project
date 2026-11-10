<script setup lang="ts">
import { BriefcaseBusiness, Clock3, Shield, TrendingUp, Trophy, Waves } from 'lucide-vue-next';

defineProps<{
  monthCount: number;
  activeCount: number;
  wonCount: number;
  winRate: string;
  openingCount: number;
  pendingDeposit: number;
}>();

const formatWan = (value: number) => `${(value / 10000).toFixed(2)}万`;
</script>

<template>
  <section :class="$style.overview">
    <article data-tone="blue">
      <span>本月投标项目</span><strong>{{ monthCount }}</strong
      ><i><BriefcaseBusiness :size="17" /></i>
    </article>
    <article data-tone="purple">
      <span>进行中项目</span><strong>{{ activeCount }}</strong
      ><i><Waves :size="17" /></i>
    </article>
    <article data-tone="green">
      <span>本月中标数</span><strong>{{ wonCount }}</strong
      ><i><Trophy :size="17" /></i>
    </article>
    <article data-tone="cyan">
      <span>中标率</span><strong>{{ winRate }}</strong
      ><i><TrendingUp :size="17" /></i>
    </article>
    <article data-tone="orange">
      <span>待开标项目</span><strong>{{ openingCount }}</strong
      ><i><Clock3 :size="17" /></i>
    </article>
    <article data-tone="red">
      <span>待退还保证金</span><strong>{{ formatWan(pendingDeposit) }}</strong
      ><i><Shield :size="17" /></i>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply grid grid-cols-6 gap-4 py-4;
}
.overview article {
  @apply relative flex min-h-[98px] flex-col justify-center rounded border border-l-[3px] bg-white p-4;
  border-color: var(--ui-border);
}
.overview span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.overview strong {
  @apply mt-3 text-[22px] font-bold;
}
.overview i {
  @apply absolute right-4 top-4 flex h-8 w-8 items-center justify-center rounded not-italic;
}
.overview article[data-tone='blue'] {
  border-left-color: var(--app-primary);
}
.overview article[data-tone='blue'] i {
  color: var(--app-primary);
  background: var(--app-primary-soft);
}
.overview article[data-tone='purple'] {
  border-left-color: #6554c0;
}
.overview article[data-tone='purple'] i {
  color: #6554c0;
  background: #eae6ff;
}
.overview article[data-tone='green'] {
  border-left-color: var(--ui-success);
}
.overview article[data-tone='green'] i {
  color: var(--ui-success);
  background: var(--ui-success-soft);
}
.overview article[data-tone='cyan'] {
  border-left-color: #00a3bf;
}
.overview article[data-tone='cyan'] i {
  color: #00a3bf;
  background: #e6fcff;
}
.overview article[data-tone='orange'] {
  border-left-color: var(--ui-warning);
}
.overview article[data-tone='orange'] i {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}
.overview article[data-tone='red'] {
  border-left-color: var(--ui-danger);
}
.overview article[data-tone='red'] i {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}
@media (max-width: 1050px) {
  .overview {
    @apply grid-cols-3;
  }
}
</style>
