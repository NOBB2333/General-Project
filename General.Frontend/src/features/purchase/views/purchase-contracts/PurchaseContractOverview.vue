<script setup lang="ts">
import { computed } from 'vue';
import type { PurchaseContractRow } from '../../model/purchase-contracts.types';
const props = defineProps<{ rows: readonly PurchaseContractRow[] }>();
const summary = computed(() => ({
  active: props.rows.filter((r) => r.status === '履约中').length,
  pending: props.rows.filter((r) => r.status === '待审批').length,
  amount: props.rows.reduce((s, r) => s + r.amount, 0),
  due: props.rows.filter((r) => r.status !== '已完成' && r.dueDate >= '2026-09-16' && r.dueDate <= '2026-10-16').length,
}));
</script>
<template>
  <section :class="$style.stats">
    <article>
      <p>履约中</p>
      <strong :class="$style.active">{{ summary.active }}</strong>
    </article>
    <article>
      <p>待审批/用印</p>
      <strong :class="$style.pending">{{ summary.pending }}</strong>
    </article>
    <article>
      <p>合同总额</p>
      <strong>¥{{ (summary.amount / 10000).toFixed(1) }}万</strong>
    </article>
    <article>
      <p>待付金额</p>
      <strong>¥308.0万</strong>
    </article>
    <article>
      <p>30天内到期</p>
      <strong>{{ summary.due }}</strong>
    </article>
  </section>
</template>
<style module>
@reference "../../../../styles/index.css";
.stats {
  @apply mb-5 grid grid-cols-5 gap-4;
}
.stats article {
  @apply rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stats p {
  @apply text-[12px];
  color: #6b778c;
}
.stats strong {
  @apply mt-1 block text-[22px] font-bold;
  color: #172b4d;
}
.stats .active {
  color: #0052cc;
}
.stats .pending {
  color: #ff8b00;
}
</style>
