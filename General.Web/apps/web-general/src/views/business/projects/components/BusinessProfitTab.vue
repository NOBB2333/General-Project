<script lang="ts" setup>
import { Card, Table } from 'ant-design-vue';

import { useBusinessProjectsContext } from '../context';

const {
  detail,
  detailTableScroll,
  formatMoney,
  historyColumns,
} = useBusinessProjectsContext();
</script>

<template>
<div class="business-projects__tab-grid">
  <Card :bordered="false" title="利润结果">
    <div class="business-projects__summary-grid">
      <div class="business-projects__summary-item"><span>合同收入</span><strong>{{ formatMoney(detail.summary.contractAmount) }}</strong></div>
      <div class="business-projects__summary-item"><span>经营成本</span><strong>{{ formatMoney(detail.summary.costAmount) }}</strong></div>
      <div class="business-projects__summary-item"><span>毛利额</span><strong>{{ formatMoney(detail.summary.grossProfitAmount) }}</strong></div>
      <div class="business-projects__summary-item"><span>毛利率</span><strong>{{ typeof detail.summary.grossProfitRate === 'number' ? `${detail.summary.grossProfitRate.toFixed(1)}%` : '--' }}</strong></div>
    </div>
  </Card>

  <Card :bordered="false" title="预测与历史">
    <Table
      class="business-projects__table"
      :columns="historyColumns"
      :data-source="detail.forecastHistories"
      :pagination="false"
      row-key="id"
      :scroll="detailTableScroll"
      size="middle"
    />
  </Card>
</div>
</template>
