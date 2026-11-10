<script setup lang="ts">
import { ArrowDown, ArrowUp } from 'lucide-vue-next';
import type { OpportunityRow } from '@/features/crm/model/opportunities.types';
import OpportunityTableRow from './OpportunityTableRow.vue';

defineProps<{ rows: readonly OpportunityRow[] }>();
</script>

<template>
  <div :class="$style.scroll">
    <table :class="$style.table">
      <thead>
        <tr>
          <th>商机名称</th>
          <th>客户</th>
          <th>阶段</th>
          <th>红绿灯</th>
          <th>优先级</th>
          <th>金额 <ArrowUp :size="10" /><ArrowDown :size="10" /></th>
          <th>概率</th>
          <th>预计成交 <ArrowUp :size="10" /><ArrowDown :size="10" /></th>
          <th>负责人</th>
        </tr>
      </thead>
      <tbody>
        <OpportunityTableRow
          v-for="row in rows"
          :key="row.id"
          :row="row"
        />
        <tr v-if="!rows.length">
          <td
            colspan="9"
            :class="$style.empty"
          >
            当前条件下没有商机
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style module>
@reference "../../../../styles/index.css";

.scroll {
  @apply overflow-x-auto;
}
.table {
  @apply w-full min-w-[1260px] border-collapse text-left text-[12px];
  color: var(--ui-text-muted);
}
.table th {
  @apply h-[52.5px] whitespace-nowrap px-4 font-semibold;
  color: var(--ui-text-subtle);
  background: var(--ui-surface-hover);
  border-bottom: 1px solid var(--ui-border);
}
.table th svg {
  @apply inline;
}
.table th:nth-child(1) {
  width: 270px;
}
.table th:nth-child(2) {
  width: 240px;
}
.table th:nth-child(3) {
  width: 110px;
}
.table th:nth-child(4),
.table th:nth-child(5),
.table th:nth-child(9) {
  width: 100px;
}
.table th:nth-child(6) {
  width: 150px;
}
.table th:nth-child(7),
.table th:nth-child(8) {
  width: 140px;
}
.empty {
  @apply h-36 text-center;
  color: var(--ui-text-subtle);
}
</style>
