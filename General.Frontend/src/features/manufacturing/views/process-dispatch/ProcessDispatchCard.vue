<script setup lang="ts">
import { Clock3, LockKeyhole, Package, RefreshCw, UserRoundPlus } from 'lucide-vue-next';
import { UiButton } from '@/components/ui';
import type { ProcessDispatchOperation } from '../../model/process-dispatch.types';

defineProps<{
  operation: ProcessDispatchOperation;
}>();

const emit = defineEmits<{
  select: [operation: ProcessDispatchOperation];
  assign: [operation: ProcessDispatchOperation];
}>();
</script>

<template>
  <article
    :class="$style.card"
    :data-operation-id="operation.id"
  >
    <div :class="$style.cardMain">
      <span
        :class="$style.topRail"
        aria-hidden="true"
      />
      <div :class="$style.codeRow">
        <button
          type="button"
          :class="$style.codeButton"
          :aria-label="`查看生产工单 ${operation.code}`"
          @click="emit('select', operation)"
        >
          {{ operation.code }}
        </button>
        <span :class="[$style.status, operation.lane === 'assigned' && $style.statusActive]">
          {{ operation.lane === 'assigned' ? '进行中' : '待派工' }}
        </span>
      </div>
      <p :class="$style.product">{{ operation.product }}</p>
      <div :class="$style.operationRow">
        <span :class="$style.operationNo">{{ operation.operationNo }}</span>
        <strong>{{ operation.operationName }}</strong>
      </div>
      <div :class="$style.meta">
        <span
          ><Package
            :size="12"
            :stroke-width="1.8"
          />{{ operation.quantity }} {{ operation.unit }}</span
        >
        <span
          ><Clock3
            :size="12"
            :stroke-width="1.8"
          />{{ operation.standardHours }}h</span
        >
        <span>◷ {{ operation.dueDays }}天</span>
      </div>
      <div
        v-if="operation.lane === 'assigned'"
        :class="$style.assignee"
      >
        <span>已派</span>
        <strong>{{ operation.assignee || operation.owner }}</strong>
      </div>
    </div>

    <footer :class="$style.footer">
      <span
        v-if="operation.predecessorPending"
        :class="$style.predecessor"
      >
        <LockKeyhole
          :size="11"
          :stroke-width="1.8"
        />
        前道未完
      </span>
      <UiButton
        :class="$style.assignButton"
        variant="primary"
        size="xs"
        :aria-label="`${operation.lane === 'assigned' ? '改派' : '派工'} ${operation.code} 工序 ${operation.operationNo} ${operation.operationName}`"
        @click="emit('assign', operation)"
      >
        <template #leading>
          <RefreshCw
            v-if="operation.lane === 'assigned'"
            :size="12"
            :stroke-width="1.8"
          />
          <UserRoundPlus
            v-else
            :size="12"
            :stroke-width="1.8"
          />
        </template>
        {{ operation.lane === 'assigned' ? '改派' : '派工' }}
      </UiButton>
    </footer>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.card {
  @apply shrink-0 overflow-hidden rounded border bg-white p-3;
  border-color: var(--ui-border);
  border-radius: 3.5px;
}

.cardMain {
  @apply block w-full p-0 text-left;
  color: var(--ui-text);
}

.codeButton {
  @apply border-0 p-0 text-[12px] font-semibold;
  color: var(--app-primary);
  background: transparent;
}

.codeButton:hover,
.codeButton:focus-visible {
  text-decoration: underline;
}

.topRail {
  @apply mb-2 block h-[3px] w-full rounded-full;
  background: #a5adba;
}

.codeRow {
  @apply flex items-center justify-between gap-2;
}

.status {
  @apply rounded px-2 py-1 text-[10px] font-medium;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.statusActive {
  color: #ff8b00;
  background: #fffae6;
}

.product {
  @apply mt-2 truncate text-[11px];
  color: var(--ui-text-subtle);
}

.operationRow {
  @apply mt-2 flex items-center gap-2;
}

.operationNo {
  @apply rounded px-2 py-1 text-[11px] font-semibold;
  background: var(--ui-surface-hover);
}

.operationRow strong {
  @apply truncate text-[12px] font-semibold;
}

.meta {
  @apply mt-2 flex flex-wrap items-center gap-3 text-[10px];
  color: var(--ui-text-subtle);
}

.meta span {
  @apply inline-flex items-center gap-1;
}

.assignee {
  @apply mt-2 flex items-center gap-2 text-[10px];
  color: var(--ui-text-subtle);
}

.assignee strong {
  color: var(--ui-text);
}

.footer {
  @apply mt-2 flex min-h-7 items-center justify-between gap-2;
}

.predecessor {
  @apply inline-flex items-center gap-1 rounded px-2 py-1 text-[10px];
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
}

.assignButton {
  @apply ml-auto;
}
</style>
