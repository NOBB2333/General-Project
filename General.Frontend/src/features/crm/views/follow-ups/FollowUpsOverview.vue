<script setup lang="ts">
import { Building2, CalendarClock, ClipboardList, Zap } from 'lucide-vue-next';

defineProps<{ total: number; weekCount: number; pendingCount: number; customerCount: number }>();
</script>

<template>
  <section :class="$style.overview">
    <article>
      <span><ClipboardList :size="14" />总跟进记录</span><strong>{{ total }}</strong>
    </article>
    <article>
      <span><Zap :size="14" />本周跟进</span><strong :class="$style.primaryValue">{{ weekCount }}</strong>
    </article>
    <article>
      <span><CalendarClock :size="14" />待跟进</span><strong :class="$style.dangerValue">{{ pendingCount }}</strong>
    </article>
    <article>
      <span><Building2 :size="14" />覆盖客户</span><strong>{{ customerCount }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.overview {
  @apply mt-4 grid grid-cols-4 gap-4;
}
.overview article {
  @apply flex min-h-[84px] flex-col justify-center rounded border bg-white p-4;
  border-color: var(--ui-border);
}
.overview span {
  @apply flex items-center gap-1.5 text-[12px];
  color: var(--ui-text-subtle);
}
.overview strong {
  @apply mt-2 text-[23px] font-bold;
  color: var(--ui-text);
}
.overview .primaryValue {
  color: var(--app-primary);
}
.overview .dangerValue {
  color: #ff5630;
}
@media (max-width: 900px) {
  .overview {
    @apply grid-cols-2;
  }
}
</style>
