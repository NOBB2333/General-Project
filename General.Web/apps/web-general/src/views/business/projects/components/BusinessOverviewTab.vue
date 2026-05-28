<script lang="ts" setup>
import { Card, Table } from 'ant-design-vue';

import { useBusinessProjectsContext } from '../context';

const {
  closingItems,
  currentProject,
  detail,
  detailTableScroll,
  formatMoney,
  historyColumns,
} = useBusinessProjectsContext();
</script>

<template>
<div class="business-projects__tab-grid">
  <Card :bordered="false" title="经营摘要">
    <div class="business-projects__summary-grid">
      <div class="business-projects__summary-item"><span>当前项目</span><strong>{{ currentProject?.name ?? '--' }}</strong></div>
      <div class="business-projects__summary-item"><span>合同收入</span><strong>{{ formatMoney(detail.summary.contractAmount) }}</strong></div>
      <div class="business-projects__summary-item"><span>分包与采购</span><strong>{{ formatMoney(detail.summary.procurementAmount) }}</strong></div>
      <div class="business-projects__summary-item"><span>年底预计回款</span><strong>{{ formatMoney(detail.summary.yearEndReceivableForecast) }}</strong></div>
      <div class="business-projects__summary-item"><span>经营收尾状态</span><strong>{{ detail.summary.businessCloseStatus }}</strong></div>
      <div class="business-projects__summary-item"><span>项目阶段</span><strong>{{ detail.summary.status }}</strong></div>
    </div>
  </Card>

  <Card :bordered="false" title="经营收尾视图">
    <div class="business-projects__closing-grid">
      <article
        v-for="item in closingItems"
        :key="item.key"
        :class="['business-projects__closing-item', { 'is-warning': item.status === 'warning' }]"
      >
        <span>{{ item.label }}</span>
        <strong>{{ item.value }}</strong>
      </article>
    </div>
  </Card>

  <Card :bordered="false" title="最近变化">
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
