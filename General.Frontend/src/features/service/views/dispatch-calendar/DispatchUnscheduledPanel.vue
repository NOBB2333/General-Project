<script setup lang="ts">
import { Clock3, Sparkles } from 'lucide-vue-next';
import type { DispatchUnscheduledRow } from '../../model/dispatch-calendar.types';

defineProps<{
  orders: readonly DispatchUnscheduledRow[];
}>();

const emit = defineEmits<{
  select: [order: DispatchUnscheduledRow];
}>();
</script>

<template>
  <aside
    :class="$style.sidebar"
    aria-label="未排期工单"
  >
    <header :class="$style.sidebarHeader">
      <h2>
        <Clock3
          :size="14"
          :stroke-width="1.8"
        />
        未排期（{{ orders.length }}）
      </h2>
      <p>已派工 · 待与客户约定上门时间，可在详情页「预约客户」</p>
    </header>

    <div :class="$style.orderList">
      <button
        v-for="item in orders"
        :key="item.id"
        type="button"
        :class="$style.orderCard"
        :data-unscheduled-code="item.code"
        @click="emit('select', item)"
      >
        <span :class="$style.orderHeading">
          <strong>{{ item.code }}</strong>
          <small
            :class="$style.priority"
            :data-priority="item.priority"
          >
            {{ item.priority }}
          </small>
        </span>
        <span :class="$style.orderTitle">{{ item.title }}</span>
        <span :class="$style.orderMeta">
          <small :class="$style.serviceType">{{ item.serviceType }}</small>
          <small>{{ item.engineerName }}</small>
        </span>
      </button>

      <p
        v-if="!orders.length"
        :class="$style.emptyOrders"
      >
        当前筛选下没有未排期工单
      </p>
    </div>

    <div :class="$style.suggestion">
      <Sparkles
        :size="14"
        :stroke-width="1.8"
      />
      <p>建议：当日已 3 单的工程师不再分配新上门；优先把紧急/停机单排进近 2 天空档。</p>
    </div>
  </aside>
</template>

<style module>
@reference "../../../../styles/index.css";

.sidebar {
  @apply flex w-[280px] shrink-0 flex-col rounded border bg-white p-3;
  border-color: var(--ui-border);
  border-radius: var(--app-control-radius);
}

.sidebarHeader h2 {
  @apply flex items-center gap-1.5 text-[13px] font-semibold;
  color: var(--ui-text);
}

.sidebarHeader h2 svg {
  color: var(--ui-warning);
}

.sidebarHeader p {
  @apply mt-2 text-[10px] leading-4;
  color: var(--ui-text-subtle);
}

.orderList {
  @apply mt-3 flex flex-col gap-2;
}

.orderCard {
  @apply flex w-full flex-col rounded border px-3 py-2 text-left outline-none transition-colors;
  color: var(--ui-text);
  background: white;
  border-color: var(--ui-border);
}

.orderCard:hover {
  border-color: #b3d4ff;
  background: color-mix(in srgb, var(--app-primary-soft) 35%, white);
}

.orderCard:focus-visible {
  box-shadow: 0 0 0 2px var(--ui-focus);
}

.orderHeading {
  @apply flex items-center justify-between gap-2;
}

.orderHeading strong {
  @apply text-[11px] font-bold;
  color: var(--app-primary);
}

.priority {
  @apply flex h-5 min-w-5 items-center justify-center rounded px-1 text-[9px] font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.priority[data-priority='中'] {
  color: var(--ui-warning);
  background: var(--ui-warning-soft);
}

.priority[data-priority='高'] {
  color: var(--ui-danger);
  background: var(--ui-danger-soft);
}

.orderTitle {
  @apply mt-1 truncate text-[11px] font-semibold;
}

.orderMeta {
  @apply mt-1.5 flex items-center gap-2 text-[9px];
  color: var(--ui-text-subtle);
}

.serviceType {
  @apply rounded px-1.5 py-0.5;
  color: #00a3bf;
  background: #e6fcff;
}

.emptyOrders {
  @apply flex min-h-[96px] items-center justify-center text-center text-[11px];
  color: var(--ui-text-subtle);
}

.suggestion {
  @apply mt-3 flex items-start gap-2 rounded px-2.5 py-2 text-[10px] leading-4;
  color: #6554c0;
  background: #f3f0ff;
}

.suggestion svg {
  @apply mt-0.5 shrink-0;
}

@media (max-width: 980px) {
  .sidebar {
    width: 100%;
  }

  .orderList {
    @apply grid grid-cols-3;
  }

  .suggestion {
    @apply mt-3;
  }
}
</style>
