<script setup lang="ts">
import { Grid2X2, UserRoundPlus } from 'lucide-vue-next';
import type { ProcessDispatchLane, ProcessDispatchOperation } from '../../model/process-dispatch.types';
import ProcessDispatchCard from './ProcessDispatchCard.vue';

defineProps<{
  pendingOperations: ProcessDispatchOperation[];
  pendingHours: number;
  lanes: ProcessDispatchLane[];
}>();

const emit = defineEmits<{
  select: [operation: ProcessDispatchOperation];
  assign: [operation: ProcessDispatchOperation];
}>();

function formatHours(value: number) {
  return Number.isInteger(value) ? String(value) : value.toFixed(1);
}
</script>

<template>
  <section
    :class="$style.board"
    data-module-kanban
    data-kanban-mode="work-center-dispatch"
  >
    <article
      :class="$style.pendingPool"
      data-kanban-lane="pending"
    >
      <header :class="$style.panelHeader">
        <h2>
          <UserRoundPlus
            :size="16"
            :stroke-width="1.8"
          />
          待派工序池
        </h2>
        <span>{{ pendingOperations.length }} 道 · {{ Math.round(pendingHours) }}h</span>
      </header>
      <div
        :class="$style.pendingCards"
        data-kanban-pending-scroller
      >
        <ProcessDispatchCard
          v-for="operation in pendingOperations"
          :key="operation.id"
          :operation="operation"
          @select="emit('select', $event)"
          @assign="emit('assign', $event)"
        />
      </div>
    </article>

    <article :class="$style.assignedPanel">
      <header :class="$style.panelHeader">
        <h2>
          <Grid2X2
            :size="16"
            :stroke-width="1.8"
          />
          按工作中心已派工序
        </h2>
      </header>

      <div
        :class="$style.laneScroller"
        data-kanban-lane-scroller
      >
        <section
          v-for="lane in lanes"
          :key="lane.id"
          :class="$style.workCenterLane"
          :data-kanban-lane="lane.id"
        >
          <header :class="$style.laneHeader">
            <div :class="$style.laneHeading">
              <strong>{{ lane.name }}</strong>
              <span>{{ formatHours(lane.scheduledHours) }}/{{ lane.capacityHours }}h</span>
            </div>
            <progress
              :class="$style.loadProgress"
              :value="lane.scheduledHours"
              :max="lane.capacityHours"
              :aria-label="`${lane.name} ${formatHours(lane.scheduledHours)}/${lane.capacityHours}h，本周负荷 ${lane.loadPercent}%`"
            />
            <small>本周负荷 {{ lane.loadPercent }}%</small>
          </header>

          <div :class="$style.laneCards">
            <ProcessDispatchCard
              v-for="operation in lane.operations"
              :key="operation.id"
              :operation="operation"
              @select="emit('select', $event)"
              @assign="emit('assign', $event)"
            />
            <div
              v-if="!lane.operations.length"
              :class="$style.emptyLane"
            >
              无已派工序
            </div>
          </div>
        </section>
      </div>
    </article>
  </section>
</template>

<style module>
@reference "../../../../styles/index.css";

.board {
  @apply grid h-[560px] min-w-0 gap-4;
  grid-template-columns: 300px minmax(0, 1fr);
}

.pendingPool,
.assignedPanel {
  @apply flex min-h-0 min-w-0 flex-col overflow-hidden rounded border bg-white;
  border-color: var(--ui-border);
  border-radius: 3.5px;
}

.panelHeader {
  @apply flex h-[46px] shrink-0 items-center justify-between gap-3 border-b px-4;
  border-color: var(--ui-border);
}

.panelHeader h2 {
  @apply flex items-center gap-2 text-[13px] font-semibold;
  color: var(--ui-text);
}

.panelHeader h2 svg {
  color: var(--app-primary);
}

.pendingPool .panelHeader h2 svg {
  color: #ff8b00;
}

.panelHeader > span {
  @apply text-[11px] font-medium;
  color: var(--ui-text-subtle);
}

.pendingCards {
  @apply flex min-h-0 flex-1 flex-col gap-3 overflow-y-auto p-3;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.laneScroller {
  @apply flex min-h-0 flex-1 gap-3 overflow-x-auto p-3;
  scrollbar-width: thin;
  scrollbar-color: #c1c7d0 transparent;
}

.workCenterLane {
  @apply flex w-[245px] shrink-0 flex-col rounded p-3;
  background: var(--ui-surface-hover);
}

.laneHeader {
  @apply shrink-0;
}

.laneHeading {
  @apply flex items-center justify-between gap-2;
}

.laneHeading strong {
  @apply truncate text-[12px] font-semibold;
}

.laneHeading span {
  @apply shrink-0 text-[10px] font-semibold;
  color: var(--ui-text-subtle);
}

.loadProgress {
  @apply mt-2 block h-1.5 w-full overflow-hidden rounded-full border-0;
  color: var(--app-primary);
  background: var(--ui-border-subtle);
  appearance: none;
}

.loadProgress::-webkit-progress-bar {
  background: var(--ui-border-subtle);
  border-radius: 999px;
}

.loadProgress::-webkit-progress-value {
  background: var(--app-primary);
  border-radius: 999px;
}

.loadProgress::-moz-progress-bar {
  background: var(--app-primary);
  border-radius: 999px;
}

.laneHeader small {
  @apply mt-2 block text-[10px];
  color: #97a0af;
}

.laneCards {
  @apply mt-3 flex min-h-0 flex-1 flex-col gap-3;
}

.emptyLane {
  @apply flex min-h-[64px] items-center justify-center rounded border border-dashed text-[11px];
  color: #97a0af;
  border-color: var(--ui-border);
  background: rgba(255, 255, 255, 0.45);
}

.pendingCards::-webkit-scrollbar,
.laneScroller::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}

.pendingCards::-webkit-scrollbar-thumb,
.laneScroller::-webkit-scrollbar-thumb {
  background: #c1c7d0;
  border-radius: 999px;
}

@media (max-width: 1050px) {
  .board {
    grid-template-columns: 280px minmax(760px, 1fr);
  }
}
</style>
