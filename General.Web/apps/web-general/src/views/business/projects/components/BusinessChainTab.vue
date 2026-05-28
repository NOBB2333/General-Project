<script lang="ts" setup>
import { Card, Table } from 'ant-design-vue';

import { useBusinessProjectsContext } from '../context';

const {
  businessColumns,
  detail,
  detailTableScroll,
  formatMoney,
  procurementColumns,
} = useBusinessProjectsContext();
</script>

<template>
<div class="business-projects__tab-grid">
  <Card :bordered="false" title="商务链与分包链">
    <Table
      class="business-projects__table"
      :columns="businessColumns"
      :data-source="detail.businessChains"
      :pagination="false"
      row-key="id"
      :scroll="detailTableScroll"
      size="middle"
    />
  </Card>

  <Card :bordered="false" title="分包与采购台账">
    <Table
      class="business-projects__table"
      :columns="procurementColumns"
      :data-source="detail.procurements"
      :pagination="false"
      row-key="id"
      :scroll="detailTableScroll"
      size="middle"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.key === 'amount'">{{ formatMoney(record.amount) }}</template>
      </template>
    </Table>
  </Card>
</div>
</template>
