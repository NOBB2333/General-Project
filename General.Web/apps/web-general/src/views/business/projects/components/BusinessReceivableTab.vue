<script lang="ts" setup>
import { Card, Table } from 'ant-design-vue';

import type { BusinessApi } from '#/api/core';

import { useBusinessProjectsContext } from '../context';

const {
  detail,
  detailTableScroll,
  formatMoney,
  receivableColumns,
} = useBusinessProjectsContext();
</script>

<template>
<div class="business-projects__tab-grid">
  <Card :bordered="false" title="收尾专项视图">
    <div class="business-projects__summary-grid">
      <div class="business-projects__summary-item"><span>经营收尾状态</span><strong>{{ detail.summary.businessCloseStatus }}</strong></div>
      <div class="business-projects__summary-item"><span>项目状态</span><strong>{{ detail.summary.status }}</strong></div>
      <div class="business-projects__summary-item"><span>待回款事项</span><strong>{{ detail.receivables.filter((item: BusinessApi.ReceivableItem) => item.status.includes('待回款')).length }} 项</strong></div>
      <div class="business-projects__summary-item"><span>年底预计回款</span><strong>{{ formatMoney(detail.summary.yearEndReceivableForecast) }}</strong></div>
    </div>
  </Card>

  <Table
    class="business-projects__table"
    :columns="receivableColumns"
    :data-source="detail.receivables"
    :pagination="false"
    row-key="id"
    :scroll="detailTableScroll"
    size="middle"
  >
    <template #bodyCell="{ column, record }">
      <template v-if="column.key === 'totalAmount'">{{ formatMoney(record.totalAmount) }}</template>
      <template v-else-if="column.key === 'receivedAmount'">{{ formatMoney(record.receivedAmount) }}</template>
    </template>
  </Table>
</div>
</template>
