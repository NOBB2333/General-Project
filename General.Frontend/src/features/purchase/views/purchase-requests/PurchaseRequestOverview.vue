<script setup lang="ts">
import { ArrowRight } from 'lucide-vue-next';
import { computed } from 'vue';
import type { PurchaseRequestRow } from '../../model/purchase-requests.types';

const props = defineProps<{ rows: readonly PurchaseRequestRow[] }>();
const stages = ['草稿', '审批中', '询价中', '比价中', '采购中', '已完成'] as const;
const summary = computed(() => ({
  approval: props.rows.filter((row) => row.status === '审批中').length,
  quote: props.rows.filter((row) => row.status === '询价中' || row.status === '比价中').length,
  purchase: props.rows.filter((row) => row.status === '采购中').length,
  done: props.rows.filter((row) => row.status === '已完成').length,
  amount: props.rows.reduce((total, row) => total + row.amount, 0),
}));
</script>

<template>
  <section
    :class="$style.workflow"
    aria-label="采购申请流程"
  >
    <template
      v-for="(stage, index) in stages"
      :key="stage"
    >
      <span :class="[$style.stage, index === 0 && $style.stageActive]">{{ stage }}</span>
      <ArrowRight
        v-if="index < stages.length - 1"
        :size="12"
        :stroke-width="1.8"
      />
    </template>
  </section>

  <section
    :class="$style.stats"
    aria-label="采购申请统计"
  >
    <article>
      <p>审批中</p>
      <strong :class="$style.approval">{{ summary.approval }}</strong>
    </article>
    <article>
      <p>询价/比价中</p>
      <strong :class="$style.quote">{{ summary.quote }}</strong>
    </article>
    <article>
      <p>采购中</p>
      <strong :class="$style.purchase">{{ summary.purchase }}</strong>
    </article>
    <article>
      <p>已完成</p>
      <strong :class="$style.done">{{ summary.done }}</strong>
    </article>
    <article>
      <p>本月金额</p>
      <strong>¥{{ (summary.amount / 10_000).toFixed(1) }}万</strong>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workflow {
  @apply mb-5 flex items-center justify-center gap-2 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
  color: #c1c7d0;
}
.stage {
  @apply rounded px-3 py-1.5 text-[11px] font-medium;
  color: #5e6c84;
  background: var(--ui-surface-hover);
}
.stageActive {
  @apply text-white;
  background: var(--app-primary);
}
.stats {
  @apply mb-5 grid grid-cols-5 gap-4;
}
.stats article {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stats p {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.stats strong {
  @apply mt-1 block text-[22px] font-bold;
  color: var(--ui-text);
}
.approval {
  color: #ffab00 !important;
}
.quote {
  color: #6554c0 !important;
}
.purchase {
  color: #0052cc !important;
}
.done {
  color: #00875a !important;
}
@media (max-width: 900px) {
  .stats {
    @apply grid-cols-2;
  }
  .workflow {
    @apply overflow-x-auto;
  }
}
</style>
