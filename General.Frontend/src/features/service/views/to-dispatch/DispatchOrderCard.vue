<script setup lang="ts">
import { Clock3, MapPin, ScanLine, Sparkles, UserRoundPlus } from 'lucide-vue-next';
import { UiButton, UiCheckbox, UiSelect } from '@/components/ui';
import type { UiSelectOption } from '@/components/ui';
import type { DispatchEngineer, DispatchOrderRow } from '../../model/to-dispatch.types';

defineProps<{
  order: DispatchOrderRow;
  selected: boolean;
  recommendedEngineer: DispatchEngineer;
  engineer: string;
  serviceMethod: string;
  engineerOptions: readonly UiSelectOption[];
  serviceMethodOptions: readonly UiSelectOption[];
}>();

const emit = defineEmits<{
  toggle: [checked: boolean];
  'update:engineer': [value: string];
  'update:serviceMethod': [value: string];
  dispatch: [];
  open: [];
}>();
</script>

<template>
  <article :class="[$style.card, selected && $style.selectedCard]">
    <UiCheckbox
      :model-value="selected"
      :class="$style.rowCheckbox"
      :label="`选择工单 ${order.code}`"
      @update:model-value="emit('toggle', $event === true)"
    />

    <div :class="$style.orderContent">
      <div :class="$style.badgeRow">
        <ScanLine
          v-if="order.source === '客户扫码报修'"
          :size="12"
          :stroke-width="1.8"
          :class="$style.scanIcon"
        />
        <button
          type="button"
          :class="$style.codeLink"
          @click="emit('open')"
        >
          {{ order.code }}
        </button>
        <span :class="$style.typeBadge">{{ order.type }}</span>
        <span :class="$style.urgencyBadge">{{ order.urgency }}</span>
        <span
          v-if="order.isShutdown"
          :class="$style.dangerBadge"
          >停机</span
        >
        <span
          v-if="order.overdue"
          :class="$style.dangerBadge"
        >
          <Clock3
            :size="9"
            :stroke-width="1.8"
          />
          超时
        </span>
      </div>

      <h2>{{ order.title }}</h2>

      <div :class="$style.metaRow">
        <span>{{ order.customerName }} · {{ order.contactPerson }}</span>
        <span>
          <MapPin
            :size="10"
            :stroke-width="1.8"
          />
          {{ order.address }}
        </span>
        <span>{{ order.createdAt.slice(5, 16) }}</span>
      </div>

      <div :class="$style.recommendation">
        <Sparkles
          :size="12"
          :stroke-width="1.8"
        />
        <span>
          AI 推荐 <strong>{{ recommendedEngineer.name }}</strong
          >（{{ recommendedEngineer.team }}）：{{ order.recommendationReasons.slice(0, 2).join('；') }}
        </span>
      </div>
    </div>

    <aside :class="$style.assignmentPanel">
      <label>
        <span>派给工程师</span>
        <UiSelect
          :model-value="engineer"
          :options="engineerOptions"
          :class="$style.assignmentSelect"
          :label="`为 ${order.code} 选择工程师`"
          size="sm"
          @update:model-value="emit('update:engineer', $event)"
        />
      </label>
      <label>
        <span>服务方式</span>
        <UiSelect
          :model-value="serviceMethod"
          :options="serviceMethodOptions"
          :class="$style.assignmentSelect"
          :label="`为 ${order.code} 选择服务方式`"
          size="sm"
          @update:model-value="emit('update:serviceMethod', $event)"
        />
      </label>
      <UiButton
        :class="$style.dispatchButton"
        variant="primary"
        size="sm"
        @click="emit('dispatch')"
      >
        <template #leading>
          <UserRoundPlus
            :size="13"
            :stroke-width="1.8"
          />
        </template>
        派工
      </UiButton>
    </aside>
  </article>
</template>

<style module>
@reference "../../../../styles/index.css";

.card {
  @apply flex items-start gap-3 rounded-lg border bg-white p-4 transition-colors;
  border-color: var(--ui-border);
}

.selectedCard {
  border-color: var(--app-primary);
}

.rowCheckbox {
  @apply mt-0.5;
}

.orderContent {
  @apply min-w-0 flex-1;
}

.badgeRow {
  @apply mb-1 flex flex-wrap items-center gap-2;
}

.scanIcon {
  color: #00a3bf;
}

.codeLink {
  @apply font-mono text-[12px] font-semibold hover:underline;
  color: var(--app-primary);
}

.typeBadge,
.urgencyBadge,
.dangerBadge {
  @apply inline-flex items-center gap-0.5 rounded px-2 py-0.5 text-[10px] font-semibold;
}

.typeBadge {
  color: #006644;
  background: #e3fcef;
}

.urgencyBadge {
  color: #ff8b00;
  background: #fff0b3;
}

.dangerBadge {
  @apply font-bold;
  color: var(--ui-danger);
  background: #ffebe6;
}

.orderContent h2 {
  @apply m-0 text-[13px] font-medium;
  color: var(--ui-text);
}

.metaRow {
  @apply mt-1 flex flex-wrap items-center gap-3 text-[11px];
  color: var(--ui-text-subtle);
}

.metaRow > span {
  @apply inline-flex items-center gap-0.5;
}

.recommendation {
  @apply mt-2 flex items-start gap-1.5 rounded-lg border px-3 py-2 text-[11px];
  color: #6554c0;
  background: #faf9ff;
  border-color: #eae6ff;
}

.recommendation > svg {
  @apply mt-0.5 shrink-0;
}

.assignmentPanel {
  @apply w-[220px] shrink-0 space-y-2;
}

.assignmentPanel label > span {
  @apply mb-1 block text-[11px];
  color: var(--ui-text-subtle);
}

.assignmentSelect,
.dispatchButton {
  @apply w-full;
}

@media (max-width: 860px) {
  .card {
    @apply flex-wrap;
  }

  .assignmentPanel {
    @apply ml-7 w-full;
  }
}
</style>
