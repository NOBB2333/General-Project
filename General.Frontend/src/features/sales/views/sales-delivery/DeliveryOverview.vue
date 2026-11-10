<script setup lang="ts">
import { Box, CheckCircle2, Clock3, Star, Truck, UserRound, UsersRound } from 'lucide-vue-next';
import type { DeliveryScope } from '../../model/sales-delivery.types';

defineProps<{
  scope: DeliveryScope;
  counts: Record<DeliveryScope, number>;
  pending: number;
  transit: number;
  signed: number;
  shippedQty: number;
  totalQty: number;
}>();

const emit = defineEmits<{ 'update:scope': [scope: DeliveryScope] }>();
const stages = ['销售报价', '销售订单', '发货出库', '开票', '收款'] as const;
const scopes = [
  { id: 'all', label: '所有', icon: UsersRound },
  { id: 'mine', label: '我负责的', icon: UserRound },
  { id: 'team', label: '下属负责的', icon: UsersRound },
  { id: 'watched', label: '我关注的', icon: Star },
] as const;
</script>

<template>
  <section :class="$style.workflow">
    <template
      v-for="(stage, index) in stages"
      :key="stage"
    >
      <span :class="[$style.stage, index === 2 && $style.stageActive]">
        <b>{{ index + 1 }}</b
        >{{ stage }}
      </span>
      <i
        v-if="index < stages.length - 1"
        aria-hidden="true"
        >›</i
      >
    </template>
  </section>

  <nav
    :class="$style.scopes"
    aria-label="负责人范围"
  >
    <button
      v-for="item in scopes"
      :key="item.id"
      type="button"
      :class="[$style.scope, scope === item.id && $style.scopeActive]"
      :aria-pressed="scope === item.id"
      @click="emit('update:scope', item.id)"
    >
      <component
        :is="item.icon"
        :size="13"
        :stroke-width="1.8"
      />
      {{ item.label }}
      <b>{{ counts[item.id] }}</b>
    </button>
  </nav>

  <section :class="$style.metrics">
    <article :class="[$style.metric, $style.warning]">
      <div>
        <span>待发货</span><strong>{{ pending }}</strong>
      </div>
      <i
        ><Clock3
          :size="20"
          :stroke-width="1.8"
      /></i>
    </article>
    <article :class="[$style.metric, $style.transit]">
      <div>
        <span>运输中</span><strong>{{ transit }}</strong>
      </div>
      <i
        ><Truck
          :size="20"
          :stroke-width="1.8"
      /></i>
    </article>
    <article :class="[$style.metric, $style.success]">
      <div>
        <span>已签收</span><strong>{{ signed }}</strong>
      </div>
      <i
        ><CheckCircle2
          :size="20"
          :stroke-width="1.8"
      /></i>
    </article>
    <article :class="[$style.metric, $style.rate]">
      <div>
        <span>本月发货率</span><strong>{{ Math.round((shippedQty / totalQty) * 100) }}%</strong>
      </div>
      <i
        ><Box
          :size="20"
          :stroke-width="1.8"
      /></i>
      <footer>
        <progress
          :value="shippedQty"
          :max="totalQty"
          :aria-label="`本月发货率 ${Math.round((shippedQty / totalQty) * 100)}%`"
        />
        <small>{{ shippedQty }} / {{ totalQty }} 件</small>
      </footer>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.workflow {
  @apply my-4 flex min-h-[60px] items-center justify-center gap-3 rounded-lg border bg-white p-4;
  border-color: var(--ui-border);
}
.stage {
  @apply flex items-center gap-1.5 rounded-full px-4 py-1.5 text-[12px] font-medium;
  color: #5e6c84;
  background: #f4f5f7;
}
.stage b {
  @apply flex h-5 w-5 items-center justify-center rounded-full text-[10px] font-bold;
  background: #dfe1e6;
}
.stageActive {
  @apply text-white shadow-sm;
  background: var(--app-primary);
}
.stageActive b {
  background: rgb(255 255 255 / 20%);
}
.workflow > i {
  @apply text-[20px] not-italic;
  color: #c1c7d0;
}
.scopes {
  @apply mb-4 flex items-center gap-1 rounded-lg border bg-white p-1;
  border-color: var(--ui-border);
}
.scope {
  @apply flex items-center gap-1.5 rounded-md px-3 py-1.5 text-[12px] transition-colors;
  color: #5e6c84;
}
.scope:hover {
  color: var(--ui-text);
  background: var(--ui-surface-hover);
}
.scope b {
  @apply rounded-full px-1.5 py-0.5 text-[10px] font-semibold leading-none;
  background: #ebecf0;
}
.scopeActive {
  @apply font-semibold text-white shadow-sm;
  background: var(--app-primary);
}
.scopeActive:hover {
  @apply text-white;
  background: var(--app-primary);
}
.scopeActive b {
  @apply text-white;
  background: rgb(255 255 255 / 20%);
}
.metrics {
  @apply mb-5 grid grid-cols-4 gap-4;
}
.metric {
  @apply relative flex min-h-[120px] items-center justify-between rounded-lg border border-l-[3px] bg-white p-4;
  border-color: var(--ui-border);
}
.metric div {
  @apply flex flex-col;
}
.metric span {
  @apply text-[12px];
  color: var(--ui-text-subtle);
}
.metric strong {
  @apply mt-1 text-[24px] font-bold;
}
.metric > i {
  @apply flex h-10 w-10 items-center justify-center rounded-lg not-italic;
}
.warning {
  border-left-color: #ffab00;
}
.warning strong,
.warning > i {
  color: #ffab00;
}
.warning > i {
  background: #fffae6;
}
.transit {
  border-left-color: #0065ff;
}
.transit strong,
.transit > i {
  color: #0065ff;
}
.transit > i {
  background: #e6fcff;
}
.success {
  border-left-color: var(--ui-success);
}
.success strong,
.success > i {
  color: var(--ui-success);
}
.success > i {
  background: #e3fcef;
}
.rate {
  @apply flex-wrap;
  border-left-color: var(--app-primary);
}
.rate strong,
.rate > i {
  color: var(--app-primary);
}
.rate > i {
  background: var(--app-primary-soft);
}
.rate footer {
  @apply w-full;
}
.rate progress {
  @apply block h-1.5 w-full overflow-hidden rounded-full border-0;
  background: #ebecf0;
}
.rate progress::-webkit-progress-bar {
  background: #ebecf0;
}
.rate progress::-webkit-progress-value {
  @apply rounded-full;
  background: var(--app-primary);
}
.rate progress::-moz-progress-bar {
  @apply rounded-full;
  background: var(--app-primary);
}
.rate small {
  @apply mt-1 block text-[10px];
  color: #97a0af;
}
@media (max-width: 960px) {
  .stage {
    @apply px-2;
  }
  .metrics {
    @apply grid-cols-2;
  }
}
</style>
