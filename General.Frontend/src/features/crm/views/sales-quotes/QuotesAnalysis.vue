<script setup lang="ts">
import { computed } from 'vue';
import type { QuoteStatus, SalesQuoteRow } from '@/features/crm/model/sales-quotes.types';

const props = defineProps<{ rows: readonly SalesQuoteRow[] }>();
const statuses: readonly QuoteStatus[] = ['草稿', '待审核', '已发送', '已接受', '已拒绝', '已过期'];
const statusData = computed(() =>
  statuses.map((status) => ({
    status,
    count: props.rows.filter((row) => row.status === status).length,
    amount: props.rows.filter((row) => row.status === status).reduce((sum, row) => sum + row.amount, 0),
  })),
);
const customerData = computed(() => [...props.rows].sort((left, right) => right.amount - left.amount).slice(0, 5));
const maxAmount = computed(() => Math.max(1, ...customerData.value.map((row) => row.amount)));
const currency = new Intl.NumberFormat('zh-CN');
</script>

<template>
  <section :class="$style.analysis">
    <header>
      <div>
        <h2>报价分析</h2>
        <p>按报价状态和客户金额查看当前报价结构</p>
      </div>
      <strong>{{ rows.length }} 份报价</strong>
    </header>
    <div :class="$style.columns">
      <section :class="$style.panel">
        <h3>状态分布</h3>
        <div :class="$style.statusGrid">
          <article
            v-for="item in statusData"
            :key="item.status"
          >
            <span>{{ item.status }}</span>
            <strong>{{ item.count }}</strong>
            <small>¥{{ currency.format(item.amount) }}</small>
          </article>
        </div>
      </section>
      <section :class="$style.panel">
        <h3>客户报价金额 TOP 5</h3>
        <div :class="$style.rankList">
          <article
            v-for="(row, index) in customerData"
            :key="row.id"
          >
            <span>{{ index + 1 }}</span>
            <div>
              <strong>{{ row.customer }}</strong>
              <progress
                :value="row.amount"
                :max="maxAmount"
                :aria-label="`${row.customer}报价金额`"
              />
            </div>
            <em>¥{{ currency.format(row.amount) }}</em>
          </article>
        </div>
      </section>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.analysis {
  @apply mt-4 border bg-white;
  border-color: var(--ui-border);
}
.analysis > header {
  @apply flex items-center justify-between border-b px-5 py-4;
  border-color: var(--ui-border);
}
.analysis h2 {
  @apply text-[16px] font-semibold;
}
.analysis header p {
  @apply mt-1 text-[11px];
  color: var(--ui-text-subtle);
}
.analysis header > strong {
  @apply text-[12px] font-medium;
  color: var(--app-primary);
}
.columns {
  @apply grid grid-cols-2 gap-0;
}
.panel {
  @apply min-h-[330px] p-5;
}
.panel + .panel {
  border-left: 1px solid var(--ui-border);
}
.panel h3 {
  @apply mb-4 text-[13px] font-semibold;
}
.statusGrid {
  @apply grid grid-cols-3 gap-3;
}
.statusGrid article {
  @apply flex min-h-24 flex-col justify-center rounded border p-3;
  border-color: var(--ui-border-subtle);
  background: var(--ui-surface-hover);
}
.statusGrid span,
.statusGrid small {
  @apply text-[11px];
  color: var(--ui-text-subtle);
}
.statusGrid strong {
  @apply my-1 text-[20px];
}
.rankList {
  @apply space-y-4;
}
.rankList article {
  @apply grid grid-cols-[24px_1fr_auto] items-center gap-3;
}
.rankList article > span {
  @apply flex h-5 w-5 items-center justify-center rounded-full text-[10px] text-white;
  background: var(--app-primary);
}
.rankList article div {
  @apply min-w-0;
}
.rankList strong {
  @apply block truncate text-[12px];
}
.rankList progress {
  @apply mt-1 block h-1.5 w-full overflow-hidden rounded-full border-0;
  background: var(--ui-surface-hover);
}
.rankList progress::-webkit-progress-bar {
  background: var(--ui-surface-hover);
}
.rankList progress::-webkit-progress-value {
  @apply rounded-full;
  background: var(--app-primary);
}
.rankList progress::-moz-progress-bar {
  @apply rounded-full;
  background: var(--app-primary);
}
.rankList em {
  @apply text-[12px] font-semibold not-italic;
  color: var(--ui-text-muted);
}
@media (max-width: 800px) {
  .columns {
    @apply grid-cols-1;
  }
  .panel + .panel {
    border-left: 0;
    border-top: 1px solid var(--ui-border);
  }
}
</style>
