<script lang="ts" setup>
import { Table, Tag } from 'ant-design-vue';

import { useBusinessProjectsContext } from '../context';

const {
  budgetColumns,
  detail,
  detailTableScroll,
  formatMoney,
} = useBusinessProjectsContext();
</script>

<template>
<div class="business-projects__tab-panel">
  <Table
    class="business-projects__table"
    :columns="budgetColumns"
    :data-source="detail.budgetExecutions"
    :pagination="false"
    row-key="id"
    :scroll="detailTableScroll"
    size="middle"
  >
    <template #bodyCell="{ column, record }">
      <template v-if="column.key === 'adjustedAmount'">{{ formatMoney(record.adjustedAmount) }}</template>
      <template v-else-if="column.key === 'executedAmount'">{{ formatMoney(record.executedAmount) }}</template>
      <template v-else-if="column.key === 'varianceAmount'">
        <Tag :color="typeof record.varianceAmount === 'number' && record.varianceAmount > 0 ? 'red' : 'blue'">
          {{ formatMoney(record.varianceAmount) }}
        </Tag>
      </template>
    </template>
  </Table>
</div>
</template>
