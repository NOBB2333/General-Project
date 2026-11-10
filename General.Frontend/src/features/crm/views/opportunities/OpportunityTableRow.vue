<script setup lang="ts">
import type { OpportunityPriority, OpportunityRow, OpportunityStage } from '@/features/crm/model/opportunities.types';

defineProps<{ row: OpportunityRow }>();

const currency = new Intl.NumberFormat('zh-CN');
const stageClass: Record<OpportunityStage, string> = {
  线索: 'stageLead',
  意向: 'stageIntent',
  方案: 'stageProposal',
  报价: 'stageQuote',
  谈判: 'stageNegotiation',
  赢单: 'stageWon',
  输单: 'stageLost',
};
const priorityClass: Record<OpportunityPriority, string> = {
  高: 'priorityHigh',
  中: 'priorityMedium',
  低: 'priorityLow',
};
</script>

<template>
  <tr :class="$style.row">
    <td>
      <strong :class="$style.name">{{ row.name }}</strong>
      <small>{{ row.code }}</small>
    </td>
    <td>
      <strong :class="$style.customer">{{ row.customer }}</strong>
      <small>{{ row.contact }}</small>
    </td>
    <td>
      <span :class="[$style.stage, $style[stageClass[row.stage]]]">{{ row.stage }}</span>
    </td>
    <td>
      <span
        :class="[$style.traffic, $style[row.traffic]]"
        :aria-label="row.traffic === 'green' ? '绿灯' : row.traffic === 'orange' ? '橙灯' : '红灯'"
      />
    </td>
    <td>
      <span :class="[$style.priority, $style[priorityClass[row.priority]]]">{{ row.priority }}</span>
    </td>
    <td>
      <strong :class="$style.amount">¥{{ currency.format(row.amount) }}</strong>
    </td>
    <td>
      <span :class="$style.probability">
        <progress
          :class="[$style.probabilityBar, $style[row.traffic]]"
          :value="row.probability"
          max="100"
          :aria-label="`${row.name}成交概率`"
        />
        {{ row.probability }}%
      </span>
    </td>
    <td>{{ row.expectedDate }}</td>
    <td>{{ row.owner }}</td>
  </tr>
</template>

<style module>
@reference "../../../../styles/index.css";

.row {
  @apply h-[72px] border-b transition-colors;
  border-color: var(--ui-border-subtle);
}
.row:hover {
  background: var(--ui-surface-hover);
}
.row td {
  @apply whitespace-nowrap px-4;
}
.row small {
  @apply mt-1 block text-[11px];
  color: #97a0af;
}
.name {
  @apply block text-[12px] font-semibold;
  color: var(--app-primary);
}
.customer {
  @apply block max-w-[210px] truncate text-[12px] font-medium;
  color: var(--ui-text-muted);
}
.stage,
.priority {
  @apply inline-flex rounded px-2 py-1 text-[11px] font-semibold;
}
.stageLead {
  color: #42526e;
  background: #ebecf0;
}
.stageIntent {
  color: #0052cc;
  background: #deebff;
}
.stageProposal {
  color: #ff8b00;
  background: #fff0b3;
}
.stageQuote {
  color: #6554c0;
  background: #eae6ff;
}
.stageNegotiation,
.stageWon {
  color: #00875a;
  background: #e3fcef;
}
.stageLost {
  color: #de350b;
  background: #ffebe6;
}
.traffic {
  @apply block h-3 w-3 rounded-full;
}
.green {
  background: #00875a;
}
.orange {
  background: #ff8b00;
}
.red {
  background: #de350b;
}
.priorityHigh {
  color: #de350b;
  background: #ffebe6;
}
.priorityMedium {
  color: #ff8b00;
  background: #fff0b3;
}
.priorityLow {
  color: #6b778c;
  background: #f4f5f7;
}
.amount {
  @apply font-semibold;
  color: var(--ui-text);
}
.probability {
  @apply flex items-center gap-2 text-[11px];
  color: var(--ui-text-subtle);
}
.probabilityBar {
  @apply block h-1.5 w-[54px] overflow-hidden rounded-full border-0;
  background: #ebecf0;
}
.probabilityBar::-webkit-progress-bar {
  background: #ebecf0;
}
.probabilityBar::-webkit-progress-value {
  @apply rounded-full;
  background: currentcolor;
}
.probabilityBar::-moz-progress-bar {
  @apply rounded-full;
  background: currentcolor;
}
.probabilityBar.green {
  color: #00875a;
}
.probabilityBar.orange {
  color: #ff8b00;
}
.probabilityBar.red {
  color: #de350b;
}
</style>
