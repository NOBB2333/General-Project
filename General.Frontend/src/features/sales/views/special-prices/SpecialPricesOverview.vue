<script setup lang="ts">
import { CheckCircle2, CircleAlert, ClipboardList, ShoppingCart } from 'lucide-vue-next';

defineProps<{ pending: number; approved: number; ordered: number; waiting: number }>();
</script>

<template>
  <section
    :class="$style.grid"
    aria-label="特价概览"
  >
    <article :class="[$style.card, $style.pending]">
      <CircleAlert
        :size="18"
        :stroke-width="1.8"
      />
      <div>
        <span>审批中</span><strong>{{ pending }}</strong>
      </div>
    </article>
    <article :class="[$style.card, $style.approved]">
      <CheckCircle2
        :size="18"
        :stroke-width="1.8"
      />
      <div>
        <span>已通过</span><strong>{{ approved }}</strong>
      </div>
    </article>
    <article :class="[$style.card, $style.ordered]">
      <ShoppingCart
        :size="18"
        :stroke-width="1.8"
      />
      <div>
        <span>已下单</span><strong>{{ ordered }}</strong>
      </div>
    </article>
    <article :class="[$style.card, $style.waiting]">
      <ClipboardList
        :size="18"
        :stroke-width="1.8"
      />
      <div>
        <span>已通过待下单</span><strong>{{ waiting }}</strong>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.grid {
  @apply mb-4 grid grid-cols-4 gap-3;
}
.card {
  @apply flex min-h-[60px] items-center gap-3 rounded-lg px-4 py-3;
}
.card span {
  @apply block text-[11px];
  color: var(--ui-text-subtle);
}
.card strong {
  @apply mt-0.5 block text-[20px] font-semibold;
  color: var(--ui-text);
}
.pending {
  color: #ff8b00;
  background: #fff7e8;
}
.approved {
  color: #00b86b;
  background: #edfff5;
}
.ordered {
  color: #00875a;
  background: #e3fcef;
}
.waiting {
  color: #ffab00;
  background: #fffae6;
}
</style>
