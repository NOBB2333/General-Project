<script setup lang="ts">
import { computed } from 'vue';
import type { StockLockRecord } from '../../model/stock-locks.types';

const props = defineProps<{ rows: readonly StockLockRecord[] }>();
const products = computed(() =>
  props.rows.slice(0, 8).map((row) => ({ ...row, remaining: Math.max(0, row.quantity - row.released) })),
);
</script>

<template>
  <section :class="$style.overview">
    <header>
      <h2>商品锁定概况</h2>
      <p>按商品查看锁定、释放和剩余数量</p>
    </header>
    <div :class="$style.grid">
      <article
        v-for="item in products"
        :key="item.id"
      >
        <div>
          <strong>{{ item.product }}</strong
          ><span>{{ item.specification }}</span>
        </div>
        <dl>
          <div>
            <dt>锁定</dt>
            <dd>{{ item.quantity }}</dd>
          </div>
          <div>
            <dt>已释放</dt>
            <dd>{{ item.released }}</dd>
          </div>
          <div>
            <dt>剩余</dt>
            <dd>{{ item.remaining }}</dd>
          </div>
        </dl>
      </article>
    </div>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";
.overview {
  @apply p-5;
}
.overview header {
  @apply border-b pb-4;
  border-color: var(--ui-border-subtle);
}
.overview h2 {
  @apply m-0 text-[15px] font-semibold;
}
.overview p {
  @apply m-0 mt-1 text-[11px];
  color: var(--ui-text-muted);
}
.grid {
  @apply mt-4 grid grid-cols-2 gap-3;
}
.grid article {
  @apply flex items-center justify-between rounded-lg border p-4;
  border-color: var(--ui-border);
  background: var(--ui-surface-subtle);
}
.grid article > div {
  @apply min-w-0;
}
.grid strong {
  @apply block truncate text-[12px];
}
.grid span {
  @apply mt-1 block text-[10px];
  color: var(--ui-text-muted);
}
.grid dl {
  @apply ml-4 grid shrink-0 grid-cols-3 gap-4;
}
.grid dt {
  @apply text-[9px];
  color: var(--ui-text-subtle);
}
.grid dd {
  @apply m-0 mt-1 text-[14px] font-semibold;
  color: var(--app-primary);
}
</style>
