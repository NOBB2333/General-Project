<script setup lang="ts">
import { Banknote, Clock3, RotateCcw, Truck } from 'lucide-vue-next';

defineProps<{ total: number; pending: number; processing: number; refundedAmount: number }>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="采购退货概览"
  >
    <article :class="$style.card">
      <p>
        <RotateCcw
          :size="16"
          :stroke-width="1.8"
        />退货总次数
      </p>
      <strong>{{ total }}</strong>
    </article>
    <article :class="[$style.card, $style.pending]">
      <p>
        <Clock3
          :size="16"
          :stroke-width="1.8"
        />待处理
      </p>
      <strong>{{ pending }}</strong>
    </article>
    <article :class="[$style.card, $style.processing]">
      <p>
        <Truck
          :size="16"
          :stroke-width="1.8"
        />退货处理中
      </p>
      <strong>{{ processing }}</strong>
    </article>
    <article :class="[$style.card, $style.refunded]">
      <p>
        <Banknote
          :size="16"
          :stroke-width="1.8"
        />已退款总额
      </p>
      <strong>¥{{ refundedAmount.toLocaleString('zh-CN') }}</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mb-5 grid grid-cols-4 gap-4;
}
.card {
  @apply min-h-[85px] rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.card p {
  @apply mb-1 flex items-center gap-2 text-[12px];
  color: var(--ui-text-subtle);
}
.card strong {
  @apply mt-1 block text-[22px] font-bold;
  color: var(--ui-text);
}
.pending p svg,
.pending strong {
  color: #ffab00;
}
.processing p svg,
.processing strong {
  color: #6554c0;
}
.refunded p svg,
.refunded strong {
  color: var(--ui-success);
}
</style>
